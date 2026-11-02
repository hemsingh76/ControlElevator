using ElevatorSimulation.Core;
using ElevatorSimulation.Models;

namespace ElevatorSimulation.Patterns.States
{
    /// <summary>The elevator door and the floor door slide shut together.</summary>
    public class DoorsClosingState : IElevatorState
    {
        public static readonly DoorsClosingState Instance = new DoorsClosingState();

        private DoorsClosingState() { }

        public string Name { get { return "Doors closing"; } }

        public void Enter(IElevatorContext elevator)
        {
            elevator.RaiseEvent(ElevatorEventType.DoorsClosing, "Doors closing at floor " + elevator.CurrentFloor);
        }

        public void Tick(IElevatorContext elevator, double elapsedSeconds)
        {
            elevator.DoorOpenness -= 100.0 * elapsedSeconds / SimulationSettings.DoorMoveSeconds;
            if (elevator.DoorOpenness <= 0)
            {
                elevator.DoorOpenness = 0;
                elevator.RaiseEvent(ElevatorEventType.DoorsClosed, "Doors closed at floor " + elevator.CurrentFloor);
                elevator.ChangeState(IdleState.Instance);
            }
        }

        public double SecondsUntilDoorsClosed(IElevatorInfo elevator)
        {
            return elevator.DoorOpenness / 100.0 * SimulationSettings.DoorMoveSeconds;
        }
    }
}
