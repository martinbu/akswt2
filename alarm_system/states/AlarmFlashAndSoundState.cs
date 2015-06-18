using alarm_system_common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace alarm_system.states
{
    internal class AlarmFlashAndSoundState : AlarmSystemStateBase
    {
        private readonly int switchToFlashTime;

        internal AlarmFlashAndSoundState(Context context, int switchToFlashTime)
            : base(context, AlarmSystemState.AlarmFlashAndSound)
        {
            this.switchToFlashTime = switchToFlashTime;
            CancelAlarm();
        }

        private CancellationTokenSource cancelAlarm = null;

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
                await Task.Delay(TimeSpan.FromMilliseconds(switchToFlashTime), cancelAlarm.Token);
                ChangeStateTo(AlarmSystemState.AlarmFlash);
            }
            catch (TaskCanceledException) { }
            catch (ObjectDisposedException) { }
        }

        private void CancelAlarm() {
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
