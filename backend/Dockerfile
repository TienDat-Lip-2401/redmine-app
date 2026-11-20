# ---- Build & publish the API ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY RedmineApp.csproj .
RUN dotnet restore RedmineApp.csproj
COPY . .
RUN dotnet publish RedmineApp.csproj -c Release -o /app --no-restore

# ---- Generate SQL scripts for the database ----
# InitialBaseline is an empty migration (the base tables were created database-first),
# so a brand-new database cannot be built from migrations alone. We generate:
#   schema.sql      full schema from the current model (used only on an empty database)
#   history.sql     marks every migration as applied after schema.sql
#   migrations.sql  idempotent migration script (applies pending migrations on existing DBs)
FROM build AS sql
# dotnet-tools.json pins dotnet-ef 10 (needs .NET 10 SDK); use EF 8 tools here and call them by path.
RUN dotnet tool install --tool-path /tools dotnet-ef --version 8.0.11
# Design-time needs a connection string to build the DbContext; nothing connects to it.
ENV ConnectionStrings__DefaultConnection="Server=none;Database=none;TrustServerCertificate=True"
RUN mkdir /sql \
 && /tools/dotnet-ef dbcontext script -o /sql/schema.sql \
 && /tools/dotnet-ef migrations script --idempotent -o /sql/migrations.sql \
 && { echo "IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL CREATE TABLE [__EFMigrationsHistory] ([MigrationId] nvarchar(150) NOT NULL PRIMARY KEY, [ProductVersion] nvarchar(32) NOT NULL);"; \
      /tools/dotnet-ef migrations list --no-connect --prefix-output | grep '^data:' | awk '{print $2}' \
      | while read -r id; do echo "INSERT INTO [__EFMigrationsHistory] VALUES (N'$id', N'8.0.0');"; done; \
    } > /sql/history.sql \
 && cp docker/db-init.sh /sql/db-init.sh \
 && sed -i 's/\r$//' /sql/db-init.sh

# ---- One-shot job: create/upgrade the database (uses sqlcmd from the SQL Server image) ----
FROM mcr.microsoft.com/mssql/server:2022-latest AS dbinit
COPY --from=sql /sql /sql
ENTRYPOINT ["bash", "/sql/db-init.sh"]
CMD []

# ---- API runtime ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "RedmineApp.dll"]
