using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Avalonia.Controls.ApplicationLifetimes;
using VideoTimeMarker.Avalonia.Services;


namespace VideoTimeMarker.Avalonia.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private readonly IFFmpegService _ffmpegService;

        public static readonly StyledProperty<string?> SelectedVideoPathProperty =
            AvaloniaProperty.Register<MainWindowViewModel, string?>(nameof(SelectedVideoPath));

        [ObservableProperty]
        private string? selectedVideoPath;

        public string? FilePathText { get; set; }

        [ObservableProperty]
        private TimeSpan videoDuration;

        [ObservableProperty]
        private bool isProcessing;

        [ObservableProperty]
        private double processingProgress;

        [ObservableProperty]
        private string? processingStatus;

        [ObservableProperty]
        private bool isProgressVisible;

        [ObservableProperty]
        private bool isStatusVisible;

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

        private void FFmpegService_ProgressChanged(object? sender, ProgressEventArgs e)
        {
            ProcessingProgress = e.Progress;
            ProcessingStatus = e.Status;
            IsProgressVisible = true;
            IsStatusVisible = true;
        }

        public void OnVideoOpened(TimeSpan duration)
        {
            VideoDuration = duration;
        }

        [RelayCommand]
        private async Task SelectVideoAsync()
        {
            var topLevel = TopLevel.GetTopLevel(Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop ? desktop.MainWindow : null);
            if (topLevel == null) return;

            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "选择视频文件",
                AllowMultiple = false,
                FileTypeFilter = new[]
                {
                    new FilePickerFileType("视频文件")
                    {
                        Patterns = new[] { "*.mp4", "*.avi", "*.mkv" }
                    },
                    new FilePickerFileType("所有文件")
                    {
                        Patterns = new[] { "*.*" }
                    }
                }
            });

            if (files.Count > 0)
            {
                SelectedVideoPath = files[0].Path.LocalPath;
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

        [RelayCommand]
        private async Task AddWatermarkAsync()
        {
            if (string.IsNullOrEmpty(SelectedVideoPath) || !File.Exists(SelectedVideoPath))
            {
                // TODO: Show error message
                return;
            }

            if (!DateTime.TryParse(StartTime, out var time) || SelectedDate == null)
            {
                // TODO: Show error message
                return;
            }

            var startDateTime = SelectedDate.Value.Date.Add(time.TimeOfDay);

            if (!int.TryParse(FontSizeText, out var fontSize) ||
                !int.TryParse(WatermarkXText, out var x) ||
                !int.TryParse(WatermarkYText, out var y))
            {
                // TODO: Show error message
                return;
            }

            IsProcessing = true;
            try
            {
                var outputPath = Path.Combine(
                    Path.GetDirectoryName(SelectedVideoPath)!,
                    Path.GetFileNameWithoutExtension(SelectedVideoPath) + "_watermark" + Path.GetExtension(SelectedVideoPath));

                await _ffmpegService.AddDynamicTimeWatermark(
                    SelectedVideoPath,
                    outputPath,
                    startDateTime,
                    VideoDuration,
                    fontSize,
                    x,
                    y);
            }
            finally
            {
                IsProcessing = false;
            }
        }

        [RelayCommand]
        private async Task CropVideoAsync()
        {
            if (string.IsNullOrEmpty(SelectedVideoPath) || !File.Exists(SelectedVideoPath))
            {
                // TODO: Show error message
                return;
            }

            if (!int.TryParse(CropWidthText, out var width) ||
                !int.TryParse(CropHeightText, out var height) ||
                !int.TryParse(CropXText, out var x) ||
                !int.TryParse(CropYText, out var y))
            {
                // TODO: Show error message
                return;
            }

            IsProcessing = true;
            try
            {
                var outputPath = Path.Combine(
                    Path.GetDirectoryName(SelectedVideoPath)!,
                    Path.GetFileNameWithoutExtension(SelectedVideoPath) + "_cropped" + Path.GetExtension(SelectedVideoPath));

                await _ffmpegService.CropVideo(
                    SelectedVideoPath,
                    outputPath,
                    width,
                    height,
                    x,
                    y);
            }
            finally
            {
                IsProcessing = false;
            }
        }

        [RelayCommand]
        private async Task CropAndAddWatermarkAsync()
        {
            if (string.IsNullOrEmpty(SelectedVideoPath) || !File.Exists(SelectedVideoPath))
            {
                // TODO: Show error message
                return;
            }

            if (!DateTime.TryParse(StartTime, out var time) || SelectedDate == null)
            {
                // TODO: Show error message
                return;
            }

            if (!int.TryParse(CropWidthText, out var width) ||
                !int.TryParse(CropHeightText, out var height) ||
                !int.TryParse(CropXText, out var x) ||
                !int.TryParse(CropYText, out var y) ||
                !int.TryParse(FontSizeText, out var fontSize) ||
                !int.TryParse(WatermarkXText, out var watermarkX) ||
                !int.TryParse(WatermarkYText, out var watermarkY))
            {
                // TODO: Show error message
                return;
            }

            var startDateTime = SelectedDate.Value.Date.Add(time.TimeOfDay);

            IsProcessing = true;
            try
            {
                var outputPath = Path.Combine(
                    Path.GetDirectoryName(SelectedVideoPath)!,
                    Path.GetFileNameWithoutExtension(SelectedVideoPath) + "_cropped_watermark" + Path.GetExtension(SelectedVideoPath));

                await _ffmpegService.CropAndAddWatermark(
                    SelectedVideoPath,
                    outputPath,
                    width,
                    height,
                    x,
                    y,
                    startDateTime,
                    VideoDuration,
                    fontSize,
                    watermarkX,
                    watermarkY);
            }
            finally
            {
                IsProcessing = false;
            }
        }
    }
}