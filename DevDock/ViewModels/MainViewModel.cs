using System.Windows;
using System.Windows.Input;
using DevDock.Commands;
using DevDock.Services;

namespace DevDock.ViewModels
{
    // Main shell view model for the application.
    // It owns the child feature view models and coordinates cross-feature UI state,
    // such as the selected tab, quick reminder banner, and persistent notes panel.
    public class MainViewModel : BaseViewModel
    {
        private readonly NotesStorageService _notesStorageService = new NotesStorageService();

        private string _selectedTab = "Tasks";
        private string _deadlineReminder = string.Empty;
        private bool _isReminderVisible;
        private bool _isReminderDismissedCompletely = false;
        private string _quoteText = "Stay consistent. Small progress every day adds up to big results.";
        private string _notesText = string.Empty;

        // Child view models are created once and kept alive for the lifetime of the shell,
        // which keeps feature state available while the user switches tabs.
        public TaskBoardViewModel TaskBoard { get; } = new TaskBoardViewModel();
        public PomodoroViewModel Pomodoro { get; } = new PomodoroViewModel();
        public MusicPlayerViewModel MusicPlayer { get; } = new MusicPlayerViewModel();
        public SnippetsViewModel Snippets { get; } = new SnippetsViewModel();

        public MainViewModel()
        {
            // Simple tab-selection commands keep view logic out of the code-behind.
            SelectTasksCommand = new RelayCommand(_ => SelectedTab = "Tasks");
            SelectTimerCommand = new RelayCommand(_ => SelectedTab = "Timer");
            SelectSnippetsCommand = new RelayCommand(_ => SelectedTab = "Snippets");
            SelectNotesCommand = new RelayCommand(_ => SelectedTab = "Notes");
            SelectMusicCommand = new RelayCommand(_ => SelectedTab = "Music");

            DismissReminderCommand = new RelayCommand(_ =>
            {
                // This intentionally hides the banner until the app is recreated.
                // The dismissal is a UI-level choice rather than a persisted task state.
                IsReminderVisible = false;
                _isReminderDismissedCompletely = true;
            });

            // The main window is hidden instead of closed so the tray icon can continue
            // to control the app without shutting down the process.
            CloseWindowCommand = new RelayCommand(_ => Application.Current.MainWindow?.Hide());

            // Notes are loaded once on startup and then auto-saved whenever the text changes.
            var savedNotes = _notesStorageService.GetNotes();
            NotesText = savedNotes.Content;

            // Mirror the task board reminder into the shell so it can be shown in a global banner.
            DeadlineReminder = TaskBoard.UpcomingReminderText;
            IsReminderVisible = TaskBoard.HasUpcomingReminder && !_isReminderDismissedCompletely;

            // Listen for task-board reminder changes instead of duplicating reminder calculation here.
            TaskBoard.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(TaskBoardViewModel.UpcomingReminderText) ||
                    e.PropertyName == nameof(TaskBoardViewModel.HasUpcomingReminder))
                {
                    DeadlineReminder = TaskBoard.UpcomingReminderText;

                    if (!_isReminderDismissedCompletely)
                    {
                        IsReminderVisible = TaskBoard.HasUpcomingReminder;
                    }
                }
            };
        }

        public string SelectedTab
        {
            get => _selectedTab;
            set => SetProperty(ref _selectedTab, value);
        }

        public string DeadlineReminder
        {
            get => _deadlineReminder;
            set => SetProperty(ref _deadlineReminder, value);
        }

        public bool IsReminderVisible
        {
            get => _isReminderVisible;
            set => SetProperty(ref _isReminderVisible, value);
        }

        public string QuoteText
        {
            get => _quoteText;
            set => SetProperty(ref _quoteText, value);
        }

        public string NotesText
        {
            get => _notesText;
            set
            {
                if (SetProperty(ref _notesText, value))
                {
                    // Auto-save keeps the notes panel lightweight and removes the need
                    // for a separate save button.
                    _notesStorageService.SaveNotes(value);
                }
            }
        }

        public ICommand SelectTasksCommand { get; }
        public ICommand SelectTimerCommand { get; }
        public ICommand SelectSnippetsCommand { get; }
        public ICommand SelectNotesCommand { get; }
        public ICommand SelectMusicCommand { get; }
        public ICommand DismissReminderCommand { get; }
        public ICommand CloseWindowCommand { get; }
    }
}
