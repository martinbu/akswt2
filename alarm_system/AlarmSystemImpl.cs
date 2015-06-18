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
        private readonly int ALLOWED_WRONG_PIN_CODE_COUNT = 3;
        private readonly int ALLOWED_WRONG_SET_PIN_CODE_COUNT = 3;

        private int wrongSetPinCodeCounter = 0;

        private string alarmSystemPinCode = "1234";

        #region initialization

        public AlarmSystemImpl()
        {
            Initialize();
        }

        public AlarmSystemImpl(int switchToArmedTime, int switchToFlashTime, 
            int switchToSilentAndOpenTime, int allowedWrongPinCodeCount, int allowedWrongSetPinCodeCount)
        {
            this.switchToArmedTime = switchToArmedTime;
            this.switchToFlashTime = switchToFlashTime;
            this.switchToSilentAndOpenTime = switchToSilentAndOpenTime;
            this.ALLOWED_WRONG_PIN_CODE_COUNT = allowedWrongPinCodeCount;
            this.ALLOWED_WRONG_SET_PIN_CODE_COUNT = allowedWrongSetPinCodeCount;

            Initialize();
        }

        private void Initialize() {

            ShutDownAll();
            
            AlarmSystemStates = new Dictionary<AlarmSystemState, AlarmSystemStateBase>();
            AddState(new OpenAndUnlockedState(this));
            AddState(new OpenAndLockedState(this));
            AddState(new ClosedAndUnlockedState(this));
            AddState(new ClosedAndLockedState(this, switchToArmedTime));
            AddState(new ArmedState(this, ALLOWED_WRONG_PIN_CODE_COUNT));
            AddState(new SilentAndOpenState(this));
            AddState(new AlarmFlashAndSoundState(this, switchToFlashTime));
            AddState(new AlarmFlashState(this, switchToSilentAndOpenTime));

            CurrentState = AlarmSystemState.OpenAndUnlocked;

            initializedAlarmSystem.Add(this);
        }

        #endregion


        #region AlarmSystem

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

        public void SetPinCode(string pinCode, string newPinCode)
        {
            AlarmSystemStates[CurrentState].SetPinCode(pinCode, newPinCode);
        }

        public event EventHandler<StateChangedEventArgs> StateChanged;
        public event EventHandler<string> MessageArrived;


        public void ShutDown()
        {
            AlarmSystemStates.Values.ToList().ForEach(e => e.ShutDown());
        }

        #endregion


        #region Context

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

        public void SendMessage(string message)
        {
            if (MessageArrived != null)
            {
                MessageArrived(this, message);
            }
        }

        PinCheckResult Context.CheckPinCode(string pinCode)
        {
            if (pinCode == this.alarmSystemPinCode)
                return PinCheckResult.CORRECT;

            return PinCheckResult.INCORRECT;
        }

        PinCheckResult Context.SetPinCode(string pinCode, string newPinCode)
        {
            if ((this as Context).CheckPinCode(pinCode) == PinCheckResult.CORRECT)
            {
                wrongSetPinCodeCounter = 0;
                this.alarmSystemPinCode = newPinCode;
                return PinCheckResult.CORRECT;
            }

            wrongSetPinCodeCounter++;

            if (wrongSetPinCodeCounter >= ALLOWED_WRONG_SET_PIN_CODE_COUNT)
                return PinCheckResult.ALARM;

            return PinCheckResult.INCORRECT;
        }

        #endregion

        private void AddState(AlarmSystemStateBase alarmSystemState)
        {
            AlarmSystemStates.Add(alarmSystemState.StateType, alarmSystemState);
        }

        public static void ShutDownAll()
        {
            //Console.WriteLine("Shut down '{0}' AlarmSystems", initializedAlarmSystem.Count);
            initializedAlarmSystem.ForEach(e => e.ShutDown());

            initializedAlarmSystem.Clear();
        }
    }
}
