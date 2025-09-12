@echo off
net stop "MSSQLSERVER" /y
if %errorlevel% neq 0 (
    echo Failed to stop SQL Server service. Check if the service name is correct or run as Administrator.
    pause
    exit /b %errorlevel%
)

echo Running tests in offline mode...
dotnet test --results-directory allure-results Gamesbakery.BusinessLogic.Tests.csproj

net start "MSSQLSERVER"
if %errorlevel% neq 0 (
    echo Failed to start SQL Server service. Manual intervention may be required.
    pause
    exit /b %errorlevel%
)

echo Tests completed. Press any key to exit...
pause