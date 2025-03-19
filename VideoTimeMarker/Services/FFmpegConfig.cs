using System;
using System.IO;

namespace VideoTimeMarker.Services
{
    public static class FFmpegConfig
    {   
        private static string? _ffmpegPath;

        public static string FFmpegPath
        {
            get
            {
                if (string.IsNullOrEmpty(_ffmpegPath))
                {
                    // 首先检查应用程序目录下的ffmpeg文件夹
                    var appDir = AppDomain.CurrentDomain.BaseDirectory;
                    var ffmpegExePath = Path.Combine(appDir, "ffmpeg", "ffmpeg.exe");
                    
                    if (File.Exists(ffmpegExePath))
                    {
                        _ffmpegPath = ffmpegExePath;
                    }
                    else
                    {
                        throw new FileNotFoundException("找不到FFmpeg可执行文件，请确保ffmpeg.exe已放置在应用程序目录的ffmpeg文件夹中。");
                    }
                }
                
                return _ffmpegPath;
            }
        }

        public static void Initialize(string? customPath = null)
        {
            if (!string.IsNullOrEmpty(customPath))
            {
                if (File.Exists(customPath))
                {
                    _ffmpegPath = customPath;
                    return;
                }
                throw new FileNotFoundException($"指定的FFmpeg路径无效：{customPath}");
            }

            var appDir = AppDomain.CurrentDomain.BaseDirectory;
            var ffmpegDir = Path.Combine(appDir, "ffmpeg");
            var ffmpegExePath = Path.Combine(ffmpegDir, "ffmpeg.exe");

            // 如果ffmpeg目录不存在，创建它
            if (!Directory.Exists(ffmpegDir))
            {
                Directory.CreateDirectory(ffmpegDir);
            }

            // 检查是否需要从嵌入资源复制ffmpeg.exe
            if (!File.Exists(ffmpegExePath))
            {
                var embeddedFfmpegPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg.exe");
                if (File.Exists(embeddedFfmpegPath))
                {
                    File.Copy(embeddedFfmpegPath, ffmpegExePath, true);
                }
            }

            _ffmpegPath = ffmpegExePath;
        }
    }
}