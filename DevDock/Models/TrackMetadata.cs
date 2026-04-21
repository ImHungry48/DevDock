namespace DevDock.Models
{
    public class TrackMetadata
    {
        public string Title { get; set; } = string.Empty;
        public string Artist { get; set; } = "Unknown Artist";
        public string Album { get; set; } = "Unknown Album";
        public string FileName { get; set; } = string.Empty;
        public string? AlbumArtFileName { get; set; }
    }
}