using FileSystem.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace FileSystem.ViewModels
{
    public class DirectoryEntryViewModel : INotifyPropertyChanged
    {
        public DirectoryEntry DirectoryEntry { get; }

        public string Name => DirectoryEntry.Name;

        // 添加类型和修改时间属性
        public string Type => "文件夹";
        public System.DateTime ModifiedTime => DirectoryEntry.ModifiedTime;
        // 文件夹大小为null
        public long? Size => null;

        public ObservableCollection<DirectoryEntryViewModel> Children { get; }
        public ObservableCollection<FileEntryViewModel> Files { get; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }

        public DirectoryEntryViewModel(DirectoryEntry directoryEntry)
        {
            DirectoryEntry = directoryEntry;

            Children = new ObservableCollection<DirectoryEntryViewModel>(
                directoryEntry.Children
                    .OfType<DirectoryEntry>()
                    .Select(child => new DirectoryEntryViewModel(child))
            );

            Files = new ObservableCollection<FileEntryViewModel>(
                directoryEntry.Children
                    .Where(c => !(c is DirectoryEntry))
                    .Select(f => new FileEntryViewModel((FileEntry)f))
            );
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}