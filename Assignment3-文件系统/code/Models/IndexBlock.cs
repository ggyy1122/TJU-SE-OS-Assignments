using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace FileSystem.Models
{
    /// <summary>
    /// 索引块的数据结构
    /// </summary>
    public class IndexBlock
    {
        public int BlockSize { get; private set; }         // 单块大小，字节
        public int PointerSize { get; private set; }       // 指针大小，字节
        public int MaxPointers { get; private set; }       // 最大指针数
        private int[] DataBlockPointers;                    // 数据块指针数组
        public int UsedCount { get; private set; }         // 已用指针数
        [JsonProperty("indexBlockId")]
        public int indexBlockId { get; private set; }         // 索引块的块号

        /// <summary>
        /// 构造函数
        /// </summary>
        public IndexBlock(int blockSize, int pointerSize,int id)
        {
            indexBlockId = id;
            BlockSize = blockSize;
            PointerSize = pointerSize;
            MaxPointers = BlockSize / PointerSize;
            DataBlockPointers = new int[MaxPointers];
            for (int i = 0; i < MaxPointers; i++)
            {
                DataBlockPointers[i] = -1;  // -1 表示空闲指针
            }
            UsedCount = 0;
        }

        /// <summary>
        /// 添加一个数据块指针
        /// </summary>
        public bool AddPointer(int blockNumber)
        {
            if (UsedCount >= MaxPointers)
                return false;

            for (int i = 0; i < MaxPointers; i++)
            {
                if (DataBlockPointers[i] == -1)
                {
                    DataBlockPointers[i] = blockNumber;
                    UsedCount++;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 移除一个数据块指针
        /// </summary>
        public bool RemovePointer(int blockNumber)
        {
            for (int i = 0; i < MaxPointers; i++)
            {
                if (DataBlockPointers[i] == blockNumber)
                {
                    DataBlockPointers[i] = -1;
                    UsedCount--;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 获取指定索引的数据块指针
        /// </summary>
        public int GetPointer(int index)
        {
            if (index < 0 || index >= MaxPointers)
                throw new IndexOutOfRangeException("索引块指针索引超出范围");

            return DataBlockPointers[index];
        }

        /// <summary>
        /// 是否已满
        /// </summary>
        public bool IsFull()
        {
            return UsedCount >= MaxPointers;
        }

        /// <summary>
        /// 获取当前有效指针数
        /// </summary>
        public int PointerCount()
        {
            return UsedCount;
        }
        /// <summary>
        /// 清空所有数据块指针
        /// </summary>
        public void ClearPointers()
        {
            for (int i = 0; i < MaxPointers; i++)
            {
                DataBlockPointers[i] = -1;
            }
            UsedCount = 0;
        }

    }

}
