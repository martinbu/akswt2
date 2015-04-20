using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace alarm_system
{
    public interface AlarmSystemState
    {
        void Open();
        void Close();

        void Lock();
        void Unlock();
    }
}
