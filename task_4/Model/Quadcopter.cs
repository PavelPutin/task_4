﻿using System.ComponentModel;
using System.Runtime.CompilerServices;
using task_4.shared;

namespace task_4.Model
{
    public class Quadcopter : INotifyPropertyChanged
    {
        private static int COUNTER;
        private static Semaphore loadingOnPolarStation = new(AppConfiguration.Instance.MAXIMUM_NUMBER_QUADCOPTERS_SERVICED, AppConfiguration.Instance.MAXIMUM_NUMBER_QUADCOPTERS_SERVICED);
        private static Semaphore loadingOnPort = new(AppConfiguration.Instance.MAXIMUM_NUMBER_QUADCOPTERS_SERVICED, AppConfiguration.Instance.MAXIMUM_NUMBER_QUADCOPTERS_SERVICED);
        public enum State
        {
            PREFLYING_PREPARING_WAITING,
            PREFLYING_PREPARING,
            READY_TO_FLY,
            TAKING_OFF,
            TRAVELLING,
            BROKEN,
            MECHANIC_WAITING,
            REPAIRING,
            LANDING
        }
        public enum Place
        {
            PORT,
            POLAR_STATION
        }

        private int id = Interlocked.Increment(ref COUNTER);
        private State currentState = State.PREFLYING_PREPARING_WAITING;
        private bool decommissionRequest = false;
        private Place destination = Place.POLAR_STATION;
        private int position = 0;
        private QuadOperator? controllingOperator;

        object controllingLocker = new();

        public State CurrentState {
            get => currentState;
            private set
            {
                currentState = value;
                OnPropertyChanged(nameof(CurrentState));
            }
        }
        public bool DecommissionRequest
        {
            get => decommissionRequest;
            private set
            {
                decommissionRequest = value;
                OnPropertyChanged(nameof(DecommissionRequest));
            }
        }
        public Place Destination
        {
            get => destination;
            private set
            {
                destination = value;
                OnPropertyChanged(nameof(Destination));
            }
        }
        public int Position
        {
            get => position;
            set
            {
                position = value;
                OnPropertyChanged(nameof(Position));
            }
        }
        public QuadOperator? ControllingOerator
        {
            get => controllingOperator;
            set
            {
                controllingOperator = value;
                OnPropertyChanged(nameof(ControllingOerator));
            }
        }
        public object ControllingLocker => controllingLocker;

        public void StartExploitation()
        {
            Logger.Instance.Log(ToString(), "Введён в эксплуатацию");
            while (true)
            {
                switch (CurrentState)
                {
                    case State.PREFLYING_PREPARING_WAITING:
                        Logger.Instance.Log(ToString(), "Ожидает предполётную подготовку");
                        switch (Destination)
                        {
                            case Place.POLAR_STATION: loadingOnPort.WaitOne(); break;
                            case Place.PORT: loadingOnPolarStation.WaitOne(); break;
                        }
                        CurrentState = State.PREFLYING_PREPARING;
                        break;
                    case State.PREFLYING_PREPARING:
                        Logger.Instance.Log(ToString(), "Начал предполётную подготовку");
                        Thread.Sleep(TimeSpan.FromSeconds(AppConfiguration.Instance.QUADCOPTER_LOADING_TIME));
                        switch (Destination)
                        {
                            case Place.POLAR_STATION: loadingOnPort.Release(); break;
                            case Place.PORT: loadingOnPolarStation.Release(); break;
                        }
                        Logger.Instance.Log(ToString(), "Закончил предполётную подготовку");
                        Logger.Instance.Log(ToString(), "Ожидает оператора");
                        CurrentState = State.READY_TO_FLY;
                        break;
                    case State.READY_TO_FLY:
                        ReadyToFly?.Invoke(this);
                        break;
                    case State.TAKING_OFF:
                        Logger.Instance.Log(ToString(), "Взлетает");
                        Thread.Sleep(TimeSpan.FromSeconds(AppConfiguration.Instance.QUADCOPTER_TAKEOFF_TIME));
                        Logger.Instance.Log(ToString(), "Успешно взлетел");
                        CurrentState = State.TRAVELLING;
                        break;
                    case State.TRAVELLING:
                        Logger.Instance.Log(ToString(), "Летит к " + Destination);
                        bool cameToDestination =
                            Destination == Place.POLAR_STATION && Position == AppConfiguration.Instance.DISTANCE ||
                            Destination == Place.PORT && Position == 0;
                        while (!cameToDestination)
                        {
                            Logger.Instance.Log(ToString(), "Пролетает над точкой " + Position);
                            Thread.Sleep(TimeSpan.FromSeconds(1));
                            switch (Destination)
                            {
                                case Place.POLAR_STATION: Position = Math.Min(Position + AppConfiguration.Instance.QUADCOPTER_TRAVEL_SPEED, AppConfiguration.Instance.DISTANCE); break;
                                case Place.PORT: Position = Math.Max(Position - AppConfiguration.Instance.QUADCOPTER_TRAVEL_SPEED, 0); break;
                            }
                            
                            cameToDestination =
                                Destination == Place.POLAR_STATION && Position == AppConfiguration.Instance.DISTANCE ||
                                Destination == Place.PORT && Position == 0;
                        }
                        Logger.Instance.Log(ToString(), "Долетел до " + Destination);
                        Destination = Destination == Place.POLAR_STATION ? Place.PORT : Place.POLAR_STATION;
                        CurrentState = State.LANDING;
                        break;
                    case State.LANDING:
                        Logger.Instance.Log(ToString(), "Приземляется");
                        Thread.Sleep(TimeSpan.FromSeconds(AppConfiguration.Instance.QUADCOPTER_LANDING_TIME));
                        Logger.Instance.Log(ToString(), "Успешно приземлился");
                        CurrentState = State.PREFLYING_PREPARING_WAITING;
                        ReleaseControll?.Invoke(this);
                        break;
                }
            }
        }

        public void OnGotQuadcopterControll(QuadOperator quadOperator, Quadcopter quadcopter)
        {
            if (this == quadcopter)
            {
                ControllingOerator = quadOperator;
                Logger.Instance.Log(ToString(), "Начинает взлёт");
                CurrentState = State.TAKING_OFF;
            }
        }

        public delegate void ReadyToFlyEventHandler(Quadcopter readyOne);
        public event ReadyToFlyEventHandler? ReadyToFly;

        public delegate void ReleaseControllEventHandler(Quadcopter quadcopter);
        public event ReleaseControllEventHandler? ReleaseControll;

        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        override public string ToString()
        {
            return "Квадрокоптер " + id;
        }
    }
}
