using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace MemoryManagement.Models
{
    internal class LRUReplacer : IPageReplacer
    {
        private readonly LinkedList<int> _accessOrder = new LinkedList<int>();
        private readonly Dictionary<int, LinkedListNode<int>> _pageMap = new Dictionary<int, LinkedListNode<int>>();

        public bool AccessPage(int page)
        {
            if (_pageMap.ContainsKey(page))
            {
                // 如果页面已存在，将其移到链表尾部（表示最近使用）
                _accessOrder.Remove(_pageMap[page]);
                _accessOrder.AddLast(_pageMap[page]);
                return true;
            }
            else
            {
                // 如果页面不存在，添加到链表尾部
                var node = _accessOrder.AddLast(page);
                _pageMap[page] = node;
                return false;
            }
        }

        public int GetVictimPage()
        {
            // 移除并返回链表头部的页面（最久未使用）
            var victim = _accessOrder.First.Value;
            _accessOrder.RemoveFirst();
            _pageMap.Remove(victim);

            var newQueue = new Queue<int>();
            return victim;
        }

        public bool ContainPage(int page)
        {
            return _pageMap.ContainsKey(page);
        }
        public void PrintQueueElements(Action<string> logCallback)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("LRU调度列:  ");
            foreach (var item in _accessOrder)
            {
                sb.Append(item).Append(" ");
            }
            string output = sb.ToString().TrimEnd();
            logCallback(output);
        }
    }
}