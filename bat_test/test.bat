@echo off

Set count=0

:startTelnet
start telnet 127.0.0.1 1492
timeout /t 1 /nobreak
if %count% gtr 15 (goto :endTelnet) else (set /a count+=1)
echo "telnet iniciado"
goto :startTelnet

:endTelnet
echo fin
cls
exit /b