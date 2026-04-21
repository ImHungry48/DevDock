using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using DevDock.Commands;
using DevDock.Models;
using DevDock.Services;

namespace DevDock.ViewModels
{
    // View model that exposes track metadata and playback commands to the UI.
    // It delegates actual audio playback to MusicPlayerService and keeps only the
    // UI-facing state here.
    public class MusicPlayerViewModel : BaseViewModel
    {
        private readonly MusicPlayerService _musicPlayerService;

        private Track? _currentTrack;
        private bool _isPlaying;
        private int _currentTrackIndex = -1;

        public MusicPlayerViewModel()
        {
            _musicPlayerService = new MusicPlayerService();
            _musicPlayerService.TrackEnded += OnTrackEnded;

            // Load the available track metadata once at startup.
            Tracks = new ObservableCollection<Track>(_musicPlayerService.LoadTracks());

            // Default to the first track so the UI can show meaningful metadata
            // before the user presses play.
            if (Tracks.Any())
            {
                _currentTrackIndex = 0;
                CurrentTrack = Tracks[_currentTrackIndex];
            }

            PreviousCommand = new RelayCommand(_ => PlayPrevious(), _ => HasTracks);
            PlayPauseCommand = new RelayCommand(_ => TogglePlayPause(), _ => HasTracks);
            NextCommand = new RelayCommand(_ => PlayNext(), _ => HasTracks);
        }

        public ObservableCollection<Track> Tracks { get; }

        public bool HasTracks => Tracks.Count > 0;

        public Track? CurrentTrack
        {
            get => _currentTrack;
            set
            {
                _currentTrack = value;
                OnPropertyChanged();
                // These derived properties are separate to simplify XAML bindings.
                OnPropertyChanged(nameof(CurrentTrackTitle));
                OnPropertyChanged(nameof(CurrentTrackArtist));
                OnPropertyChanged(nameof(CurrentTrackAlbum));
                OnPropertyChanged(nameof(CurrentAlbumArtPath));
            }
        }

        public string CurrentTrackTitle => CurrentTrack?.Title ?? "No track loaded";
        public string CurrentTrackArtist => CurrentTrack?.Artist ?? "Unknown Artist";
        public string CurrentTrackAlbum => CurrentTrack?.Album ?? string.Empty;
        public string? CurrentAlbumArtPath => CurrentTrack?.AlbumArtPath;

        public bool IsPlaying
        {
            get => _isPlaying;
            set
            {
                _isPlaying = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(PlayPauseLabel));
            }
        }

        public string PlayPauseLabel => IsPlaying ? "⏸" : "▶";

        public ICommand PreviousCommand { get; }
        public ICommand PlayPauseCommand { get; }
        public ICommand NextCommand { get; }

        private void TogglePlayPause()
        {
            if (CurrentTrack == null)
                return;

            if (!IsPlaying)
            {
                _musicPlayerService.Play(CurrentTrack.FilePath);
                IsPlaying = true;
            }
            else
            {
                _musicPlayerService.Pause();
                IsPlaying = false;
            }
        }

        private void PlayNext()
        {
            if (!HasTracks)
                return;

            // Wrap around so the player behaves like a continuous playlist.
            _currentTrackIndex = (_currentTrackIndex + 1) % Tracks.Count;
            CurrentTrack = Tracks[_currentTrackIndex];

            _musicPlayerService.Play(CurrentTrack.FilePath);
            IsPlaying = true;
        }

        private void PlayPrevious()
        {
            if (!HasTracks)
                return;

            _currentTrackIndex--;

            if (_currentTrackIndex < 0)
            {
                _currentTrackIndex = Tracks.Count - 1;
            }

            CurrentTrack = Tracks[_currentTrackIndex];

            _musicPlayerService.Play(CurrentTrack.FilePath);
            IsPlaying = true;
        }

        private void OnTrackEnded()
        {
            // Auto-advance keeps the playlist moving without requiring view logic.
            PlayNext();
        }
    }
}
