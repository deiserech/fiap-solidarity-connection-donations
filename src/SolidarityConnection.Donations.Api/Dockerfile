ARG BUILD_CONFIGURATION=Release

FROM mcr.microsoft.com/dotnet/aspnet:8.0-bookworm-slim AS base
WORKDIR /app
EXPOSE 80

RUN apt-get update \
 && apt-get install -y curl ca-certificates gnupg \
 && echo 'deb [signed-by=/etc/apt/keyrings/newrelic-apt.gpg] http://apt.newrelic.com/debian/ newrelic non-free' | tee /etc/apt/sources.list.d/newrelic.list \
 && curl -fsSL https://download.newrelic.com/NEWRELIC_APT_2DAD550E.public | gpg --dearmor > /etc/apt/keyrings/newrelic-apt.gpg \
 && apt-get update \
 && apt-get install -y newrelic-dotnet-agent \
 && apt-get clean \
 && rm -rf /var/lib/apt/lists/*

ENV CORECLR_ENABLE_PROFILING=1 \
	CORECLR_PROFILER={36032161-FFC0-4B61-B559-F6C5D41BAE5A} \
	CORECLR_NEWRELIC_HOME=/usr/local/newrelic-dotnet-agent \
	CORECLR_PROFILER_PATH=/usr/local/newrelic-dotnet-agent/libNewRelicProfiler.so \
	NEW_RELIC_APP_NAME=SolidarityConnection.Donations \
	ASPNETCORE_URLS=http://0.0.0.0:80

FROM mcr.microsoft.com/dotnet/sdk:8.0-noble AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/SolidarityConnection.Donations.Api/SolidarityConnection.Donations.Api.csproj", "src/SolidarityConnection.Donations.Api/"]
COPY ["src/SolidarityConnection.Donations.Application/SolidarityConnection.Donations.Application.csproj", "src/SolidarityConnection.Donations.Application/"]
COPY ["src/SolidarityConnection.Donations.Domain/SolidarityConnection.Donations.Domain.csproj", "src/SolidarityConnection.Donations.Domain/"]
COPY ["src/SolidarityConnection.Donations.Infrastructure/SolidarityConnection.Donations.Infrastructure.csproj", "src/SolidarityConnection.Donations.Infrastructure/"]
RUN dotnet restore "src/SolidarityConnection.Donations.Api/SolidarityConnection.Donations.Api.csproj"
COPY . .
WORKDIR "/src/src/SolidarityConnection.Donations.Api"
RUN dotnet build "SolidarityConnection.Donations.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "SolidarityConnection.Donations.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .


ENTRYPOINT ["dotnet", "SolidarityConnection.Donations.Api.dll"]