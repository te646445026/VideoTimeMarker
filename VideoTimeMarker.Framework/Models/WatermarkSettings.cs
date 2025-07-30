using VideoTimeMarker.Framework.Helpers;
using System;

namespace VideoTimeMarker.Framework.Models
{
    public class WatermarkSettings : ObservableObject
    {
        private string _format = "HH:mm:ss";
        private double _positionX = 10;
        private double _positionY = 10;
        private int _fontSize = 16;
        private string _fontColor = "white";
        private bool _isEnabled = true;

        public string Format
        {
            get => _format;
            set => SetProperty(ref _format, value);
        }

        public double PositionX
        {
            get => _positionX;
            set => SetProperty(ref _positionX, value);
        }

        public double PositionY
        {
            get => _positionY;
            set => SetProperty(ref _positionY, value);
        }

        public int FontSize
        {
            get => _fontSize;
            set => SetProperty(ref _fontSize, value);
        }

        public string FontColor
        {
            get => _fontColor;
            set => SetProperty(ref _fontColor, value);
        }

        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value);
        }
    }
}