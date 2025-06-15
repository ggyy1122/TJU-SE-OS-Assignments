using FileSystem.Models;
using System.ComponentModel;

namespace FileSystem.ViewModels
{
    public class FileEntryViewModel : INotifyPropertyChanged
    {
        public FileEntry FileEntry { get; }
        public string Name => FileEntry.Name;

        // 添加类型、修改时间和大小属性
        public string Type => "文件";
        public System.DateTime ModifiedTime => FileEntry.ModifiedTime;
        public long? Size => FileEntry.Size;

        public FileEntryViewModel(FileEntry fileEntry)
        {
            FileEntry = fileEntry;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}