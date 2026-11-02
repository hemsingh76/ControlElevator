using System.Collections.Generic;
using System.Linq;
using ElevatorSimulation.Models;
using ElevatorSimulation.Patterns.Observers;

namespace ElevatorSimulation.Core
{

    public class ElevatorController : IElevatorEventObserver
    {
        private readonly List<IElevator> _elevators;
        private readonly IRequestScheduler _scheduler;
        private readonly bool[][] _externalLights;   // _externalLights[elevatorIndex][floor]
        private readonly List<IRequestPanelObserver> _observers = new List<IRequestPanelObserver>();

        public bool AutoAssign { get; set; }

        public ElevatorController(IEnumerable<IElevator> elevators, IRequestScheduler scheduler)
        {
            _elevators = elevators.ToList();
            _scheduler = scheduler;

            _externalLights = new bool[_elevators.Count][];
            for (int i = 0; i < _externalLights.Length; i++)
            {
                _externalLights[i] = new bool[SimulationSettings.FloorCount];
            }

            foreach (IElevator elevator in _elevators)
            {
                elevator.AttachEventObserver(this); // listen, so a call light can go off on arrival
            }
        }

        public void Attach(IRequestPanelObserver observer)
        {
            if (observer != null && !_observers.Contains(observer))
            {
                _observers.Add(observer);
            }
        }

        private void NotifyLights(int elevatorId)
        {
            bool[] copy = (bool[])_externalLights[elevatorId - 1].Clone();
            foreach (IRequestPanelObserver observer in _observers.ToArray())
            {
                observer.ExternalLightsChanged(elevatorId, copy);
            }
        }

        private bool AnyLightOn(int floor)
        {
            return _externalLights.Any(lights => lights[floor]);
        }

        public void PressExternalButton(int elevatorId, int floor)
        {
            if (_externalLights[elevatorId - 1][floor])
            {
                return; // this button is already lit: the elevator is already on its way
            }

            IElevator target;
            string note;

            if (AutoAssign)
            {
                if (AnyLightOn(floor))
                {
                    return; // some elevator is already coming to this floor
                }

                double estimatedSeconds;
                target = _scheduler.ChooseElevator(_elevators, floor, out estimatedSeconds);
                note = string.Format(" (auto-assigned to elevator {0}, estimated response {1:0.0} s)",
                    target.Id, estimatedSeconds);
            }
            else
            {
                target = _elevators.First(e => e.Id == elevatorId);
                note = string.Format(" (call button of elevator {0})", target.Id);
            }

            _externalLights[target.Id - 1][floor] = true;
            NotifyLights(target.Id);
            target.AddStop(floor, RequestSource.External, note);
        }

        public void PressInternalButton(int elevatorId, int floor)
        {
            IElevator elevator = _elevators.First(e => e.Id == elevatorId);
            elevator.AddStop(floor, RequestSource.Internal, string.Empty);
        }

        // --- IElevatorEventObserver

        public void ElevatorEventOccurred(ElevatorEvent elevatorEvent)
        {
            bool doorsOpenedHere = elevatorEvent.EventType == ElevatorEventType.DoorsOpening ||
                                   elevatorEvent.EventType == ElevatorEventType.DoorsOpened;

            bool[] lights = _externalLights[elevatorEvent.ElevatorId - 1];
            if (doorsOpenedHere && lights[elevatorEvent.CurrentFloor])
            {
                lights[elevatorEvent.CurrentFloor] = false;
                NotifyLights(elevatorEvent.ElevatorId);
            }
        }
    }
}
