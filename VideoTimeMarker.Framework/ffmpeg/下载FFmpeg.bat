@echo off
echo ========================================
echo VideoTimeMarker.Framework FFmpeg 下载助手
echo ========================================
echo.
echo 此脚本将帮助您下载FFmpeg可执行文件
echo.
echo 请按照以下步骤操作：
echo.
echo 1. 访问 FFmpeg 官方下载页面：
echo    https://www.gyan.dev/ffmpeg/builds/
echo.
echo 2. 下载 "release builds" 中的 "ffmpeg-release-essentials.zip"
echo.
echo 3. 解压下载的文件
echo.
echo 4. 从解压后的 bin 文件夹中复制以下文件到当前目录：
echo    - ffmpeg.exe
echo    - ffprobe.exe
echo.
echo 5. 复制完成后，您可以运行 VideoTimeMarker.Framework 应用程序
echo.
echo ========================================
echo.
echo 是否要打开下载页面？(Y/N)
set /p choice=请输入您的选择: 

if /i "%choice%"=="Y" (
    start https://www.gyan.dev/ffmpeg/builds/
    echo 已打开下载页面，请按照上述步骤下载文件
) else (
    echo 请手动访问上述网址下载FFmpeg文件
)

echo.
echo 下载完成后，请将 ffmpeg.exe 和 ffprobe.exe 放置在此文件夹中
echo.
pause