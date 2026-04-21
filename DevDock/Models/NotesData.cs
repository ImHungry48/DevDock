namespace DevDock.Models
{
    // Single-record notes model.
    // The app currently treats notes as one shared document rather than a collection of note entries.
    public class NotesData
    {
        public int Id { get; set; } = 1;
        public string Content { get; set; } = string.Empty;
    }
}