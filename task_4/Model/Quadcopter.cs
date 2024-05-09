using System.ComponentModel;
using System.Runtime.CompilerServices;
using task_4.shared;

namespace task_4.Model
{
    public class Quadcopter : INotifyPropertyChanged
    {
        private static int COUNTER;
        private static Semaphore loadingOnPolarStation = new(AppConfiguration.Instance.MAXIMUM_NUMBER_QUADCOPTERS_SERVICED, AppConfiguration.Instance.MAXIMUM_NUMBER_QUADCOPTERS_SERVICED);
        private static Semaphore loadingOnPort = new(AppConfiguration.Instance.MAXIMUM_NUMBER_QUADCOPTERS_SERVICED, AppConfiguration.Instance.MAXIMUM_NUMBER_QUADCOPTERS_SERVICED);
        public enum State
        {
            PREFLYING_PREPARING_WAITING,
            PREFLYING_PREPARING,
            READY_TO_FLY,
            TAKING_OFF,
            TRAVELLING,
            BROKEN,
            MECHANIC_WAITING,
            REPAIRING,
            LANDING
        }
        public enum Place
        {
            PORT,
            POLAR_STATION
        }

        private int id = Interlocked.Increment(ref COUNTER);
        private State currentState = State.PREFLYING_PREPARING_WAITING;
        private bool decommissionRequest = false;
        private Place destination = Place.POLAR_STATION;
        private int position = 0;

        public State CurrentState {
            get => currentState;
            private set
            {
                currentState = value;
                OnPropertyChanged(nameof(CurrentState));
            }
        }
        public bool DecommissionRequest 
        { 
            get => decommissionRequest;
            private set
            {
                decommissionRequest = value;
                OnPropertyChanged(nameof(DecommissionRequest));
            } 
        }
        public Place Destination 
        {
            get => destination;
            private set
            {
                destination = value;
                OnPropertyChanged(nameof(Destination));
            }
        }
        public int Position
        {
            get => position;
            set
            {
                position = value;
                OnPropertyChanged(nameof(Position));
            }
        }

        public void StartExploitation() 
        {
            
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        override public string ToString()
        {
            return "Квадрокоптер " + id;
        }
    }
}
