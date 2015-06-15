using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace alarm_system_common
{
    public interface AlarmSystem
    {
        AlarmSystemState CurrentState { get; }

        event EventHandler<StateChangedEventArgs> StateChanged;

        void Open();

        void Close();

        void Lock();

        void Unlock(string pinCode);

        void ShutDown();
    }
}
