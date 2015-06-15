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

        private Dictionary<AlarmSystemState, AlarmSystemStateBase> AlarmSystemStates { get; set; }

        private static List<AlarmSystem> initializedAlarmSystem = new List<AlarmSystem>();

        private readonly int switchToArmedTime = 2000;
        private readonly int switchToFlashTime = 4000;
        private readonly int switchToSilentAndOpenTime = 5000;

        private string alarmSystemPinCode = "1234";

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
            
            AlarmSystemStates = new Dictionary<AlarmSystemState, AlarmSystemStateBase>();
            AddState(new OpenAndUnlockedState(this));
            AddState(new OpenAndLockedState(this));
            AddState(new ClosedAndUnlockedState(this));
            AddState(new ClosedAndLockedState(this, switchToArmedTime));
            AddState(new ArmedState(this));
            AddState(new SilentAndOpenState(this));
            AddState(new AlarmFlashAndSoundState(this, switchToFlashTime));
            AddState(new AlarmFlashState(this, switchToSilentAndOpenTime));

            CurrentState = AlarmSystemState.OpenAndUnlocked;

            initializedAlarmSystem.Add(this);
        }

        public AlarmSystemState CurrentState { get; private set; }

        public void Open()
        {
            AlarmSystemStates[CurrentState].Open();
        }

        public void Close()
        {
            AlarmSystemStates[CurrentState].Close();
        }

        public void Lock()
        {
            AlarmSystemStates[CurrentState].Lock();
        }

        public void Unlock(string pinCode)
        {
            AlarmSystemStates[CurrentState].Unlock(pinCode);
        }

        public void ChangeState(AlarmSystemState oldStateType, AlarmSystemState newStateType)
        {
            if (oldStateType == newStateType)
                return;

            CurrentState = newStateType;
            AlarmSystemStates[CurrentState].GotActive();

            if (StateChanged != null)
            {
                StateChanged(this, new StateChangedEventArgs(oldStateType, newStateType));
            }
        }

        PinCheckResult Context.checkPinCode(string pinCode)
        {
            if (pinCode == this.alarmSystemPinCode)
                return PinCheckResult.CORRECT;

            return PinCheckResult.INCORRECT;
        }


        private void AddState(AlarmSystemStateBase alarmSystemState)
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
