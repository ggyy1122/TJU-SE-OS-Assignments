
namespace MemoryManagement.Models
{


    internal class MemoryManager
    {
        public  int _MemoryFrameCount;                                         //内存页面数
        public  int _TaskFrameCount;                                           //任务页面数
        public  int _pageSize;                                                 //页面大小，10条指令
     
        public  List<int> _physicalMemory = new List<int>();                   //内存页框列表，假设内存可以加载4页，那可能是[page1，page2，page3，page4]
        public int _physicalMemoryCount = 0;
        public  Dictionary<int, (int First, int Second)> _pageTable= new Dictionary<int, (int, int)>() ;//页表
        public int PageFaultCount { get; private set; } = 0;                            //缺页计数器
        //构造函数
        public MemoryManager(int MemoryFrameCount,int TaskFrameCount, int pageSize)
        {
            Reset(MemoryFrameCount, TaskFrameCount, pageSize); // 初始化状态
        }
    


        // 重置所有状态
        public void Reset(int MemoryFrameCount, int TaskFrameCount, int pageSize)
        {
            _physicalMemoryCount = 0;
            _MemoryFrameCount = MemoryFrameCount;
            _TaskFrameCount = TaskFrameCount;
            _pageSize = pageSize;
           _pageTable =new  Dictionary<int, (int First, int Second)>();
            calculatePageTable();
           
            //replacer.Reset();
        }
        //计算页表映射
        public void calculatePageTable()
        {
            for(int i=0;i<_TaskFrameCount*_pageSize;i++)
            {
                _pageTable[i] = (i/_pageSize,i%_pageSize);
            }
        }
        //加入页面
        public void addPage(int pageNumber)
        {
            if (_physicalMemory.Count >= _MemoryFrameCount)
                throw new InvalidOperationException("物理内存已满，无法换入");
            _physicalMemory.Add(pageNumber);
        }
        //置换页面
        public int swapPage(int oldNumber,int newNumber)
        {
            int index = 0;
            for(int i=0;i<_physicalMemory.Count;i++)
            {
                if (oldNumber == _physicalMemory[i])
                {
                    _physicalMemory[i] = newNumber;
                    index = i;
                }
            }
            return index;
        }
     
    }
}
