using System.ComponentModel;
using System.Runtime.CompilerServices;
using task_4.shared;

namespace task_4.Model
{
    public class Quadcopter : INotifyPropertyChanged
    {
        private static int COUNTER;
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

        private int id = Interlocked.Increment(ref COUNTER);
        private State currentState = State.PREFLYING_PREPARING;
        private bool decommissionRequest = false;
        private Place destination = Place.POLAR_STATION;
        private int position = 0;
        private QuadOperator? quadOperator;
        private readonly AutoResetEvent controlling = new(false);
        private readonly AutoResetEvent broken = new(false);

        private readonly List<AutoResetEvent> operatorsWaiting = [];

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
        public QuadOperator? QuadOperator
        {
            get => quadOperator;
            set
            {
                quadOperator = value;
                OnPropertyChanged(nameof(QuadOperator));
            }
        }
        public AutoResetEvent Controlling => controlling;
        public AutoResetEvent Broken => broken;

        public void StartExploitation() 
        {
            Logger.Instance.Log(ToString(), "Квадрокоптер начал работу");
            while (!(CurrentState == State.PREFLYING_PREPARING && DecommissionRequest))
            {
                switch (CurrentState)
                {
                    case State.PREFLYING_PREPARING:
                        Logger.Instance.Log(ToString(), "Начало предполётной подготовки");
                        Thread.Sleep(TimeSpan.FromSeconds(AppConfiguration.Instance.QUADCOPTER_LOADING_TIME));
                        Logger.Instance.Log(ToString(), "Предполётная подготовка завершена");
                        CurrentState = State.READY_TO_FLY;
                        break;
                    case State.READY_TO_FLY:
                        // todo: add operator waiting
                        Logger.Instance.Log(ToString(), "Квадрокоптер готов к полёту");
                        controlling.Set();
                        AutoResetEvent.WaitAny([.. operatorsWaiting]);
                        CurrentState = State.TAKING_OFF;
                        break;
                    case State.TAKING_OFF:
                        Logger.Instance.Log(ToString(), "Квадрокоптер взлетает");
                        Thread.Sleep(TimeSpan.FromSeconds(AppConfiguration.Instance.QUADCOPTER_TAKEOFF_TIME));
                        Logger.Instance.Log(ToString(), "Квадрокоптер успешно взлетел");
                        CurrentState = State.TRAVELLING;
                        break;
                    case State.TRAVELLING:
                        Logger.Instance.Log(ToString(), "Квадрокоптер в пути");
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
                            Logger.Instance.Log(ToString(), "Квадрокоптер прибыл в " + Destination);
                            Destination = Destination == Place.PORT ? Place.POLAR_STATION : Place.PORT;
                            CurrentState = State.LANDING;
                        }
                        //else if (Random.Shared.NextDouble() < AppConfiguration.Instance.QUADCOPTER_BREAKDOWN_RATE)
                        //{
                        //    CurrentState = State.BROKEN;
                        //}
                        break;
                    case State.LANDING:
                        Logger.Instance.Log(ToString(), "Квадрокоптер приземляется");
                        Thread.Sleep(AppConfiguration.Instance.QUADCOPTER_LANDING_TIME);
                        Logger.Instance.Log(ToString(), "Квадрокоптер успешно приземлился");
                        Logger.Instance.Log(ToString(), "Отключение от оператора " + quadOperator.ToString());
                        quadOperator.WaitControlling.Set();
                        Logger.Instance.Log(ToString(), "Квадрокоптер отключился от оператора");
                        CurrentState = State.PREFLYING_PREPARING;
                        break;
                    case State.BROKEN:
                        if (quadOperator == null)
                        {
                            throw new Exception("Оператор не найден");
                        }
                        quadOperator.WaitControlling.Set();
                        Broken.Set();
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

        public void AddOperatorsWaiting(AutoResetEvent handler)
        {
            operatorsWaiting.Add(handler);
        }

        public delegate void OnBroke(Quadcopter quadcopter);
        public event OnBroke? Broke;

        public void OnStartControlling(QuadOperator quadOperator, AutoResetEvent handle)
        {
            if (handle == controlling)
            {
                QuadOperator = quadOperator;
                Logger.Instance.Log(ToString(), "Установлена связь с оператором " + quadOperator.ToString());
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        override
        public string ToString()
        {
            return "Квадрокоптер " + id;
        }
    }
}
