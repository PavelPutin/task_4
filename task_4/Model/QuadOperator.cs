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
        private object tryingGetRepairLock = new();

        public int Id => id;
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
                IMechanic mechanic = this;
                switch (CurrentState)
                {
                    case State.TRAVELLING_TO_BROKEN_QUADCOPTER:
                        Logger.Instance.Log(ToString(), "Едет к " + QuadcopterForRepair!.ToString());
                        bool cameToDestination = Position == QuadcopterForRepair.Position;
                        while (!cameToDestination)
                        {
                            Logger.Instance.Log(ToString(), "Проезжает точку " + Position);
                            Thread.Sleep(TimeSpan.FromSeconds(1));

                            Position = Math.Min(Position + mechanic.Speed, QuadcopterForRepair.Position);

                            cameToDestination = Position == QuadcopterForRepair.Position;
                        }
                        if (cameToDestination)
                        {
                            Logger.Instance.Log(ToString(), "Доехал до " + QuadcopterForRepair!.ToString());
                            CurrentState = State.REPAIRING;
                        }
                        break;
                    case State.REPAIRING:
                        Logger.Instance.Log(ToString(), "Начал ремонт " + QuadcopterForRepair!.ToString());
                        startRepair?.Invoke(this);
                        Thread.Sleep(TimeSpan.FromSeconds(mechanic.RepairTime));
                        Logger.Instance.Log(ToString(), "Закончил ремонт " + QuadcopterForRepair!.ToString());
                        finishRepair?.Invoke(this);
                        QuadcopterForRepair = null;
                        CurrentState = State.TRAVELLING_BACK;
                        break;
                    case State.TRAVELLING_BACK:
                        Logger.Instance.Log(ToString(), "Едет к порту");
                        bool cameBackToDestination = Position == 0;
                        while (!cameBackToDestination)
                        {
                            Logger.Instance.Log(ToString(), "Проезжает точку " + Position);
                            Thread.Sleep(TimeSpan.FromSeconds(1));

                            Position = Math.Max(Position - mechanic.Speed, 0);

                            cameBackToDestination = Position == 0;
                        }
                        if (cameBackToDestination)
                        {
                            Logger.Instance.Log(ToString(), "Доехал до порта");
                            CurrentState = State.WAITING;
                        }
                        break;
                }
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
                ControllingQuadcopter = null;
                CurrentState = State.WAITING;
            }
        }

        public bool TryStartTravelling(Quadcopter brokenOne)
        {
            lock (tryingGetRepairLock)
            {
                if (CurrentState == State.WAITING && !FireRequest && Interlocked.CompareExchange(ref brokenOne.repairingLocker, 1, 0) == 0)
                {
                    Logger.Instance.Log(ToString(), "Поехал чинить " + brokenOne.ToString());
                    QuadcopterForRepair = brokenOne;
                    CurrentState = State.TRAVELLING_TO_BROKEN_QUADCOPTER;
                    return true;
                }
                return false;
            }
        }

        public delegate void GotQuadcopterControllEventHandler(QuadOperator quadOperator, Quadcopter quadcopter);
        public event GotQuadcopterControllEventHandler? GotQuadcopterControll;

        public delegate void FiredEventHandler(QuadOperator quadOperator);
        public event FiredEventHandler? Fired;

        public delegate void FinishRepairEventHandler(IMechanic mechanic);
        public event FinishRepairEventHandler? FinishRepair;

        private IMechanic.FinishRepairEventHandler? finishRepair;
        event IMechanic.FinishRepairEventHandler? IMechanic.FinishRepair
        {
            add
            {
                finishRepair += value;
            }

            remove
            {
                finishRepair -= value;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        private IMechanic.StartRepairEventHandler? startRepair;
        event IMechanic.StartRepairEventHandler? IMechanic.StartRepair
        {
            add
            {
                startRepair += value;
            }

            remove
            {
                startRepair -= value;
            }
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
            return "Оператор " + id + "(" + CurrentState + ")";
        }
    }
}
