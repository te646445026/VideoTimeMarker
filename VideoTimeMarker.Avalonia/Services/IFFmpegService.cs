using System;

namespace VideoTimeMarker.Avalonia.Services
{
    public class ProgressEventArgs : EventArgs
    {
        public double Progress { get; set; }
        public string Status { get; set; }

        public ProgressEventArgs(double progress, string status)
        {
            Progress = progress;
            Status = status;
        }
    }

    public interface IFFmpegService
    {
        event EventHandler<ProgressEventArgs>? ProgressChanged;
        Task<int> AddDynamicTimeWatermark(string inputFile, string outputFile, DateTime startTime, TimeSpan duration, int fontSize = 45, int x = 20, int y = 20);
        Task<int> CropVideo(string inputFile, string outputFile, int width, int height, int x, int y);
        Task<int> CropAndAddWatermark(string inputFile, string outputFile, int width, int height, int x, int y, DateTime startTime, TimeSpan duration, int fontSize = 40, int watermarkX = 18, int watermarkY = 18);
        Task<int> ExecuteCommandAsync(string inputFile, string command);
        Task<TimeSpan> GetVideoDurationAsync(string inputFile);
        Task<(int width, int height)?> GetVideoInfoAsync(string inputFile);
    }
}