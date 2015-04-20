using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace alarm_system.states
{
    class OpenAndLockedState : AlarmSystemState
    {
        public void Open()
        {
            throw new UnsupportedStateActionException(this, "Open");
        }

        public void Close()
        {
            throw new UnsupportedStateActionException(this, "Close");
        }

        public void Lock()
        {
            throw new UnsupportedStateActionException(this, "Lock");
        }

        public void Unlock()
        {
            throw new UnsupportedStateActionException(this, "Unlock");
        }
    }
}
