@echo off
title xdPlayer Release Builder

echo ==============================
echo      xdPlayer Release Build
echo ==============================
echo.

set ROOT=%~dp0
set RELEASE=%ROOT%Release

echo Cleaning old release...
if exist "%RELEASE%" rmdir /s /q "%RELEASE%"

mkdir "%RELEASE%"

echo.
echo Publishing Launcher...

dotnet publish "%ROOT%xdPlayer.Launcher\xdPlayer.Launcher.csproj" ^
-c Release ^
-o "%RELEASE%"

if errorlevel 1 goto FAILED

echo.
echo Publishing Player...

dotnet publish "%ROOT%xdPlayer.App\xdPlayer.App.csproj" ^
-c Release ^
-o "%RELEASE%"

if errorlevel 1 goto FAILED

echo.
echo ==============================
echo Release successfully created!
echo ==============================
echo.

explorer "%RELEASE%"

pause
exit

:FAILED

echo.
echo ==============================
echo Build FAILED
echo ==============================

pause