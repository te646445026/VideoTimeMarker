using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using VideoTimeMarker.Services;

namespace VideoTimeMarker
{
    public partial class MainWindow : Window
    {
        private readonly IFFmpegService _ffmpegService;
        private string? _selectedVideoPath;
        private TimeSpan _videoDuration;
        private bool _isProcessing;
        private Point _startPoint;
        private bool _isSelecting;

        public MainWindow(IFFmpegService ffmpegService)
        {
            InitializeComponent();
            _ffmpegService = ffmpegService;
            StartDatePicker.SelectedDate = DateTime.Today;
            StartTimePicker.Text = DateTime.Now.ToString("HH:mm:ss");

            VideoPlayer.MediaOpened += VideoPlayer_MediaOpened;
            _ffmpegService.ProgressChanged += FFmpegService_ProgressChanged;
        }

        private void VideoPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            _videoDuration = VideoPlayer.NaturalDuration.TimeSpan;
        }

        private void OnSelectVideoClick(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "视频文件|*.mp4;*.avi;*.mkv|所有文件|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                _selectedVideoPath = openFileDialog.FileName;
                FilePathTextBox.Text = _selectedVideoPath;

                VideoPlayer.Source = new Uri(_selectedVideoPath);
                VideoPlayer.Play();
            }
        }

        private void OnPlayClick(object sender, RoutedEventArgs e)
        {
            VideoPlayer.Play();
        }

        private void OnPauseClick(object sender, RoutedEventArgs e)
        {
            VideoPlayer.Pause();
        }

        private void OnStopClick(object sender, RoutedEventArgs e)
        {
            VideoPlayer.Stop();
        }

        private void FFmpegService_ProgressChanged(object? sender, ProgressEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                ProcessingProgressBar.Value = e.Progress;
                ProcessingStatusText.Text = e.Status;
            });
        }

        private async void OnAddWatermarkClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedVideoPath))
            {
                MessageBox.Show("请先选择视频文件", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!DateTime.TryParse(StartTimePicker.Text, out var time) || StartDatePicker.SelectedDate == null)
            {
                MessageBox.Show("请输入有效的时间格式", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var startTime = StartDatePicker.SelectedDate.Value.Add(time.TimeOfDay);
            if (!int.TryParse(FontSizeTextBox.Text, out var fontSize) || fontSize <= 0)
            {
                MessageBox.Show("请输入有效的字体大小", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(WatermarkXTextBox.Text, out var x) || x < 0)
            {
                MessageBox.Show("请输入有效的X坐标", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(WatermarkYTextBox.Text, out var y) || y < 0)
            {
                MessageBox.Show("请输入有效的Y坐标", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var outputPath = System.IO.Path.Combine(
                System.IO.Path.GetDirectoryName(_selectedVideoPath)!,
                $"{System.IO.Path.GetFileNameWithoutExtension(_selectedVideoPath)}_watermarked{System.IO.Path.GetExtension(_selectedVideoPath)}"
            );

            if (_isProcessing)
            {
                MessageBox.Show("正在处理中，请等待...", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                _isProcessing = true;
                ProcessingProgressBar.Visibility = Visibility.Visible;
                ProcessingStatusText.Visibility = Visibility.Visible;
                ProcessingProgressBar.Value = 0;
                var result = await _ffmpegService.AddDynamicTimeWatermark(_selectedVideoPath, outputPath, startTime, _videoDuration, fontSize, x, y);
                _isProcessing = false;
                ProcessingProgressBar.Visibility = Visibility.Collapsed;
                ProcessingStatusText.Visibility = Visibility.Collapsed;

                if (result == 0)
                {
                    MessageBox.Show("水印添加成功！", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("水印添加失败", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                _isProcessing = false;
                ProcessingProgressBar.Visibility = Visibility.Collapsed;
                ProcessingStatusText.Visibility = Visibility.Collapsed;
                MessageBox.Show($"处理过程中发生错误：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void OnCropVideoClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedVideoPath))
            {
                MessageBox.Show("请先选择视频文件", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            // 显示操作说明
            MessageBox.Show("注意：裁剪功能将保留选中区域的内容，其他区域将被移除", "操作说明", MessageBoxButton.OK, MessageBoxImage.Information);

            if (!int.TryParse(CropWidthTextBox.Text, out var width) || width <= 0)
            {
                MessageBox.Show("请输入有效的裁剪宽度", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(CropHeightTextBox.Text, out var height) || height <= 0)
            {
                MessageBox.Show("请输入有效的裁剪高度", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(CropXTextBox.Text, out var cropX) || cropX < 0)
            {
                MessageBox.Show("请输入有效的裁剪X坐标", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(CropYTextBox.Text, out var cropY) || cropY < 0)
            {
                MessageBox.Show("请输入有效的裁剪Y坐标", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var outputPath = System.IO.Path.Combine(
                System.IO.Path.GetDirectoryName(_selectedVideoPath)!,
                $"{System.IO.Path.GetFileNameWithoutExtension(_selectedVideoPath)}_cropped{System.IO.Path.GetExtension(_selectedVideoPath)}"
            );

            if (_isProcessing)
            {
                MessageBox.Show("正在处理中，请等待...", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                _isProcessing = true;
                ProcessingProgressBar.Visibility = Visibility.Visible;
                ProcessingStatusText.Visibility = Visibility.Visible;
                ProcessingProgressBar.Value = 0;
                var result = await _ffmpegService.CropVideo(_selectedVideoPath, outputPath, width, height, cropX, cropY);
                _isProcessing = false;
                ProcessingProgressBar.Visibility = Visibility.Collapsed;
                ProcessingStatusText.Visibility = Visibility.Collapsed;

                if (result == 0)
                {
                    MessageBox.Show("视频裁剪成功！", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("视频裁剪失败", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                _isProcessing = false;
                ProcessingProgressBar.Visibility = Visibility.Collapsed;
                ProcessingStatusText.Visibility = Visibility.Collapsed;
                MessageBox.Show($"处理过程中发生错误：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnCanvasMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
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

        private void OnCanvasMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
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

        private void OnCanvasMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
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
                    
                    // 更新裁剪参数
                    CropXTextBox.Text = originalX.ToString();
                    CropYTextBox.Text = originalY.ToString();
                    CropWidthTextBox.Text = originalWidth.ToString();
                    CropHeightTextBox.Text = originalHeight.ToString();
                }
                else
                {
                    // 如果无法获取原始视频尺寸，则使用相对坐标
                    CropXTextBox.Text = ((int)relativeX).ToString();
                    CropYTextBox.Text = ((int)relativeY).ToString();
                    CropWidthTextBox.Text = ((int)width).ToString();
                    CropHeightTextBox.Text = ((int)height).ToString();
                }
            }
        }

        private void OnOpenOutputFolderClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedVideoPath))
            {
                MessageBox.Show("请先选择视频文件", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var folderPath = System.IO.Path.GetDirectoryName(_selectedVideoPath);
                if (!string.IsNullOrEmpty(folderPath) && System.IO.Directory.Exists(folderPath))
                {
                    System.Diagnostics.Process.Start("explorer.exe", folderPath);
                }
                else
                {
                    MessageBox.Show("无法打开文件夹，路径不存在", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开文件夹时发生错误：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private async void OnCropAndAddWatermarkClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedVideoPath))
            {
                MessageBox.Show("请先选择视频文件", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            // 显示操作说明
            MessageBox.Show("注意：此操作将同时裁剪视频并添加时间水印", "操作说明", MessageBoxButton.OK, MessageBoxImage.Information);

            // 验证裁剪参数
            if (!int.TryParse(CropWidthTextBox.Text, out var width) || width <= 0)
            {
                MessageBox.Show("请输入有效的裁剪宽度", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(CropHeightTextBox.Text, out var height) || height <= 0)
            {
                MessageBox.Show("请输入有效的裁剪高度", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(CropXTextBox.Text, out var cropX) || cropX < 0)
            {
                MessageBox.Show("请输入有效的裁剪X坐标", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(CropYTextBox.Text, out var cropY) || cropY < 0)
            {
                MessageBox.Show("请输入有效的裁剪Y坐标", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            // 验证水印参数
            if (!DateTime.TryParse(StartTimePicker.Text, out var time) || StartDatePicker.SelectedDate == null)
            {
                MessageBox.Show("请输入有效的时间格式", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var startTime = StartDatePicker.SelectedDate.Value.Add(time.TimeOfDay);
            if (!int.TryParse(FontSizeTextBox.Text, out var fontSize) || fontSize <= 0)
            {
                MessageBox.Show("请输入有效的字体大小", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(WatermarkXTextBox.Text, out var watermarkX) || watermarkX < 0)
            {
                MessageBox.Show("请输入有效的水印X坐标", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(WatermarkYTextBox.Text, out var watermarkY) || watermarkY < 0)
            {
                MessageBox.Show("请输入有效的水印Y坐标", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var outputPath = System.IO.Path.Combine(
                System.IO.Path.GetDirectoryName(_selectedVideoPath)!,
                $"{System.IO.Path.GetFileNameWithoutExtension(_selectedVideoPath)}_cropped_watermarked{System.IO.Path.GetExtension(_selectedVideoPath)}"
            );

            if (_isProcessing)
            {
                MessageBox.Show("正在处理中，请等待...", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                _isProcessing = true;
                ProcessingProgressBar.Visibility = Visibility.Visible;
                ProcessingStatusText.Visibility = Visibility.Visible;
                ProcessingProgressBar.Value = 0;
                var result = await _ffmpegService.CropAndAddWatermark(_selectedVideoPath, outputPath, width, height, cropX, cropY, startTime, _videoDuration, fontSize, watermarkX, watermarkY);
                _isProcessing = false;
                ProcessingProgressBar.Visibility = Visibility.Collapsed;
                ProcessingStatusText.Visibility = Visibility.Collapsed;

                if (result == 0)
                {
                    MessageBox.Show("视频裁剪并添加水印成功！", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("视频处理失败", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                _isProcessing = false;
                ProcessingProgressBar.Visibility = Visibility.Collapsed;
                ProcessingStatusText.Visibility = Visibility.Collapsed;
                MessageBox.Show($"处理过程中发生错误：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}