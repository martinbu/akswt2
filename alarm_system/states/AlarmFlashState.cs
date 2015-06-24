using alarm_system_common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace alarm_system.states
{
    internal class AlarmFlashState : AlarmSystemStateBase
    {
        private readonly TimeSpan switchToSilentAndOpenTime;
        private CancellationTokenSource cancelAlarm = null;

        internal AlarmFlashState(Context context, TimeSpan switchToSilentAndOpenTime)
            : base(context, AlarmSystemState.AlarmFlash)
        {
            this.switchToSilentAndOpenTime = switchToSilentAndOpenTime;
            CancelAlarm();
        }

        internal override void Unlock(string pinCode)
        {
            if (Context.CheckPinCode(pinCode) == PinCheckResult.CORRECT)
            {
                CancelAlarm();
                base.ChangeStateTo(AlarmSystemState.OpenAndUnlocked);
            }
        }

        internal override void GotActive()
        {
            base.GotActive();
            Task.Factory.StartNew(turnOffSound, cancelAlarm.Token);
        }

        private async void turnOffSound()
        {
            try {
                await Task.Delay(switchToSilentAndOpenTime, cancelAlarm.Token);
                ChangeStateTo(AlarmSystemState.SilentAndOpen);
            }
            catch (TaskCanceledException) { }
            catch (ObjectDisposedException) { }
        }

        private void CancelAlarm()
        {
            if (cancelAlarm != null)
            {
                cancelAlarm.Cancel();
                cancelAlarm.Dispose();
            }

            cancelAlarm = new CancellationTokenSource();
        }

        internal override void ShutDown()
        {
            CancelAlarm();
        }
    }
}
