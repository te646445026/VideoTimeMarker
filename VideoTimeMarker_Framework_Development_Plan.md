# VideoTimeMarker .NET Framework 版本开发计划

## 项目概述

基于现有的VideoTimeMarker (.NET 8.0) 项目，创建一个兼容.NET Framework 4.7.2的版本，保持核心功能不变，但调整技术栈以适应Framework环境。

## 技术栈对比

### 原版本 (VideoTimeMarker)
- **目标框架**: .NET 8.0-windows
- **UI框架**: WPF + MaterialDesignThemes
- **MVVM**: CommunityToolkit.Mvvm
- **依赖注入**: Microsoft.Extensions.DependencyInjection
- **FFmpeg**: FFmpeg.AutoGen
- **语言特性**: C# 12, Nullable引用类型, ImplicitUsings

### Framework版本 (VideoTimeMarker.Framework)
- **目标框架**: .NET Framework 4.7.2
- **UI框架**: WPF (内置控件为主)
- **MVVM**: 自定义实现 (ObservableObject + RelayCommand)
- **依赖注入**: 简化的手动注入
- **FFmpeg**: 直接调用ffmpeg.exe进程
- **语言特性**: C# 7.3兼容

## 开发阶段规划

### 第一阶段：项目架构搭建 (1-2天)

#### 1.1 创建项目结构
```
VideoTimeMarker.Framework/
├── VideoTimeMarker.Framework.csproj
├── App.xaml
├── App.xaml.cs
├── MainWindow.xaml
├── MainWindow.xaml.cs
├── Models/
│   ├── VideoInfo.cs
│   └── WatermarkSettings.cs
├── ViewModels/
│   ├── BaseViewModel.cs
│   └── MainWindowViewModel.cs
├── Services/
│   ├── IFFmpegService.cs
│   ├── FFmpegService.cs
│   └── DialogService.cs
├── Helpers/
│   ├── RelayCommand.cs
│   └── ObservableObject.cs
├── Resources/
│   ├── Styles/
│   └── Images/
└── FFmpeg/
    ├── ffmpeg.exe
    ├── ffprobe.exe
    └── README.md
```

#### 1.2 项目配置要点
- 目标框架：.NET Framework 4.7.2
- 平台目标：x86 (兼容32位系统)
- 输出类型：Windows应用程序
- 启用WPF支持

### 第二阶段：核心功能实现 (3-4天)

#### 2.1 基础MVVM框架
```csharp
// 轻量级ObservableObject实现
public abstract class ObservableObject : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;
    
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    
    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}

// 简化的RelayCommand实现
public class RelayCommand : ICommand
{
    private readonly Action<object> _execute;
    private readonly Func<object, bool> _canExecute;
    
    public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }
    
    public event EventHandler CanExecuteChanged
    {
        add { CommandManager.RequerySuggested += value; }
        remove { CommandManager.RequerySuggested -= value; }
    }
    
    public bool CanExecute(object parameter) => _canExecute?.Invoke(parameter) ?? true;
    public void Execute(object parameter) => _execute(parameter);
}
```

#### 2.2 FFmpeg服务优化
- 使用进程调用方式，避免复杂的互操作
- 实现进度监控和错误处理
- 优化内存使用，适合低配置电脑

#### 2.3 UI简化设计
- 移除Material Design依赖，使用原生WPF控件
- 简化动画效果，减少GPU负担
- 优化布局，提高响应速度

### 第三阶段：性能优化 (2-3天)

#### 3.1 内存优化
- 实现视频预览的延迟加载
- 优化大文件处理流程
- 添加内存使用监控

#### 3.2 UI响应优化
- 异步操作处理
- 进度条和状态提示
- 防止UI冻结

#### 3.3 兼容性处理
- Win7系统特殊处理
- 低分辨率屏幕适配
- 字体和DPI适配

### 第四阶段：功能完善 (2天)

#### 4.1 错误处理
- 完善异常捕获和用户提示
- 日志记录功能
- 崩溃恢复机制

#### 4.2 用户体验
- 添加使用说明
- 快捷键支持
- 设置保存和恢复

## 技术实现细节

### 依赖包选择
```xml
<PackageReference Include="System.ValueTuple" Version="4.5.0" />
<!-- 避免使用过多第三方包，保持轻量级 -->
```

### FFmpeg集成策略
1. **静态部署**: 将FFmpeg可执行文件打包到应用程序中
2. **版本选择**: 使用较旧但稳定的FFmpeg版本，确保Win7兼容性
3. **参数优化**: 针对低性能设备优化FFmpeg参数

### UI设计原则
1. **简洁性**: 减少视觉效果，专注功能
2. **响应性**: 确保在低配置下流畅运行
3. **可访问性**: 支持键盘操作，适合不同用户

### 性能优化策略
1. **延迟加载**: 视频预览和处理按需加载
2. **内存管理**: 及时释放不需要的资源
3. **多线程**: 合理使用后台线程，避免阻塞UI

## 部署和分发

