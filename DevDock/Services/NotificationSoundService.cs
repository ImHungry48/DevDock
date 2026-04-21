using System;
using System.IO;
using System.Windows.Media;

namespace DevDock.Services
{
    /// <summary>
    /// Plays short notification audio, such as timer completion sounds.
    /// </summary>
    public class NotificationSoundService
    {
        private readonly MediaPlayer _player = new MediaPlayer();

        public event Action<string>? SoundErrorOccurred;

        public NotificationSoundService()
        {
            _player.MediaOpened += (_, _) =>
            {
                // Rewind to the beginning before each play so repeated notifications are consistent.
                _player.Position = TimeSpan.Zero;
                _player.Play();
            };

            _player.MediaFailed += (_, e) =>
            {
                // This service currently shows UI directly.
                // That is acceptable for a small app, though a larger design would usually bubble
                // the error up so the viewmodel decides how to present it.
                SoundErrorOccurred?.Invoke(
                    $"Sound failed: {e.ErrorException?.Message}");
            };
        }

        /// <summary>
        /// Plays the configured timer completion sound from the Assets folder.
        /// </summary>
        public void PlayTimerCompleteSound()
        {
            string path = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Assets",
                "notification-sound.mp3");

            if (!File.Exists(path))
            {
                SoundErrorOccurred?.Invoke($"Sound file not found: {path}");
                return;
            }
            // Stop any current playback so rapid repeated notifications restart cleanly.

            _player.Stop();
            _player.Open(new Uri(path, UriKind.Absolute));
        }
    }
}