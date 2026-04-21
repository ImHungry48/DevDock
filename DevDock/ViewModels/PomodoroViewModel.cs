using System;
using System.Windows.Input;
using System.Windows.Threading;
using DevDock.Commands;
using DevDock.Models;
using DevDock.Services;

namespace DevDock.ViewModels
{
    // View model for the Pomodoro timer.
    // It manages timer state, the active session mode, and commands for switching
    // between focus and break periods.
    public class PomodoroViewModel : BaseViewModel
    {
        private readonly DispatcherTimer _timer;

        private PomodoroMode _currentMode;
        private TimeSpan _remainingTime;
        private bool _isRunning;

        private readonly NotificationSoundService _notificationSoundService;

        public PomodoroViewModel()
        {
            // DispatcherTimer is appropriate here because updates happen on the UI thread,
            // which keeps property changes safe for WPF bindings.
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += OnTimerTick;

            // Default durations can later be surfaced as user settings if needed.
            FocusMinutes = 25;
            ShortBreakMinutes = 5;
            LongBreakMinutes = 15;

            SetMode(PomodoroMode.Focus);

            SetFocusCommand = new RelayCommand(_ => SetMode(PomodoroMode.Focus));
            SetShortBreakCommand = new RelayCommand(_ => SetMode(PomodoroMode.ShortBreak));
            SetLongBreakCommand = new RelayCommand(_ => SetMode(PomodoroMode.LongBreak));

            StartCommand = new RelayCommand(_ => Start(), _ => !IsRunning);
            PauseCommand = new RelayCommand(_ => Pause(), _ => IsRunning);
            ResetCommand = new RelayCommand(_ => Reset());

            _notificationSoundService = new NotificationSoundService();
        }

        public int FocusMinutes { get; set; }
        public int ShortBreakMinutes { get; set; }
        public int LongBreakMinutes { get; set; }

        public PomodoroMode CurrentMode
        {
            get => _currentMode;
            set
            {
                _currentMode = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CurrentModeLabel));
            }
        }

        public string CurrentModeLabel => CurrentMode switch
        {
            PomodoroMode.Focus => "Focus",
            PomodoroMode.ShortBreak => "Short Break",
            PomodoroMode.LongBreak => "Long Break",
            _ => "Pomodoro"
        };

        public TimeSpan RemainingTime
        {
            get => _remainingTime;
            set
            {
                _remainingTime = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayTime));
            }
        }

        public string DisplayTime => RemainingTime.ToString(@"mm\:ss");

        public bool IsRunning
        {
            get => _isRunning;
            set
            {
                _isRunning = value;
                OnPropertyChanged();
                // Force WPF to re-evaluate command CanExecute so the Start and Pause
                // buttons enable and disable immediately when state changes.
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public ICommand SetFocusCommand { get; }
        public ICommand SetShortBreakCommand { get; }
        public ICommand SetLongBreakCommand { get; }

        public ICommand StartCommand { get; }
        public ICommand PauseCommand { get; }
        public ICommand ResetCommand { get; }

        private void OnTimerTick(object? sender, EventArgs e)
        {
            if (RemainingTime > TimeSpan.Zero)
            {
                RemainingTime = RemainingTime.Subtract(TimeSpan.FromSeconds(1));
            }
            else
            {
                Pause();
                _notificationSoundService.PlayTimerCompleteSound();
            }
        }

        private void SetMode(PomodoroMode mode)
        {
            // Changing modes always pauses the current session and resets the timer
            // to the configured duration for that mode.
            Pause();
            CurrentMode = mode;

            RemainingTime = mode switch
            {
                PomodoroMode.Focus => TimeSpan.FromMinutes(FocusMinutes),
                PomodoroMode.ShortBreak => TimeSpan.FromMinutes(ShortBreakMinutes),
                PomodoroMode.LongBreak => TimeSpan.FromMinutes(LongBreakMinutes),
                _ => TimeSpan.FromMinutes(25)
            };
        }

        private void Start()
        {
            if (RemainingTime <= TimeSpan.Zero)
                Reset();

            _timer.Start();
            IsRunning = true;
        }

        private void Pause()
        {
            _timer.Stop();
            IsRunning = false;
        }

        private void Reset()
        {
            SetMode(CurrentMode);
        }
    }
}
