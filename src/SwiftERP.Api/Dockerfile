FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy the whole solution — the modular monolith's project references span many folders, so a
# selective COPY here would be more fragile than it's worth for a project this size.
COPY . .
RUN dotnet restore SwiftERP.slnx
RUN dotnet publish src/SwiftERP.Api/SwiftERP.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .

# Render sets $PORT at container *runtime*, not build time, so ASPNETCORE_URLS can't be baked in
# as a Dockerfile ENV (that would only see a build-time PORT, which doesn't exist). The shell form
# here defers the substitution to when the container actually starts.
ENTRYPOINT ASPNETCORE_URLS=http://+:${PORT:-8080} dotnet SwiftERP.Api.dll
