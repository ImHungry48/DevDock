using System.Linq;
using DevDock.Data;
using DevDock.Models;

/// <summary>
/// Stores the app's notes content as a single persistent record.
/// </summary>
namespace DevDock.Services
{
    public class NotesStorageService
    {
        /// <summary>
        /// Retrieves the notes row, creating a default one if it does not exist yet.
        /// </summary>
        public NotesData GetNotes()
        {
            using var db = new AppDbContext();

            // This service assumes the app supports one notes document, stored under Id = 1.
            var notes = db.Notes.FirstOrDefault(n => n.Id == 1);

            if (notes == null)
            {
                notes = new NotesData
                {
                    Id = 1,
                    Content = string.Empty
                };

                db.Notes.Add(notes);
                db.SaveChanges();
            }

            return notes;
        }

        /// <summary>
        /// Saves the current notes content into the single persisted notes row.
        /// </summary>
        public void SaveNotes(string content)
        {
            using var db = new AppDbContext();

            var notes = db.Notes.FirstOrDefault(n => n.Id == 1);

            if (notes == null)
            {
                notes = new NotesData
                {
                    Id = 1,
                    Content = content
                };

                db.Notes.Add(notes);
            }
            else
            {
                notes.Content = content;
            }

            db.SaveChanges();
        }
    }
}