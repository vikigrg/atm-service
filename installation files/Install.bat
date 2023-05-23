setlocal
cd /d %~dp0
InstallUtil.exe ATMISOService.exe
@echo off
set /p id=Press any key to continue: 
