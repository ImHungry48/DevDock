using DevDock.Commands;
using DevDock.Models;
using DevDock.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace DevDock.ViewModels
{
    // View model for the task board / Kanban feature.
    // It owns the task collection, the per-lane filtered views, task creation form state,
    // and the reminder text shown elsewhere in the app.
    public class TaskBoardViewModel : BaseViewModel
    {
        private readonly TaskStorageService _storageService = new();

        private ObservableCollection<TaskItem> _allTasks = new();

        private string _newTaskTitle = string.Empty;
        private string _newTaskDescription = string.Empty;
        private DateTime? _newTaskDueDate = DateTime.Today;
        private PriorityLevel _newTaskPriority = PriorityLevel.Medium;
        private int _newTaskEstimatedMinutes = 30;
        private TaskStatus _newTaskStatus = TaskStatus.Backlog;
        private string _newTaskSubtasksText = string.Empty;

        public ObservableCollection<TaskItem> AllTasks
        {
            get => _allTasks;
            set
            {
                if (SetProperty(ref _allTasks, value))
                {
                    RefreshLanes();
                }
            }
        }

        // Separate observable collections keep XAML binding simple for each Kanban lane.
        public ObservableCollection<TaskItem> BacklogTasks { get; } = new();
        public ObservableCollection<TaskItem> TodoTasks { get; } = new();
        public ObservableCollection<TaskItem> InProgressTasks { get; } = new();
        public ObservableCollection<TaskItem> CodeReviewTasks { get; } = new();
        public ObservableCollection<TaskItem> CompletedTasks { get; } = new();

        public Array PriorityOptions => Enum.GetValues(typeof(PriorityLevel));
        public Array StatusOptions => Enum.GetValues(typeof(TaskStatus));

        private string _upcomingReminderText = string.Empty;

        public string UpcomingReminderText
        {
            get => _upcomingReminderText; 
            set
            {
                if (SetProperty(ref _upcomingReminderText, value))
                {
                    OnPropertyChanged(nameof(HasUpcomingReminder));
                }
            }
        }

        public bool HasUpcomingReminder => !string.IsNullOrWhiteSpace(UpcomingReminderText);

        // These properties represent the transient add-task form, not a saved task.
        public string NewTaskTitle
        {
            get => _newTaskTitle;
            set => SetProperty(ref _newTaskTitle, value);
        }

        public string NewTaskDescription
        {
            get => _newTaskDescription;
            set => SetProperty(ref _newTaskDescription, value);
        }

        public DateTime? NewTaskDueDate
        {
            get => _newTaskDueDate;
            set => SetProperty(ref _newTaskDueDate, value);
        }

        public PriorityLevel NewTaskPriority
        {
            get => _newTaskPriority;
            set => SetProperty(ref _newTaskPriority, value);
        }

        public int NewTaskEstimatedMinutes
        {
            get => _newTaskEstimatedMinutes;
            set => SetProperty(ref _newTaskEstimatedMinutes, value);
        }

        public TaskStatus NewTaskStatus
        {
            get => _newTaskStatus;
            set => SetProperty(ref _newTaskStatus, value);
        }

        public string NewTaskSubtasksText
        {
            get => _newTaskSubtasksText;
            set => SetProperty(ref _newTaskSubtasksText, value);
        }

        public ICommand AddTaskCommand { get; }
        public ICommand DeleteTaskCommand { get; }
        public ICommand MoveTaskForwardCommand { get; }
        public ICommand MoveTaskBackwardCommand { get; }

        public ICommand SaveTaskCommand { get; }

        public ICommand EditTaskCommand { get; }

        public TaskBoardViewModel()
        {
            AddTaskCommand = new RelayCommand(_ => AddNewTask());

            DeleteTaskCommand = new RelayCommand(task =>
            {
                if (task is TaskItem taskItem)
                {
                    RemoveTask(taskItem);
                }
            });

            MoveTaskForwardCommand = new RelayCommand(task =>
            {
                if (task is TaskItem taskItem)
                {
                    MoveTask(taskItem, GetNextStatus(taskItem.Status));
                }
            });

            MoveTaskBackwardCommand = new RelayCommand(task =>
            {
                if (task is TaskItem taskItem)
                {
                    MoveTask(taskItem, GetPreviousStatus(taskItem.Status));
                }
            });

            SaveTaskCommand = new RelayCommand(task =>
            {
                if (task is TaskItem taskItem)
                {
                    _storageService.SaveTask(taskItem);
                    LoadTasks();
                }
            });

            EditTaskCommand = new RelayCommand(task =>
            {
                if (task is TaskItem taskItem)
                {
                    OpenEditTaskWindow(taskItem);
                }
            });

            _storageService.InitializeDatabase();
            LoadTasks();
        }

        public void LoadTasks()
        {
            AllTasks = new ObservableCollection<TaskItem>(_storageService.GetAllTasks());
            RefreshLanes();
            RefreshReminder();
        }

        public void AddTask(TaskItem task)
        {
            _storageService.SaveTask(task);
            LoadTasks();
        }

        public void RemoveTask(TaskItem task)
        {
            _storageService.DeleteTask(task);
            LoadTasks();
        }

        public void MoveTask(TaskItem task, TaskStatus newStatus)
        {
            task.Status = newStatus;
            _storageService.SaveTask(task);
            LoadTasks();
        }

        public void RefreshLanes()
        {
            RebuildLane(BacklogTasks, TaskStatus.Backlog);
            RebuildLane(TodoTasks, TaskStatus.Todo);
            RebuildLane(InProgressTasks, TaskStatus.InProgress);
            RebuildLane(CodeReviewTasks, TaskStatus.CodeReview);
            RebuildLane(CompletedTasks, TaskStatus.Completed);
        }

        private void AddNewTask()
        {
            if (string.IsNullOrWhiteSpace(NewTaskTitle))
                return;

            var task = new TaskItem
            {
                Title = NewTaskTitle.Trim(),
                Description = NewTaskDescription?.Trim() ?? string.Empty,
                DueDate = NewTaskDueDate,
                Priority = NewTaskPriority,
                EstimatedMinutes = NewTaskEstimatedMinutes,
                Status = NewTaskStatus
            };

            // Each non-empty line becomes one subtask. This keeps the input UI lightweight
            // while still supporting multiple subtasks during creation.
            var subtaskLines = (NewTaskSubtasksText ?? string.Empty)
                .Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                .Select(line => line.Trim())
                .Where(line => !string.IsNullOrWhiteSpace(line));

            foreach (var line in subtaskLines)
            {
                task.Subtasks.Add(new SubtaskItem
                {
                    TaskId = task.Id,
                    Title = line,
                    IsCompleted = false
                });
            }

            AddTask(task);
            ClearNewTaskForm();
        }

        private void ClearNewTaskForm()
        {
            NewTaskTitle = string.Empty;
            NewTaskDescription = string.Empty;
            NewTaskDueDate = DateTime.Today;
            NewTaskPriority = PriorityLevel.Medium;
            NewTaskEstimatedMinutes = 30;
            NewTaskStatus = TaskStatus.Backlog;
            NewTaskSubtasksText = string.Empty;
        }

        // These helper methods define the linear workflow of the board.
        private TaskStatus GetNextStatus(TaskStatus status)
        {
            return status switch
            {
                TaskStatus.Backlog => TaskStatus.Todo,
                TaskStatus.Todo => TaskStatus.InProgress,
                TaskStatus.InProgress => TaskStatus.CodeReview,
                TaskStatus.CodeReview => TaskStatus.Completed,
                TaskStatus.Completed => TaskStatus.Completed,
                _ => status
            };
        }

        private TaskStatus GetPreviousStatus(TaskStatus status)
        {
            return status switch
            {
                TaskStatus.Completed => TaskStatus.CodeReview,
                TaskStatus.CodeReview => TaskStatus.InProgress,
                TaskStatus.InProgress => TaskStatus.Todo,
                TaskStatus.Todo => TaskStatus.Backlog,
                TaskStatus.Backlog => TaskStatus.Backlog,
                _ => status
            };
        }

        private void RebuildLane(ObservableCollection<TaskItem> lane, TaskStatus status)
        {
            lane.Clear();

            var filteredTasks = AllTasks
                .Where(t => t.Status == status)
                .OrderBy(t => t.DueDate ?? DateTime.MaxValue)
                .ThenByDescending(t => t.Priority)
                .ToList();

            foreach (var task in filteredTasks)
            {
                lane.Add(task);
            }
        }

        private void OpenEditTaskWindow(TaskItem task)
        {
            // Work on a detached copy so changes are only committed if the dialog is confirmed.
            var editableCopy = new TaskItem
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                DueDate = task.DueDate,
                Priority = task.Priority,
                EstimatedMinutes = task.EstimatedMinutes,
                Status = task.Status,
                Subtasks = task.Subtasks
                    .Select(s => new SubtaskItem
                    {
                        Id = s.Id,
                        TaskId = s.TaskId,
                        Title = s.Title,
                        IsCompleted = s.IsCompleted
                    })
                    .ToList()
            };

            // This introduces a small amount of view coupling in the view model,
            // but it keeps edit orchestration simple for a portfolio-scale app.
            var editWindow = new Views.EditTaskWindow(editableCopy)
            {
                Owner = Application.Current.MainWindow
            };

            bool? result = editWindow.ShowDialog();
            if (result == true && SaveTaskCommand.CanExecute(editableCopy))
            {
                SaveTaskCommand.Execute(editableCopy);
            }
        }

        private void RefreshReminder()
        {
            var activeTasks = AllTasks
                .Where(t => t.Status != TaskStatus.Completed && t.DueDate.HasValue)
                .ToList();

            var dueTodayTasks = activeTasks
                .Where(t => t.DueDate!.Value.Date == DateTime.Today)
                .OrderBy(t => t.Title)
                .ToList();

            // Collapse many same-day reminders into one summary so the banner stays readable.
            if (dueTodayTasks.Count > 3)
            {
                UpcomingReminderText = "Reminder: Several items due today. Please review the board.";
                return;
            }

            if (dueTodayTasks.Count >= 1)
            {
                string joinedTitles = string.Join(", ", dueTodayTasks.Select(t => t.Title));
                UpcomingReminderText = $"Reminder: Due today — {joinedTitles}.";
                return;
            }

            var overdueTask = activeTasks
                .Where(t => t.DueDate!.Value.Date < DateTime.Today)
                .OrderBy(t => t.DueDate)
                .FirstOrDefault();

            if (overdueTask != null)
            {
                UpcomingReminderText = $"Reminder: {overdueTask.Title} is overdue (was due {overdueTask.DueDate:MMM d}).";
                return;
            }

            var nextUpcomingTask = activeTasks
                .Where(t => t.DueDate!.Value.Date > DateTime.Today)
                .OrderBy(t => t.DueDate)
                .FirstOrDefault();

            if (nextUpcomingTask != null)
            {
                DateTime due = nextUpcomingTask.DueDate!.Value.Date;

                if (due == DateTime.Today.AddDays(1))
                {
                    UpcomingReminderText = $"Reminder: {nextUpcomingTask.Title} is due tomorrow.";
                }
                else
                {
                    UpcomingReminderText = $"Reminder: {nextUpcomingTask.Title} is due {due:MMM d}.";
                }

                return;
            }

            UpcomingReminderText = string.Empty;
        }

        // Temporary development helper.
        // Call manually if want to repopulate the DB with starter tasks
        /*
        private void SeedSampleData()
        {
            var sampleTasks = new[]
            {
                new TaskItem
                {
                    Title = "Refactor tray open behavior",
                    Description = "Clean up tray interaction logic",
                    DueDate = DateTime.Today.AddDays(1),
                    Priority = PriorityLevel.High,
                    EstimatedMinutes = 45,
                    Status = TaskStatus.Backlog,
                    Subtasks =
                    {
                        new SubtaskItem { Title = "Review TrayService", IsCompleted = true },
                        new SubtaskItem { Title = "Handle toggle edge cases", IsCompleted = false }
                    }
                },
                new TaskItem
                {
                    Title = "Build add-task dialog",
                    Description = "Allow users to create tasks from the popup",
                    DueDate = DateTime.Today,
                    Priority = PriorityLevel.Urgent,
                    EstimatedMinutes = 60,
                    Status = TaskStatus.Todo
                },
                new TaskItem
                {
                    Title = "Main popup shell",
                    Description = "Finish popup shell layout",
                    DueDate = DateTime.Today,
                    Priority = PriorityLevel.High,
                    EstimatedMinutes = 90,
                    Status = TaskStatus.InProgress
                }
            };

            foreach (var task in sampleTasks)
            {
                foreach (var subtask in task.Subtasks)
                {
                    subtask.TaskId = task.Id;
                }

                _storageService.SaveTask(task);
            }
        } */
    }
}