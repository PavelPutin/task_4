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
        private List<Thread> quadcopterThreads = [];
        public ObservableCollection<Quadcopter> Quadcopters { get; } = [];
        private List<Thread> quadOperatorThreads = [];
        public ObservableCollection<QuadOperator> QuadOperators { get; } = [];
        public Logger Logger { get; } = Logger.Instance;

        public void Init()
        {
            for (int i = 0; i < AppConfiguration.Instance.QUADCOPTERS_INIT_NUMBER; i++)
            {
                Quadcopter quadcopter = new();
                Quadcopters.Add(quadcopter);
                Thread thread = new(quadcopter.StartExploitation);
                quadcopterThreads.Add(thread);
            }

            for (int i = 0; i < AppConfiguration.Instance.OPERATORS_INIT_NUMBER; i++)
            {
                QuadOperator quadOperator = new();
                QuadOperators.Add(quadOperator);
                Thread thread = new(quadOperator.StartWorking);
                quadOperatorThreads.Add(thread);
            }

            foreach (var quadcopter in Quadcopters)
            {
                foreach(var quadOperator in QuadOperators)
                {
                    quadcopter.AddOperatorsWaiting(quadOperator.WaitControlling);
                    quadOperator.AddQuadcoptersWaiting(quadcopter.Controlling);
                    quadOperator.StartControlling += quadcopter.OnStartControlling;
                }
            }

            foreach(var thread in quadOperatorThreads)
            {
                thread.Start();
            }

            foreach (var thread in quadcopterThreads)
            {
                thread.Start();
            }
        }
    }
}
