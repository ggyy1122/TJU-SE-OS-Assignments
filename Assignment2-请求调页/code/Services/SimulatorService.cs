using System;
using MemoryManagement.Models;
namespace MemoryManagement.Services
{
    internal class SimulatorService
    {
        public  MemoryManager _manager;                                        //内存管理
        private int _currentInstruction;                                       //当前指令的逻辑地址
        private int _MemoryFrameCount;                                         //内存页面数
        public int _TaskFrameCount;                                           //任务页面数
        public int _pageSize;                                                 //页面大小，10条指令
        public int InstructionCount;
        public IPageReplacer _replacer;
        private readonly Random random = new Random();
        public int defalutPageCount = 0;
        //构造函数,默认参数
        public SimulatorService()
        {
            InstructionCount = 0;
            _pageSize = 10;
            _TaskFrameCount=32;
            _MemoryFrameCount = 4;
            _currentInstruction = 0;
            _replacer = new LRUReplacer();
            defalutPageCount = 0;
            _manager = new MemoryManager(
            MemoryFrameCount: 4,    // 4个物理页框
             TaskFrameCount: 32,      // 32个逻辑页
             pageSize: 10            // 每页10条指令
             );
        }
        //重置函数
        public void reset(int PageSize,int taskFrameCount,int memoryFrameCount,int currentInstruction, IPageReplacer replacer)
        {
            defalutPageCount = 0;
            InstructionCount = 0;
            _pageSize = PageSize;
            _TaskFrameCount = taskFrameCount;
            _MemoryFrameCount = memoryFrameCount;
            _currentInstruction = 0;
            _replacer = replacer;
           _manager = new MemoryManager(
            MemoryFrameCount: memoryFrameCount,    // 物理页框
             TaskFrameCount: taskFrameCount,      // 逻辑页
             pageSize: PageSize            // 每页指令条数

             );
        }
        public void switchReplacer(IPageReplacer re)
        {
            _replacer = re;
        }
        //单步执行函数,执行x处的指令
        public event Action<string>? OnLog;  // 新增日志事件

        private void Log(string message)
        {
            OnLog?.Invoke($"{message}");
        }

        //string address, bool isPageFault, string swappedOut, string swappedIn
        public void Step(int x, Action<int, int, int, int> loadPageCallback = null, Action<int, bool, int, int> logCallback = null)
        {
            _currentInstruction = x;
            InstructionCount++;
            bool Default;
            int swapOut = -1;
            int swapIn = -1;
            // Log($"[执行指令 {x}]");

            int inPage = _manager._pageTable[x].First;
            // Log($"[所属页 {inPage}]");
            if (_manager._physicalMemory.Contains(inPage))
            {
                _replacer.AccessPage(inPage);
                // Log($"命中: 页{inPage}已在物理内存");
                Default = false;
               
            }
            else
            {
                defalutPageCount++;
                _replacer.AccessPage(inPage);
                Default = true;
                // Log($"缺页: 页{inPage}未加载");
                int memoryframeIndex = 0;
                if (_manager._physicalMemory.Count < _MemoryFrameCount)
                {
                    _manager.addPage(inPage);
                    memoryframeIndex = _manager._physicalMemory.Count - 1;
                    // Log($"已换入页{inPage}到框{_manager._physicalMemory.Count - 1}");
                    swapIn = inPage;
                }
                else
                {
                    int outPage = _replacer.GetVictimPage();
                     memoryframeIndex = _manager.swapPage(outPage, inPage);
                    //  Log($"置换: 换出框{memoryframeIndex}的页{outPage} → 换入页{inPage}");
                    swapIn = inPage;
                    swapOut = outPage;
                }
                // 触发页面加载回调（新增的核心逻辑）
                if (loadPageCallback != null)
                {
                    loadPageCallback(memoryframeIndex, inPage, inPage*_pageSize, inPage * _pageSize+_pageSize-1);
                }
            }
            // Log("当前物理内存：[" +
            //string.Join(", ", _manager._physicalMemory) + "]");
            // 触发日志回调
            if (logCallback != null)
            {
                logCallback(_currentInstruction, Default,swapOut,swapIn);
            }
        }
        //获得下一地址
        public int getRandomNext(int cur)
        {
            double rand = random.NextDouble();
            int totalInstructions = _TaskFrameCount * _pageSize;
            int next = cur;

            // 顺序执行 (50%概率)
            if (rand < 0.5)
            {
                next = (cur + 1) % totalInstructions;
            }
            else
            {
                int maxJump = totalInstructions / 2; // 最大跳转量
                int dx = random.Next(1, maxJump);    // 1~maxJump-1的随机跳转量

                // 25%概率向后跳
                if (rand < 0.75)
                {
                    next = (cur + dx) % totalInstructions;
                }
                // 25%概率向前跳
                else
                {
                    next = (cur - dx + totalInstructions) % totalInstructions;
                }
            }

            // 确保不返回当前指令
            return (next == cur) ? (next + 1) % totalInstructions : next;
        }
        public int getSequenceNext(int cur)
        {
            return cur+1;
        }
        public bool IsExecutionComplete()
        {
            if(InstructionCount>=_pageSize*_TaskFrameCount)
            return true;
            return false;
        }

    }
}
