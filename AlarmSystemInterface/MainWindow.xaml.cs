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
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            alarmSystem = new AlarmSystemModel(2000, 4000, 5000);
            alarmSystem.StateChanged += alarmSystem_StateChanged;
            AlarmSystemState = alarmSystem.CurrentStateType.ToString();
        }

        void alarmSystem_StateChanged(object sender, StateChangedEventArgs e)
        {
            AlarmSystemState = e.NewStateType.ToString();
        }

        private AlarmSystem alarmSystem;

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
            alarmSystem.Unlock();
        }

        private String _AlarmSystemState;

        public String AlarmSystemState
        {
            get { return _AlarmSystemState; }
            set 
            {
                _AlarmSystemState = value;
                NotifyChange("AlarmSystemState");
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyChange(String propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
