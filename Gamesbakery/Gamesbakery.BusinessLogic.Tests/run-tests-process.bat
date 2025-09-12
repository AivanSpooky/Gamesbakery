@echo off
echo Before test run: %time%
tasklist | find /c "dotnet.exe" > process_count.txt
dotnet test --results-directory allure-results Gamesbakery.BusinessLogic.Tests.csproj --verbosity detailed
echo After test run: %time%
tasklist | find /c "dotnet.exe" >> process_count.txt
type process_count.txt
pause


echo Tests completed. Press any key to exit...
pause