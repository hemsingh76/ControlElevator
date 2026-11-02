using System;
using System.Collections.Generic;
using ElevatorSimulation.Models;
using ElevatorSimulation.Patterns.Observers;
using ElevatorSimulation.Patterns.States;

namespace ElevatorSimulation.Core
{

    public class Elevator : IElevator, IElevatorContext
    {
        private readonly List<IElevatorChangeObserver> _changeObservers = new List<IElevatorChangeObserver>();
        private readonly List<IElevatorEventObserver> _eventObservers = new List<IElevatorEventObserver>();
        private readonly List<int> _pendingStops = new List<int>();   // FIFO queue of floors still to visit
        private readonly bool[] _panelLights = new bool[SimulationSettings.FloorCount];

        public int Id { get; }

        /// <summary>Exact position in floors (0.0 .. FloorCount-1).</summary>
        public double Position { get; set; }

        /// <summary>The floor shown on the display window.</summary>
        public int CurrentFloor { get; set; }

        /// <summary>Floor of the trip in progress, or null.</summary>
        public int? Destination { get; set; }

        public Direction Direction { get; set; }

        /// <summary>0 = closed, 100 = fully open.</summary>
        public double DoorOpenness { get; set; }

        /// <summary>Seconds spent in the current state.</summary>
        public double StateSeconds { get; private set; }

        public IElevatorState State { get; private set; }

        public Elevator(int id, int startFloor)
        {
            Id = id;
            CurrentFloor = startFloor;
            Position = startFloor;
            Direction = Direction.Idle;
            State = IdleState.Instance;
        }

        // ---- Observer pattern (the subject)

        public void AttachChangeObserver(IElevatorChangeObserver observer)
        {
            if (observer != null && !_changeObservers.Contains(observer))
            {
                _changeObservers.Add(observer);
            }
        }

        public void DetachChangeObserver(IElevatorChangeObserver observer)
        {
            _changeObservers.Remove(observer);
        }

        public void AttachEventObserver(IElevatorEventObserver observer)
        {
            if (observer != null && !_eventObservers.Contains(observer))
            {
                _eventObservers.Add(observer);
            }
        }

        public void DetachEventObserver(IElevatorEventObserver observer)
        {
            _eventObservers.Remove(observer);
        }

        private void NotifyChanged()
        {
            ElevatorSnapshot snapshot = CreateSnapshot();
            foreach (IElevatorChangeObserver observer in _changeObservers.ToArray())
            {
                observer.ElevatorChanged(snapshot);
            }
        }

        public void RaiseEvent(ElevatorEventType type, string description)
        {
            var elevatorEvent = new ElevatorEvent(Id, type, CurrentFloor, Destination, description);
            foreach (IElevatorEventObserver observer in _eventObservers.ToArray())
            {
                observer.ElevatorEventOccurred(elevatorEvent);
            }
        }

        public ElevatorSnapshot CreateSnapshot()
        {
            return new ElevatorSnapshot(Id, Position, CurrentFloor, Destination, Direction,
                State.Name, DoorOpenness, (bool[])_panelLights.Clone(), _pendingStops.ToArray());
        }

        //---- Running

        public void Start()
        {
            RaiseEvent(ElevatorEventType.Started, "Elevator ready at floor " + CurrentFloor);
            NotifyChanged();
        }

        public void Tick(double elapsedSeconds)
        {
            StateSeconds += elapsedSeconds;
            State.Tick(this, elapsedSeconds);   // State pattern: the current state decides what happens

            if (!(State is IdleState))
            {
                NotifyChanged();
            }
        }

        // --- State pattern (the context)

        public void ChangeState(IElevatorState newState)
        {
            State = newState;
            StateSeconds = 0;
            State.Enter(this);
            NotifyChanged();
        }

        // -- Requests

        public void AddStop(int floor, RequestSource source, string note)
        {
            if (floor < 0 || floor >= SimulationSettings.FloorCount)
            {
                throw new ArgumentOutOfRangeException("floor");
            }

            string who = source == RequestSource.Internal ? "Car panel" : "Floor request button";

            // Doors are already opening/open on this floor: keep them open a little longer.
            bool doorsActiveHere = CurrentFloor == floor && (State is DoorsOpeningState || State is DoorsOpenState);
            if (doorsActiveHere)
            {
                StateSeconds = 0;
                RaiseEvent(ElevatorEventType.DoorsOpened,
                    who + " request for floor " + floor + ": doors already open here, wait timer restarted" + note);
                NotifyChanged();
                return;
            }

            if (source == RequestSource.Internal)
            {
                _panelLights[floor] = true;
            }

            // Already heading there or already queued: nothing more to do.
            if (Destination == floor || _pendingStops.Contains(floor))
            {
                NotifyChanged();
                return;
            }

            _pendingStops.Add(floor);
            RaiseEvent(ElevatorEventType.RequestQueued, who + " request for floor " + floor + " queued" + note);
            NotifyChanged();
        }

        public bool HasPendingStops { get { return _pendingStops.Count > 0; } }

        public int PeekNextStop()
        {
            return _pendingStops[0];
        }

        public int TakeNextStop()
        {
            int floor = _pendingStops[0];
            _pendingStops.RemoveAt(0);
            return floor;
        }

        public int[] GetPendingStops()
        {
            return _pendingStops.ToArray();
        }

        public void ClearPanelLight(int floor)
        {
            _panelLights[floor] = false;
        }
    }
}
