namespace DevDock.Models
{
    // Runtime playback model used by the music player.
    // This stores fully resolved paths that the player can use directly.
    public class Track
    {
        public string Title { get; set; } = string.Empty;
        public string Artist { get; set; } = "Unknown Artist";
        public string Album { get; set; } = "Unknown Album";
        public string FilePath { get; set; } = string.Empty;
        public string? AlbumArtPath { get; set; }
    }
}