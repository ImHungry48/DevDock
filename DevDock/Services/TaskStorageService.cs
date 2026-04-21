using System;
using DevDock.Data;
using DevDock.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace DevDock.Services
{
    /// <summary>
    /// Handles persistence for tasks and their subtasks.
    /// </summary>
    public class TaskStorageService
    {
        /// <summary>
        /// Ensures the local database exists before task operations are used.
        /// </summary>
        public void InitializeDatabase()
        {
            using var db = new AppDbContext();
            db.Database.EnsureCreated();
        }

        /// <summary>
        /// Returns all tasks with subtasks eagerly loaded for immediate UI use.
        /// </summary>
        public List<TaskItem> GetAllTasks()
        {
            using var db = new AppDbContext();
            return db.Tasks
                .Include(t => t.Subtasks)
                .ToList();
        }

        /// <summary>
        /// Saves a task and its subtasks.
        /// This method supports both create and update flows.
        /// </summary>
        public void SaveTask(TaskItem task)
        {
            using var db = new AppDbContext();

            var existingTask = db.Tasks
                .FirstOrDefault(t => t.Id == task.Id);

            if (existingTask == null)
            {
                // Insert the main task record first.
                db.Tasks.Add(new TaskItem
                {
                    Id = task.Id,
                    Title = task.Title,
                    Description = task.Description,
                    DueDate = task.DueDate,
                    Priority = task.Priority,
                    EstimatedMinutes = task.EstimatedMinutes,
                    Status = task.Status
                });

                // Insert subtasks as separate rows linked by TaskId.
                // New Guid values are generated here so the service owns persistence identity.
                foreach (var s in task.Subtasks)
                {
                    db.Subtasks.Add(new SubtaskItem
                    {
                        Id = Guid.NewGuid(),
                        TaskId = task.Id,
                        Title = s.Title,
                        IsCompleted = s.IsCompleted
                    });
                }

                db.SaveChanges();
                return;
            }

            // Update the scalar task fields.
            existingTask.Title = task.Title;
            existingTask.Description = task.Description;
            existingTask.DueDate = task.DueDate;
            existingTask.Priority = task.Priority;
            existingTask.EstimatedMinutes = task.EstimatedMinutes;
            existingTask.Status = task.Status;

            db.SaveChanges();

            // Current strategy: replace the entire subtask list on every edit.
            // This is simple and reliable for small collections, though it does mean
            // subtask identities are not preserved across updates.
            var oldSubtasks = db.Subtasks
                .Where(s => s.TaskId == existingTask.Id)
                .ToList();

            db.Subtasks.RemoveRange(oldSubtasks);
            db.SaveChanges();

            foreach (var s in task.Subtasks)
            {
                db.Subtasks.Add(new SubtaskItem
                {
                    Id = Guid.NewGuid(),
                    TaskId = existingTask.Id,
                    Title = s.Title,
                    IsCompleted = s.IsCompleted
                });
            }

            db.SaveChanges();
        }

        /// <summary>
        /// Deletes the task and its loaded subtasks.
        /// </summary>

        public void DeleteTask(TaskItem task)
        {
            using var db = new AppDbContext();

            var existingTask = db.Tasks
                .Include(t => t.Subtasks)
                .FirstOrDefault(t => t.Id == task.Id);

            if (existingTask != null)
            {
                db.Tasks.Remove(existingTask);
                db.SaveChanges();
            }
        }
    }
}