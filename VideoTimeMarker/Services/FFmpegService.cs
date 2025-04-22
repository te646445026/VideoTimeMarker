using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace VideoTimeMarker.Services
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

    /// <summary>
    /// FFmpeg服务的实现，负责执行FFmpeg命令和处理动态时间水印
    /// </summary>
    public class FFmpegService : IFFmpegService
    {
        private TimeSpan _videoDuration;
        public event EventHandler<ProgressEventArgs>? ProgressChanged;
        /// <summary>
        /// 添加动态时间水印
        /// </summary>
        public async Task<int> AddDynamicTimeWatermark(string inputFile, string outputFile, DateTime startTime, TimeSpan duration, int fontSize = 45, int x = 20, int y = 20)
        {
            // 计算结束时间
            var endTime = startTime.Add(duration);
            
            // 修改输出文件名，添加时间戳
            var fileInfo = new System.IO.FileInfo(outputFile);
            var directory = fileInfo.DirectoryName;
            var fileNameWithoutExt = System.IO.Path.GetFileNameWithoutExtension(outputFile);
            var extension = fileInfo.Extension;
            var timeStamp = startTime.ToString("yyyyMMdd_HHmmss");
            outputFile = System.IO.Path.Combine(directory!, $"{fileNameWithoutExt}_{timeStamp}{extension}");
            
            // 获取视频时长信息
            var durationInfo = await GetVideoDurationAsync(inputFile);
            var totalSeconds = durationInfo.TotalSeconds;

            // 构建FFmpeg命令，使用drawtext filter添加动态时间水印
            var command = $"-i \"{inputFile}\" -vf \"drawtext=fontfile=arial.ttf:fontsize={fontSize}:fontcolor=red:" +
                $"text='%{{pts\\:localtime\\:{new DateTimeOffset(startTime).ToUnixTimeSeconds()}}}'" +
                $":x={x}:y={y}\" -c:a copy \"{outputFile}\"";

            OnProgressChanged(0, "开始处理视频...");
                
            return await ExecuteCommandAsync(inputFile, command);
        }

        public async Task<int> CropVideo(string inputFile, string outputFile, int width, int height, int x, int y)
        {
            // 修改输出文件名，添加裁剪信息
            var fileInfo = new System.IO.FileInfo(outputFile);
            var directory = fileInfo.DirectoryName;
            var fileNameWithoutExt = System.IO.Path.GetFileNameWithoutExtension(outputFile);
            var extension = fileInfo.Extension;
            var timeStamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            outputFile = System.IO.Path.Combine(directory!, $"{fileNameWithoutExt}_crop_{timeStamp}{extension}");

            // 获取视频信息以确定原始尺寸
            var videoInfo = await GetVideoInfoAsync(inputFile);
            if (!videoInfo.HasValue)
            {
                throw new Exception("无法获取视频信息");
            }

            var (videoWidth, videoHeight) = videoInfo.Value;

            // 构建FFmpeg命令，使用crop滤镜裁剪选中区域
            var command = $"-i \"{inputFile}\" -vf \"crop={width}:{height}:{x}:{y}\" -c:a copy \"{outputFile}\"";

            OnProgressChanged(0, "开始裁剪视频（保留选中区域的内容）...");

            return await ExecuteCommandAsync(inputFile, command);
        }
        
        public async Task<int> CropAndAddWatermark(string inputFile, string outputFile, int width, int height, int x, int y, DateTime startTime, TimeSpan duration, int fontSize = 40, int watermarkX = 18, int watermarkY = 18)
        {
            // 计算结束时间
            var endTime = startTime.Add(duration);
            
            // 修改输出文件名，添加裁剪和时间戳信息
            var fileInfo = new System.IO.FileInfo(outputFile);
            var directory = fileInfo.DirectoryName;
            var fileNameWithoutExt = System.IO.Path.GetFileNameWithoutExtension(outputFile);
            var extension = fileInfo.Extension;
            var timeStamp = startTime.ToString("yyyyMMdd_HHmmss");
            outputFile = System.IO.Path.Combine(directory!, $"{fileNameWithoutExt}_crop_watermark_{timeStamp}{extension}");
            
            // 获取视频信息以确定原始尺寸
            var videoInfo = await GetVideoInfoAsync(inputFile);
            if (!videoInfo.HasValue)
            {
                throw new Exception("无法获取视频信息");
            }
            
            // 构建FFmpeg命令，使用复合滤镜同时完成裁剪和添加水印
            var command = $"-i \"{inputFile}\" -vf \"crop={width}:{height}:{x}:{y},drawtext=fontfile=arial.ttf:fontsize={fontSize}:fontcolor=red:" +
                $"text='%{{pts\\:localtime\\:{new DateTimeOffset(startTime).ToUnixTimeSeconds()}}}'" +
                $":x={watermarkX}:y={watermarkY}\" -c:a copy \"{outputFile}\"";

            OnProgressChanged(0, "开始裁剪视频并添加水印...");
            
            return await ExecuteCommandAsync(inputFile, command);
        }

        /// <summary>
        /// 获取视频时长
        /// </summary>
        /// <param name="inputFile">输入视频文件路径</param>
        /// <returns>视频时长</returns>
        private async Task<TimeSpan> GetVideoDurationAsync(string inputFile)
        {
            var ffprobePath = Path.Combine(Path.GetDirectoryName(FFmpegConfig.FFmpegPath) ?? string.Empty, "ffprobe.exe");
            if (!File.Exists(ffprobePath))
            {
                throw new FileNotFoundException("找不到ffprobe.exe，请确保它与ffmpeg.exe在同一目录下。");
            }

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = ffprobePath,
                    Arguments = $"-v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 \"{inputFile}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            try
            {
                process.Start();
                var output = await process.StandardOutput.ReadToEndAsync();
                var error = await process.StandardError.ReadToEndAsync();
                await process.WaitForExitAsync();

                if (process.ExitCode != 0)
                {
                    throw new Exception($"ffprobe执行失败：{error}");
                }

                if (double.TryParse(output, out double seconds))
                {
                    return TimeSpan.FromSeconds(seconds);
                }

                throw new Exception("无法解析视频时长信息");
            }
            catch (Exception ex) when (ex is not FileNotFoundException)
            {
                throw new Exception($"获取视频时长失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 获取视频信息（宽度和高度）
        /// </summary>
        private async Task<(int width, int height)?> GetVideoInfoAsync(string inputFile)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = FFmpegConfig.FFmpegPath,
                    Arguments = $"-i \"{inputFile}\"",
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            try
            {
                process.Start();
                var output = await process.StandardError.ReadToEndAsync();
                process.WaitForExit();

                // 解析FFmpeg输出以获取视频尺寸
                var videoSizeMatch = System.Text.RegularExpressions.Regex.Match(output, @"Stream.*Video.*\s(\d+)x(\d+)");
                if (videoSizeMatch.Success)
                {
                    var width = int.Parse(videoSizeMatch.Groups[1].Value);
                    var height = int.Parse(videoSizeMatch.Groups[2].Value);
                    return (width, height);
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new Exception($"获取视频信息失败：{ex.Message}");
            }
        }



        public async Task<int> ExecuteCommandAsync(string inputFile, string command)
        {
            Process? process = null;
            try
            {
                _videoDuration = await GetVideoDurationAsync(inputFile);
                if (_videoDuration == TimeSpan.Zero)
                {
                    throw new Exception("无法获取视频时长，请检查视频文件是否有效");
                }

                process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = FFmpegConfig.FFmpegPath,
                        Arguments = command,
                        UseShellExecute = false,
                        RedirectStandardError = true,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };

                process.Start();

                // 异步读取输出
                var progressTask = Task.Run(async () =>
                {
                var reader = process.StandardError;
                string? line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    // 解析FFmpeg输出以更新进度
                    if (line.Contains("time="))
                    {
                        var timeMatch = System.Text.RegularExpressions.Regex.Match(line, @"time=(\d{2}):(\d{2}):(\d{2})\.(\d{2})");
                        if (timeMatch.Success)
                        {
                            var processedTime = new TimeSpan(
                                0,
                                int.Parse(timeMatch.Groups[1].Value),
                                int.Parse(timeMatch.Groups[2].Value),
                                int.Parse(timeMatch.Groups[3].Value),
                                int.Parse(timeMatch.Groups[4].Value) * 10
                            );
                            var progress = _videoDuration.TotalSeconds > 0 ? 
                                (processedTime.TotalSeconds / _videoDuration.TotalSeconds) * 100 : 0;
                            OnProgressChanged(progress, $"处理进度：{progress:F1}%");
                        }
                    }
                }
            });

                // 等待进度监控任务和进程完成
                await Task.WhenAll(
                    progressTask,
                    process.WaitForExitAsync()
                );
                return process.ExitCode;
            }
            catch (Exception ex)
            {
                OnProgressChanged(0, $"处理失败：{ex.Message}");
                throw new Exception($"执行FFmpeg命令失败：{ex.Message}");
            }
            finally
            {
                // 确保进程被正确释放
                if (process != null && !process.HasExited)
                {
                    try
                    {
                        process.Kill();
                        process.Dispose();
                    }
                    catch
                    {
                        // 忽略清理过程中的错误
                    }
                }
            }
        }

        private void OnProgressChanged(double progress, string status)
        {
            ProgressChanged?.Invoke(this, new ProgressEventArgs(progress, status));
        }
    }
}