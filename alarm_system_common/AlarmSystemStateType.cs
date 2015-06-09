using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace alarm_system_common
{
    public enum AlarmSystemStateType
    {
        OpenAndUnlocked,
        ClosedAndUnlocked,
        OpenAndLocked,
        ClosedAndLocked,
        Armed,
        SilentAndOpen,
        AlarmFlashAndSound,
        AlarmFlash
    }
}
