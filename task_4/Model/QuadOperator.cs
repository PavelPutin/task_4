using System.ComponentModel;
using System.Runtime.CompilerServices;
using task_4.shared;
using task_4.ViewModel;

namespace task_4.Model
{
    public class QuadOperator : IMechanic
    {
        private static int COUNTER;

        public enum State
        {
            WAITING,
            QUADCOPTER_CONTROLLING,
            TRAVELLING_TO_BROKEN_QUADCOPTER,
            REPAIRING,
            TRAVELLING_BACK
        }

        public QuadOperator()
        {
            thread = new(StartWorking);
            thread.IsBackground = true;
        }

        private int id = Interlocked.Increment(ref COUNTER);
        private Thread thread;
        private State currentState = State.WAITING;
        private int position = 0;
        private bool fireRequest = false;
        private Quadcopter? controllingQuadcopter;
        private Quadcopter? quadcopterForRepair;

        private object tryingGetControllLock = new();

        public State CurrentState 
        {
            get => currentState; 
            private set
            {
                currentState = value;
                OnPropertyChanged(nameof(CurrentState));
            } 
        }
        public int Position
        {
            get => position;
            private set
            {
                position = value;
                OnPropertyChanged(nameof(Position));
            }
        }
        public bool FireRequest
        {
            get => fireRequest;
            private set
            {
                fireRequest = value;
                OnPropertyChanged(nameof(FireRequest));
            }
        }
        public int RepairTime => AppConfiguration.Instance.OPERATOR_REPAIR_TIME;
        public Quadcopter? QuadcopterForRepair
        {
            get => quadcopterForRepair;
            set
            {
                quadcopterForRepair = value;
                OnPropertyChanged(nameof(QuadcopterForRepair));
            }
        }
        public Quadcopter? ControllingQuadcopter
        {
            get => controllingQuadcopter;
            set
            {
                controllingQuadcopter = value;
                OnPropertyChanged(nameof(ControllingQuadcopter));
            }
        }
        public Thread Thread => thread;
        public void StartWorking()
        {
            Logger.Instance.Log(ToString(), "Начал работу");
            while (!(CurrentState == State.WAITING && FireRequest))
            {
                //switch (CurrentState)
                //{
                //    case State.WAITING: Logger.Instance.Log(ToString(), "Ожидает"); break;
                //}
            }
            Logger.Instance.Log(ToString(), "Уволен");
            Fired?.Invoke(this);
        }

        public void OnReadyToFly(Quadcopter readyOne)
        {
            lock(tryingGetControllLock)
            {
                if (CurrentState == State.WAITING && !FireRequest && Interlocked.CompareExchange(ref readyOne.controllingLocker, 1, 0) == 0)
                {
                    Logger.Instance.Log(ToString(), "Получил управление над " + readyOne.ToString());
                    ControllingQuadcopter = readyOne;
                    CurrentState = State.QUADCOPTER_CONTROLLING;
                    GotQuadcopterControll?.Invoke(this, readyOne);
                }
            }
        }

        public void OnReleaseControll(Quadcopter quadcopter)
        {
            if (controllingQuadcopter == quadcopter)
            {
                Logger.Instance.Log(ToString(), "Прекратил управление " + quadcopter.ToString());
                Interlocked.Exchange(ref quadcopter.controllingLocker, 0);
                controllingQuadcopter = null;
                CurrentState = State.WAITING;
            }
        }

        public delegate void GotQuadcopterControllEventHandler(QuadOperator quadOperator, Quadcopter quadcopter);
        public event GotQuadcopterControllEventHandler? GotQuadcopterControll;

        public delegate void FiredEventHandler(QuadOperator quadOperator);
        public event FiredEventHandler? Fired;

        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        private RelayCommand? fireOperator;
        public RelayCommand? FireOperator
        {
            get
            {
                return fireOperator ??= new RelayCommand(obj =>
                {
                    FireRequest = true;
                    Logger.Instance.Log(ToString(), "Получил запрос на увольнение");
                });
            }
        }

        public override string ToString()
        {
            return "Оператор " + id;
        }
    }
}
