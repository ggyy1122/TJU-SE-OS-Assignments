using FileSystem.Models;
using FileSystem.Services;
using FileSystem.Commands;
using System.ComponentModel;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Windows;
using System.Globalization;
using System.Windows.Data;
using Microsoft.VisualBasic;
using System.Text;
using ICSharpCode.AvalonEdit;
using System.Windows.Controls;

namespace FileSystem.ViewModels
{
    // 用于ListView的统一条目（文件和文件夹都可用）
    public class EntryViewModel
    {
        public string Name { get; set; }
        public string Type { get; set; } // "文件" 或 "文件夹"
        public DateTime ModifiedTime { get; set; }
        public long? Size { get; set; } // 文件才有，文件夹为 null
    }
    // MainViewModel 是 MVVM 模式中的“VM”，绑定到主窗口
    public class MainViewModel : INotifyPropertyChanged
    {
        // 根目录的 ViewModel，整个目录树的起点（TreeView绑定用）
        public DirectoryEntryViewModel Root { get; }

        // 当前选中的目录（TreeView和文件列表同步用）
        private DirectoryEntryViewModel _selectedDirectory;
        public DirectoryEntryViewModel SelectedDirectory
        {
            get => _selectedDirectory;
            set
            {
                if (_selectedDirectory != value)
                {
                    _selectedDirectory = value;
                    if (_selectedDirectory != null)
                        _fileSystemManager.CurrentDirectory = _selectedDirectory.DirectoryEntry;
                    OnPropertyChanged(nameof(SelectedDirectory));
                    OnPropertyChanged(nameof(CurrentFiles)); // 目录变了，文件列表也刷新
                    OnPropertyChanged(nameof(CurrentPath));//当前路径也会刷新
                    OnPropertyChanged(nameof(CurrentEntries));  // 添加这一行！
                }
            }
        }

        // 当前选中的文件（ListView绑定用）
        private FileEntryViewModel _selectedFile;
        public FileEntryViewModel SelectedFile
        {
            get => _selectedFile;
            set
            {
                _selectedFile = value;
                OnPropertyChanged(nameof(SelectedFile));
            }
        }

        // 当前目录下的所有文件列表（ListView显示用）
        public ObservableCollection<FileEntryViewModel> CurrentFiles => SelectedDirectory?.Files;


        // 当前选中的条目（可能是文件或目录）
        private object _selectedEntry;
        public object SelectedEntry
        {
            get => _selectedEntry;
            set
            {
                _selectedEntry = value;
                OnPropertyChanged(nameof(SelectedEntry));
            }
        }

        // 当前目录下的所有条目（包括文件和目录）
        public ObservableCollection<object> CurrentEntries
        {
            get
            {
                var entries = new ObservableCollection<object>();
                if (SelectedDirectory != null)
                {
                    foreach (var dir in SelectedDirectory.Children)
                        entries.Add(dir);
                    foreach (var file in SelectedDirectory.Files)
                        entries.Add(file);
                }
                return entries;
            }
        }



        // 新建目录时输入的名字
        private string _newDirName;
        public string NewDirName
        {
            get => _newDirName;
            set { _newDirName = value; OnPropertyChanged(nameof(NewDirName)); }
        }

        // 新建文件时输入的名字
        private string _newFileName;
        public string NewFileName
        {
            get => _newFileName;
            set { _newFileName = value; OnPropertyChanged(nameof(NewFileName)); }
        }

        // 文件内容编辑区的内容（读写文件时用）
        private string _fileContent;
        public string FileContent
        {
            get => _fileContent;
            set { _fileContent = value; OnPropertyChanged(nameof(FileContent)); }
        }
        //获取当前目录的路径
        public string CurrentPath
        {
            get => _fileSystemManager?.GetCurrentPath() ?? "";
        }


        // 添加用于重命名的属性
        private string _newEntryName;
        public string NewEntryName
        {
            get => _newEntryName;
            set
            {
                _newEntryName = value;
                OnPropertyChanged(nameof(NewEntryName));
            }
        }

