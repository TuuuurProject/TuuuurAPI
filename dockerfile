# Étape build (SDK)
FROM mcr.microsoft.com/dotnet/sdk:8.0.414 AS build
WORKDIR /src

# Copier seulement les projets nécessaires pour l'API
COPY Tuuuur.sln ./
COPY src/Tuuuur.API/Tuuuur.API.csproj src/Tuuuur.API/
COPY src/Tuuuur.Core/Tuuuur.Core.csproj src/Tuuuur.Core/
COPY src/Tuuuur.Domain/Tuuuur.Domain.csproj src/Tuuuur.Domain/
COPY src/Tuuuur.Infrastructure/Tuuuur.Infrastructure.csproj src/Tuuuur.Infrastructure/
COPY src/Tuuuur.Database/Tuuuur.Database.sqlproj src/Tuuuur.Database/

# Restaurer uniquement ce qui est nécessaire pour l'API
RUN dotnet restore src/Tuuuur.API/Tuuuur.API.csproj

# Copier le reste du code
COPY . .

# Publier l'API en Release
WORKDIR /src/src/Tuuuur.API
RUN dotnet publish -c Release -o /app

# Étape runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0.14 AS runtime
WORKDIR /app
COPY --from=build /app ./
EXPOSE 7260
EXPOSE 5253
ENTRYPOINT ["dotnet", "Tuuuur.API.dll"]
