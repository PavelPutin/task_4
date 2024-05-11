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
        private object bookenProcessingLock = new();

        public ObservableCollection<Quadcopter> Quadcopters { get; } = [];
        public ObservableCollection<QuadOperator> QuadOperators { get; } = [];
        public ObservableCollection<SpecialistMechanic> SpecialMechanics { get; } = [];
        public Logger Logger { get; } = Logger.Instance;

        public void Init()
        {
            for (int i = 0; i < AppConfiguration.Instance.QUADCOPTERS_INIT_NUMBER; i++)
            {
                Quadcopter quadcopter = new();
                Quadcopters.Add(quadcopter);
                quadcopter.Decommissioned += OnDecommissioned;
                quadcopter.Broken += OnQuadcopterBroken;
            }

            for (int i = 0; i < AppConfiguration.Instance.OPERATORS_INIT_NUMBER; i++)
            {
                QuadOperator quadOperator = new();
                QuadOperators.Add(quadOperator);
                quadOperator.Fired += OnOperatorFired;
            }

            for (int i = 0; i < AppConfiguration.Instance.SPECIALIZED_MECHANICS_INIT_NUMBER; i++)
            {
                SpecialistMechanic mechanic = new();
                SpecialMechanics.Add(mechanic);
                mechanic.Fired += OnMechanicFired;
            }

            foreach (var quadcopter in Quadcopters)
            {
                foreach(var quadOperator in QuadOperators)
                {
                    quadcopter.ReadyToFly += quadOperator.OnReadyToFly;
                    quadcopter.ReleaseControll += quadOperator.OnReleaseControll;
                    quadOperator.GotQuadcopterControll += quadcopter.OnGotQuadcopterControll;
                }
            }

            foreach(var quadcopter in Quadcopters)
            {
                quadcopter.Thread.Start();
            }

            foreach (var quadOperator in QuadOperators)
            {
                quadOperator.Thread.Start();
            }

            foreach (var mechanic in SpecialMechanics)
            {
                mechanic.Thread.Start();
            }
        }

        private void OnDecommissioned(Quadcopter quadcopter)
        {
            quadcopter.Decommissioned -= OnDecommissioned;
            quadcopter.Broken -= OnQuadcopterBroken;
            foreach (var quadOperator in QuadOperators)
            {
                quadcopter.ReadyToFly -= quadOperator.OnReadyToFly;
                quadcopter.ReleaseControll -= quadOperator.OnReleaseControll;
                quadOperator.GotQuadcopterControll -= quadcopter.OnGotQuadcopterControll;
            }
            App.Current?.Dispatcher.Invoke(() =>
            {
                Quadcopters.Remove(quadcopter);
            });
        }

        private void OnQuadcopterBroken(Quadcopter quadcopter)
        {
            lock (bookenProcessingLock)
            {
                IMechanic? selected = null;
                foreach (var mechanic in SpecialMechanics)
                {
                    if (mechanic.TryStartTravelling(quadcopter))
                    {
                        selected = mechanic;
                    }
                }

                if (selected == null)
                {
                    foreach (var mechanic in QuadOperators)
                    {
                        if (mechanic.TryStartTravelling(quadcopter))
                        {
                            selected = mechanic;
                        }
                    }
                }

                if (selected != null)
                {
                    quadcopter.StartWaiting(selected);
                }
            }
        }

        private void OnOperatorFired(QuadOperator quadOperator)
        {
            quadOperator.Fired -= OnOperatorFired;
            foreach (var quadcopter in Quadcopters)
            {
                quadcopter.ReadyToFly -= quadOperator.OnReadyToFly;
                quadcopter.ReleaseControll -= quadOperator.OnReleaseControll;
                quadOperator.GotQuadcopterControll -= quadcopter.OnGotQuadcopterControll;
            }
            App.Current?.Dispatcher.Invoke(() =>
            {
                QuadOperators.Remove(quadOperator);
            });
        }

        private void OnMechanicFired(SpecialistMechanic mechanic)
        {
            mechanic.Fired -= OnMechanicFired;
            App.Current?.Dispatcher.Invoke(() =>
            {
                SpecialMechanics.Remove(mechanic);
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
                    App.Current?.Dispatcher.Invoke(() =>
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
                    quadcopter.Broken += OnQuadcopterBroken;

                    quadcopter.Thread.Start();
                });
            }
        }

        private RelayCommand? addQuadOperator;
        public RelayCommand? AddQuadOperator
        {
            get
            {
                return addQuadOperator ??= new RelayCommand(obj =>
                {
                    QuadOperator quadOperator = new();
                    App.Current?.Dispatcher.Invoke(() =>
                    {
                        QuadOperators.Add(quadOperator);
                    });

                    foreach (var quadcopter in Quadcopters)
                    {
                        quadcopter.ReadyToFly += quadOperator.OnReadyToFly;
                        quadcopter.ReleaseControll += quadOperator.OnReleaseControll;
                        quadOperator.GotQuadcopterControll += quadcopter.OnGotQuadcopterControll;
                    }
                    quadOperator.Fired += OnOperatorFired;

                    quadOperator.Thread.Start();
                });
            }
        }

        private RelayCommand? addMechanic;
        public RelayCommand? AddMechanic
        {
            get
            {
                return addMechanic ??= new RelayCommand(obj =>
                {
                    SpecialistMechanic mechanic = new();
                    App.Current?.Dispatcher.Invoke(() =>
                    {
                        SpecialMechanics.Add(mechanic);
                    });

                    mechanic.Fired += OnMechanicFired;
                    mechanic.Thread.Start();
                });
            }
        }
    }
}
