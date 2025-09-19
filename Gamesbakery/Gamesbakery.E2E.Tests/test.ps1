# Gamesbakery API Test Script
$baseUrl = "http://localhost:8080"

Write-Host "=== Gamesbakery E2E Test ===" -ForegroundColor Green
Write-Host "Base URL: $baseUrl" -ForegroundColor Yellow

try {
    # 1. Health Check
    Write-Host "`n1. Health Check:" -ForegroundColor Cyan
    $health = Invoke-RestMethod -Uri "$baseUrl/health" -Method Get -TimeoutSec 10
    Write-Host "   Status: OK" -ForegroundColor Green
    Write-Host "   Response: $health" -ForegroundColor Gray

    # 2. Login
    Write-Host "`n2. Login:" -ForegroundColor Cyan
    $loginBody = @{username="testuser"; password="pass123"} | ConvertTo-Json
    try {
        $login = Invoke-RestMethod -Uri "$baseUrl/account/login" -Method Post -Body $loginBody -ContentType "application/json" -TimeoutSec 10
        Write-Host "   Status: SUCCESS" -ForegroundColor Green
        Write-Host "   Response: $login" -ForegroundColor Gray
    }
    catch {
        Write-Host "   Status: FAILED ($($_.Exception.Response.StatusCode))" -ForegroundColor Red
        Write-Host "   Response: $($_.Exception.Message)" -ForegroundColor Gray
    }

    # 3. Top-up Balance
    Write-Host "`n3. Top-up Balance:" -ForegroundColor Cyan
    $topupBody = @{amount=100.50} | ConvertTo-Json
    try {
        $topup = Invoke-RestMethod -Uri "$baseUrl/account/topup" -Method Post -Body $topupBody -ContentType "application/json" -TimeoutSec 10
        Write-Host "   Status: SUCCESS" -ForegroundColor Green
        Write-Host "   Response: $topup" -ForegroundColor Gray
    }
    catch {
        Write-Host "   Status: FAILED ($($_.Exception.Response.StatusCode))" -ForegroundColor Red
        Write-Host "   Response: $($_.Exception.Message)" -ForegroundColor Gray
    }

    # 4. Check Balance
    Write-Host "`n4. Check Balance:" -ForegroundColor Cyan
    try {
        $balance = Invoke-RestMethod -Uri "$baseUrl/account/balance" -Method Get -TimeoutSec 10
        Write-Host "   Status: SUCCESS" -ForegroundColor Green
        Write-Host "   Balance: $($balance.balance) $($balance.currency)" -ForegroundColor Yellow
        if ($balance.balance -ge 100.50) {
            Write-Host "   ✓ Balance verification PASSED" -ForegroundColor Green
        } else {
            Write-Host "   ✗ Balance verification FAILED (expected >= 100.50)" -ForegroundColor Red
        }
    }
    catch {
        Write-Host "   Status: FAILED ($($_.Exception.Response.StatusCode))" -ForegroundColor Red
        Write-Host "   Response: $($_.Exception.Message)" -ForegroundColor Gray
    }

    Write-Host "`n=== Test Complete ===" -ForegroundColor Green
}
catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
}