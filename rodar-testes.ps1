Write-Host "Limpando resultados anteriores..." -ForegroundColor Yellow
Remove-Item -Recurse -Force ./TestResults -ErrorAction SilentlyContinue
Remove-Item -Recurse -Force ./RelatorioCobertura -ErrorAction SilentlyContinue

Write-Host "Rodando testes..." -ForegroundColor Yellow
dotnet test ./FraudSys.Tests/FraudSys.Tests.csproj `
    --collect:"XPlat Code Coverage" `
    --results-directory ./TestResults `
    --logger "console;verbosity=detailed"

Write-Host "Gerando relatorio de cobertura..." -ForegroundColor Yellow
reportgenerator `
    -reports:"./TestResults/**/coverage.cobertura.xml" `
    -targetdir:"./RelatorioCobertura" `
    -reporttypes:"Html;TextSummary"

Write-Host "`nResumo de Cobertura:" -ForegroundColor Cyan
Get-Content ./RelatorioCobertura/Summary.txt

Write-Host "`nAbrindo relatorio no navegador..." -ForegroundColor Green
Start-Process ./RelatorioCobertura/index.html