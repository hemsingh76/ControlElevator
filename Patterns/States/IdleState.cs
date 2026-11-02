using ElevatorSimulation.Core;
using ElevatorSimulation.Models;

namespace ElevatorSimulation.Patterns.States
{
    /// <summary>Doors closed, standing still, waiting for work.</summary>
    public class IdleState : IElevatorState
    {
        public static readonly IdleState Instance = new IdleState();

        private IdleState() { }

        public string Name { get { return "Idle"; } }

        public void Enter(IElevatorContext elevator)
        {
            elevator.Direction = Direction.Idle;
            elevator.Destination = null;
            elevator.DoorOpenness = 0;
            elevator.RaiseEvent(ElevatorEventType.Idle, "Waiting at floor " + elevator.CurrentFloor);
        }

        public void Tick(IElevatorContext elevator, double elapsedSeconds)
        {
            if (!elevator.HasPendingStops)
            {
                return;
            }

            if (elevator.PeekNextStop() == elevator.CurrentFloor)
            {
                // Somebody asked for the floor we are standing on: just open the doors.
                elevator.TakeNextStop();
                elevator.ChangeState(DoorsOpeningState.Instance);
            }
            else
            {
                elevator.ChangeState(MovingState.Instance);
            }
        }

        public double SecondsUntilDoorsClosed(IElevatorInfo elevator)
        {
            return 0;
        }
    }
}
