﻿using System;
using System.Drawing.Printing;
using System.Threading.Tasks;
using System.Windows;
using NAudio.Wave;

namespace Meridian.Services.Media.Core
{
    /// <summary>
    /// NAudio Media Player implementation
    /// </summary>
    public class NaudioMediaPlayer : MediaPlayerBase
    {
        private IWavePlayer _wavePlayer;
        private WaveStream _outputStream;
        private WaveChannel32 _volumeStream;
        private bool _initialized;
        private Uri _source;
        private TimeSpan _duration;

        public override TimeSpan Position
        {
            get
            {
                if (_outputStream == null)
                    return TimeSpan.Zero;

                var currentPos = _outputStream.CurrentTime;
                if (Math.Round(currentPos.TotalSeconds, 2) >= Math.Round(_duration.TotalSeconds, 2))
                    SwitchNext();

                return TimeSpan.FromSeconds(Math.Min(Duration.TotalSeconds, currentPos.TotalSeconds));
            }
            set
            {
                if (_outputStream != null)
                    _outputStream.CurrentTime = value;
            }
        }

        public override TimeSpan Duration
        {
            get { return _duration; }
        }

        public override Uri Source
        {
            get { return _source; }
            set
            {
                if (_source == value)
                    return;

                _source = value;

                InitSource();
            }
        }

        public override double Volume
        {
            get
            {
                if (_volumeStream != null)
                    return _volumeStream.Volume;

                return 0f;
            }
            set
            {
                if (_volumeStream != null)
                    _volumeStream.Volume = (float)value;
            }
        }

        public override void Initialize()
        {
            _wavePlayer = new WaveOutEvent();
        }

        private void SwitchNext()
        {
            if (MediaEnded != null)
                MediaEnded(this, EventArgs.Empty);
        }

        public override void Play()
        {
            if (!_initialized)
                return;

            _wavePlayer.Play();
        }

        public override void Pause()
        {
            _wavePlayer.Pause();
        }

        public override void Stop()
        {
            _wavePlayer.Stop();
        }

        private void InitSource()
        {
            if (_outputStream != null)
            {
                _outputStream.Dispose();
                _outputStream = null;
            }

            if (_volumeStream != null)
            {
                _volumeStream.Dispose();
                _volumeStream = null;
            }

            _initialized = false;

            if (_source == null)
                return;

            try
            {
                _outputStream = new MediaFoundationReader(_source.OriginalString);
                _volumeStream = new WaveChannel32(_outputStream);
                _wavePlayer.Init(_volumeStream);
                _duration = _outputStream.TotalTime;

                _initialized = true;

                if (MediaOpened != null)
                    MediaOpened(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                if (MediaFailed != null)
                    MediaFailed(this, ex);
            }
        }
    }
}
