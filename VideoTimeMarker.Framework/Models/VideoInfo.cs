using VideoTimeMarker.Framework.Helpers;
using System;

namespace VideoTimeMarker.Framework.Models
{
    public class VideoInfo : ObservableObject
    {
        private string _fileName;
        private string _filePath;
        private TimeSpan _duration;
        private string _format;
        private long _fileSize;

        public string FileName
        {
            get => _fileName;
            set => SetProperty(ref _fileName, value);
        }

        public string FilePath
        {
            get => _filePath;
            set => SetProperty(ref _filePath, value);
        }

        public TimeSpan Duration
        {
            get => _duration;
            set => SetProperty(ref _duration, value);
        }

        public string Format
        {
            get => _format;
            set => SetProperty(ref _format, value);
        }

        public long FileSize
        {
            get => _fileSize;
            set => SetProperty(ref _fileSize, value);
        }
    }
}