using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevDock.Models
{
    // Child task entity stored under a parent TaskItem.
    public class SubtaskItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        // Foreign key back to the owning task.
        public Guid TaskId { get; set; }
        public string Title { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
    }
}