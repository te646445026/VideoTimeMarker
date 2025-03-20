using Microsoft.Win32;
using System.Windows;
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
    }
}