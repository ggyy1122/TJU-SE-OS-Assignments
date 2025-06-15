using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Documents;
using MemoryManagement.Models;
using MemoryManagement.Services;
using System.Windows.Controls.Primitives;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Net;

namespace MemoryManagement
{
    public class LogEntry
    {
        public int Index { get; set; }
        public string Address { get; set; }
        public string PageFault { get; set; }
        public string SwappedOut { get; set; }
        public string SwappedIn { get; set; }
    }
    public partial class MainWindow : Window
    {
        private SimulatorService simulator;
        private int _currentInstruction = 0;
        private Dictionary<int, List<int>> memoryFrameContents = new Dictionary<int, List<int>>();
        private int order=1;//0是顺序 1是乱序
        public MainWindow()
        {
            InitializeComponent();
            LogListView.ItemsSource = _logEntries;
            simulator = new SimulatorService();
            //simulator.OnLog += Simulator_OnLog;
            LoadParameters();
        }

    

        private void ReadParameters_Click(object sender, RoutedEventArgs e)
        {
            LoadParameters();
            MessageBox.Show("参数已更新", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private void ResetParameters_Click(object sender, RoutedEventArgs e)
        {
            // 重置所有参数为默认值
            PageSizeBox.Text = "10";
            TaskFrameCountBox.Text = "32";
            MemoryFrameCountBox.Text = "4";
            CurrentInstructionBox.Text = "0";
            ReplacerComboBox.SelectedIndex = 0;
            ExecutionOrderComboBox.SelectedIndex = 0;
            PageFaultCountBox.Text = "0";
            PageFaultRateBox.Text = "0";
            LoadParameters();
            MessageBox.Show("参数已重置", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private void LoadParameters()
        {
            int pageSize = TryParseOrDefault(PageSizeBox.Text, 10);
            int taskFrameCount = TryParseOrDefault(TaskFrameCountBox.Text, 32);
            int memoryFrameCount = TryParseOrDefault(MemoryFrameCountBox.Text, 4);
            _currentInstruction = TryParseOrDefault(CurrentInstructionBox.Text, 0);
            string replacerName = ((ComboBoxItem)ReplacerComboBox.SelectedItem).Content.ToString();
            string selectedOrder = ((ComboBoxItem)ExecutionOrderComboBox.SelectedItem).Content.ToString();

            order = selectedOrder == "顺序执行"
                ? 0  // 顺序执行：当前指令+1
                : 1; // 乱序执行：调用随机方法

            IPageReplacer replacer = replacerName switch
            {
                "FIFO" => new FIFOReplacer(),
                "LRU" => new LRUReplacer(),
                _ => new FIFOReplacer(),
            };

            simulator.reset(pageSize, taskFrameCount, memoryFrameCount, _currentInstruction, replacer);
            LoadMemoryFrames(memoryFrameCount);
        }

        private int TryParseOrDefault(string input, int defaultValue)
        {
            return int.TryParse(input, out int value) ? value : defaultValue;
        }

        private Dictionary<int, TextBlock> loadedPageLabels = new Dictionary<int, TextBlock>(); // 新增：保存"页面Y"标签的引用

        private void LoadMemoryFrames(int memoryFrameCount)
        {
            MemoryFramePanel.Children.Clear();
            memoryFrameContents.Clear();
            loadedPageLabels.Clear();

            for (int i = 0; i < memoryFrameCount; i++)
            {
                memoryFrameContents[i] = new List<int>();

                var scrollViewer = new ScrollViewer
                {
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                    HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
                };

                var uniformGrid = new UniformGrid
                {
                    Columns = 1,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                };

                scrollViewer.Content = uniformGrid;

                // 创建双行标题区域（完全保持原样）
                var headerStack = new StackPanel
                {
                    Orientation = Orientation.Vertical,
                    Background = Brushes.WhiteSmoke,
                    Children =
            {
                new TextBlock
                {
                    Text = $"内存页{i}",
                    FontSize = 12,
                    Foreground = Brushes.DarkSlateBlue,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 2, 0, 0)
                },
                new TextBlock
                {
                    Name = $"pageLabel_{i}",
                    Text = "",
                    FontSize = 11,
                    FontStyle = FontStyles.Italic,
                    Foreground = Brushes.DarkOrange,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 4)
                }
            }
                };

                loadedPageLabels[i] = (TextBlock)headerStack.Children[1];

                // 仅修改边框部分：添加圆角和阴影
                var border = new Border
                {
                    BorderBrush = Brushes.Gray,
                    BorderThickness = new Thickness(1),
                    Margin = new Thickness(5),
                    Background = Brushes.White,
                    CornerRadius = new CornerRadius(4), // 添加圆角
                    Effect = new System.Windows.Media.Effects.DropShadowEffect // 添加阴影
                    {
                        Color = Colors.LightGray,
                        BlurRadius = 4,
                        ShadowDepth = 1,
                        Opacity = 0.3
                    },
                    Child = new StackPanel
                    {
                        Orientation = Orientation.Vertical,
                        Children = { headerStack, scrollViewer } // 保持原结构
                    }
                };

                MemoryFramePanel.Children.Add(border);
            }
        }
        // 新增方法：更新内存框中加载的页面号
        public void UpdateLoadedPageNumber(int frameIndex, int pageNumber)
        {
           if( loadedPageLabels.TryGetValue(frameIndex, out var label))
            {
                label.Text = $"页面{pageNumber}";
            }
        }
        /*
        private void AddLog(string message)
        {
            Paragraph paragraph = new Paragraph(new Run($"{message}"));
            LogTextBox.Document.Blocks.Add(paragraph);
            LogTextBox.ScrollToEnd();
        }*/
        private ObservableCollection<LogEntry> _logEntries = new ObservableCollection<LogEntry>();
        private int _logIndex = 1;

        // 更新缺页统计信息
        private void UpdatePageFaultStats(int pageFaultCount, int totalInstructions)
        {
            // 更新缺页数
            PageFaultCountBox.Text = pageFaultCount.ToString();

            // 计算并更新缺页率（百分比）
            double faultRate = totalInstructions > 0 ? (double)pageFaultCount / totalInstructions * 100 : 0;
            PageFaultRateBox.Text = $"{faultRate:F2}%";  // 保留两位小数
        }
        private void AddLog(int address, bool isPageFault, int swappedOut, int swappedIn)
        {
            Dispatcher.Invoke(() =>
            {
                _logEntries.Add(new LogEntry
                {
                    Index = _logIndex++,
                    Address = address.ToString(),
                    PageFault = isPageFault ? "是" : "否",
                    SwappedOut = swappedOut >= 0 ? swappedOut.ToString() : "-",
                    SwappedIn = swappedIn >= 0 ? swappedIn.ToString() : "-"
                });

                // 自动滚动到最后
                LogListView.ScrollIntoView(_logEntries.Last());
            });
        }

        // 修改ClearLog_Click
        private void ClearLog_Click(object sender, RoutedEventArgs e)
        {
            _logEntries.Clear();
            _logIndex = 1;
        }
        private bool _isAutoRunning = false;
        private CancellationTokenSource _cts;

        private async void AutoRun_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;

            // 停止逻辑
            if (_isAutoRunning)
            {
                _cts?.Cancel();
                button.Content = "连续执行";
                return;
            }

            // 启动逻辑
            _isAutoRunning = true;
            button.Content = "停止执行";
            _cts = new CancellationTokenSource();

            try
            {
                while (!_cts.Token.IsCancellationRequested)
                {
                    // 关键点：直接调用原有方法
                    Dispatcher.Invoke(() => StepExecute_Click(sender, e));

                    // 自动停止检测
                    if (simulator.IsExecutionComplete()) {
                        endAction();
                        break; }

                    await Task.Delay(100, _cts.Token); // 控制执行速度
                }
            }
            catch (TaskCanceledException) { /* 正常停止 */ }
            finally
            {
                _isAutoRunning = false;
                button.Content = "连续执行";
                _cts?.Dispose();
            }
        }
        private void StepExecute_Click(object sender, RoutedEventArgs e)
        {
            ResetFrameColors();
            // 执行当前指令
            simulator.Step(_currentInstruction, LoadPageToMemoryFrame, AddLog);


            // 更新所有内存页的高亮状态
            RefreshHighlightInAllMemoryFrames();

            // 打印替换算法状态
          //  simulator._replacer.PrintQueueElements(AddLog);
            // 获取下一条指令
            if (order == 1)
                _currentInstruction = simulator.getRandomNext(_currentInstruction);
            else
                _currentInstruction = simulator.getSequenceNext(_currentInstruction);

            CurrentInstructionBox.Text = _currentInstruction.ToString();
            UpdatePageFaultStats(simulator.defalutPageCount, simulator.InstructionCount);
            // 自动检测完成条件
            if (simulator.IsExecutionComplete()&&!_isAutoRunning)
            {
                endAction();
            }
        }
        private void endAction()
        {
            MessageBox.Show(
            "所有指令执行完成！\n模拟器将自动重置。",  // 提示内容
            "执行完成",                            // 标题
            MessageBoxButton.OK,                  // 按钮
            MessageBoxImage.Information          // 图标
        );

            CurrentInstructionBox.Text = "0";
            PageFaultCountBox.Text = "0";
            PageFaultRateBox.Text = "0";
            LoadParameters();
            
        }
        private void LoadPageToMemoryFrame(int memoryFrameIndex, int taskFrameIndex, int startInstruction, int endInstruction)
        {
            if (memoryFrameIndex >= MemoryFramePanel.Children.Count)
                return;

            var border = (Border)MemoryFramePanel.Children[memoryFrameIndex];

            border.Background = Brushes.GreenYellow;

            // 原有逻辑保持不变...
            memoryFrameContents[memoryFrameIndex] = Enumerable.Range(startInstruction, endInstruction - startInstruction + 1).ToList();
            var stackPanel = (StackPanel)border.Child;
            var scrollViewer = (ScrollViewer)stackPanel.Children[1];
            var uniformGrid = (UniformGrid)scrollViewer.Content;
            UpdateLoadedPageNumber(memoryFrameIndex, taskFrameIndex);
            uniformGrid.Children.Clear();

            uniformGrid.Columns = 1;
            uniformGrid.Rows = 0;
            uniformGrid.Margin = new Thickness(5, 0, 5, 0);

            for (int instruction = startInstruction; instruction <= endInstruction; instruction++)
            {
                var btn = CreateInstructionButton(instruction);
                btn.Margin = new Thickness(2, 4, 2, 4);
                btn.HorizontalAlignment = HorizontalAlignment.Stretch;
                uniformGrid.Children.Add(btn);
            }
        }
        private void ResetFrameColors()
        {
            foreach (var child in MemoryFramePanel.Children)
            {
                if (child is Border border)
                {
                    border.Background = Brushes.White; // 直接恢复为白色
                }
            }
        }
        private Button CreateInstructionButton(int instruction)
        {
            // 1. 创建基础按钮
            var btn = new Button
            {
                Content = instruction.ToString(),
                Tag = instruction,
                Margin = new Thickness(2),
                Padding = new Thickness(5),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                IsHitTestVisible = false,
                Background = Brushes.LightYellow, // 使用系统预定义颜色测试
                BorderBrush = Brushes.LightGray,
                BorderThickness = new Thickness(1),
                Foreground = Brushes.Black,
                MinWidth = 40,
                MinHeight = 30,
                FontSize = 10
            };

            // 2. 移除所有可能的样式干扰
            btn.Style = null;
            btn.Template = null;

            // 3. 创建确保显示颜色的圆角模板
            var template = new ControlTemplate(typeof(Button));

            var border = new FrameworkElementFactory(typeof(Border));
            border.Name = "border";
            border.SetValue(Border.BackgroundProperty, new TemplateBindingExtension(Button.BackgroundProperty));
            border.SetValue(Border.BorderBrushProperty, new TemplateBindingExtension(Button.BorderBrushProperty));
            border.SetValue(Border.BorderThicknessProperty, new TemplateBindingExtension(Button.BorderThicknessProperty));
            border.SetValue(Border.CornerRadiusProperty, new CornerRadius(4));

            var content = new FrameworkElementFactory(typeof(ContentPresenter));
            content.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            content.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
            content.SetValue(ContentPresenter.ContentProperty, new TemplateBindingExtension(Button.ContentProperty));

            border.AppendChild(content);
            template.VisualTree = border;

            // 4. 应用模板
            btn.Template = template;

            // 5. 强制重绘
            btn.InvalidateVisual();
            btn.InvalidateArrange();

            UpdateButtonHighlight(btn, instruction);
            return btn;
        }

        private void UpdateButtonHighlight(Button btn, int instruction)
        {
            if (instruction == _currentInstruction)
            {
                btn.Background = Brushes.Yellow;
                btn.FontWeight = FontWeights.Bold;
                btn.Foreground = Brushes.Red;
            }
            else
            {
                // 修改这里：恢复为初始颜色而不是固定白色
                btn.Background = Brushes.LightYellow; // 与Create方法中的初始颜色一致
                btn.FontWeight = FontWeights.Normal;
                btn.Foreground = Brushes.Black;
            }
        }

        private void RefreshHighlightInAllMemoryFrames()
        {
            foreach (var frame in memoryFrameContents)
            {
                int frameIndex = frame.Key;
                var instructions = frame.Value;

                if (frameIndex >= MemoryFramePanel.Children.Count || instructions.Count == 0)
                    continue;

                var border = (Border)MemoryFramePanel.Children[frameIndex];
                var stackPanel = (StackPanel)border.Child;
                var scrollViewer = (ScrollViewer)stackPanel.Children[1];
                var uniformGrid = (UniformGrid)scrollViewer.Content;

                foreach (var child in uniformGrid.Children)
                {
                    if (child is Button btn && btn.Tag is int inst)
                    {
                        UpdateButtonHighlight(btn, inst);
                    }
                }
            }
        }
        private void LogListView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (LogListView.View is GridView gridView)
            {
                double scrollBarWidth = 18; // 留个 18 像素的滚动条宽度
                double totalWidth = LogListView.ActualWidth - scrollBarWidth;

                if (totalWidth > 0)
                {
                    double colWidth = totalWidth / gridView.Columns.Count;
                    foreach (var column in gridView.Columns)
                    {
                        column.Width = colWidth;
                    }
                }
            }
        }





    }
}