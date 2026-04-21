using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using DevDock.Models;

namespace DevDock.Views
{
    public partial class EditTaskWindow : Window
    {
        private readonly TaskItem _task;

        public EditTaskWindow(TaskItem task)
        {
            InitializeComponent();
            _task = task;
            DataContext = _task;

            PriorityComboBox.ItemsSource = Enum.GetValues(typeof(PriorityLevel)).Cast<PriorityLevel>();
            PriorityComboBox.SelectedItem = _task.Priority;

            StatusComboBox.ItemsSource = Enum.GetValues(typeof(DevDock.Models.TaskStatus)).Cast<DevDock.Models.TaskStatus>();
            StatusComboBox.SelectedItem = _task.Status;
        }

        private void AddSubtaskButton_Click(object sender, RoutedEventArgs e)
        {
            string title = NewSubtaskTextBox.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(title))
                return;

            _task.Subtasks.Add(new SubtaskItem
            {
                TaskId = _task.Id,
                Title = title,
                IsCompleted = false
            });

            SubtasksListBox.Items.Refresh();
            NewSubtaskTextBox.Clear();
        }

        private void RemoveSubtaskButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is SubtaskItem subtask)
            {
                _task.Subtasks.Remove(subtask);
                SubtasksListBox.Items.Refresh();
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            _task.Priority = (PriorityLevel)PriorityComboBox.SelectedItem;
            _task.Status = (DevDock.Models.TaskStatus)StatusComboBox.SelectedItem;

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Header_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}