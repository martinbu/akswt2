using alarm_system;
using alarm_system_common;
using alarm_system_model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AlarmSystemInterface
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private AlarmSystem alarmSystem;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            alarmSystem = new AlarmSystemModel(TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(4), TimeSpan.FromSeconds(4), 3, 3);
            alarmSystem.StateChanged += alarmSystem_StateChanged;
            alarmSystem.MessageArrived += alarmSystem_MessageArrived;
            AlarmSystemState = alarmSystem.CurrentState.ToString();
            AlarmSystemPin = "####";
            NewAlarmSystemPin = "4321";
        }

        async void alarmSystem_MessageArrived(object sender, string message)
        {
            NewPinSet = message;
            await Task.Delay(TimeSpan.FromSeconds(2));
            NewPinSet = "";
        }

        void alarmSystem_StateChanged(object sender, StateChangedEventArgs e)
        {
            AlarmSystemState = e.NewStateType.ToString();
        }

        private void Open(object sender, RoutedEventArgs e)
        {
            alarmSystem.Open();
        }

        private void Close(object sender, RoutedEventArgs e)
        {
            alarmSystem.Close();
        }

        private void Lock(object sender, RoutedEventArgs e)
        {
            alarmSystem.Lock();
        }

        private void Unlock(object sender, RoutedEventArgs e)
        {
            alarmSystem.Unlock(AlarmSystemPin);
        }

        private string _AlarmSystemState;

        public string AlarmSystemState
        {
            get { return _AlarmSystemState; }
            set 
            {
                _AlarmSystemState = value;
                NotifyChange("AlarmSystemState");
            }
        }

        private string _AlarmSystemPin;

        public string AlarmSystemPin
        {
            get { return _AlarmSystemPin; }
            set
            { 
                _AlarmSystemPin = value;
                NotifyChange("AlarmSystemPin");
            }
        }

        private string _NewAlarmSystemPin;

        public string NewAlarmSystemPin
        {
            get { return _NewAlarmSystemPin; }
            set
            {
                _NewAlarmSystemPin = value;
                NotifyChange("NewAlarmSystemPin");
            }
        }

        private string _NewPinSet;

        public string NewPinSet
        {
            get { return _NewPinSet; }
            set 
            { 
                _NewPinSet = value;
                NotifyChange("NewPinSet");
            }
        }



        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyChange(String propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private void SetNewPinCode_Click(object sender, RoutedEventArgs e)
        {
            alarmSystem.SetPinCode(AlarmSystemPin, NewAlarmSystemPin);
        }
    }
}
