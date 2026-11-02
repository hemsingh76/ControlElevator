using ElevatorSimulation.Core;
using ElevatorSimulation.Models;

namespace ElevatorSimulation.Patterns.States
{
    /// <summary>The elevator door and the floor door slide open together.</summary>
    public class DoorsOpeningState : IElevatorState
    {
        public static readonly DoorsOpeningState Instance = new DoorsOpeningState();

        private DoorsOpeningState() { }

        public string Name { get { return "Doors opening"; } }

        public void Enter(IElevatorContext elevator)
        {
            elevator.ClearPanelLight(elevator.CurrentFloor);
            elevator.RaiseEvent(ElevatorEventType.DoorsOpening,
                "Elevator door and floor door opening at floor " + elevator.CurrentFloor);
        }

        public void Tick(IElevatorContext elevator, double elapsedSeconds)
        {
            elevator.DoorOpenness += 100.0 * elapsedSeconds / SimulationSettings.DoorMoveSeconds;
            if (elevator.DoorOpenness >= 100)
            {
                elevator.DoorOpenness = 100;
                elevator.ChangeState(DoorsOpenState.Instance);
            }
        }

        public double SecondsUntilDoorsClosed(IElevatorInfo elevator)
        {
            double opening = (100 - elevator.DoorOpenness) / 100.0 * SimulationSettings.DoorMoveSeconds;
            return opening + SimulationSettings.DoorOpenSeconds + SimulationSettings.DoorMoveSeconds;
        }
    }
}
