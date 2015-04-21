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
        }

        private CancellationTokenSource cancelAlarm = new CancellationTokenSource();

        internal override void Unlock()
        {
            cancelAlarm.Cancel();
            base.ChangeStateTo(AlarmSystemStateType.OpenAndUnlocked);
        }

        internal override void GotActive()
        {
            base.GotActive();
            Task.Factory.StartNew(turnOffSound, cancelAlarm.Token);
        }

        private async void turnOffSound()
        {
            await Task.Delay(TimeSpan.FromSeconds(30));
            ChangeStateTo(AlarmSystemStateType.AlarmFlash);
        }
    }
}
