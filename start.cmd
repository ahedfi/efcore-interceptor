@echo off
REM Quick start script for the EF Core Interceptor example

echo Starting PostgreSQL container...
docker compose up -d

echo Waiting for database to be ready...
timeout /t 3 /nobreak

echo.
echo Starting .NET API (http://localhost:5080)...
echo.
echo Once the API is running, open src/Api/efcore-interceptor.http in VS Code or Visual Studio
echo to execute the test requests.
echo.
echo Press Ctrl+C in this terminal to stop the API.
echo.

cd src/Api
dotnet run
