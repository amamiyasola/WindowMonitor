@echo off
>nul 2>&1 "%SYSTEMROOT%\system32\cacls.exe" "%SYSTEMROOT%\system32\config\system"
if '%errorlevel%' NEQ '0' (goto UACPrompt) else ( goto gotAdmin )
:UACPrompt
echo Set UAC = CreateObject^("Shell.Application"^) > "%temp%\getadmin.vbs"
echo UAC.ShellExecute "%~s0", "", "", "runas", 1 >> "%temp%\getadmin.vbs"
"%temp%\getadmin.vbs"
exit /B
:gotAdmin
if exist "%temp%\getadmin.vbs" ( del "%temp%\getadmin.vbs" )
pushd "%CD%"
CD /D "%~dp0"
cd %cd%
@echo on
cd %~dp0
set b=%cd%
sc stop WindowMonitorService
sc delete WindowMonitorService
sc create WindowMonitorService binpath= "%b%\WindowMonitorApp.exe " start=auto displayname= "��̨��ط���"
sc start WindowMonitorService
echo "������Ҫ����,������������"
exit