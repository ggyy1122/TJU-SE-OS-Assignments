using System;
using System.Collections.Generic;
using System.Text;

namespace MemoryManagement.Models
{
    internal class FIFOReplacer : IPageReplacer
    {
        private readonly Queue<int> _queue = new Queue<int>();

        public bool AccessPage(int page)
        {
            if (!_queue.Contains(page)) // 仅当页面不在队列中时加入
            {
                _queue.Enqueue(page);
                return false;
            }
            return true;
        }

        public int GetVictimPage()
        {
          
            return _queue.Dequeue(); // 移除并返回队列头的页面


        }
        public bool ContainPage(int page)
        {
            if (_queue.Contains(page)) 
            {
                return true;
            }
            return false;
        }
        public void PrintQueueElements(Action<string> logCallback)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("FIFO调度列:  ");

            foreach (var item in _queue)
            {
                sb.Append(item).Append("  ");
            }

            logCallback(sb.ToString().TrimEnd());


        }
    }
}