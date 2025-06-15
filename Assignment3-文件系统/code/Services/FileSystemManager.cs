
using System.Text;
using System.Xml.Linq;
using System.IO;
using FileSystem.Models;



namespace FileSystem.Services
{
    /// <summary>
    /// 文件管理器  用有Root这个数据结构对象管理目录
    /// 同时有CurrentDirectory记录当前文件
    /// </summary>
    public class FileSystemManager
    {
        const string spaceManagerFile = "freespace_state.json";           // 用于存储空闲空间管理器状态
        const string fileManagerFile = "file_state.json";                // 用于目录状态
        public int BlockSize { get; } = 1024 * 4;                           //块大小4KB
        public int PointerSize { get; } = 4;                              //指针4B
        public int totalBlocks = 10000;                                      //假设有10000

        public DiskManager diskManager;                                    //虚拟磁盘
        public DirectoryEntry Root { get; private set; }                   //根文件夹目录
        public DirectoryEntry CurrentDirectory { get;  set; }        //当前文件夹
        private FreeSpaceManager spaceManager;                                //空间管理器
       

        /// <summary>
        /// 文件管理器的构造函数
        /// </summary>
        public FileSystemManager()
        {
            // 初始化根目录，根目录名通常是 "/"
            Root = new DirectoryEntry("root");
            diskManager = new DiskManager();
            CurrentDirectory = Root;
            // 初始化空间管理器，传入总块数
            spaceManager = new FreeSpaceManager(totalBlocks);
            if (File.Exists(spaceManagerFile))
            {
                var loadedSpaceManager = FreeSpaceManager.LoadFromFile(spaceManagerFile);
                if (loadedSpaceManager != null)
                {
                    spaceManager = loadedSpaceManager;
                    Console.WriteLine($"空闲空间状态已从 {spaceManagerFile} 加载。");
                }
                else
                {
                    spaceManager.InitialFreeSpace(); // 加载失败时初始化默认状态
                    Console.WriteLine("加载空闲空间状态失败，已初始化默认状态。");
                }
            }
            else
            {
                spaceManager.InitialFreeSpace(); // 文件不存在时初始化默认状态
                Console.WriteLine("未找到空闲空间状态文件，已初始化默认状态。");
            }
            //初始化目录
            if (File.Exists(fileManagerFile))
            {
                try
                {
                    var loadedRoot = DirectoryEntry.LoadFromFile(fileManagerFile);
                   if (loadedRoot != null)
                    {
                        Root = loadedRoot;
                        CurrentDirectory = Root;
                        Console.WriteLine($"目录状态已从 {fileManagerFile} 加载。");
                    }
                    else
                    {
                        Console.WriteLine("加载目录状态失败，已初始化空目录。");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"加载目录状态出错: {ex.Message}，已初始化空目录。");
                }
            }
            else
            {
                Console.WriteLine("未找到目录状态文件，已初始化空目录。");
            }
        }

        /// <summary>
        /// 创建目录,返回新创建的目录对象
        /// </summary>
        public DirectoryEntry CreateDirectory(string name)
        {
            // 检查当前目录下是否有重名
            if (CurrentDirectory.Children.Any(e => e.Name == name))
            {
                throw new Exception("目录已存在");
            }

            var newDir = new DirectoryEntry(name);
            CurrentDirectory.AddChild(newDir);
            Root.SaveToFile(fileManagerFile);
            Console.WriteLine($"目录状态已保存到 {fileManagerFile}。");
            return newDir;
        }

        /// <summary>
        /// 递归删除目录及其所有子项（文件和目录）
        /// </summary>
        public void DeleteDirectory(string name)
        {
            // 找目录，注意是 DirectoryEntry 类型
            DirectoryEntry dirToDelete = CurrentDirectory.Children
                          .OfType<DirectoryEntry>()  // 只找目录
                          .FirstOrDefault(e => e.Name == name);
            if (dirToDelete == null)
            {
                Console.WriteLine($"目录 {name} 不存在。");
                return;
            }

            // 递归删除目录内容
            DeleteDirectoryRecursive(dirToDelete);

            // 删除目录本身
            CurrentDirectory.RemoveChild(dirToDelete);

            Console.WriteLine($"目录 {name} 删除成功。");
            Root.SaveToFile(fileManagerFile);
            Console.WriteLine($"目录状态已保存到 {fileManagerFile}。");
        }

