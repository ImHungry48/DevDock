using System;
using DevDock.ViewModels;

namespace DevDock.Models
{
    // This model also acts as a bindable view-facing object.
    // That keeps snippet editing simple in the UI, even though it mixes model and presentation concerns.
    public class CodeSnippet : BaseViewModel
    {
        private Guid _id = Guid.NewGuid();
        private string _title = string.Empty;
        private string _content = string.Empty;

        public Guid Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public string Content
        {
            get => _content;
            set => SetProperty(ref _content, value);
        }
    }
}