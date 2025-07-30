using System.Windows;

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
            
            // 简化的启动逻辑，不使用依赖注入以减少内存占用
            var mainWindow = new MainWindow();
            mainWindow.Show();
        }
    }
}