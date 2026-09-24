# FFmpeg 可执行文件目录

## 说明
此文件夹用于存放FFmpeg相关的可执行文件，这些文件是VideoTimeMarker.Framework应用程序正常运行所必需的。

## 需要的文件
1. 请将FFmpeg可执行文件（ffmpeg.exe）放置在此文件夹中
2. 请将FFprobe可执行文件（ffprobe.exe）放置在此文件夹中

## 下载方式
1. 直接下载 Win7 兼容版本（FFmpeg 4.4.1，64位）：
   https://github.com/GyanD/codexffmpeg/releases/download/4.4.1/ffmpeg-4.4.1-full_build.zip
2. 或访问FFmpeg官方网站：https://ffmpeg.org/download.html
3. 推荐使用静态编译版本（static builds）
4. 解压下载的文件，找到bin目录下的ffmpeg.exe和ffprobe.exe
5. 将这两个文件复制到当前文件夹中

## 版本要求
- Win7 系统必须使用 FFmpeg 4.x～6.x 版本，**FFmpeg 7.0 及以上已不再支持 Win7**
- 推荐使用FFmpeg 4.4.1（上述直链），兼容性最好
- 32位 Win7 需要使用 32位（x86）版本的 FFmpeg 构建
- 注意：gyan.dev 当前页面提供的 release 构建（7.x/8.x）无法在 Win7 上运行

## 注意事项
- 不要重命名ffmpeg.exe和ffprobe.exe文件
- 确保这两个文件具有执行权限
- 文件大小通常在50-100MB左右
- 如果遇到"找不到文件"错误，请检查文件是否正确放置在此目录

## 文件结构
```
ffmpeg/
├── ffmpeg.exe      (必需 - 视频处理主程序)
├── ffprobe.exe     (必需 - 视频信息获取工具)
└── README.md       (本说明文件)
```

## 许可证
FFmpeg使用LGPL许可证，请确保遵守相关许可证条款。