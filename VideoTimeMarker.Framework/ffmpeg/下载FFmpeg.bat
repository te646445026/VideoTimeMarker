@echo off
title FFmpeg Download Helper

echo ========================================
echo VideoTimeMarker.Framework FFmpeg Helper
echo ========================================
echo.
echo This script will help you download FFmpeg files.
echo.
echo Steps to follow:
echo.
echo 1. Visit FFmpeg download page:
echo    https://www.gyan.dev/ffmpeg/builds/
echo.
echo 2. Download "ffmpeg-release-essentials.zip" from "release builds"
echo.
echo 3. Extract the downloaded file
echo.
echo 4. Copy these files from the bin folder to this directory:
echo    - ffmpeg.exe
echo    - ffprobe.exe
echo.
echo 5. After copying, you can run VideoTimeMarker.Framework
echo.
echo ========================================
echo.

:menu
echo Choose an option:
echo [1] Open download page
echo [2] Check current files
echo [3] Exit
echo.
set /p choice=Enter your choice (1-3): 

if "%choice%"=="1" goto open_page
if "%choice%"=="2" goto check_files
if "%choice%"=="3" goto exit
echo Invalid choice. Please enter 1, 2, or 3.
echo.
goto menu

:open_page
echo.
echo Opening download page...
start "" "https://www.gyan.dev/ffmpeg/builds/"
echo Download page opened. Please follow the steps above.
echo.
goto menu

:check_files
echo.
echo Checking FFmpeg files in current folder...
if exist "ffmpeg.exe" (
    echo [OK] ffmpeg.exe found
) else (
    echo [MISSING] ffmpeg.exe not found
)

if exist "ffprobe.exe" (
    echo [OK] ffprobe.exe found
) else (
    echo [MISSING] ffprobe.exe not found
)

if exist "ffmpeg.exe" if exist "ffprobe.exe" (
    echo.
    echo Great! All required FFmpeg files are present.
    echo You can now run VideoTimeMarker.Framework application.
) else (
    echo.
    echo Please download the missing files and place them in this folder.
)
echo.
goto menu

:exit
echo.
echo After downloading, place these files in this folder:
echo - ffmpeg.exe
echo - ffprobe.exe
echo.
echo Then you can use VideoTimeMarker.Framework normally!
echo.
echo Press any key to exit...
pause >nul