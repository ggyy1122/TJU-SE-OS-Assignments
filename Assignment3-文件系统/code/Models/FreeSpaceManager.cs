using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.Json;

namespace FileSystem.Models
{
    /// <summary>
    /// 空闲空间管理类 - 使用成组链接法实现
    /// 模拟磁盘块空间管理，管理空闲块链表，支持块的分配和释放
    /// </summary>
    internal class FreeSpaceManager
    {
        public int totalBlocks { get; set; }                       //总的磁盘块数
        public int groupSize { get; set; }                      //组的大小
        public Dictionary<int, List<int>> groupTable { get; set; } // 磁盘上其它管理块内容
        public int superBlockIndex { get; set; }             //超级块所在的磁盘块

        // 添加一个默认构造函数
        public FreeSpaceManager()
        {
        }
        public FreeSpaceManager(int totalBlocks)
        {
            // 可以在这里初始化默认值或保持为空
            this.totalBlocks = totalBlocks;
        }

        /// <summary>
        /// 初始化空闲空间管理器
        /// </summary>
        public void InitialFreeSpace()
        {
            totalBlocks = 10000;
            groupSize = 10;
            superBlockIndex = 0;//初始化函数，将0号块加载在超级块中
            groupTable = new Dictionary<int, List<int>>();
            //比如现在有20块 每组5块 那么0号管理块需要 5 1 2 3 4 5    1号管理块 5 6 7 8 9 10   6号管理块 5 11 12 13 14 15   11号管理块 4 16 17 18 19
            int cur = 1;
            int curStack = 0;
            int nextStack = 0;
            while (cur < totalBlocks)
            {
                int used = 0;//当前栈的填充量
                List<int> freeStack = new List<int>();   //当前管理块的栈部分
                freeStack.Add(0);
                while (used < groupSize && cur < totalBlocks)
                {

                    if (used == 0)//栈底元素
                    {
                        if (cur + groupSize > totalBlocks)//如果后面没有栈了
                        {
                            freeStack.Add(-1);
                            freeStack.Add(cur);
                            used++;
                        }
                        else
                        {
                            freeStack.Add(cur);
                            nextStack = cur;
                        }
                    }
                    else//不是栈底元素
                    {
                        freeStack.Add(cur);
                    }
                    freeStack[0]++;
                    used++;
                    cur++;
                }
                groupTable[curStack] = freeStack;
                curStack = nextStack;
                if (cur == totalBlocks && ((totalBlocks - 1) % groupSize == 0))
                {
                    List<int> tempStack = new List<int>();
                    tempStack.Add(0);
                    tempStack.Add(-1);
                    groupTable[curStack] = tempStack;
                }
            }
        }
        public void SaveToFile(string filePath)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    IncludeFields = true, // 显式包含字段
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase // 可选：统一命名风格
                };

