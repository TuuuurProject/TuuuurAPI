using Asp.Versioning;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Tuuuur.API.Middlewares;
using Tuuuur.API.Swagger;
using Tuuuur.Core;
using Tuuuur.Domain.Configuration;
using Tuuuur.Infrastructure;
using Tuuuur.Infrastructure.Data.EntityFramework;
using FluentValidation;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;
using System.Text.Json.Serialization;
using Tuuuur.API.Notifications;

namespace Tuuuur.API;

[ExcludeFromCodeCoverage]
internal static class Program
{
    private const string HealthUrl = "/health";
    private static readonly string[] m_SqlServerTags = new string[] { "db", "sql", "sqlserver" };
    private static readonly string[] m_DbContextCheckTags = new string[] { "ef", "dbcontext" };
    private static readonly string[] m_ProcessAllocatedMemoryHealthCheckTags = new string[] { "allocatedmemory", "memory" };

    [SuppressMessage("Style", "IDE1006:Styles d'affectation de noms", Justification = "Inherited named")]
    public static async Task Main(string[] p_Args)
    {
        WebApplicationBuilder v_Builder = WebApplication.CreateBuilder(p_Args);

        string v_ConnectionString =
            v_Builder.Configuration.GetConnectionString(TuuuurContext.ConnectionStringName);
        int v_MaxAllocatedMemory = int.Parse(v_Builder.Configuration["AllocatedMemory"]);
        string v_XmlCommentsFilePath =
            $"{AppDomain.CurrentDomain.BaseDirectory}{Path.DirectorySeparatorChar}{typeof(Program).Assembly.GetName().Name}.xml";

        // Logger
        v_Builder.Host.UseSerilog((_, _, p_Configuration) =>
            p_Configuration.ReadFrom.Configuration(v_Builder.Configuration));

        // Autofac for module registration
        v_Builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
        v_Builder.Host.ConfigureContainer<ContainerBuilder>(p_Builder =>
        {
            p_Builder.RegisterModule<ApiModule>(); //from API itself
            p_Builder.RegisterModule<CoreModule>(); //from core library
            p_Builder.RegisterModule<InfrastructureModule>(); //from infrastructure library
        });

        // Add services to the container.
        v_Builder.Services.AddValidatorsFromAssemblyContaining<ApiModule>();
        v_Builder.Services.AddMediatR(p_Config => p_Config.RegisterServicesFromAssembly(typeof(CoreModule).Assembly));
        v_Builder.Services.AddControllers()
            .AddJsonOptions(p_Options =>
                {
                    // To enable display of enums as string
                    p_Options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                    p_Options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                }
            );

        // Swagger
        v_Builder.Services.AddEndpointsApiExplorer();

        v_Builder.Services
            .AddApiVersioning(p_O =>
            {
                p_O.AssumeDefaultVersionWhenUnspecified = true;
                p_O.DefaultApiVersion = new ApiVersion(1, 0);
                p_O.ApiVersionReader = new UrlSegmentApiVersionReader();
            });

        v_Builder.Services.AddApiVersioning().AddApiExplorer(p_O =>
        {
            p_O.GroupNameFormat = "'v'VVV";
            p_O.SubstituteApiVersionInUrl = true;
        });

        v_Builder.Services.AddSwaggerGen(p_X =>
        {
            // integrate xml comments
            p_X.IncludeXmlComments(v_XmlCommentsFilePath);

            p_X.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme,
                new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please enter into field your Jwt",
                    Name = HeaderNames.Authorization,
                    Type = SecuritySchemeType.Http,
                    Scheme = JwtBearerDefaults.AuthenticationScheme
                });

            p_X.AddSecurityRequirement(
                new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = JwtBearerDefaults.AuthenticationScheme
                            }
                        },
                        Array.Empty<string>()
                    }
                }
            );
        });
        v_Builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();

        // EF Core
        v_Builder.Services.AddDbContext<TuuuurContext>((_, p_OptionsBuilder) =>
            p_OptionsBuilder
                .UseSqlServer(
                    v_ConnectionString,
                    p_SqlServerOptionsBuilder =>
                    {
                        p_SqlServerOptionsBuilder
                            .CommandTimeout((int)TimeSpan.FromMinutes(3).TotalSeconds)
                            .EnableRetryOnFailure();
                    }));

        // AutoMapper
        v_Builder.Services.AddAutoMapper(typeof(Program).Assembly, typeof(CoreModule).Assembly,
            typeof(InfrastructureModule).Assembly);

        // Health check
        v_Builder.Services.AddHealthChecks()
            .AddSqlServer(v_ConnectionString, healthQuery: "SELECT 1;", name: "Sql Server",
                tags: m_SqlServerTags)
            .AddDbContextCheck<TuuuurContext>(name: "EF DbContext", tags: m_DbContextCheckTags)
            .AddProcessAllocatedMemoryHealthCheck(v_MaxAllocatedMemory, name: "Allocated Memory",
                tags: m_ProcessAllocatedMemoryHealthCheckTags);

        v_Builder.Services.AddHealthChecksUI(p_Setup =>
        {
            p_Setup.DisableDatabaseMigrations();
            p_Setup.AddHealthCheckEndpoint("Tuuuur API", HealthUrl);
            p_Setup.MaximumHistoryEntriesPerEndpoint(30);
            p_Setup.SetEvaluationTimeInSeconds(60); //every minute
        }).AddInMemoryStorage();

        // Authentication
        JwtConfiguration v_JwtConf = new();

        v_Builder.Configuration.Bind(JwtConfiguration.SectionName, v_JwtConf);

        v_Builder.Services.AddSingleton(v_JwtConf);

        v_Builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme,
                p_O =>

                    p_O.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidIssuer = v_JwtConf.Issuer,
                        ValidAudience = v_JwtConf.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey
                            (Encoding.UTF8.GetBytes(v_JwtConf.Key)),
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = false,
                        ValidateIssuerSigningKey = true
                    });

        // CORS
        v_Builder.Services.AddCors(static options =>
        {
            options.AddDefaultPolicy(
                static policy =>
                {
#pragma warning disable S5122 //we accept any origin for this api
                    policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
#pragma warning disable S5122
                }
            );
        });
        
        // Add SignalR
        v_Builder.Services.AddSignalR();
        
        // Razor
        v_Builder.Services.AddRazorTemplating();

        WebApplication v_App = v_Builder.Build();

        // Configure the HTTP request pipeline.
        if (v_App.Environment.IsDevelopment())
        {
            v_App.UseSwagger();
            v_App.UseSwaggerUI();
        }

        v_App.UseHttpsRedirection();

        v_App.UseCors();

        v_App.UseRouting();

        v_App.UseAuthentication();
        v_App.UseAuthorization();

        v_App.UseSerilogRequestLogging();

        v_App.UseMiddleware<HandleExceptionMiddleware>();
        
        v_App.MapHub<NotificationsHub>("notifications");
        
        v_App.MapControllers();
        v_App.MapHealthChecksUI(p_Setup =>
        {
            p_Setup.UseRelativeApiPath = true;
            p_Setup.ApiPath = $"{HealthUrl}-api";
            p_Setup.UIPath = $"{HealthUrl}-ui";
        });

        v_App.MapHealthChecks(HealthUrl, new HealthCheckOptions()
        {
            AllowCachingResponses = false,
            Predicate = _ => true,
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        await v_App.RunAsync();
    }
}