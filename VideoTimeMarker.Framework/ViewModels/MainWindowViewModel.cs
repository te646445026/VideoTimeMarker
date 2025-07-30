using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using VideoTimeMarker.Framework.Helpers;
using VideoTimeMarker.Framework.Services;

namespace VideoTimeMarker.Framework.ViewModels
{
    public class MainWindowViewModel : ObservableObject
    {
        private readonly FFmpegService _ffmpegService;

        private string _selectedVideoPath;
        private string _filePathText;
        private TimeSpan _videoDuration;
        private bool _isProcessing;
        private double _processingProgress;
        private string _processingStatus;
        private Visibility _processingProgressVisibility = Visibility.Collapsed;
        private Visibility _processingStatusVisibility = Visibility.Collapsed;
        private DateTime _selectedDate = DateTime.Today;
        private string _startTime = DateTime.Now.ToString("HH:mm:ss");
        private string _fontSizeText = "40";
        private string _watermarkXText = "18";
        private string _watermarkYText = "18";
        private string _cropWidthText;
        private string _cropHeightText;
        private string _cropXText;
        private string _cropYText;

        public string SelectedVideoPath
        {
            get => _selectedVideoPath;
            set => SetProperty(ref _selectedVideoPath, value);
        }

        public string FilePathText
        {
            get => _filePathText;
            set => SetProperty(ref _filePathText, value);
        }

        public TimeSpan VideoDuration
        {
            get => _videoDuration;
            set => SetProperty(ref _videoDuration, value);
        }

        public bool IsProcessing
        {
            get => _isProcessing;
            set => SetProperty(ref _isProcessing, value);
        }

        public double ProcessingProgress
        {
            get => _processingProgress;
            set => SetProperty(ref _processingProgress, value);
        }

        public string ProcessingStatus
        {
            get => _processingStatus;
            set => SetProperty(ref _processingStatus, value);
        }

        public Visibility ProcessingProgressVisibility
        {
            get => _processingProgressVisibility;
            set => SetProperty(ref _processingProgressVisibility, value);
        }

        public Visibility ProcessingStatusVisibility
        {
            get => _processingStatusVisibility;
            set => SetProperty(ref _processingStatusVisibility, value);
        }

        public DateTime SelectedDate
        {
            get => _selectedDate;
            set => SetProperty(ref _selectedDate, value);
        }

        public string StartTime
        {
            get => _startTime;
            set => SetProperty(ref _startTime, value);
        }

        public string FontSizeText
        {
            get => _fontSizeText;
            set => SetProperty(ref _fontSizeText, value);
        }

        public string WatermarkXText
        {
            get => _watermarkXText;
            set => SetProperty(ref _watermarkXText, value);
        }

        public string WatermarkYText
        {
            get => _watermarkYText;
            set => SetProperty(ref _watermarkYText, value);
        }

        public string CropWidthText
        {
            get => _cropWidthText;
            set => SetProperty(ref _cropWidthText, value);
        }

        public string CropHeightText
        {
            get => _cropHeightText;
            set => SetProperty(ref _cropHeightText, value);
        }

        public string CropXText
        {
            get => _cropXText;
            set => SetProperty(ref _cropXText, value);
        }

        public string CropYText
        {
            get => _cropYText;
            set => SetProperty(ref _cropYText, value);
        }

        public RelayCommand SelectVideoCommand { get; }
        public RelayCommand PlayVideoCommand { get; }
        public RelayCommand PauseVideoCommand { get; }
        public RelayCommand StopVideoCommand { get; }
        public RelayCommand AddWatermarkCommand { get; }
        public RelayCommand CropVideoCommand { get; }
        public RelayCommand CropAndAddWatermarkCommand { get; }
        public RelayCommand OpenOutputFolderCommand { get; }

        public event Action RequestPlayVideo;
        public event Action RequestPauseVideo;
        public event Action RequestStopVideo;