        // 重命名命令
        public ICommand RenameCommand { get; }
        // 下面是所有与按钮/操作对应的命令（Command），用于XAML绑定
        public ICommand CreateDirCommand { get; }
        public ICommand CreateFileCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand ChangeDirCommand { get; }
        public ICommand UpDirCommand { get; }
        public ICommand ReadFileCommand { get; }
        public ICommand WriteFileCommand { get; }


        // 文件系统的核心模型，操作都靠它
        public FileSystemManager _fileSystemManager;
        public ObservableCollection<DirectoryEntryViewModel> RootNodes { get; }

        public MainViewModel()
        {
            // 初始化文件系统和目录树
            _fileSystemManager = new FileSystemManager();
            Root = new DirectoryEntryViewModel(_fileSystemManager.Root);
            RootNodes = new ObservableCollection<DirectoryEntryViewModel> { Root }; // 包一层集合
            SelectedDirectory = Root;

            CreateDirCommand = new RelayCommand(_ =>
            {
                if (SelectedDirectory != null)
                {
                    string name = NewDirName;
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        string baseName = "新建文件夹";
                        name = baseName;
                        int index = 1;
                        var dirNames = SelectedDirectory.Children.Select(d => d.Name).ToHashSet();
                        while (dirNames.Contains(name))
                        {
                            name = $"{baseName}({index++})";
                        }
                    }
                    try
                    {
                        _fileSystemManager.CurrentDirectory = SelectedDirectory.DirectoryEntry;
                        _fileSystemManager.CreateDirectory(name.Trim());
                        RefreshTree();
                        NewDirName = "";
                    }
                    catch (Exception ex)
                    {
                        ShowError(ex.Message);
                    }
                }
            }, _ => true);