        // 递归删除目录里的所有内容
        private void DeleteDirectoryRecursive(DirectoryEntry dir)
        {
            // 复制一份避免修改集合时出错
            var childrenCopy = dir.Children.ToList();

            foreach (var child in childrenCopy)
            {
                if (child.Type == EntryType.Directory)  // 统一用 EntryType 字段名
                {
                    // 递归删除子目录
                    DeleteDirectoryRecursive((DirectoryEntry)child);
                    dir.RemoveChild(child);
                }
                else
                {
                    // 删除文件，传当前目录对象
                    DeleteFile(child.Name, dir);
                    dir.RemoveChild(child);
                }
            }
        }






        /// <summary>
        /// 创建文件
        /// </summary>
        // 只创建文件，不分配数据块（大小默认为0）
        public FileEntry CreateFile(string name)
        {
            // 重名校验
            if (CurrentDirectory.Children.Any(e => e.Name == name))
            {
                throw new Exception("文件已存在");
            }

            // 分配一个索引块
            int indexBlockId = spaceManager.AllocateBlock();
            diskManager.ClearBlock(indexBlockId);//新增
            if (indexBlockId == -1)
                throw new Exception("空间不足");
            Console.WriteLine($"分配索引块{indexBlockId}");
            spaceManager.PrintSuperBlock();
            spaceManager.PrintGroupTable();
            Console.WriteLine();

            var indexBlock = new IndexBlock(BlockSize, PointerSize, indexBlockId);

            // 文件初始大小为0，无数据块指针

            var fileEntry = new FileEntry(name, EntryType.File)
            {
                Size = 0,
                IndexBlock = indexBlock
            };

            CurrentDirectory.AddChild(fileEntry);
            //记录到文件
            spaceManager.SaveToFile(spaceManagerFile);
            Console.WriteLine($"空闲空间状态已保存到 {spaceManagerFile}。");
            Root.SaveToFile(fileManagerFile);
            Console.WriteLine($"目录状态已保存到 {fileManagerFile}。");
            return fileEntry;
        }


        // 写入数据，分配数据块，实现写时复制
        public void WriteFile(FileEntry fileEntry, byte[] data)
        {
            // 计算需要多少块
            int requiredBlocks = (data.Length + BlockSize - 1) / BlockSize;

            // 先分配新的数据块
            List<int> newDataBlocks = new List<int>();
            try
            {
                for (int i = 0; i < requiredBlocks; i++)
                {
                    int blockId = spaceManager.AllocateBlock();
                    Console.WriteLine($"分配数据块{blockId}");
                    spaceManager.PrintSuperBlock();
                    spaceManager.PrintGroupTable();
                    Console.WriteLine();
                    if (blockId == -1)
                    {
                        throw new Exception("空间不足");
                    }
                    newDataBlocks.Add(blockId);
                }

                // 把数据写入这些磁盘
                for (int i = 0; i < requiredBlocks; i++)
                {
                    int start = i * BlockSize;
                    int length = Math.Min(BlockSize, data.Length - start);
                    byte[] chunk = new byte[BlockSize];
                    Array.Copy(data, start, chunk, 0, length);
                    Console.WriteLine($"写入磁盘{newDataBlocks[i]}");
                    diskManager.WriteBlock(newDataBlocks[i], chunk);
                }
                int index_id = fileEntry.IndexBlock.indexBlockId;
                // 回收索引块的旧指针
                for (int i = 0; i < diskManager.PointerCount(index_id); i++)
                {
                    int oldBlockId = diskManager.GetPointer(index_id, i);
                    if (oldBlockId != -1)
                    {
                        spaceManager.FreeBlock(oldBlockId);
                        Console.WriteLine($"释放索引块指针{oldBlockId}");
                        spaceManager.PrintSuperBlock();
                        spaceManager.PrintGroupTable();
                        Console.WriteLine();
                    }
                }

                // 更新索引块指针
                diskManager.ClearBlock(index_id);
                foreach (var blockId in newDataBlocks)
                {
                    diskManager.AddPointer(index_id, blockId);
                }

                //记录到文件
                fileEntry.Size = data.Length;
                fileEntry.ModifiedTime = DateTime.Now;
                spaceManager.SaveToFile(spaceManagerFile);
                Console.WriteLine($"空闲空间状态已保存到 {spaceManagerFile}。");
                Root.SaveToFile(fileManagerFile);
                Console.WriteLine($"目录状态已保存到 {fileManagerFile}。");

            }
            catch (Exception)
            {
                // 出错时释放新分配的块
                foreach (var blockId in newDataBlocks)
                {
                    spaceManager.FreeBlock(blockId);
                }
                throw;
            }
        }


