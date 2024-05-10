using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using task_4.Model;
using task_4.shared;

namespace task_4.ViewModel
{
    public class SimulationViewModel
    {
        public ObservableCollection<Quadcopter> Quadcopters { get; } = [];
        public ObservableCollection<QuadOperator> QuadOperators { get; } = [];
        public Logger Logger { get; } = Logger.Instance;

        public void Init()
        {
            for (int i = 0; i < AppConfiguration.Instance.QUADCOPTERS_INIT_NUMBER; i++)
            {
                Quadcopter quadcopter = new();
                Quadcopters.Add(quadcopter);
            }

            for (int i = 0; i < AppConfiguration.Instance.OPERATORS_INIT_NUMBER; i++)
            {
                QuadOperator quadOperator = new();
                QuadOperators.Add(quadOperator);
            }

            foreach (var quadcopter in Quadcopters)
            {
                foreach(var quadOperator in QuadOperators)
                {
                    quadcopter.ReadyToFly += quadOperator.OnReadyToFly;
                    quadcopter.ReleaseControll += quadOperator.OnReleaseControll;
                    quadOperator.GotQuadcopterControll += quadcopter.OnGotQuadcopterControll;
                }
                quadcopter.Decommissioned += OnDecommissioned;
            }

            foreach(var quadcopter in Quadcopters)
            {
                quadcopter.Thread.Start();
            }

            foreach (var quadOperator in QuadOperators)
            {
                quadOperator.Thread.Start();
            }
        }

        private void OnDecommissioned(Quadcopter quadcopter)
        {
            quadcopter.Decommissioned -= OnDecommissioned;
            foreach (var quadOperator in QuadOperators)
            {
                quadcopter.ReadyToFly -= quadOperator.OnReadyToFly;
                quadcopter.ReleaseControll -= quadOperator.OnReleaseControll;
                quadOperator.GotQuadcopterControll -= quadcopter.OnGotQuadcopterControll;
            }
            App.Current.Dispatcher.Invoke(() =>
            {
                Quadcopters.Remove(quadcopter);
            });
        }

        private RelayCommand? addQuadcopter;
        public RelayCommand? AddQuadcopter
        {
            get
            {
                return addQuadcopter ??= new RelayCommand(obj =>
                {
                    Quadcopter quadcopter = new();
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        Quadcopters.Add(quadcopter);
                    });

                    foreach (var quadOperator in QuadOperators)
                    {
                        quadcopter.ReadyToFly += quadOperator.OnReadyToFly;
                        quadcopter.ReleaseControll += quadOperator.OnReleaseControll;
                        quadOperator.GotQuadcopterControll += quadcopter.OnGotQuadcopterControll;
                    }
                    quadcopter.Decommissioned += OnDecommissioned;

                    quadcopter.Thread.Start();
                });
            }
        }
    }
}
