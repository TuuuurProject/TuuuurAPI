using Tuuuur.API.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Tuuuur.API.Tests.Middlewares
{
    public class HandleExceptionMiddlewareTests
    {
        [Fact]
        public async Task TestMiddleware_WithEndpoint_ExpectedResponseAsync()
        {
            using IHost v_Host = await new HostBuilder()
                .ConfigureWebHost(p_WebBuilder =>
                {
                    p_WebBuilder
                        .UseTestServer()
                        .ConfigureServices(p_Services =>
                        {
                            p_Services.AddRouting();
                        })
                        .Configure(p_App =>
                        {
                            p_App.UseRouting();
                            p_App.UseMiddleware<HandleExceptionMiddleware>();
                            p_App.UseEndpoints(p_Endpoints =>
                            {
                                p_Endpoints.MapGet("/hello", () =>
                                    TypedResults.Text("Hello Tests"));
                            });
                        });
                })
                .StartAsync();

            HttpClient v_Client = v_Host.GetTestClient();

            HttpResponseMessage v_Response = await v_Client.GetAsync("/hello");

            v_Response.IsSuccessStatusCode.Should().BeTrue();
            string v_ResponseBody = await v_Response.Content.ReadAsStringAsync();
            v_ResponseBody.Should().BeEquivalentTo("Hello Tests");
        }

        [Fact]
        public async Task TestMiddleware_ErrorResponseAsync()
        {
            // Arrange
            DefaultHttpContext v_Context = new();
            v_Context.Response.Body = new MemoryStream();
            v_Context.Request.Path = "/";

            HandleExceptionMiddleware v_Middleware = new((p_InnerHttpContext) =>
            {
                throw new Exception("Something went wrong");
            }, Mock.Of<ILogger<HandleExceptionMiddleware>>());

            // Act
            await v_Middleware.InvokeAsync(v_Context);

            // Assert
            v_Context.Response.Body.Seek(0, SeekOrigin.Begin);
            string v_Body = await new StreamReader(v_Context.Response.Body).ReadToEndAsync();
            v_Body.Should().BeEquivalentTo(string.Empty);
            v_Context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            v_Context.Response.ContentType.Should().BeEquivalentTo("application/json");
        }
    }
}