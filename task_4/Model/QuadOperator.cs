using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace task_4.Model
{
    public class QuadOperator
    {
        private static int COUNTER;

        private int id = Interlocked.Increment(ref COUNTER);
        private AutoResetEvent waitControlling = new(true);
        private readonly List<AutoResetEvent> quadcoptersWaiting = [];

        public AutoResetEvent WaitControlling => waitControlling;

        public void StartWorking()
        {
            Logger.Instance.Log(ToString(), "Оператор начал работу");
            while (true)
            {
                Logger.Instance.Log(ToString(), "Оператор ожидает готового к полёту квадрокоптера");
                int handlerIndex = AutoResetEvent.WaitAny([.. quadcoptersWaiting]);
                Logger.Instance.Log(ToString(), "Оператор включает пульт");
                StartControlling?.Invoke(this, quadcoptersWaiting[handlerIndex]);
            }
        }

        public void AddQuadcoptersWaiting(AutoResetEvent handler)
        {
            quadcoptersWaiting.Add(handler);
        }

        public delegate void OnStartControlling(QuadOperator quadOperator, AutoResetEvent handle);
        public event OnStartControlling? StartControlling;

        public override string ToString()
        {
            return "Оператор " + id;
        }
    }
}
