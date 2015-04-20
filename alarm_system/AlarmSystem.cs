using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace alarm_system
{
    public interface AlarmSystem : AlarmSystemState
    {
        AlarmSystemState State { get; }
    }
}
