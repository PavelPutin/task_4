using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace task_4.Model
{
    public class QuadOperator : INotifyPropertyChanged
    {
        private static int COUNTER;

        public enum State
        {
            WAITING_FOR_QUADCOPTER,
            QUADCOPTER_CONTROLLING,
            TRAVELLING_TO_BROKEN_QUADCOPTER,
            REPAIRING,
            TRAVELLING_BACK
        }

        private int id = Interlocked.Increment(ref COUNTER);
        private State currentState = State.WAITING_FOR_QUADCOPTER;
        private int position = 0;
        private bool fireRequest = false;
        private AutoResetEvent waitControlling = new(true);
        private readonly List<AutoResetEvent> waitReadyToFlyQuadCopter = [];

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
        public AutoResetEvent WaitControlling => waitControlling;

        public void StartWorking()
        {
            Logger.Instance.Log(ToString(), "Оператор начал работу");
            while (!(CurrentState == State.WAITING_FOR_QUADCOPTER && FireRequest))
            {
                switch (CurrentState)
                {
                    case State.WAITING_FOR_QUADCOPTER:
                        Logger.Instance.Log(ToString(), "Оператор ожидает готового к полёту квадрокоптера");
                        int handlerIndex = AutoResetEvent.WaitAny([.. waitReadyToFlyQuadCopter]);
                        Logger.Instance.Log(ToString(), "Оператор включает пульт");
                        StartControlling?.Invoke(this, waitReadyToFlyQuadCopter[handlerIndex]);
                        CurrentState = State.QUADCOPTER_CONTROLLING;
                        break;
                    case State.QUADCOPTER_CONTROLLING:
                        break;
                    case State.TRAVELLING_TO_BROKEN_QUADCOPTER:
                        break;
                    case State.REPAIRING:
                        break;
                    case State.TRAVELLING_BACK:
                        break;
                }
                
            }
        }

        public void AddQuadcoptersWaiting(AutoResetEvent handler)
        {
            waitReadyToFlyQuadCopter.Add(handler);
        }

        public delegate void OnStartControlling(QuadOperator quadOperator, AutoResetEvent handle);
        public event OnStartControlling? StartControlling;

        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        public override string ToString()
        {
            return "Оператор " + id;
        }
    }
}
