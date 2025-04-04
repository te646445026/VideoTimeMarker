using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using VideoTimeMarker.Services;

namespace VideoTimeMarker.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private readonly IFFmpegService _ffmpegService;

        [ObservableProperty]
        private string? selectedVideoPath;

        [ObservableProperty]
        private string? filePathText;

        [ObservableProperty]
        private TimeSpan videoDuration;

        [ObservableProperty]
        private bool isProcessing;

        [ObservableProperty]
        private double processingProgress;

        [ObservableProperty]
        private string? processingStatus;

        [ObservableProperty]
        private Visibility processingProgressVisibility = Visibility.Collapsed;

        [ObservableProperty]
        private Visibility processingStatusVisibility = Visibility.Collapsed;

        [ObservableProperty]
        private DateTime? selectedDate = DateTime.Today;

        [ObservableProperty]
        private string startTime = DateTime.Now.ToString("HH:mm:ss");

        [ObservableProperty]
        private string fontSizeText = "40";

        [ObservableProperty]
        private string watermarkXText = "18";

        [ObservableProperty]
        private string watermarkYText = "18";

        [ObservableProperty]
        private string? cropWidthText;

        [ObservableProperty]
        private string? cropHeightText;

        [ObservableProperty]
        private string? cropXText;

        [ObservableProperty]
        private string? cropYText;

        public event Action? RequestPlayVideo;
        public event Action? RequestPauseVideo;
        public event Action? RequestStopVideo;

        public MainWindowViewModel(IFFmpegService ffmpegService)
        {
            _ffmpegService = ffmpegService;
            _ffmpegService.ProgressChanged += FFmpegService_ProgressChanged;
        }

        public void OnVideoOpened(TimeSpan duration)
        {
            VideoDuration = duration;
        }

        [RelayCommand]
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

        [RelayCommand]
        private void PlayVideo()
        {
            RequestPlayVideo?.Invoke();
        }

        [RelayCommand]
        private void PauseVideo()
        {
            RequestPauseVideo?.Invoke();
        }

        [RelayCommand]
        private void StopVideo()
        {
            RequestStopVideo?.Invoke();
        }

        private void FFmpegService_ProgressChanged(object? sender, ProgressEventArgs e)
        {
            ProcessingProgress = e.Progress;
            ProcessingStatus = e.Status;
        }

        [RelayCommand]
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

            var startTime = SelectedDate.Value.Add(time.TimeOfDay);
            var outputPath = Path.Combine(
                Path.GetDirectoryName(SelectedVideoPath)!,
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

        [RelayCommand]
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
                Path.GetDirectoryName(SelectedVideoPath)!,
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

        [RelayCommand]
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

            var startTime = SelectedDate.Value.Add(time.TimeOfDay);
            var outputPath = Path.Combine(
                Path.GetDirectoryName(SelectedVideoPath)!,
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

        [RelayCommand]
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