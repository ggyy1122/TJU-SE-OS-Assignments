using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoryManagement.Models
{
    public interface IPageReplacer
    {
  
        bool AccessPage(int page);
        int GetVictimPage();
        bool ContainPage(int page);
        public void PrintQueueElements(Action<string> logCallback)
        {
           
        }
        //void Reset();
    }
}
