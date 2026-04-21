using DevDock.Models;
using Microsoft.EntityFrameworkCore;

namespace DevDock.Data
{
    /// <summary>
    /// EF Core database context for the application's persisted data.
    /// </summary>
    public class AppDbContext : DbContext
    {
        // Task board entities.
        public DbSet<TaskItem> Tasks => Set<TaskItem>();
        public DbSet<SubtaskItem> Subtasks => Set<SubtaskItem>();

        // Single persisted notes document.
        public DbSet<NotesData> Notes => Set<NotesData>();

        // Stored code snippets.
        public DbSet<CodeSnippet> CodeSnippets => Set<CodeSnippet>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Uses a local SQLite database file created in the app's working directory.
            optionsBuilder.UseSqlite("Data Source=devdock.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TaskItem>()
                // One task can contain many subtasks.
                .HasMany(t => t.Subtasks)
                .WithOne()
                .HasForeignKey(s => s.TaskId)
                // Deleting a task automatically removes its child subtasks.
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