                string jsonString = JsonSerializer.Serialize(this, options);
                File.WriteAllText(filePath, jsonString);
                Console.WriteLine($"状态已保存到 {filePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"保存失败: {ex.Message}");
            }
        }

        public static FreeSpaceManager LoadFromFile(string filePath)
        {
            try
            {
                string jsonString = File.ReadAllText(filePath);
                var options = new JsonSerializerOptions
                {
                    IncludeFields = true, // 显式包含字段
                    PropertyNameCaseInsensitive = true // 忽略大小写
                };

                var manager = JsonSerializer.Deserialize<FreeSpaceManager>(jsonString, options);

                // 验证必要字段
                if (manager?.groupTable == null)
                {
                    Console.WriteLine("警告：加载的状态数据不完整");
                    return null;
                }

                Console.WriteLine($"状态已从 {filePath} 加载");
                return manager;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"加载失败: {ex.Message}");
                return null;
            }
        }
        /// <summary>
        /// 打印当前 groupTable 中所有管理块及其空闲块信息
        /// </summary>
        public void PrintGroupTable()
        {
            Console.WriteLine("成组链接表内容：");
            foreach (var pair in groupTable)
            {
                Console.Write($"管理块 {pair.Key}: ");
                foreach (var block in pair.Value)
                {
                    Console.Write($"{block} ");
                }
                Console.WriteLine();
            }
        }

        /// <summary>
        /// 打印当前超级块的信息
        /// </summary>
        public void PrintSuperBlock()
        {
            if (superBlockIndex == -1)
            {
                Console.Write($"磁盘空间不足");
                return;
            }
            Console.Write($"超级块 {superBlockIndex}: ");
            foreach (var item in groupTable[superBlockIndex])
            {
                Console.Write($"{item} ");
            }
            Console.WriteLine();
        }

        /// <summary>
        /// 分配一个空闲磁盘块，返回块号，返回-1表示磁盘空间不足，无法分配
        /// </summary>
        public int AllocateBlock()
        {
            int A_Block = 0;
            //如果是没有空闲块了
            if (groupTable[superBlockIndex][1] == -1 && groupTable[superBlockIndex][0] == 0)
            {
                return -1;
            }
            //如果超级块空闲块有2个以上空闲盘
            if (groupTable[superBlockIndex][0] > 1)
            {
                A_Block = groupTable[superBlockIndex][groupTable[superBlockIndex].Count - 1];
                groupTable[superBlockIndex].RemoveAt(groupTable[superBlockIndex].Count - 1);//出栈操作
                groupTable[superBlockIndex][0]--;
                return A_Block;
            }
            //如果超级块只有一个空闲块
            if (groupTable[superBlockIndex][0] == 1)
            {
                if (groupTable[superBlockIndex][1] != -1)
                {
                    //把当前块分配掉
                    int temp = superBlockIndex;
                    int next = groupTable[superBlockIndex][1];
                    groupTable[superBlockIndex].RemoveAt(groupTable[superBlockIndex].Count - 1);//出栈操作
                    groupTable[superBlockIndex][0]--;
                    //把下一管理块请到超级块里面来
                    superBlockIndex = next;
                    groupTable.Remove(temp);

                    return temp;
                }
                else
                {
                    A_Block = groupTable[superBlockIndex][groupTable[superBlockIndex].Count - 1];
                    groupTable[superBlockIndex].RemoveAt(groupTable[superBlockIndex].Count - 1);//出栈操作
                    groupTable[superBlockIndex][0]--;
                    return A_Block;
                }
            }
            return -1;
        }

        /// <summary>
        /// 分配多个空闲磁盘
        /// </summary>
        public void AllocateMultipleBlocks(int count)
        {
            for (int i = 0; i < count; i++)
            {
                int block = AllocateBlock();
                if (block == -1)
                {
                    Console.WriteLine($"第{i + 1}次分配，磁盘空间不足");
                    break;
                }
                Console.WriteLine($"第{i + 1}次分配，分配的块号：{block}");
                PrintSuperBlock();
                PrintGroupTable();
                Console.WriteLine("-----------------------------------");
            }
        }

        /// <summary>
        /// 释放块
        /// </summary>
        public void FreeBlock(int blockNum)
        {
            int num = 0;
            if (groupTable[superBlockIndex][1] != -1)
                num = groupTable[superBlockIndex][0];
            else
                num = groupTable[superBlockIndex][0]+1;
            //如果超级块中可以存放
            if (num < groupSize)
            {
                groupTable[superBlockIndex].Add(blockNum);
                groupTable[superBlockIndex][0]++;
            }
            else//如果满了
            {
                int temp = superBlockIndex;
                superBlockIndex = blockNum;//当前块成为了管理块
                groupTable[superBlockIndex] = new List<int>();
                groupTable[superBlockIndex].Add(1);     //当前块中1个空闲块
                groupTable[superBlockIndex].Add(temp);  //当前块记录的空闲块是上一个被移走的超级块
            }
        }

        /// <summary>
        /// 交互测试程序
        /// </summary>
        public void InteractiveAllocateAndFree()
        {
            List<int> allocatedBlocks = new List<int>(); //记录已分配块

            Console.WriteLine("输入 'A' 分配一次，'B块号' 释放指定块，例如 B12，'Q' 退出。");
            Console.WriteLine("输入 'S' 保存当前状态，'L' 加载保存的状态");

            while (true)
            {
                Console.Write("请输入指令：");
                string input = Console.ReadLine().ToUpper();

                if (input == "A")
                {
                    int block = AllocateBlock();
                    if (block == -1)
                    {
                        Console.WriteLine("磁盘空间不足，无法分配。");
                    }
                    else
                    {
                        allocatedBlocks.Add(block);
                        Console.WriteLine($"分配的块号：{block}");
                    }
                    PrintSuperBlock();
                    PrintGroupTable();
                }
                else if (input.StartsWith("B"))
                {
                    // 尝试解析块号
                    if (input.Length > 1 && int.TryParse(input.Substring(1), out int blockToFree))
                    {
                        if (allocatedBlocks.Contains(blockToFree))
                        {
                            allocatedBlocks.Remove(blockToFree);
                            FreeBlock(blockToFree);
                            Console.WriteLine($"释放块号：{blockToFree}");
                            PrintSuperBlock();
                            PrintGroupTable();
                        }
                        else
                        {
                            Console.WriteLine($"块号 {blockToFree} 未被分配或不存在，无法释放。");
                        }
                    }
                    else
                    {
                        Console.WriteLine("释放格式错误，正确格式如 B12。");
                    }
                }
                else if (input == "S")
                {
                    Console.Write("请输入保存文件名（例如：freespace.dat）: ");
                    string fileName = Console.ReadLine();
                    SaveToFile(fileName);
                }
                else if (input == "L")
                {
                    Console.Write("请输入要加载的文件名（例如：freespace.dat）: ");
                    string fileName = Console.ReadLine();
                    var loadedManager = LoadFromFile(fileName);
                    if (loadedManager != null)
                    {
                        // 复制加载的状态到当前实例
                        this.totalBlocks = loadedManager.totalBlocks;
                        this.groupSize = loadedManager.groupSize;
                        this.groupTable = loadedManager.groupTable;
                        this.superBlockIndex = loadedManager.superBlockIndex;
                        Console.WriteLine("状态已成功加载");
                        PrintSuperBlock();
                        PrintGroupTable();
                    }
                }
                else if (input == "Q")
                {
                    Console.WriteLine("已退出。");
                    break;
                }
                else
                {
                    Console.WriteLine("无效指令，请输入 'A' / 'B块号' / 'S' / 'L' / 'Q'。");
                }

                Console.WriteLine("-----------------------------------");
            }
        }
    }
}