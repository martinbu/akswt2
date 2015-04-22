using alarm_system.states;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace alarm_system
{
    public interface AlarmSystem
    {
        AlarmSystemStateType CurrentStateType { get; }

        event EventHandler<StateChangedEventArgs> StateChanged;

        void Open();

        void Close();

        void Lock();

        void Unlock();

        void ShutDown();
    }
}
