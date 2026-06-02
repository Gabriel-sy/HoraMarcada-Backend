FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Directory.Build.props", "./"]
COPY ["Directory.Packages.props", "./"]
COPY ["HoraMarcada.sln", "./"]
COPY ["src/HoraMarcada.Api/HoraMarcada.Api.csproj", "src/HoraMarcada.Api/"]
COPY ["src/HoraMarcada.Application/HoraMarcada.Application.csproj", "src/HoraMarcada.Application/"]
COPY ["src/HoraMarcada.Domain/HoraMarcada.Domain.csproj", "src/HoraMarcada.Domain/"]
COPY ["src/HoraMarcada.Infrastructure/HoraMarcada.Infrastructure.csproj", "src/HoraMarcada.Infrastructure/"]
RUN dotnet restore "src/HoraMarcada.Api/HoraMarcada.Api.csproj"
COPY . .
RUN dotnet build "src/HoraMarcada.Api/HoraMarcada.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/HoraMarcada.Api/HoraMarcada.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:5213
EXPOSE 5213
ENTRYPOINT ["dotnet", "HoraMarcada.Api.dll"]
