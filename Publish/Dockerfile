FROM mcr.microsoft.com/dotnet/sdk:8.0

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY . .

RUN mkdir -p /app/keys

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV INSTANCE_NAME="Servidor Docker"

EXPOSE 8080

ENTRYPOINT ["dotnet", "WebApplication1.Web.dll"]
