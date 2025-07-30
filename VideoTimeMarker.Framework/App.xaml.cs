using System;
using System.IO;
using System.Windows;
using VideoTimeMarker.Framework.Services;

namespace VideoTimeMarker.Framework
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // 检查FFmpeg文件是否存在
            if (!CheckFFmpegFiles())
            {
                ShowFFmpegMissingDialog();
                Shutdown();
                return;
            }
            
            // 简化的启动逻辑，不使用依赖注入以减少内存占用
            var mainWindow = new MainWindow();
            mainWindow.Show();
        }
        
        private bool CheckFFmpegFiles()
        {
            try
            {
                var appDir = AppDomain.CurrentDomain.BaseDirectory;
                var ffmpegPath = Path.Combine(appDir, "ffmpeg", "ffmpeg.exe");
                var ffprobePath = Path.Combine(appDir, "ffmpeg", "ffprobe.exe");
                
                return File.Exists(ffmpegPath) && File.Exists(ffprobePath);
            }
            catch
            {
                return false;
            }
        }
        
        private void ShowFFmpegMissingDialog()
        {
            var message = "缺少必要的FFmpeg文件！\n\n" +
                         "应用程序需要以下文件才能正常运行：\n" +
                         "• ffmpeg.exe\n" +
                         "• ffprobe.exe\n\n" +
                         "请将这些文件放置在应用程序目录的 ffmpeg 文件夹中。\n\n" +
                         "您可以：\n" +
                         "1. 运行 ffmpeg 文件夹中的 '下载FFmpeg.bat' 获取帮助\n" +
                         "2. 手动从 https://ffmpeg.org 下载FFmpeg\n\n" +
                         "应用程序将退出。";
                         
            MessageBox.Show(message, "VideoTimeMarker - 缺少FFmpeg文件", 
                          MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}