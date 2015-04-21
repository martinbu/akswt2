using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace alarm_system.states
{
    internal abstract class AlarmSystemState
    {

        protected Context Context { get; private set; }

        public AlarmSystemStateType StateType { get; private set; }

        protected AlarmSystemState(Context context, AlarmSystemStateType stateType)
        {
            this.Context = context;
            this.StateType = stateType;
        }

        protected void ChangeStateTo(AlarmSystemStateType newStateType) {
            Context.ChangeState(this.StateType, newStateType);
        }

        internal virtual void GotActive()
        {
            Console.WriteLine(this.GetType().Name + " is active now!");
        }

        internal virtual void Open()
        {
            Console.WriteLine(this.GetType().Name + " has no Open Command. Ignore.");
        }

        internal virtual void Close()
        {
            Console.WriteLine(this.GetType().Name + " has no Close Command. Ignore.");
        }

        internal virtual void Lock()
        {
            Console.WriteLine(this.GetType().Name + " has no Lock Command. Ignore.");
        }

        internal virtual void Unlock()
        {
            Console.WriteLine(this.GetType().Name + " has no Unlock Command. Ignore.");
        }
    }
}
