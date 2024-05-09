using System.ComponentModel;
using System.Runtime.CompilerServices;
using task_4.shared;

namespace task_4.Model
{
    public class Quadcopter : INotifyPropertyChanged
    {
        public enum State
        {
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

        private State currentState = State.PREFLYING_PREPARING;
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
            while (!(CurrentState == State.PREFLYING_PREPARING && DecommissionRequest))
            {
                switch (CurrentState)
                {
                    case State.PREFLYING_PREPARING:
                        Thread.Sleep(TimeSpan.FromSeconds(AppConfiguration.Instance.QUADCOPTER_LOADING_TIME));
                        CurrentState = State.READY_TO_FLY;
                        break;
                    case State.READY_TO_FLY:
                        // todo: add operator waiting
                        CurrentState = State.PREFLYING_PREPARING;
                        break;
                    case State.TAKING_OFF:
                        Thread.Sleep(TimeSpan.FromSeconds(AppConfiguration.Instance.QUADCOPTER_TAKEOFF_TIME));
                        CurrentState = State.TRAVELLING;
                        break;
                    case State.TRAVELLING:
                        Thread.Sleep(TimeSpan.FromSeconds(1));
                        switch (Destination)
                        {
                            case Place.POLAR_STATION:
                                Position = Math.Min(
                                    AppConfiguration.Instance.DISTANCE, 
                                    Position + AppConfiguration.Instance.QUADCOPTER_TRAVEL_SPEED);
                                break;
                            case Place.PORT:
                                Position = Math.Max(
                                    0,
                                    Position - AppConfiguration.Instance.QUADCOPTER_TRAVEL_SPEED);
                                break;
                        }

                        bool cameToDestination = 
                            Destination == Place.POLAR_STATION && Position == AppConfiguration.Instance.DISTANCE || 
                            Destination == Place.PORT && Position == 0;
                        if (cameToDestination)
                        {
                            CurrentState = State.LANDING;
                        }
                        else if (Random.Shared.NextDouble() < AppConfiguration.Instance.QUADCOPTER_BREAKDOWN_RATE)
                        {
                            // todo: add broken event emit
                            Broke?.Invoke(this);
                            CurrentState = State.BROKEN;
                        }
                        break;
                    case State.LANDING:
                        Thread.Sleep(AppConfiguration.Instance.QUADCOPTER_LANDING_TIME);
                        CurrentState = State.PREFLYING_PREPARING;
                        break;
                    case State.BROKEN:
                        Thread.Sleep(AppConfiguration.Instance.QUADCOPTER_LANDING_TIME);
                        CurrentState = State.MECHANIC_WAITING;
                        break;
                    case State.MECHANIC_WAITING:
                        // todo: made signal mechanic came waiting
                        break;
                    case State.REPAIRING:
                        // todo: make signal repairing complete waiting
                        break;
                };
            }
        }

        public delegate void OnBroke(Quadcopter quadcopter);
        public event OnBroke? Broke;

        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