### 安装包制作
- 使用WiX或Inno Setup制作安装程序
- 包含.NET Framework 4.7.2运行时检测
- 自动安装必要的Visual C++运行库

### 系统要求
- **操作系统**: Windows 7 SP1 及以上
- **内存**: 最低2GB RAM
- **硬盘**: 100MB可用空间
- **.NET Framework**: 4.7.2或更高版本

### 兼容性测试
- Win7 32位/64位系统测试
- 低配置硬件测试
- 不同分辨率屏幕测试

## 开发时间表

| 阶段 | 任务 | 预计时间 | 关键里程碑 | 当前状态 |
|------|------|----------|------------|----------|
| 第一阶段 | 项目架构搭建 | 1-2天 | 项目结构完成，基础框架就绪 | ✅ **已完成** |
| 第二阶段 | 核心功能实现 | 3-4天 | 主要功能可用，基本UI完成 | ✅ **已完成** |
| 第三阶段 | 性能优化 | 2-3天 | 性能达标，Win7兼容性确认 | 🔄 **进行中** |
| 第四阶段 | 功能完善 | 2天 | 错误处理完善，用户体验优化 | ⏳ **待开始** |
| **总计** | | **8-11天** | **完整可发布版本** | **70%完成** |

## 当前项目状态 (更新时间: 2024年12月)

### ✅ 已完成的工作

#### 1. 项目架构搭建
- ✅ 项目结构完整建立
- ✅ .NET Framework 4.7.2 SDK风格项目配置
- ✅ WPF基础框架搭建
- ✅ MVVM模式实现 (ObservableObject, RelayCommand)
- ✅ 依赖注入容器配置

#### 2. 核心功能实现
- ✅ FFmpeg服务完整实现
  - ✅ 动态时间水印添加
  - ✅ 视频裁剪功能
  - ✅ 裁剪+水印组合功能
  - ✅ 进度监控和状态反馈
  - ✅ FFmpeg文件检查和用户提示
- ✅ 主窗口UI完成
  - ✅ 视频选择和预览
  - ✅ 裁剪区域选择
  - ✅ 水印参数设置
  - ✅ 操作按钮和进度显示
- ✅ ViewModel完整实现
  - ✅ 视频文件处理逻辑
  - ✅ 命令绑定和事件处理
  - ✅ 数据验证和错误处理

#### 3. 兼容性处理
- ✅ C# 7.3语法兼容性修复
- ✅ .NET Framework 4.7.2兼容性确认
- ✅ 项目成功编译和运行

#### 4. FFmpeg集成和部署
- ✅ FFmpeg文件夹结构建立
- ✅ 构建时文件复制配置
- ✅ 应用启动时FFmpeg文件检查
- ✅ 用户友好的缺失文件提示
- ✅ FFmpeg下载指导文档和脚本

### 🔄 当前进行中的工作

#### 性能优化阶段
- 🔄 内存使用优化
- 🔄 UI响应性能测试
- 🔄 Win7系统兼容性验证

### ⏳ 待完成的工作

#### 1. 性能优化完善
- ⏳ 大文件处理优化
- ⏳ 低配置设备测试
- ⏳ 内存泄漏检查

#### 2. 功能完善
- ⏳ 错误处理机制完善
- ⏳ 用户设置保存/恢复
- ⏳ 日志记录功能
- ⏳ 使用说明和帮助文档

#### 3. 部署准备
- ⏳ 安装包制作
- ⏳ FFmpeg可执行文件打包
- ⏳ 系统要求验证

### 🎯 下一步计划

1. **立即任务** (1-2天)
   - 在真实Win7环境中测试应用程序
   - 性能基准测试和优化
   - 内存使用监控和优化

2. **短期任务** (3-5天)
   - 完善错误处理和用户反馈
   - 添加应用程序设置功能
   - 准备部署和安装包

3. **中期任务** (1周)
   - 用户测试和反馈收集
   - 文档编写和完善
   - 最终版本发布准备

## 风险评估和应对

### 主要风险
1. **Win7兼容性问题**: 某些.NET Framework特性可能不支持
2. **性能瓶颈**: 低配置设备可能无法流畅运行
3. **FFmpeg兼容性**: 旧版本FFmpeg功能限制

### 应对策略
1. **充分测试**: 在真实Win7环境中测试
2. **性能监控**: 实时监控内存和CPU使用
3. **降级方案**: 准备功能简化版本

## 后续维护计划

### 版本更新策略
- 主要更新：每季度一次
- 安全更新：及时发布
- 兼容性更新：根据系统变化调整

### 用户支持
- 提供详细的使用文档
- 建立问题反馈渠道
- 定期收集用户需求

## 总结

本开发计划旨在创建一个轻量级、高兼容性的视频时间水印工具，特别针对Win7系统和低性能电脑进行优化。通过合理的技术选择和性能优化，确保应用在目标环境下稳定运行，同时保持核心功能的完整性。

开发过程中将持续关注性能表现和用户体验，确保最终产品能够满足目标用户群体的实际需求。