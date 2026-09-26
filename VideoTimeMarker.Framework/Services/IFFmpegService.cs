using System;
using System.Threading.Tasks;

namespace VideoTimeMarker.Framework.Services
{
    /// <summary>
    /// 视频处理服务接口，定义视频处理的核心方法
    /// </summary>
    public interface IFFmpegService
    {
        event EventHandler<ProgressEventArgs> ProgressChanged;
        
        /// <summary>
        /// 执行FFmpeg命令
        /// </summary>
        /// <param name="inputFile">输入文件路径</param>
        /// <param name="command">FFmpeg命令</param>
        /// <returns>命令执行结果代码</returns>
        Task<int> ExecuteCommandAsync(string inputFile, string command);

        /// <summary>
        /// 添加动态时间水印
        /// </summary>
        /// <param name="inputFile">输入视频文件路径</param>
        /// <param name="outputFile">输出视频文件路径</param>
        /// <param name="startTime">水印起始时间</param>
        /// <param name="duration">视频时长</param>
        /// <param name="fontSize">字体大小</param>
        /// <param name="x">水印X坐标</param>
        /// <param name="y">水印Y坐标</param>
        /// <returns>处理结果代码</returns>
        Task<int> AddDynamicTimeWatermark(string inputFile, string outputFile, DateTime startTime, TimeSpan duration, int fontSize = 40, int x = 18, int y = 18);

        /// <summary>
        /// 裁剪视频
        /// </summary>
        /// <param name="inputFile">输入视频文件路径</param>
        /// <param name="outputFile">输出视频文件路径</param>
        /// <param name="width">裁剪宽度</param>
        /// <param name="height">裁剪高度</param>
        /// <param name="x">裁剪起始X坐标</param>
        /// <param name="y">裁剪起始Y坐标</param>
        /// <returns>处理结果代码</returns>
        Task<int> CropVideo(string inputFile, string outputFile, int width, int height, int x, int y);
        
        /// <summary>
        /// 裁剪视频并添加动态时间水印
        /// </summary>
        /// <param name="inputFile">输入视频文件路径</param>
        /// <param name="outputFile">输出视频文件路径</param>
        /// <param name="width">裁剪宽度</param>
        /// <param name="height">裁剪高度</param>
        /// <param name="x">裁剪起始X坐标</param>
        /// <param name="y">裁剪起始Y坐标</param>
        /// <param name="startTime">水印起始时间</param>
        /// <param name="duration">视频时长</param>
        /// <param name="fontSize">水印字体大小</param>
        /// <param name="watermarkX">水印X坐标</param>
        /// <param name="watermarkY">水印Y坐标</param>
        /// <returns>处理结果代码</returns>
        Task<int> CropAndAddWatermark(string inputFile, string outputFile, int width, int height, int x, int y, DateTime startTime, TimeSpan duration, int fontSize = 40, int watermarkX = 18, int watermarkY = 18);

        /// <summary>
        /// 去除原视频水印区域（delogo插值修复）并在指定位置添加新的动态时间水印
        /// </summary>
        /// <param name="inputFile">输入视频文件路径</param>
        /// <param name="outputFile">输出视频文件路径</param>
        /// <param name="delogoX">水印区域起始X坐标</param>
        /// <param name="delogoY">水印区域起始Y坐标</param>
        /// <param name="delogoWidth">水印区域宽度</param>
        /// <param name="delogoHeight">水印区域高度</param>
        /// <param name="startTime">新水印起始时间</param>
        /// <param name="duration">视频时长</param>
        /// <param name="fontSize">新水印字体大小</param>
        /// <param name="watermarkX">新水印X坐标</param>
        /// <param name="watermarkY">新水印Y坐标</param>
        /// <returns>处理结果代码</returns>
        Task<int> RemoveWatermarkAndAddTime(string inputFile, string outputFile, int delogoX, int delogoY, int delogoWidth, int delogoHeight, DateTime startTime, TimeSpan duration, int fontSize = 40, int watermarkX = 18, int watermarkY = 18);
    }
}