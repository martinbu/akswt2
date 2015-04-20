using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace alarm_system
{
    class UnsupportedStateActionException : Exception
    {
        public UnsupportedStateActionException(AlarmSystemState state, String action) : 
            base("Action '" + action + "' not supported by state '" + state.GetType().Name + "'")
        {
        }
    }
}