        public MainWindowViewModel()
        {
            _ffmpegService = new FFmpegService();
            _ffmpegService.ProgressChanged += FFmpegService_ProgressChanged;

            SelectVideoCommand = new RelayCommand(param => SelectVideo());
            PlayVideoCommand = new RelayCommand(param => PlayVideo());
            PauseVideoCommand = new RelayCommand(param => PauseVideo());
            StopVideoCommand = new RelayCommand(param => StopVideo());
            AddWatermarkCommand = new RelayCommand(async param => await AddWatermarkAsync());
            CropVideoCommand = new RelayCommand(async param => await CropVideoAsync());
            CropAndAddWatermarkCommand = new RelayCommand(async param => await CropAndAddWatermarkAsync());
            OpenOutputFolderCommand = new RelayCommand(param => OpenOutputFolder());
        }

        public void OnVideoOpened(TimeSpan duration)
        {
            VideoDuration = duration;
        }

        private void SelectVideo()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "视频文件|*.mp4;*.avi;*.mkv|所有文件|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                SelectedVideoPath = openFileDialog.FileName;
                FilePathText = SelectedVideoPath;
            }
        }

        private void PlayVideo()
        {
            RequestPlayVideo?.Invoke();
        }

        private void PauseVideo()
        {
            RequestPauseVideo?.Invoke();
        }

        private void StopVideo()
        {
            RequestStopVideo?.Invoke();
        }

        private void FFmpegService_ProgressChanged(object sender, ProgressEventArgs e)
        {
            ProcessingProgress = e.Progress;
            ProcessingStatus = e.Status;
        }

        private async Task AddWatermarkAsync()
        {
            if (string.IsNullOrEmpty(SelectedVideoPath))
            {
                MessageBox.Show("请先选择视频文件", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!DateTime.TryParse(StartTime, out var time) || SelectedDate == null)
            {
                MessageBox.Show("请输入有效的时间格式", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(FontSizeText, out var fontSize) || fontSize <= 0)
            {
                MessageBox.Show("请输入有效的字体大小", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(WatermarkXText, out var x) || x < 0)
            {
                MessageBox.Show("请输入有效的X坐标", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(WatermarkYText, out var y) || y < 0)
            {
                MessageBox.Show("请输入有效的Y坐标", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var startTime = SelectedDate.Add(time.TimeOfDay);
            var outputPath = Path.Combine(
                Path.GetDirectoryName(SelectedVideoPath) ?? string.Empty,
                $"{Path.GetFileNameWithoutExtension(SelectedVideoPath)}_watermarked{Path.GetExtension(SelectedVideoPath)}"
            );

            if (IsProcessing)
            {
                MessageBox.Show("正在处理中，请等待...", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                IsProcessing = true;
                ProcessingProgressVisibility = Visibility.Visible;
                ProcessingStatusVisibility = Visibility.Visible;
                ProcessingProgress = 0;

                var result = await _ffmpegService.AddDynamicTimeWatermark(SelectedVideoPath, outputPath, startTime, VideoDuration, fontSize, x, y);

                IsProcessing = false;
                ProcessingProgressVisibility = Visibility.Collapsed;
                ProcessingStatusVisibility = Visibility.Collapsed;

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
                IsProcessing = false;
                ProcessingProgressVisibility = Visibility.Collapsed;
                ProcessingStatusVisibility = Visibility.Collapsed;
                MessageBox.Show($"处理过程中发生错误：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task CropVideoAsync()
        {
            if (string.IsNullOrEmpty(SelectedVideoPath))
            {
                MessageBox.Show("请先选择视频文件", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBox.Show("注意：裁剪功能将保留选中区域的内容，其他区域将被移除", "操作说明", MessageBoxButton.OK, MessageBoxImage.Information);

            if (!int.TryParse(CropWidthText, out var width) || width <= 0)
            {
                MessageBox.Show("请输入有效的裁剪宽度", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(CropHeightText, out var height) || height <= 0)
            {
                MessageBox.Show("请输入有效的裁剪高度", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(CropXText, out var cropX) || cropX < 0)
            {
                MessageBox.Show("请输入有效的裁剪X坐标", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(CropYText, out var cropY) || cropY < 0)
            {
                MessageBox.Show("请输入有效的裁剪Y坐标", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var outputPath = Path.Combine(
                Path.GetDirectoryName(SelectedVideoPath) ?? string.Empty,
                $"{Path.GetFileNameWithoutExtension(SelectedVideoPath)}_cropped{Path.GetExtension(SelectedVideoPath)}"
            );

            if (IsProcessing)
            {
                MessageBox.Show("正在处理中，请等待...", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                IsProcessing = true;
                ProcessingProgressVisibility = Visibility.Visible;
                ProcessingStatusVisibility = Visibility.Visible;
                ProcessingProgress = 0;

                var result = await _ffmpegService.CropVideo(SelectedVideoPath, outputPath, width, height, cropX, cropY);

                IsProcessing = false;
                ProcessingProgressVisibility = Visibility.Collapsed;
                ProcessingStatusVisibility = Visibility.Collapsed;

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
                IsProcessing = false;
                ProcessingProgressVisibility = Visibility.Collapsed;
                ProcessingStatusVisibility = Visibility.Collapsed;
                MessageBox.Show($"处理过程中发生错误：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task CropAndAddWatermarkAsync()
        {
            if (string.IsNullOrEmpty(SelectedVideoPath))
            {
                MessageBox.Show("请先选择视频文件", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBox.Show("注意：此操作将同时裁剪视频并添加时间水印", "操作说明", MessageBoxButton.OK, MessageBoxImage.Information);

            if (!int.TryParse(CropWidthText, out var width) || width <= 0)
            {
                MessageBox.Show("请输入有效的裁剪宽度", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(CropHeightText, out var height) || height <= 0)
            {
                MessageBox.Show("请输入有效的裁剪高度", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(CropXText, out var cropX) || cropX < 0)
            {
                MessageBox.Show("请输入有效的裁剪X坐标", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(CropYText, out var cropY) || cropY < 0)
            {
                MessageBox.Show("请输入有效的裁剪Y坐标", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!DateTime.TryParse(StartTime, out var time) || SelectedDate == null)
            {
                MessageBox.Show("请输入有效的时间格式", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(FontSizeText, out var fontSize) || fontSize <= 0)
            {
                MessageBox.Show("请输入有效的字体大小", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(WatermarkXText, out var watermarkX) || watermarkX < 0)
            {
                MessageBox.Show("请输入有效的水印X坐标", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(WatermarkYText, out var watermarkY) || watermarkY < 0)
            {
                MessageBox.Show("请输入有效的水印Y坐标", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var startTime = SelectedDate.Add(time.TimeOfDay);
            var outputPath = Path.Combine(
                Path.GetDirectoryName(SelectedVideoPath) ?? string.Empty,
                $"{Path.GetFileNameWithoutExtension(SelectedVideoPath)}_cropped_watermarked{Path.GetExtension(SelectedVideoPath)}"
            );

            if (IsProcessing)
            {
                MessageBox.Show("正在处理中，请等待...", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                IsProcessing = true;
                ProcessingProgressVisibility = Visibility.Visible;
                ProcessingStatusVisibility = Visibility.Visible;
                ProcessingProgress = 0;

                var result = await _ffmpegService.CropAndAddWatermark(
                    SelectedVideoPath,
                    outputPath,
                    width,
                    height,
                    cropX,
                    cropY,
                    startTime,
                    VideoDuration,
                    fontSize,
                    watermarkX,
                    watermarkY
                );

                IsProcessing = false;
                ProcessingProgressVisibility = Visibility.Collapsed;
                ProcessingStatusVisibility = Visibility.Collapsed;

                if (result == 0)
                {
                    MessageBox.Show("视频裁剪并添加水印成功！", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("视频裁剪并添加水印失败", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                IsProcessing = false;
                ProcessingProgressVisibility = Visibility.Collapsed;
                ProcessingStatusVisibility = Visibility.Collapsed;
                MessageBox.Show($"处理过程中发生错误：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenOutputFolder()
        {
            if (string.IsNullOrEmpty(SelectedVideoPath))
            {
                MessageBox.Show("请先选择视频文件", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var folderPath = Path.GetDirectoryName(SelectedVideoPath);
                if (!string.IsNullOrEmpty(folderPath) && Directory.Exists(folderPath))
                {
                    Process.Start("explorer.exe", folderPath);
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
    }
}