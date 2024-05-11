using System.ComponentModel;
using System.Runtime.CompilerServices;
using task_4.shared;
using task_4.ViewModel;
using static task_4.Model.Quadcopter;

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

        public delegate void FiredEventHandler(SpecialistMechanic mechanic);
        public event FiredEventHandler? Fired;

        public delegate void FinishRepairEventHandler(IMechanic mechanic);
        public event FinishRepairEventHandler? FinishRepair;

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
            return "Механик " + id + "(" + CurrentState + ")";
        }
    }
}
