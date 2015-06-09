using alarm_system.states;
using alarm_system_common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace alarm_system
{
    public class AlarmSystemImpl : AlarmSystem, Context
    {

        private Dictionary<AlarmSystemStateType, AlarmSystemState> AlarmSystemStates { get; set; }

        private static List<AlarmSystem> initializedAlarmSystem = new List<AlarmSystem>();

        private readonly int switchToArmedTime = 2000;
        private readonly int switchToFlashTime = 4000;
        private readonly int switchToSilentAndOpenTime = 5000;

        public AlarmSystemImpl()
        {
            initialize();
        }

        public AlarmSystemImpl(int switchToArmedTime, int switchToFlashTime, int switchToSilentAndOpenTime)
        {
            this.switchToArmedTime = switchToArmedTime;
            this.switchToFlashTime = switchToFlashTime;
            this.switchToSilentAndOpenTime = switchToSilentAndOpenTime;

            initialize();
        }

        private void initialize() {

            ShutDownAll();
            
            AlarmSystemStates = new Dictionary<AlarmSystemStateType, AlarmSystemState>();
            AddState(new OpenAndUnlockedState(this));
            AddState(new OpenAndLockedState(this));
            AddState(new ClosedAndUnlockedState(this));
            AddState(new ClosedAndLockedState(this, switchToArmedTime));
            AddState(new ArmedState(this));
            AddState(new SilentAndOpenState(this));
            AddState(new AlarmFlashAndSoundState(this, switchToFlashTime));
            AddState(new AlarmFlashState(this, switchToSilentAndOpenTime));

            CurrentStateType = AlarmSystemStateType.OpenAndUnlocked;

            initializedAlarmSystem.Add(this);
        }

        public AlarmSystemStateType CurrentStateType { get; private set; }

        public void Open()
        {
            AlarmSystemStates[CurrentStateType].Open();
        }

        public void Close()
        {
            AlarmSystemStates[CurrentStateType].Close();
        }

        public void Lock()
        {
            AlarmSystemStates[CurrentStateType].Lock();
        }

        public void Unlock()
        {
            AlarmSystemStates[CurrentStateType].Unlock();
        }

        public void ChangeState(AlarmSystemStateType oldStateType, AlarmSystemStateType newStateType)
        {
            if (oldStateType == newStateType)
                return;

            CurrentStateType = newStateType;
            AlarmSystemStates[CurrentStateType].GotActive();

            if (StateChanged != null)
            {
                StateChanged(this, new StateChangedEventArgs(oldStateType, newStateType));
            }
        }



        private void AddState(AlarmSystemState alarmSystemState)
        {
            AlarmSystemStates.Add(alarmSystemState.StateType, alarmSystemState);
        }


        public event EventHandler<StateChangedEventArgs> StateChanged;


        public void ShutDown()
        {
            AlarmSystemStates.Values.ToList().ForEach(e => e.ShutDown());
        }

        public static void ShutDownAll()
        {
            //Console.WriteLine("Shut down '{0}' AlarmSystems", initializedAlarmSystem.Count);
            initializedAlarmSystem.ForEach(e => e.ShutDown());

            initializedAlarmSystem.Clear();
        }
    }
}
