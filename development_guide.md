# VM 视频水印工具开发文档

## 1. 项目概述

VM（Video Watermark）是一个基于WPF开发的视频水印工具，主要用于为视频添加时间水印。该工具提供了简单直观的用户界面，支持视频导入和水印添加功能。

## 2. 功能需求

### 2.1 核心功能

- 视频导入
  - 支持选择本地视频文件
  - 支持主流视频格式
  - 提供视频预览功能

- 水印添加
  - 支持添加动态时间水印
  - 可自定义水印起始时间（日期和时间）
  - 结束时间根据视频长度自动计算
  - 水印位置固定在视频左上角
  - 水印样式：红色字体，字号24，时间格式为"年-月-日 时:分:秒"

### 2.2 用户界面

- 视频播放器组件
- 文件选择按钮
- 日期选择器
- 时间选择器
- 水印添加按钮
- 进度指示器

## 3. 技术架构

### 3.1 技术栈

- 开发框架：.NET 8.0或9.0
- UI框架：WPF
- 视频处理：FFmpeg
- 视频播放：
  - MediaElement（主要方案）：WPF原生视频播放控件，具有以下优势：
    - 与WPF完美集成，支持硬件加速
    - 内存占用低，性能稳定
    - 支持主流视频格式
    - 提供基本的播放控制API
  - LibVLC.NET（备选方案）：功能更丰富的视频播放库，适用于以下场景：
    - 需要更精细的视频控制
    - 需要支持更多视频格式
    - 需要更多高级功能（如视频截图、帧提取等）

### 3.2 核心组件

#### 3.2.1 IFFmpegService

视频处理服务接口，定义了视频处理的核心方法：
```csharp
public interface IFFmpegService
{
    Task<int> ExecuteCommandAsync(string command);
}
```

#### 3.2.2 FFmpegService

FFmpeg服务的实现，负责执行FFmpeg命令和处理动态时间水印：
```csharp
public class FFmpegService : IFFmpegService
{
    public async Task<int> AddDynamicTimeWatermark(string inputFile, string outputFile, DateTime startTime, TimeSpan duration)
    {
        // 计算结束时间
        var endTime = startTime.Add(duration);
        
        // 构建FFmpeg命令，使用drawtext filter添加动态时间水印
        var command = $"-i {inputFile} -vf \"drawtext=fontfile=arial.ttf:fontsize=24:fontcolor=red:" +
            $"text='%{{pts\\:localtime\\:{startTime.ToUnixTimeSeconds()}}}'" +
            $":x=10:y=10\" -c:a copy {outputFile}";
            
        return await ExecuteCommandAsync(command);
    }

    public async Task<int> ExecuteCommandAsync(string command)
    {
        return await Task.Run(() => {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = command,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            process.Start();
            process.WaitForExit();
            return process.ExitCode;
        });
    }
}
```

#### 3.2.3 MainWindow

主界面类，包含以下主要功能：
- 视频文件选择
- 视频预览
- 水印时间设置
- 水印添加处理

## 4. 开发规范

### 4.1 代码规范

- 使用异步编程模式处理耗时操作
- 实现错误处理和用户提示
- 遵循MVVM设计模式
- 使用依赖注入管理服务

### 4.2 命名规范

- 类名：PascalCase
- 方法名：PascalCase
- 变量名：camelCase
- 接口名：以I开头，如IFFmpegService

### 4.3 注释规范

- 为公共API添加XML文档注释
- 为复杂的业务逻辑添加适当的注释
- 使用//进行单行注释

## 5. 部署说明

### 5.1 环境要求

- .NET 8.0或更高版本
- FFmpeg库
- Visual Studio 2022

### 5.2 构建步骤

1. 克隆代码仓库
2. 还原NuGet包
3. 编译解决方案
4. 运行应用程序

## 6. 测试规范

### 6.1 单元测试

- 对核心业务逻辑编写单元测试
- 使用MSTest或NUnit框架
- 保持测试代码的可维护性

### 6.2 集成测试

- 测试视频导入功能
- 测试水印添加功能
- 测试不同格式视频的兼容性

## 7. 维护计划

### 7.1 版本控制

- 使用Git进行版本控制
- 遵循语义化版本规范
- 保持提交信息的清晰性

### 7.2 问题追踪

- 使用Issue跟踪bug和新功能请求
- 及时响应用户反馈
- 定期进行代码审查

## 8. 未来展望

### 8.1 功能扩展

- 支持更多水印样式
- 添加批量处理功能
- 优化视频处理性能
- 增强用户界面交互体验

### 8.2 性能优化

- 优化视频处理速度
- 减少内存占用
- 提升用户界面响应速度

## 9. 贡献指南

### 9.1 提交规范

- 遵循代码规范
- 编写清晰的提交信息
- 确保代码经过测试

### 9.2 分支管理

- main：主分支，保持稳定
- develop：开发分支
- feature/*：新功能分支
- bugfix/*：修复分支