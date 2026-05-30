using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using MathCore.WPF.Commands;
using MathCore.WPF.ViewModels;

namespace FDTD2DLab.ViewModels
{
    public class FieldViewModel : ViewModel
    {
        private readonly List<BitmapSource> _frames = new();
        private readonly DispatcherTimer _timer = new(DispatcherPriority.Normal);
        private int _currentFrameIndex;
        private bool _isPlaying;
        private TimeSpan _frameInterval = TimeSpan.FromMilliseconds(100);

        public BitmapSource CurrentFrame => _frames.Count > 0 ? _frames[_currentFrameIndex] : null;
        public int FrameCount => _frames.Count;
        public int CurrentFrameIndex
        {
            get => _currentFrameIndex;
            set
            {
                if (Set(ref _currentFrameIndex, Math.Clamp(value, 0, _frames.Count - 1)))
                {
                    OnPropertyChanged(nameof(CurrentFrame));
                    OnPropertyChanged(nameof(Progress));
                }
            }
        }

        public bool IsPlaying { get => _isPlaying; set => Set(ref _isPlaying, value); }
        public double Progress => FrameCount > 1 ? (double)CurrentFrameIndex / (FrameCount - 1) : 0;

        public ICommand PlayCommand { get; }
        public ICommand PauseCommand { get; }
        public ICommand StopCommand { get; }

        public FieldViewModel()
        {
            PlayCommand = new LambdaCommand(Play);
            PauseCommand = new LambdaCommand(Pause);
            StopCommand = new LambdaCommand(Stop);
            _timer.Tick += (_, _) =>
            {
                if (CurrentFrameIndex < FrameCount - 1)
                    CurrentFrameIndex++;
                else
                    Pause();
            };
        }

        public void LoadFromFile(string gifPath)
        {
            _frames.Clear();
            var decoder = new GifBitmapDecoder(
                new Uri(gifPath, UriKind.Absolute),
                BitmapCreateOptions.PreservePixelFormat,
                BitmapCacheOption.OnLoad);
            foreach (var frame in decoder.Frames)
                _frames.Add(frame.Clone());
            CurrentFrameIndex = 0;
            OnPropertyChanged(nameof(CurrentFrame));
            OnPropertyChanged(nameof(Progress));
        }

        public void Play()
        {
            if (_frames.Count < 2) return;
            _timer.Interval = _frameInterval;
            _timer.Start();
            IsPlaying = true;
        }

        public void Pause()
        {
            _timer.Stop();
            IsPlaying = false;
        }

        public void Stop()
        {
            _timer.Stop();
            IsPlaying = false;
            CurrentFrameIndex = 0;
        }
    }
}