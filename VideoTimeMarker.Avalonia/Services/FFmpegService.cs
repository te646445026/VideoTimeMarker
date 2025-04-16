using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace VideoTimeMarker.Avalonia.Services
{
    public class FFmpegService : IFFmpegService
    {
        private TimeSpan _videoDuration;
        public event EventHandler<ProgressEventArgs>? ProgressChanged;

        protected virtual void OnProgressChanged(double progress, string status)
        {
            ProgressChanged?.Invoke(this, new ProgressEventArgs(progress, status));
        }

        public async Task<int> AddDynamicTimeWatermark(string inputFile, string outputFile, DateTime startTime, TimeSpan duration, int fontSize = 45, int x = 20, int y = 20)
        {
            var endTime = startTime.Add(duration);
            
            var fileInfo = new System.IO.FileInfo(outputFile);
            var directory = fileInfo.DirectoryName;
            var fileNameWithoutExt = System.IO.Path.GetFileNameWithoutExtension(outputFile);
            var extension = fileInfo.Extension;
            var timeStamp = startTime.ToString("yyyyMMdd_HHmmss");
            outputFile = System.IO.Path.Combine(directory!, $"{fileNameWithoutExt}_{timeStamp}{extension}");
            
            var durationInfo = await GetVideoDurationAsync(inputFile);
            var totalSeconds = durationInfo.TotalSeconds;

            var command = $"-i \"{inputFile}\" -vf \"drawtext=fontfile=arial.ttf:fontsize={fontSize}:fontcolor=red:" +
                $"text='%{{pts\\:localtime\\:{new DateTimeOffset(startTime).ToUnixTimeSeconds()}}}'" +
                $":x={x}:y={y}\" -c:a copy \"{outputFile}\"";

            OnProgressChanged(0, "开始处理视频...");
                
            return await ExecuteCommandAsync(inputFile, command);
        }

        public async Task<int> CropVideo(string inputFile, string outputFile, int width, int height, int x, int y)
        {
            var fileInfo = new System.IO.FileInfo(outputFile);
            var directory = fileInfo.DirectoryName;
            var fileNameWithoutExt = System.IO.Path.GetFileNameWithoutExtension(outputFile);
            var extension = fileInfo.Extension;
            var timeStamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            outputFile = System.IO.Path.Combine(directory!, $"{fileNameWithoutExt}_crop_{timeStamp}{extension}");

            var videoInfo = await GetVideoInfoAsync(inputFile);
            if (!videoInfo.HasValue)
            {
                throw new Exception("无法获取视频信息");
            }

            var command = $"-i \"{inputFile}\" -vf \"crop={width}:{height}:{x}:{y}\" -c:a copy \"{outputFile}\"";

            OnProgressChanged(0, "开始裁剪视频...");

            return await ExecuteCommandAsync(inputFile, command);
        }

        public async Task<int> CropAndAddWatermark(string inputFile, string outputFile, int width, int height, int x, int y, 
            DateTime startTime, TimeSpan duration, int fontSize = 40, int watermarkX = 18, int watermarkY = 18)
        {
            var endTime = startTime.Add(duration);
            
            var fileInfo = new System.IO.FileInfo(outputFile);
            var directory = fileInfo.DirectoryName;
            var fileNameWithoutExt = System.IO.Path.GetFileNameWithoutExtension(outputFile);
            var extension = fileInfo.Extension;
            var timeStamp = startTime.ToString("yyyyMMdd_HHmmss");
            outputFile = System.IO.Path.Combine(directory!, $"{fileNameWithoutExt}_crop_watermark_{timeStamp}{extension}");

            var videoInfo = await GetVideoInfoAsync(inputFile);
            if (!videoInfo.HasValue)
            {
                throw new Exception("无法获取视频信息");
            }

            var command = $"-i \"{inputFile}\" -vf \"crop={width}:{height}:{x}:{y}," +
                $"drawtext=fontfile=arial.ttf:fontsize={fontSize}:fontcolor=red:" +
                $"text='%{{pts\\:localtime\\:{new DateTimeOffset(startTime).ToUnixTimeSeconds()}}}'" +
                $":x={watermarkX}:y={watermarkY}\" -c:a copy \"{outputFile}\"";

            OnProgressChanged(0, "开始处理视频...");

            return await ExecuteCommandAsync(inputFile, command);
        }

        public async Task<int> ExecuteCommandAsync(string inputFile, string command)
        {
            var ffmpegPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg", "bin", "ffmpeg.exe");
            var startInfo = new ProcessStartInfo
            {
                FileName = ffmpegPath,
                Arguments = command,
                UseShellExecute = false,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = startInfo };
            process.ErrorDataReceived += (sender, e) =>
            {
                if (string.IsNullOrEmpty(e.Data)) return;

                if (e.Data.Contains("time="))
                {
                    var timeIndex = e.Data.IndexOf("time=");
                    var timeStr = e.Data.Substring(timeIndex + 5, 11);
                    var time = TimeSpan.Parse(timeStr);
                    var progress = (time.TotalSeconds / _videoDuration.TotalSeconds) * 100;
                    OnProgressChanged(progress, $"处理进度: {progress:F1}%");
                }
            };

            process.Start();
            process.BeginErrorReadLine();
            await process.WaitForExitAsync();

            OnProgressChanged(100, "处理完成");
            return process.ExitCode;
        }

        public async Task<TimeSpan> GetVideoDurationAsync(string inputFile)
        {
            var ffprobePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg", "bin", "ffprobe.exe");
            var startInfo = new ProcessStartInfo
            {
                FileName = ffprobePath,
                Arguments = $"-v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 \"{inputFile}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = startInfo };
            process.Start();
            var output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (double.TryParse(output, out double seconds))
            {
                _videoDuration = TimeSpan.FromSeconds(seconds);
                return _videoDuration;
            }

            throw new Exception("无法获取视频时长");
        }

        public async Task<(int width, int height)?> GetVideoInfoAsync(string inputFile)
        {
            var ffprobePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg", "bin", "ffprobe.exe");
            var startInfo = new ProcessStartInfo
            {
                FileName = ffprobePath,
                Arguments = $"-v error -select_streams v:0 -show_entries stream=width,height -of csv=s=x:p=0 \"{inputFile}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = startInfo };
            process.Start();
            var output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (string.IsNullOrEmpty(output))
                return null;

            var dimensions = output.Split('x');
            if (dimensions.Length == 2 &&
                int.TryParse(dimensions[0], out int width) &&
                int.TryParse(dimensions[1], out int height))
            {
                return (width, height);
            }

            return null;
        }
    }
}