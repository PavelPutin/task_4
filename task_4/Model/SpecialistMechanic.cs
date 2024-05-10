using System.ComponentModel;
using System.Runtime.CompilerServices;
using task_4.shared;
using task_4.ViewModel;

namespace task_4.Model
{
    public class SpecialistMechanic : IMechanic
    {
        private static int COUNTER;

        public enum State
        {
            WAITING,
            TRAVELLING_TO_BROKEN_QUADCOPTER,
            REPAIRING,
            TRAVELLING_BACK
        }

        public SpecialistMechanic()
        {
            thread = new(StartWorking);
            thread.IsBackground = true;
        }

        private int id = Interlocked.Increment(ref COUNTER);
        private Thread thread;
        private State currentState = State.WAITING;
        private int position = 0;
        private bool fireRequest = false;
        private Quadcopter? quadcopterForRepair;

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
        int IMechanic.RepairTime => AppConfiguration.Instance.SPECIALIST_MECHANIC_REPAIR_TIME;
        public Quadcopter? QuadcopterForRepair
        {
            get => quadcopterForRepair;
            set
            {
                quadcopterForRepair = value;
                OnPropertyChanged(nameof(QuadcopterForRepair));
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

        public delegate void FiredEventHandler(SpecialistMechanic mechanic);
        public event FiredEventHandler? Fired;

        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        private RelayCommand? fireMechanic;
        public RelayCommand? FireMechanic
        {
            get
            {
                return fireMechanic ??= new RelayCommand(obj =>
                {
                    FireRequest = true;
                    Logger.Instance.Log(ToString(), "Получил запрос на увольнение");
                });
            }
        }

        public override string ToString()
        {
            return "Механик " + id;
        }
    }
}
