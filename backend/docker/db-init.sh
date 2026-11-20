#!/bin/bash
# Creates the database on first run, then applies any pending EF migrations.
set -euo pipefail

DB_NAME="${DB_NAME:-RedmineApp}"
SQLCMD=(/opt/mssql-tools18/bin/sqlcmd -S "${DB_HOST:-db}" -U sa -P "$MSSQL_SA_PASSWORD" -C -b)

"${SQLCMD[@]}" -Q "IF DB_ID(N'$DB_NAME') IS NULL CREATE DATABASE [$DB_NAME]"

tables=$("${SQLCMD[@]}" -d "$DB_NAME" -h -1 -W -Q "SET NOCOUNT ON; SELECT COUNT(*) FROM sys.tables WHERE name <> '__EFMigrationsHistory'" | tr -d '[:space:]')

if [ "$tables" = "0" ]; then
  echo "Empty database: creating schema from the current model"
  "${SQLCMD[@]}" -d "$DB_NAME" -i /sql/schema.sql
  "${SQLCMD[@]}" -d "$DB_NAME" -i /sql/history.sql
fi

echo "Applying pending migrations"
"${SQLCMD[@]}" -d "$DB_NAME" -i /sql/migrations.sql
echo "Database ready"
