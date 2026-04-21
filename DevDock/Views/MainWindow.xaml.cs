using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using DevDock.Models;
using DevDock.ViewModels;
using System.Windows.Controls;

namespace DevDock.Views
{
    public partial class MainWindow : Window
    {
        private TranslateTransform? _quoteTransform;

        public MainWindow()
        {
            InitializeComponent();

            Loaded += MainWindow_Loaded;
            Deactivated += MainWindow_Deactivated;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            StartQuoteTicker();
        }

        private void MainWindow_Deactivated(object? sender, EventArgs e)
        {
            Hide();
        }

        private void StartQuoteTicker()
        {
            _quoteTransform = new TranslateTransform();
            QuoteTickerText.RenderTransform = _quoteTransform;

            var animation = new DoubleAnimation
            {
                From = Width,
                To = -800,
                Duration = TimeSpan.FromSeconds(15),
                RepeatBehavior = RepeatBehavior.Forever
            };

            _quoteTransform.BeginAnimation(TranslateTransform.XProperty, animation);
        }

        private void SubtaskCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not CheckBox checkBox)
                return;

            if (DataContext is not MainViewModel mainViewModel)
                return;

            var taskItem = FindAncestorDataContext<TaskItem>(checkBox);
            if (taskItem == null)
                return;

            if (mainViewModel.TaskBoard.SaveTaskCommand.CanExecute(taskItem))
            {
                mainViewModel.TaskBoard.SaveTaskCommand.Execute(taskItem);
            }
        }

        private T? FindAncestorDataContext<T>(DependencyObject start) where T : class
        {
            DependencyObject? current = start;

            while (current != null)
            {
                if (current is FrameworkElement frameworkElement && frameworkElement.DataContext is T match)
                {
                    return match;
                }

                current = VisualTreeHelper.GetParent(current);
            }

            return null;
        }

        private void TaskMenuButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.ContextMenu != null)
            {
                button.ContextMenu.PlacementTarget = button;
                button.ContextMenu.IsOpen = true;
            }
        }

        private TaskItem? GetTaskFromMenuItem(object sender)
        {
            if (sender is not MenuItem menuItem)
                return null;

            if (menuItem.Parent is not ContextMenu contextMenu)
                return null;

            if (contextMenu.PlacementTarget is not FrameworkElement placementTarget)
                return null;

            return placementTarget.DataContext as TaskItem;
        }

        private TaskBoardViewModel? GetTaskBoardFromMenuItem(object sender)
        {
            if (sender is not MenuItem menuItem)
                return null;

            if (menuItem.Parent is not ContextMenu contextMenu)
                return null;

            if (contextMenu.PlacementTarget is not FrameworkElement placementTarget)
                return null;

            return placementTarget.Tag as TaskBoardViewModel;
        }

        private void EditTaskMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var task = GetTaskFromMenuItem(sender);
            var taskBoard = GetTaskBoardFromMenuItem(sender);

            if (task != null && taskBoard?.EditTaskCommand.CanExecute(task) == true)
            {
                taskBoard.EditTaskCommand.Execute(task);
            }
        }

        private void MoveTaskBackwardMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var task = GetTaskFromMenuItem(sender);
            var taskBoard = GetTaskBoardFromMenuItem(sender);

            if (task != null && taskBoard?.MoveTaskBackwardCommand.CanExecute(task) == true)
            {
                taskBoard.MoveTaskBackwardCommand.Execute(task);
            }
        }

        private void MoveTaskForwardMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var task = GetTaskFromMenuItem(sender);
            var taskBoard = GetTaskBoardFromMenuItem(sender);

            if (task != null && taskBoard?.MoveTaskForwardCommand.CanExecute(task) == true)
            {
                taskBoard.MoveTaskForwardCommand.Execute(task);
            }
        }

        private void DeleteTaskMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var task = GetTaskFromMenuItem(sender);
            var taskBoard = GetTaskBoardFromMenuItem(sender);

            if (task != null && taskBoard?.DeleteTaskCommand.CanExecute(task) == true)
            {
                taskBoard.DeleteTaskCommand.Execute(task);
            }
        }
    }
}