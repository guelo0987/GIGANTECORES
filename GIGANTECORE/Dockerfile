# Use ASP.NET Core runtime image as base
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
ENV PORT=8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["GIGANTECORE.csproj", "."]
RUN dotnet restore "./GIGANTECORE.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "GIGANTECORE.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "GIGANTECORE.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GIGANTECORE.dll"]
