using System.Windows;
using System.Windows.Controls;
using FileSystem.ViewModels;

namespace FileSystem.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (DataContext is MainViewModel viewModel && e.NewValue is DirectoryEntryViewModel dir)
            {
                viewModel.SelectedDirectory = dir;
            }
        }

        private void ListView_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (DataContext is MainViewModel viewModel)
            {
                if (viewModel.SelectedEntry is DirectoryEntryViewModel dir)
                {
                    // 如果是目录，使用原有的切换目录命令
                    viewModel.ChangeDirCommand.Execute(dir);
                }
                else if (viewModel.SelectedEntry is FileEntryViewModel file)
                {
                    // 如果是文件，执行读取命令
                    string header = $"Current Date and Time (UTC - YYYY-MM-DD HH:MM:SS formatted): {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}\n" +
                                  $"Current User's Login: {Environment.UserName}\n";

                    // 在执行读取命令时自动添加头部信息
                    if (viewModel.ReadFileCommand.CanExecute(file))
                    {
                        viewModel.ReadFileCommand.Execute(file);
                    }
                }
            }
        }
      

    }

}