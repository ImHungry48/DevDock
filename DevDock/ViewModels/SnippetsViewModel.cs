
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using DevDock.Commands;
using DevDock.Models;
using DevDock.Services;

namespace DevDock.ViewModels
{
    // View model for creating, listing, and deleting saved code snippets.
    public class SnippetsViewModel : BaseViewModel
    {
        private readonly CodeSnippetStorageService _storageService = new CodeSnippetStorageService();

        private string _searchText = string.Empty;
        private CodeSnippet? _selectedSnippet;
        private string _selectedSnippetContent = string.Empty;

        public ObservableCollection<CodeSnippet> Snippets { get; } = new ObservableCollection<CodeSnippet>();
        public ObservableCollection<CodeSnippet> FilteredSnippets { get; } = new ObservableCollection<CodeSnippet>();

        public SnippetsViewModel()
        {
            LoadSnippets();

            AddSnippetCommand = new RelayCommand(_ => AddSnippet());
            DeleteSnippetCommand = new RelayCommand(_ => DeleteSelectedSnippet(), _ => SelectedSnippet != null);
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    ApplyFilter();
                }
            }
        }

        public CodeSnippet? SelectedSnippet
        {
            get => _selectedSnippet;
            set
            {
                if (SetProperty(ref _selectedSnippet, value))
                {
                    OnPropertyChanged(nameof(SelectedSnippetTitle));
                    SelectedSnippetContent = value?.Content ?? string.Empty;
                }
            }
        }

        public string SelectedSnippetTitle
        {
            get => SelectedSnippet?.Title ?? string.Empty;
            set
            {
                if (SelectedSnippet == null)
                {
                    return;
                }

                if (SelectedSnippet.Title != value)
                {
                    SelectedSnippet.Title = value;
                    OnPropertyChanged(nameof(SelectedSnippetTitle));
                    _storageService.SaveSnippet(SelectedSnippet);
                }
            }
        }

        public string SelectedSnippetContent
        {
            get => _selectedSnippetContent;
            set
            {
                if (SetProperty(ref _selectedSnippetContent, value))
                {
                    if (SelectedSnippet != null)
                    {
                        SelectedSnippet.Content = value;
                        _storageService.SaveSnippet(SelectedSnippet);
                    }
                }
            }
        }

        public ICommand AddSnippetCommand { get; }
        public ICommand DeleteSnippetCommand { get; }

        private void LoadSnippets()
        {
            Snippets.Clear();

            foreach (var snippet in _storageService.GetAllSnippets())
            {
                Snippets.Add(snippet);
            }

            if (Snippets.Count == 0)
            {
                var starterSnippet = new CodeSnippet
                {
                    Title = "New Snippet",
                    Content = "// Write your code here"
                };

                _storageService.SaveSnippet(starterSnippet);
                Snippets.Add(starterSnippet);
            }

            ApplyFilter();
            SelectedSnippet = FilteredSnippets.FirstOrDefault();
        }

        private void ApplyFilter()
        {
            FilteredSnippets.Clear();

            var filtered = string.IsNullOrWhiteSpace(SearchText)
                ? Snippets
                : new ObservableCollection<CodeSnippet>(
                    Snippets.Where(s =>
                        s.Title.ToLower().Contains(SearchText.ToLower()) ||
                        s.Content.ToLower().Contains(SearchText.ToLower())));

            foreach (var snippet in filtered)
            {
                FilteredSnippets.Add(snippet);
            }

            if (SelectedSnippet != null && !FilteredSnippets.Contains(SelectedSnippet))
            {
                SelectedSnippet = FilteredSnippets.FirstOrDefault();
            }
        }

        private void AddSnippet()
        {
            var newSnippet = new CodeSnippet
            {
                Title = "New Snippet",
                Content = "// Write your code here"
            };

            _storageService.SaveSnippet(newSnippet);
            Snippets.Add(newSnippet);
            ApplyFilter();
            SelectedSnippet = newSnippet;
        }

        private void DeleteSelectedSnippet()
        {
            if (SelectedSnippet == null)
            {
                return;
            }

            _storageService.DeleteSnippet(SelectedSnippet);
            Snippets.Remove(SelectedSnippet);
            ApplyFilter();
            SelectedSnippet = FilteredSnippets.FirstOrDefault();
        }
    }
}