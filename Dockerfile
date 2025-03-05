# Use ASP.NET Core runtime image as base
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080  

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files and restore dependencies
COPY ["GIGANTECORE/GIGANTECORE.csproj", "GIGANTECORE/"]
RUN dotnet restore "GIGANTECORE/GIGANTECORE.csproj"

# Copy the entire source and build
COPY . .
WORKDIR "/src/GIGANTECORE"
RUN dotnet build "GIGANTECORE.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "GIGANTECORE.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=true

FROM base AS final
WORKDIR /app

# Copy the published output to the final stage
COPY --from=publish /app/publish .

# Set ASP.NET Core to listen on port 8080 and configure environment (if needed)
ENV ASPNETCORE_URLS=http://0.0.0.0:8080

# Launch the application
ENTRYPOINT ["dotnet", "GIGANTECORE.dll"]
