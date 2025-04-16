using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Diagnostics;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;
using System.IO;
using VideoTimeMarker.Avalonia.ViewModels;
using LibVLCSharp.Avalonia;
using LibVLCSharp.Shared;
using Avalonia.Layout;
using System.Reactive.Linq;

namespace VideoTimeMarker.Avalonia.Views
{
    public partial class MainWindow : Window
    {
        private readonly MainWindowViewModel _viewModel;
        private VideoView? _videoView;
        private LibVLC? _libVLC;
        private MediaPlayer? _mediaPlayer;
        private Border? _videoContainer;

        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            _viewModel = (MainWindowViewModel)DataContext!;
            
            // 初始化LibVLC
            Core.Initialize();
            _libVLC = new LibVLC();
            _mediaPlayer = new MediaPlayer(_libVLC);

            // 初始化视频播放控件
            InitializeVideoView();
            
            // 订阅视频控制事件
            _viewModel.RequestPlayVideo += OnRequestPlayVideo;
            _viewModel.RequestPauseVideo += OnRequestPauseVideo;
            _viewModel.RequestStopVideo += OnRequestStopVideo;
            
            // 监听视频路径变化
            this.GetObservable(MainWindowViewModel.SelectedVideoPathProperty)
                .Subscribe(path => OnSelectedVideoPathChanged(path as string));

            // 监听媒体播放器事件
            _mediaPlayer.LengthChanged += MediaPlayer_LengthChanged;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            _videoContainer = this.FindControl<Border>("VideoContainer");
        }
        
        private void InitializeVideoView()
        {
            _videoView = new VideoView
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };
            
            if (_videoContainer != null)
            {
                _videoContainer.Child = _videoView;
            }

            if (_mediaPlayer != null)
            {
                _videoView.MediaPlayer = _mediaPlayer;
            }
        }
        
        private void MediaPlayer_LengthChanged(object? sender, MediaPlayerLengthChangedEventArgs e)
        {
            if (_mediaPlayer != null && e.Length > 0)
            {
                // 通知ViewModel视频已打开并传递视频时长
                _viewModel.OnVideoOpened(TimeSpan.FromMilliseconds(e.Length));
            }
        }
        
        private void OnSelectedVideoPathChanged(string? path)
        {
            if (_mediaPlayer == null) return;
            
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                _mediaPlayer.Stop();
                return;
            }
            
            if (_libVLC != null)
            {
                using var media = new Media(_libVLC, path);
                _mediaPlayer!.Media = media;
                
                // 自动开始播放
                _mediaPlayer.Play();
            }
        }
        
        private void OnRequestPlayVideo()
        {
            _mediaPlayer?.Play();
        }
        
        private void OnRequestPauseVideo()
        {
            _mediaPlayer?.Pause();
        }
        
        private void OnRequestStopVideo()
        {
            if (_mediaPlayer != null)
            {
                _mediaPlayer.Stop();
                _mediaPlayer.Time = 0;
            }
        }
    }
}