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
        abstract public void QuadcopterBrokenEventHandler(Quadcopter brokenOne);
    }
}