        /// <summary>
        /// 删除文件
        /// </summary>
        public void DeleteFile(string name,DirectoryEntry dir)
        {
            // 找文件
            var fileEntry = dir.Children.FirstOrDefault(e => e.Name == name && e.Type == EntryType.File);
            if (fileEntry == null)
            {
                throw new Exception("文件不存在或不是文件");
            }

            // 先释放数据块
            int index_id = fileEntry.IndexBlock.indexBlockId;
           // if (indexBlock != null)
            {
                for (int i = 0; i < diskManager.PointerCount(index_id); i++)
                {
                    int blockId = diskManager.GetPointer(index_id, i);//indexBlock.GetPointer(i);
                    if (blockId != -1)
                    {
                        spaceManager.FreeBlock(blockId);
                        diskManager.ClearBlock(blockId);
                        Console.WriteLine($"释放数据块{blockId}");
                        spaceManager.PrintSuperBlock();
                        spaceManager.PrintGroupTable();
                        Console.WriteLine();
                    }
                }

                // 释放索引块
                spaceManager.FreeBlock(index_id);
                diskManager.ClearBlock(index_id);
                Console.WriteLine($"释放索引块{index_id}");
                spaceManager.PrintSuperBlock();
                spaceManager.PrintGroupTable();
                Console.WriteLine();

            }

            // 从当前目录移除文件条目
            CurrentDirectory.RemoveChild(fileEntry);

            Console.WriteLine($"文件 {name} 删除成功。");

            //记录到文件
            spaceManager.SaveToFile(spaceManagerFile);
            Console.WriteLine($"空闲空间状态已保存到 {spaceManagerFile}。");
            Root.SaveToFile(fileManagerFile);
            Console.WriteLine($"目录状态已保存到 {fileManagerFile}。");
        }



