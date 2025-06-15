using System;
using System.IO;

namespace FileSystem.Services
{
    public class DiskManager : IDisposable
    {
        private const int BlockSize = 4096;       // 4KB 每块大小
        private const int BlockCount = 10000;     // 总块数
        private readonly string diskFilePath = "disk.dat";

        private FileStream diskStream;

        /// <summary>
        /// 磁盘管理器的构造函数
        /// </summary>
        public DiskManager()
        {
            // 打开或创建磁盘文件
            if (!File.Exists(diskFilePath))
            {
                // 创建新文件，并初始化大小
                using (var fs = new FileStream(diskFilePath, FileMode.CreateNew, FileAccess.ReadWrite))
                {
                    fs.SetLength(BlockSize * BlockCount); // 设置文件大小
                }
            }

            diskStream = new FileStream(diskFilePath, FileMode.Open, FileAccess.ReadWrite);
        }

        /// <summary>
        /// 读取指定块的数据，返回4KB字节数组
        /// </summary>
        public byte[] ReadBlock(int blockIndex)
        {
            if (blockIndex < 0 || blockIndex >= BlockCount)
                throw new ArgumentOutOfRangeException(nameof(blockIndex), "块索引超出范围");

            byte[] buffer = new byte[BlockSize];
            diskStream.Seek(blockIndex * BlockSize, SeekOrigin.Begin);
            int bytesRead = diskStream.Read(buffer, 0, BlockSize);
            if (bytesRead < BlockSize)
            {
                // 如果读不到完整块，用0填充剩余部分
                for (int i = bytesRead; i < BlockSize; i++) buffer[i] = 0;
            }
            return buffer;
        }


        /// <summary>
        /// 向指定块写入数据，data长度不能超过4KB
        /// </summary>
        public void WriteBlock(int blockIndex, byte[] data)
        {
            if (blockIndex < 0 || blockIndex >= BlockCount)
                throw new ArgumentOutOfRangeException(nameof(blockIndex), "块索引超出范围");

            if (data == null) throw new ArgumentNullException(nameof(data));
            if (data.Length > BlockSize)
                throw new ArgumentException($"数据大小不能超过 {BlockSize} 字节", nameof(data));

            diskStream.Seek(blockIndex * BlockSize, SeekOrigin.Begin);
            diskStream.Write(data, 0, data.Length);

            // 如果写入数据小于4KB，剩余部分用0补齐（保持块完整）
            if (data.Length < BlockSize)
            {
                byte[] zeroFill = new byte[BlockSize - data.Length];
                diskStream.Write(zeroFill, 0, zeroFill.Length);
            }
            diskStream.Flush();
        }
        /// <summary>
        /// 获取块中已存储的指针数量（每个指针 4 字节，0 表示空）
        /// </summary>
        public int PointerCount(int blockIndex)
        {
            var block = ReadBlock(blockIndex);
            int count = 0;
            for (int i = 0; i < BlockSize; i += 4)
            {
                int pointer = BitConverter.ToInt32(block, i);
                if (pointer == 0)
                    break;
                count++;
            }
            return count;
        }

        /// <summary>
        /// 获取块中第 i 个指针的值
        /// </summary>
        public int GetPointer(int blockIndex, int i)
        {
            if (i < 0 || i >= 1024)
                throw new ArgumentOutOfRangeException(nameof(i), "指针索引超出范围");
            var block = ReadBlock(blockIndex);
            return BitConverter.ToInt32(block, i * 4);
        }

        /// <summary>
        /// 向块中追加一个 4 字节的指针（非 0 值）
        /// </summary>
        public void AddPointer(int blockIndex, int pointerValue)
        {
           
            var block = ReadBlock(blockIndex);
            for (int i = 0; i < BlockSize; i += 4)
            {
                int value = BitConverter.ToInt32(block, i);
                if (value == 0)
                {
                    Array.Copy(BitConverter.GetBytes(pointerValue), 0, block, i, 4);
                    WriteBlock(blockIndex, block);
                    return;
                }
            }
            throw new InvalidOperationException("该块已满，无法再添加指针");
        }

        /// <summary>
        /// 清空块中所有内容
        /// </summary>
        public void ClearBlock(int blockIndex)
        {
            byte[] emptyBlock = new byte[BlockSize];
            WriteBlock(blockIndex, emptyBlock);
        }

        // 关闭流释放资源
        public void Dispose()
        {
            diskStream?.Dispose();
        }
    }
}
