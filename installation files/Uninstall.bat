setlocal
cd /d %~dp0
InstallUtil.exe -u ATMISOService.exe
@echo off
set /p id=Press any key to continue: 
