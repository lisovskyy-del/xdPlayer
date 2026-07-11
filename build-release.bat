@echo off
title xdPlayer Release Builder

echo ==============================
echo      xdPlayer Release Build
echo ==============================
echo.

set ROOT=%~dp0
set APP_PROJECT=%ROOT%xdPlayer.App\xdPlayer.App.csproj
set RELEASE=%ROOT%Release

echo Reading version...

for /f "tokens=2 delims=<>" %%a in ('findstr /i "<Version>" "%APP_PROJECT%"') do (
    set VERSION=%%a
)

echo Version: %VERSION%
pause

if "%VERSION%"=="" (
    echo Failed to read version from xdPlayer.App.csproj
    pause
    exit /b 1
)

set ZIP=%ROOT%xdPlayer-v%VERSION%.zip

echo Cleaning old release...

if exist "%RELEASE%" rmdir /s /q "%RELEASE%"
if exist "%ZIP%" del /q "%ZIP%"

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
echo Creating archive...

powershell -NoProfile -Command ^
"Compress-Archive -Path '%RELEASE%\*' -DestinationPath '%ZIP%' -Force"

if errorlevel 1 goto FAILED

echo.
echo ==============================
echo Release successfully created!
echo ==============================
echo.
echo Version : %VERSION%
echo Folder  : %RELEASE%
echo Archive : %ZIP%
echo.

explorer "%ROOT%"

pause
exit

:FAILED

echo.
echo ==============================
echo Build FAILED
echo ==============================

pause