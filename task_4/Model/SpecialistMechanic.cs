using System.ComponentModel;
using System.Runtime.CompilerServices;
using task_4.shared;

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

        private int id = Interlocked.Increment(ref COUNTER);
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

        public void QuadcopterBrokenEventHandler(Quadcopter brokenOne)
        {

        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        public override string ToString()
        {
            return "Механик " + id;
        }
    }
}
