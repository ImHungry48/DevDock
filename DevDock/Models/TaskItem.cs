using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevDock.Models
{
    public class TaskItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public DateTime? DueDate { get; set; }
        public PriorityLevel Priority { get; set; } = PriorityLevel.Medium;
        public int EstimatedMinutes { get; set; }
        public TaskStatus Status { get; set; } = TaskStatus.Backlog;

        // Navigation-style collection of child subtasks.
        public List<SubtaskItem> Subtasks { get; set; } = new();

        // Convenience UI properties keep simple presentation logic close to the model.

        public int CompletedSubtaskCount => Subtasks.Count(s => s.IsCompleted);
        public int TotalSubtaskCount => Subtasks.Count;
        public string SubtaskProgressText =>
            TotalSubtaskCount == 0 ? "No subtasks" : $"{CompletedSubtaskCount}/{TotalSubtaskCount} complete";
        public string DueDateText => DueDate.HasValue ? $"Due: {DueDate.Value:MMM d}" : "No due date";

    }
}