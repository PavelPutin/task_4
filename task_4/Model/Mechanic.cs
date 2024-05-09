using System.ComponentModel;
using task_4.shared;

namespace task_4.Model
{
    public interface IMechanic : INotifyPropertyChanged
    {
        public int Speed { get => AppConfiguration.Instance.MECHANIC_TRAVEL_SPEED; }
        abstract public int Position { get; protected set; }
        abstract public int RepairTime { get; }
    }
}
