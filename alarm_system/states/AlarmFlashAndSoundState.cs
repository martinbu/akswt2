using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace alarm_system.states
{
    internal class AlarmFlashAndSoundState : AlarmSystemState
    {
        internal AlarmFlashAndSoundState(Context context)
            : base(context, AlarmSystemStateType.AlarmFlashAndSound)
        {
            CancelAlarm();
        }

        private CancellationTokenSource cancelAlarm = null;

        internal override void Unlock()
        {
            CancelAlarm();
            base.ChangeStateTo(AlarmSystemStateType.OpenAndUnlocked);
        }

        internal override void GotActive()
        {
            base.GotActive();
            Task.Factory.StartNew(turnOffSound, cancelAlarm.Token);
        }

        private async void turnOffSound()
        {
            try {
                await Task.Delay(TimeSpan.FromSeconds(30), cancelAlarm.Token);
                ChangeStateTo(AlarmSystemStateType.AlarmFlash);
            }
            catch (TaskCanceledException) { }
        }

        private void CancelAlarm() {
            if (cancelAlarm != null)
            {
                cancelAlarm.Cancel();
                cancelAlarm.Dispose();
            }
            
            cancelAlarm = new CancellationTokenSource();
        }
    }
}
