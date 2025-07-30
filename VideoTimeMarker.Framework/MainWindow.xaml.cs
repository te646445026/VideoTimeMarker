using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VideoTimeMarker.Framework.ViewModels;

namespace VideoTimeMarker.Framework
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainWindowViewModel _viewModel;
        private Point _startPoint;
        private bool _isSelecting;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new MainWindowViewModel();
            DataContext = _viewModel;

            VideoPlayer.MediaOpened += VideoPlayer_MediaOpened;
            
            // 订阅ViewModel的播放控制事件
            _viewModel.RequestPlayVideo += () => VideoPlayer.Play();
            _viewModel.RequestPauseVideo += () => VideoPlayer.Pause();
            _viewModel.RequestStopVideo += () => VideoPlayer.Stop();
            
            _viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(MainWindowViewModel.SelectedVideoPath) && _viewModel.SelectedVideoPath != null)
                {
                    VideoPlayer.Source = new Uri(_viewModel.SelectedVideoPath);
                    VideoPlayer.Play();
                }
            };
        }

        private void VideoPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            _viewModel.OnVideoOpened(VideoPlayer.NaturalDuration.TimeSpan);
        }

        private void VideoPlayer_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            MessageBox.Show($"视频加载失败：{e.ErrorException.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void OnCanvasMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isSelecting = true;
            _startPoint = e.GetPosition(SelectionCanvas);
            
            // 计算视频实际显示区域
            var videoActualWidth = VideoPlayer.ActualWidth;
            var videoActualHeight = VideoPlayer.ActualHeight;
            var canvasWidth = SelectionCanvas.ActualWidth;
            var canvasHeight = SelectionCanvas.ActualHeight;
            
            // 确保坐标在视频显示区域内
            if (_startPoint.X >= (canvasWidth - videoActualWidth) / 2 && 
                _startPoint.X <= (canvasWidth + videoActualWidth) / 2 &&
                _startPoint.Y >= (canvasHeight - videoActualHeight) / 2 && 
                _startPoint.Y <= (canvasHeight + videoActualHeight) / 2)
            {
                SelectionRectangle.SetValue(Canvas.LeftProperty, _startPoint.X);
                SelectionRectangle.SetValue(Canvas.TopProperty, _startPoint.Y);
                SelectionRectangle.Width = 0;
                SelectionRectangle.Height = 0;
                SelectionRectangle.Visibility = Visibility.Visible;
            }
        }

        private void OnCanvasMouseMove(object sender, MouseEventArgs e)
        {
            if (_isSelecting)
            {
                var currentPoint = e.GetPosition(SelectionCanvas);
                
                // 计算视频实际显示区域
                var videoActualWidth = VideoPlayer.ActualWidth;
                var videoActualHeight = VideoPlayer.ActualHeight;
                var canvasWidth = SelectionCanvas.ActualWidth;
                var canvasHeight = SelectionCanvas.ActualHeight;
                
                // 确保坐标在视频显示区域内
                if (currentPoint.X >= (canvasWidth - videoActualWidth) / 2 && 
                    currentPoint.X <= (canvasWidth + videoActualWidth) / 2 &&
                    currentPoint.Y >= (canvasHeight - videoActualHeight) / 2 && 
                    currentPoint.Y <= (canvasHeight + videoActualHeight) / 2)
                {
                    var x = Math.Min(_startPoint.X, currentPoint.X);
                    var y = Math.Min(_startPoint.Y, currentPoint.Y);
                    var width = Math.Abs(currentPoint.X - _startPoint.X);
                    var height = Math.Abs(currentPoint.Y - _startPoint.Y);

                    SelectionRectangle.SetValue(Canvas.LeftProperty, x);
                    SelectionRectangle.SetValue(Canvas.TopProperty, y);
                    SelectionRectangle.Width = width;
                    SelectionRectangle.Height = height;
                }
            }
        }

        private void OnCanvasMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isSelecting = false;
            var currentPoint = e.GetPosition(SelectionCanvas);
            
            // 计算视频实际显示区域
            var videoActualWidth = VideoPlayer.ActualWidth;
            var videoActualHeight = VideoPlayer.ActualHeight;
            var canvasWidth = SelectionCanvas.ActualWidth;
            var canvasHeight = SelectionCanvas.ActualHeight;
            
            // 计算视频在Canvas中的位置偏移
            var videoLeft = (canvasWidth - videoActualWidth) / 2;
            var videoTop = (canvasHeight - videoActualHeight) / 2;
            
            // 确保坐标在视频显示区域内
            if (currentPoint.X >= videoLeft && currentPoint.X <= videoLeft + videoActualWidth &&
                currentPoint.Y >= videoTop && currentPoint.Y <= videoTop + videoActualHeight)
            {
                var x = Math.Min(_startPoint.X, currentPoint.X);
                var y = Math.Min(_startPoint.Y, currentPoint.Y);
                var width = Math.Abs(currentPoint.X - _startPoint.X);
                var height = Math.Abs(currentPoint.Y - _startPoint.Y);
                
                // 计算相对于视频的坐标（而不是相对于Canvas）
                var relativeX = Math.Max(0, x - videoLeft);
                var relativeY = Math.Max(0, y - videoTop);
                
                // 如果选择区域超出视频范围，调整宽度和高度
                if (x + width > videoLeft + videoActualWidth)
                {
                    width = videoLeft + videoActualWidth - x;
                }
                if (y + height > videoTop + videoActualHeight)
                {
                    height = videoTop + videoActualHeight - y;
                }
                
                // 计算相对于原始视频尺寸的坐标和尺寸
                if (VideoPlayer.NaturalVideoWidth > 0 && VideoPlayer.NaturalVideoHeight > 0)
                {
                    // 计算缩放比例
                    var scaleX = (double)VideoPlayer.NaturalVideoWidth / videoActualWidth;
                    var scaleY = (double)VideoPlayer.NaturalVideoHeight / videoActualHeight;
                    
                    // 转换为原始视频坐标系
                    var originalX = (int)(relativeX * scaleX);
                    var originalY = (int)(relativeY * scaleY);
                    var originalWidth = (int)(width * scaleX);
                    var originalHeight = (int)(height * scaleY);
                    
                    // 更新ViewModel中的裁剪参数
                    _viewModel.CropXText = originalX.ToString();
                    _viewModel.CropYText = originalY.ToString();
                    _viewModel.CropWidthText = originalWidth.ToString();
                    _viewModel.CropHeightText = originalHeight.ToString();
                }
                else
                {
                    // 如果无法获取原始视频尺寸，则使用相对坐标
                    _viewModel.CropXText = ((int)relativeX).ToString();
                    _viewModel.CropYText = ((int)relativeY).ToString();
                    _viewModel.CropWidthText = ((int)width).ToString();
                    _viewModel.CropHeightText = ((int)height).ToString();
                }
            }
        }
    }
}