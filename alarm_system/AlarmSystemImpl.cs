using alarm_system.states;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace alarm_system
{
    public class AlarmSystemImpl : AlarmSystem
    {

        public AlarmSystemImpl()
        {
            State = new OpenAndUnlockedState();
        }

        public AlarmSystemState State { get; set; }

        public void Open()
        {
            State.Open();
        }

        public void Close()
        {
            State.Close();
        }

        public void Lock()
        {
            State.Lock();
        }

        public void Unlock()
        {
            State.Unlock();
        }
    }
}
