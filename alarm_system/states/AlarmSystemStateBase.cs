using alarm_system_common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace alarm_system.states
{
    internal abstract class AlarmSystemStateBase
    {

        protected Context Context { get; private set; }

        public AlarmSystemState StateType { get; private set; }

        protected AlarmSystemStateBase(Context context, AlarmSystemState stateType)
        {
            this.Context = context;
            this.StateType = stateType;
        }

        protected void ChangeStateTo(AlarmSystemState newStateType) {
            Context.ChangeState(this.StateType, newStateType);
        }

        protected void ChangeStateToWithPin(AlarmSystemState newStateType, string pinCode)
        {
            if (Context.checkPinCode(pinCode) == PinCheckResult.CORRECT)
            {
                Context.ChangeState(this.StateType, newStateType);
            }
        }

        internal virtual void GotActive()
        {
            //Console.WriteLine(this.GetType().Name + " is active now!");
        }

        internal virtual void Open()
        {
            //Console.WriteLine(this.GetType().Name + " has no Open Command. Ignore.");
        }

        internal virtual void Close()
        {
            //Console.WriteLine(this.GetType().Name + " has no Close Command. Ignore.");
        }

        internal virtual void Lock()
        {
            //Console.WriteLine(this.GetType().Name + " has no Lock Command. Ignore.");
        }

        internal virtual void Unlock(string pin)
        {
            //Console.WriteLine(this.GetType().Name + " has no Unlock Command. Ignore.");
        }

        internal virtual void ShutDown()
        {
        }
    }
}
