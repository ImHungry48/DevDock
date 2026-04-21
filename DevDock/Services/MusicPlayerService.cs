using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows.Media;
using DevDock.Models;

namespace DevDock.Services
{
    /// <summary>
    /// Encapsulates music playback and track metadata loading for the in-app music player.
    /// </summary>
    public class MusicPlayerService
    {
        private readonly MediaPlayer _player = new MediaPlayer();
        private string? _currentFilePath;
        private bool _isPaused;

        /// <summary>
        /// Raised when the current track finishes so the UI or viewmodel can advance playback.
        /// </summary>
        public event Action? TrackEnded;

        public MusicPlayerService()
        {
            _player.MediaEnded += (_, _) =>
            {
                _isPaused = false;
                TrackEnded?.Invoke();
            };

            _player.MediaFailed += (_, e) =>
            {
                // Console logging is lightweight, though a real app might route this through
                // a logging service so failures can be surfaced more consistently.
                Console.WriteLine($"Music playback failed: {e.ErrorException?.Message}");
            };
        }
        /// <summary>
        /// Loads available tracks from Assets/Music/tracks.json and filters out invalid entries.
        /// </summary>

        public List<Track> LoadTracks()
        {
            string musicFolder = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Assets",
                "Music");

            string metadataPath = Path.Combine(musicFolder, "tracks.json");

            if (!File.Exists(metadataPath))
            {
                return new List<Track>();
            }

            try
            {
                string json = File.ReadAllText(metadataPath);

                var metadataList = JsonSerializer.Deserialize<List<TrackMetadata>>(
                    json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                if (metadataList == null)
                {
                    return new List<Track>();
                }

                var tracks = new List<Track>();

                foreach (var metadata in metadataList)
                {
                    if (string.IsNullOrWhiteSpace(metadata.FileName))
                    {
                        continue;
                    }

                    string filePath = Path.Combine(musicFolder, metadata.FileName);

                    if (!File.Exists(filePath))
                    {
                        continue;
                    }

                    string? albumArtPath = null;

                    if (!string.IsNullOrWhiteSpace(metadata.AlbumArtFileName))
                    {
                        string possibleArtPath = Path.Combine(musicFolder, metadata.AlbumArtFileName);

                        if (File.Exists(possibleArtPath))
                        {
                            albumArtPath = possibleArtPath;
                        }
                    }

                    tracks.Add(new Track
                    {
                        Title = metadata.Title,
                        Artist = metadata.Artist,
                        Album = metadata.Album,
                        FilePath = filePath,
                        AlbumArtPath = albumArtPath
                    });
                }

                return tracks;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading track metadata: {ex.Message}");
                return new List<Track>();
            }
        }

        /// <summary>
        /// Plays the requested file.
        /// If it is already the active file and playback was paused, this acts like resume.
        /// </summary>
        public void Play(string filePath)
        {
            if (_currentFilePath == filePath)
            {
                if (_isPaused)
                {
                    _player.Play();
                    _isPaused = false;
                }

                return;
            }

            _player.Open(new Uri(filePath, UriKind.Absolute));
            _player.Play();

            _currentFilePath = filePath;
            _isPaused = false;
        }

        /// <summary>
        /// Pauses the current track without resetting the loaded media.
        /// </summary>
        public void Pause()
        {
            _player.Pause();
            _isPaused = true;
        }

        /// <summary>
        /// Stops playback and clears the paused state.
        /// </summary>
        public void Stop()
        {
            _player.Stop();
            _isPaused = false;
        }
    }
}