            CreateFileCommand = new RelayCommand(_ =>
            {
                if (SelectedDirectory != null)
                {
                    string name = NewFileName;
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        string baseName = "新建文件.txt";
                        name = baseName;
                        int index = 1;
                        var fileNames = SelectedDirectory.Files.Select(f => f.Name).ToHashSet();
                        while (fileNames.Contains(name))
                        {
                            name = $"新建文件({index++}).txt";
                        }
                    }
                    try
                    {
                        _fileSystemManager.CurrentDirectory = SelectedDirectory.DirectoryEntry;
                        _fileSystemManager.CreateFile(name.Trim());
                        RefreshTree();
                        NewFileName = "";
                    }
                    catch (Exception ex)
                    {
                        ShowError(ex.Message);
                    }
                }
            }, _ => true);
            DeleteCommand = new RelayCommand(obj =>
            {
                if (SelectedDirectory != null)
                {
                    try
                    {
                        _fileSystemManager.CurrentDirectory = SelectedDirectory.DirectoryEntry;

                        if (obj is FileEntryViewModel fileVM)
                        {
                            _fileSystemManager.DeleteFile(fileVM.Name, SelectedDirectory.DirectoryEntry);
                        }
                        else if (obj is DirectoryEntryViewModel dirVM)
                        {
                            _fileSystemManager.DeleteDirectory(dirVM.Name);
                        }

                        RefreshTree();
                    }
                    catch (Exception ex)
                    {
                        ShowError(ex.Message);
                    }
                }
            });
            //重命名命令
            RenameCommand = new RelayCommand(obj =>
            {
                if (SelectedDirectory != null && obj != null)
                {
                    try
                    {
                        _fileSystemManager.CurrentDirectory = SelectedDirectory.DirectoryEntry;
                        string oldName = string.Empty;

                        if (obj is FileEntryViewModel fileVM)
                        {
                            oldName = fileVM.Name;
                        }
                        else if (obj is DirectoryEntryViewModel dirVM)
                        {
                            oldName = dirVM.Name;
                        }

                        if (!string.IsNullOrEmpty(oldName))
                        {
                            // 创建自定义弹窗
                            var dialog = new Window
                            {
                                Title = "重命名",
                                Width = 300,
                                Height = 150,
                                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                                ResizeMode = ResizeMode.NoResize
                            };

                            // 创建布局
                            var stackPanel = new StackPanel
                            {
                                Margin = new Thickness(10),
                                VerticalAlignment = VerticalAlignment.Center
                            };

                            // 第一行：提示文字
                            var textBlock = new TextBlock
                            {
                                Text = "请输入新名称:",
                                Margin = new Thickness(0, 0, 0, 10),
                                FontSize = 14
                            };

                            // 第二行：输入框
                            var textBox = new TextBox
                            {
                                Text = oldName,
                                FontSize = 14,
                                Margin = new Thickness(0, 0, 0, 15)
                            };
                            textBox.SelectAll();

                            // 第三行：按钮面板
                            var buttonPanel = new StackPanel
                            {
                                Orientation = Orientation.Horizontal,
                                HorizontalAlignment = HorizontalAlignment.Right
                            };

                            var okButton = new Button
                            {
                                Content = "确定",
                                Width = 80,
                                Margin = new Thickness(0, 0, 10, 0),
                                IsDefault = true
                            };

                            var cancelButton = new Button
                            {
                                Content = "取消",
                                Width = 80,
                                IsCancel = true
                            };

                            buttonPanel.Children.Add(okButton);
                            buttonPanel.Children.Add(cancelButton);

                            // 组装控件
                            stackPanel.Children.Add(textBlock);
                            stackPanel.Children.Add(textBox);
                            stackPanel.Children.Add(buttonPanel);
                            dialog.Content = stackPanel;

                            // 设置按钮行为
                            bool result = false;
                            okButton.Click += (s, e) =>
                            {
                                result = true;
                                dialog.Close();
                            };

                            cancelButton.Click += (s, e) => dialog.Close();

                            // 显示对话框
                            dialog.ShowDialog();

                            // 处理结果
                            if (result && !string.IsNullOrWhiteSpace(textBox.Text))
                            {
                                _fileSystemManager.RenameEntry(oldName, textBox.Text);
                                RefreshTree();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ShowError(ex.Message);
                    }
                }
            });
            // 切换目录命令：将当前目录切换到用户点击的目录
            ChangeDirCommand = new RelayCommand(obj =>
            {
                if (obj is DirectoryEntryViewModel dirVM)
                {
                    SelectedDirectory = dirVM;
                }
            });

            // 返回上一级目录命令
            UpDirCommand = new RelayCommand(_ =>
            {
                if (SelectedDirectory?.DirectoryEntry?.Parent != null)
                {
                    // 找到父目录的ViewModel并设置为当前选中
                    var parentDir = FindViewModelByDirectory(Root, SelectedDirectory.DirectoryEntry.Parent);
                    if (parentDir != null)
                        SelectedDirectory = parentDir;
                }
            });

            ReadFileCommand = new RelayCommand(obj =>
            {
                if (SelectedDirectory != null && obj is FileEntryViewModel fileVM)
                {
                    try
                    {
                        _fileSystemManager.CurrentDirectory = SelectedDirectory.DirectoryEntry;

                        // 1. 读取文件内容（使用现有ReadFile方法）
                        string content = _fileSystemManager.ReadFile(
                            SelectedDirectory.DirectoryEntry,
                            fileVM.Name,
                            _fileSystemManager.diskManager);

                        // 2. 处理null或空内容
                        if (content == null)
                        {
                            content = string.Empty;
                        }
                        else
                        {
                            // 3. 找到第一个null字符并截断
                            int nullIndex = content.IndexOf('\0');
                            if (nullIndex >= 0)
                            {
                                content = content.Substring(0, nullIndex);
                            }
                        }

                        // 4. 调试日志（确保不显示null）
                        Console.WriteLine($"处理后的内容: {content}");

                        // ---------- 以下保持原有界面代码不变 ----------
                        var editorWindow = new ICSharpCode.AvalonEdit.TextEditor
                        {
                            Text = content,
                            ShowLineNumbers = true,
                            WordWrap = true,
                            FontFamily = new System.Windows.Media.FontFamily("Consolas"),
                            FontSize = 12
                        };

                        var saveButton = new Button { Content = "保存", Width = 75, Height = 25, Margin = new Thickness(0, 0, 10, 0) };
                        var cancelButton = new Button { Content = "取消", Width = 75, Height = 25 };

                        var buttonPanel = new StackPanel
                        {
                            Orientation = Orientation.Horizontal,
                            HorizontalAlignment = HorizontalAlignment.Right,
                            Margin = new Thickness(10),
                            Children = { saveButton, cancelButton }
                        };

                        var mainGrid = new Grid();
                        mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                        mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                        mainGrid.Children.Add(editorWindow);
                        mainGrid.Children.Add(buttonPanel);
                        Grid.SetRow(editorWindow, 0);
                        Grid.SetRow(buttonPanel, 1);

                        var window = new Window
                        {
                            Title = $"编辑文件 - {fileVM.Name}",
                            Content = mainGrid,
                            Width = 800,
                            Height = 600,
                            WindowStartupLocation = WindowStartupLocation.CenterOwner
                        };

                        bool? dialogResult = null;
                        saveButton.Click += (s, e) => { dialogResult = true; window.Close(); };
                        cancelButton.Click += (s, e) => { dialogResult = false; window.Close(); };

                        window.ShowDialog();

                        if (dialogResult == true && content != editorWindow.Text)
                        {
                            try
                            {
                                byte[] data = Encoding.UTF8.GetBytes(editorWindow.Text);
                                _fileSystemManager.WriteFile(fileVM.FileEntry, data);
                                RefreshTree();
                            }
                            catch (Exception ex)
                            {
                                ShowError("保存文件失败: " + ex.Message);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ShowError("读取文件失败: " + ex.Message);
                    }
                }
            });
        }

        // 辅助函数：递归在树上查找指定目录的ViewModel
        private DirectoryEntryViewModel FindViewModelByDirectory(DirectoryEntryViewModel root, DirectoryEntry dir)
        {
            if (root.DirectoryEntry == dir)
                return root;
            foreach (var child in root.Children)
            {
                var found = FindViewModelByDirectory(child, dir);
                if (found != null) return found;
            }
            return null;
        }

        // 刷新目录树显示，并尽量保持原来的选中状态
        private void RefreshTree()
        {
            var path = GetPath(SelectedDirectory?.DirectoryEntry);

            // 清空并重建目录结构
            Root.Children.Clear();
            Root.Files.Clear();  // 确保先清空

            // 先处理目录
            foreach (var child in _fileSystemManager.Root.Children.OfType<DirectoryEntry>())
            {
                Root.Children.Add(new DirectoryEntryViewModel(child));
            }

            // 只处理纯文件（不是目录的）
            foreach (var file in _fileSystemManager.Root.Children.Where(c => !(c is DirectoryEntry)))
            {
                Root.Files.Add(new FileEntryViewModel((FileEntry)file));
            }

            // 恢复原来的选中目录
            var selectDir = Root;
            for (int i = 1; i < path.Count; i++)
            {
                selectDir = selectDir.Children.FirstOrDefault(x => x.Name == path[i]);
                if (selectDir == null) break;
            }
            SelectedDirectory = selectDir ?? Root;
            OnPropertyChanged(nameof(CurrentEntries));
        }

        // 辅助函数：获取某个目录到根目录的路径（用于刷新后恢复选中）
        private System.Collections.Generic.List<string> GetPath(DirectoryEntry dir)
        {
            var list = new System.Collections.Generic.List<string>();
            while (dir != null)
            {
                list.Add(dir.Name);
                dir = dir.Parent;
            }
            list.Reverse();
            return list;
        }

        // 辅助函数：弹出错误提示框
        private void ShowError(string msg)
        {
            System.Windows.MessageBox.Show(msg, "错误", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }

        // INotifyPropertyChanged接口实现，用于通知界面属性变更
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string prop) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }


    public class TypeToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string type)
            {
                return type == "文件夹" ? "📁" : "📄";
            }
            return "📄";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}