        /// <summary>
        /// 命令行函数，采用命令行方式可视化文件系统
        /// </summary>
        public void RunCommandLine()
        {
            Console.WriteLine("欢迎使用简易文件系统命令行！输入 exit 退出。");
            Console.WriteLine("======== 虚拟文件系统命令帮助 ========");
            Console.WriteLine("  mkdir 目录名          —— 创建目录");
            Console.WriteLine("  touch 文件名          —— 创建文件");
            Console.WriteLine("  write 文件名 内容     —— 写文件");
            Console.WriteLine("  readfile 文件名       —— 读文件");
            Console.WriteLine("  rm 文件名             —— 删除文件");
            Console.WriteLine("  listinfo 名称         —— 文件信息");
            Console.WriteLine("  rename 原名 新名      —— 重命名");
            Console.WriteLine("  rmdir 目录名          —— 删除目录及其内容");
            Console.WriteLine("  ls                    —— 列出当前目录内容");
            Console.WriteLine("  cd 目录名             —— 进入目录（支持..返回上级）");
            Console.WriteLine("  pwd                   —— 显示当前路径");
            Console.WriteLine("  testwrite             —— 长文本写入测试");
            Console.WriteLine("  exit                  —— 退出命令行");
            Console.WriteLine("======================================");
            Console.WriteLine();

            try
            {
                while (true)
                {
                    Console.Write($"{GetCurrentPath()}> ");
                    var input = Console.ReadLine();
                    if (input == null) continue;

                    var args = input.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (args.Length == 0) continue;

                    try
                    {
                        switch (args[0].ToLower())
                        {
                            case "exit":
                                Console.WriteLine("退出文件系统。");
                                return;

                            case "mkdir":
                                if (args.Length < 2)
                                {
                                    Console.WriteLine("用法: mkdir 目录名");
                                    break;
                                }
                                CreateDirectory(args[1]);
                                Console.WriteLine($"目录 {args[1]} 创建成功。");
                                break;
                            case "listinfo":
                                if (args.Length == 1)
                                {
                                    // 无参数，列出当前目录下所有内容
                                    Console.WriteLine("类型\t名称\t\t修改时间\t\t\t大小");
                                    foreach (var entry in CurrentDirectory.Children)
                                    {
                                        string type = entry.Type == EntryType.Directory ? "文件夹" : "文件";
                                        string sizeStr = entry.Type == EntryType.File ? entry.Size.ToString() : "";
                                        Console.WriteLine($"{type}\t{entry.Name}\t\t{entry.ModifiedTime:yyyy-MM-dd HH:mm:ss}\t{sizeStr}");
                                    }
                                }
                                else
                                {
                                    // 有参数，只显示自己
                                    string name = args[1];
                                    var entry = CurrentDirectory.Children.FirstOrDefault(e => e.Name == name);
                                    if (entry == null)
                                    {
                                        Console.WriteLine($"未找到名为 {name} 的文件或目录");
                                        break;
                                    }
                                    string type = entry.Type == EntryType.Directory ? "文件夹" : "文件";
                                    string sizeStr = entry.Type == EntryType.File ? entry.Size.ToString() : "";
                                    Console.WriteLine("类型\t名称\t\t修改时间\t\t\t大小");
                                    Console.WriteLine($"{type}\t{entry.Name}\t\t{entry.ModifiedTime:yyyy-MM-dd HH:mm:ss}\t{sizeStr}");
                                }
                                break;

                            case "touch":
                                if (args.Length < 2)
                                {
                                    Console.WriteLine("用法: touch 文件名");
                                    break;
                                }
                                try
                                {
                                    CreateFile(args[1]);  // 不需要指定大小，创建空文件
                                    Console.WriteLine($"文件 {args[1]} 创建成功。");
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine("创建失败：" + ex.Message);
                                }
                                break;

                            case "write":
                                if (args.Length < 3)
                                {
                                    Console.WriteLine("用法: write 文件名 内容");
                                    break;
                                }
                                try
                                {
                                    string fileName = args[1];
                                    string content = string.Join(" ", args.Skip(2)); // 支持写入带空格的内容

                                    var fileEntry = CurrentDirectory.Children
                                        .FirstOrDefault(f => f.Name == fileName && f.Type == EntryType.File);

                                    if (fileEntry == null)
                                    {
                                        Console.WriteLine("文件不存在");
                                        break;
                                    }

                                    byte[] data = Encoding.UTF8.GetBytes(content);
                                    WriteFile(fileEntry, data);

                                    Console.WriteLine($"写入文件 {fileName} 成功，大小 {data.Length} 字节。");
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine("写入失败：" + ex.Message);
                                }
                                break;
                            case "rename":
                                if (args.Length < 3)
                                {
                                    Console.WriteLine("用法: rename 旧名 新名");
                                    break;
                                }
                                try
                                {
                                    string oldName = args[1];
                                    string newName = args[2];

                                    // 找到要重命名的条目（文件或目录）
                                    var entry = CurrentDirectory.Children.FirstOrDefault(e => e.Name == oldName);
                                    if (entry == null)
                                    {
                                        Console.WriteLine($"未找到名为 {oldName} 的文件或目录");
                                        break;
                                    }
                                    // 检查重名
                                    if (CurrentDirectory.Children.Any(e => e.Name == newName))
                                    {
                                        Console.WriteLine($"已经存在名为 {newName} 的文件或目录");
                                        break;
                                    }
                                    entry.Name = newName;

                                    // 保存更改
                                    Root.SaveToFile(fileManagerFile);
                                    Console.WriteLine($"{oldName} 已成功重命名为 {newName}");
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine("重命名失败：" + ex.Message);
                                }
                                break;

                            case "rm":
                                if (args.Length < 2)
                                {
                                    Console.WriteLine("用法: rm 文件名");
                                    break;
                                }
                                try
                                {
                                    DeleteFile(args[1], CurrentDirectory);
                                    Console.WriteLine($"文件 {args[1]} 删除成功。");
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"删除文件失败: {ex.Message}");
                                }
                                break;

                            case "rmdir":
                                if (args.Length < 2)
                                {
                                    Console.WriteLine("用法: rmdir 目录名");
                                    break;
                                }
                                try
                                {
                                    DeleteDirectory(args[1]);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"删除目录失败: {ex.Message}");
                                }
                                break;

                            case "ls":
                                foreach (var child in CurrentDirectory.Children)
                                {
                                    string type = child.Type == EntryType.Directory ? "<DIR>" : "<FILE>";
                                    Console.WriteLine($"{type}\t{child.Name}");
                                }
                                break;
                            case "readfile":
                                if (args.Length < 2)
                                {
                                    Console.WriteLine("用法: readfile 文件名");
                                    break;
                                }
                                try
                                {
                                    string content = ReadFile(CurrentDirectory, args[1], diskManager);
                                    Console.WriteLine(content);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"读取文件失败: {ex.Message}");
                                }
                                break;

                            case "cd":
                                if (args.Length < 2)
                                {
                                    Console.WriteLine("用法: cd 目录名");
                                    break;
                                }
                                ChangeDirectory(args[1]);
                                break;

                            case "pwd":
                                Console.WriteLine(GetCurrentPath());
                                break;
                            case "testwrite":
                                try
                                {
                                    TestLongWrite();
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"测试写入失败: {ex.Message}");
                                }
                                break;


                            default:
                                Console.WriteLine("未知命令。支持命令: mkdir, touch, ls, cd, pwd, exit");
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"错误: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"发生未处理的异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 切换目录
        /// </summary>
        private void ChangeDirectory(string dirName)
        {
            if (dirName == "..")
            {
                if (CurrentDirectory.Parent != null)
                {
                    CurrentDirectory = CurrentDirectory.Parent;
                }
                else
                {
                    Console.WriteLine("已经是根目录。");
                }
            }
            else
            {
                var target = CurrentDirectory.Children
                    .OfType<DirectoryEntry>()
                    .FirstOrDefault(d => d.Name == dirName);

                if (target == null)
                {
                    Console.WriteLine($"目录 {dirName} 不存在。");
                    return;
                }
                CurrentDirectory = target;
            }
        }

        /// <summary>
        /// 获取目录
        /// </summary>
        public string GetCurrentPath()
        {
            List<string> names = new List<string>();
            DirectoryEntry dir = CurrentDirectory;
            while (dir != null)
            {
                names.Add(dir.Name);
                dir = dir.Parent;
            }
            names.Reverse();
            return "root/" + string.Join("/", names.Skip(1)); // 跳过根目录名“root”，显示为 /xxx/xxx
        }

        /// <summary>
        /// 读文件
        /// </summary>
        public string ReadFile(DirectoryEntry dir, string name, DiskManager disk)
        {
            // 找文件条目
            var fileEntry = dir.Children.FirstOrDefault(e => e.Name == name && e.Type == EntryType.File);
            if (fileEntry == null)
            {
                throw new Exception("文件不存在或不是文件");
            }

            // 找到索引块id
            int index_id = fileEntry.IndexBlock.indexBlockId;

            // 获取索引块指针数量
            int pointerCount = disk.PointerCount(index_id);

            // 用列表缓存所有数据块字节内容
            List<byte> fileData = new List<byte>();

            for (int i = 0; i < pointerCount; i++)
            {
                int dataBlockId = disk.GetPointer(index_id, i);  // 取出数据块id

                byte[] blockData = disk.ReadBlock(dataBlockId);  // 读数据块

                fileData.AddRange(blockData);  // 拼接数据
            }

            // 把所有数据块字节拼接成字符串返回
            string content = Encoding.UTF8.GetString(fileData.ToArray());

            return content;
        }
        /// <summary>
        /// 重命名目录或文件
        /// </summary>
        /// <param name="oldName">原来的名字</param>
        /// <param name="newName">新名字</param>
        public void RenameEntry(string oldName, string newName)
        {
            if (string.IsNullOrWhiteSpace(newName))
                throw new ArgumentException("新名称不能为空");

            // 找到要重命名的条目（文件或目录）
            var entry = CurrentDirectory.Children.FirstOrDefault(e => e.Name == oldName);
            if (entry == null)
                throw new Exception($"未找到名为 {oldName} 的文件或目录");

            // 检查是否重名
            if (CurrentDirectory.Children.Any(e => e.Name == newName))
                throw new Exception($"已经存在名为 {newName} 的文件或目录");

            entry.Name = newName;

            // 持久化保存
            Root.SaveToFile(fileManagerFile);
            Console.WriteLine($"{oldName} 已成功重命名为 {newName}");
        }
        public class EntryInfo
        {
            public string Name { get; set; }
            public string Type { get; set; }  // "文件" 或 "文件夹"
            public DateTime ModifiedTime { get; set; }
            public long? Size { get; set; }   // 文件显示大小，文件夹为null
        }

        public List<EntryInfo> GetCurrentDirectoryInfos()
        {
            var result = new List<EntryInfo>();
            foreach (var entry in CurrentDirectory.Children)
            {
                result.Add(new EntryInfo
                {
                    Name = entry.Name,
                    Type = entry.Type == EntryType.Directory ? "文件夹" : "文件",
                    ModifiedTime = entry.ModifiedTime,
                    Size = entry.Type == EntryType.File ? (long?)entry.Size : null
                });
            }
            return result;
        }

        public void TestLongWrite()
        {
            string fileName = "testfile.txt";

            // 如果文件不存在，先创建
            var fileEntry = CurrentDirectory.Children
                .FirstOrDefault(f => f.Name == fileName && f.Type == EntryType.File);
            if (fileEntry == null)
            {
                CreateFile(fileName);
                fileEntry = CurrentDirectory.Children
                    .FirstOrDefault(f => f.Name == fileName && f.Type == EntryType.File);
            }

            // 生成超长内容
            string content = new string('A', 5000); // 比如5000字节，看分几块
            byte[] data = Encoding.UTF8.GetBytes(content);

            // 写入文件
            WriteFile(fileEntry, data);

            Console.WriteLine($"写入文件 {fileName} 成功，大小 {data.Length} 字节。");
        }


    }
}
