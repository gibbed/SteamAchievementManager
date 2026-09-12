@echo off
chcp 65001 >nul
title Building Steam Achievement Manager...

echo ===================================================
echo   Building Steam Achievement Manager 8.0 Release
echo ===================================================
echo.

taskkill /f /im SAM.Picker.exe >nul 2>&1
taskkill /f /im SAM.Game.exe >nul 2>&1

set "MSBUILD_PATH="

for /f "usebackq tokens=*" %%i in (`"%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe" -latest -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe 2^>nul`) do (
    set "MSBUILD_PATH=%%i"
)

if not defined MSBUILD_PATH (
    if exist "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" (
        set "MSBUILD_PATH=C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"
    ) else if exist "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" (
        set "MSBUILD_PATH=C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe"
    )
)

if not defined MSBUILD_PATH (
    echo [ERROR] MSBuild.exe was not found on your system.
    echo Please install Visual Studio 2019 or 2022 with .NET Desktop Development workload.
    pause
    exit /b 1
)

echo Found MSBuild: "%MSBUILD_PATH%"
echo.

echo Building SAM.sln in Release x86 mode...
"%MSBUILD_PATH%" SAM.sln /p:Configuration=Release /p:Platform="x86" /m /nologo
if %ERRORLEVEL% NEQ 0 (
    echo.
    echo [ERROR] Build failed! Check errors above.
    pause
    exit /b %ERRORLEVEL%
)

echo.
echo Ensuring languages directory is copied to release folder...
if not exist "upload\languages" mkdir "upload\languages"
xcopy /s /q /y "languages\*" "upload\languages\" >nul

echo.
echo Cleaning debug symbol (.pdb) files from release folder...
if exist "upload\*.pdb" del /f /q "upload\*.pdb" >nul

echo ===================================================
echo   BUILD SUCCESSFUL!
echo   Output files located in 'upload\' folder.
echo ===================================================
echo.
