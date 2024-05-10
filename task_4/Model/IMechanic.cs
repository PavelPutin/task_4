using System.ComponentModel;
using task_4.shared;

namespace task_4.Model
{
    public interface IMechanic : INotifyPropertyChanged
    {
        public int Speed { get => AppConfiguration.Instance.MECHANIC_TRAVEL_SPEED; }
        abstract public int Position { get; }
        abstract public bool FireRequest { get; }
        abstract public int RepairTime { get; }
        abstract public Quadcopter? QuadcopterForRepair { get; set; }
        abstract public bool TryStartTravelling(Quadcopter brokenOne);

        public event StartRepairEventHandler? StartRepair;
        public delegate void StartRepairEventHandler(IMechanic mechanic);

        public event FinishRepairEventHandler? FinishRepair;
        public delegate void FinishRepairEventHandler(IMechanic mechanic);
    }
}
