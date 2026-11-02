using System;
using ElevatorSimulation.Core;
using ElevatorSimulation.Models;

namespace ElevatorSimulation.Patterns.States
{
    /// <summary>Doors closed, travelling to the destination. Nothing can interrupt this trip.</summary>
    public class MovingState : IElevatorState
    {
        public static readonly MovingState Instance = new MovingState();

        private MovingState() { }

        public string Name { get { return "Moving"; } }

        public void Enter(IElevatorContext elevator)
        {
            int destination = elevator.TakeNextStop();
            elevator.Destination = destination;
            elevator.Direction = destination > elevator.CurrentFloor ? Direction.Up : Direction.Down;

            elevator.RaiseEvent(ElevatorEventType.Departed, string.Format(
                "Doors closed, moving {0} from floor {1} to floor {2}",
                elevator.Direction == Direction.Up ? "up" : "down",
                elevator.CurrentFloor,
                destination));
        }

        public void Tick(IElevatorContext elevator, double elapsedSeconds)
        {
            int destination = elevator.Destination.Value;
            double step = elapsedSeconds / SimulationSettings.SecondsPerFloor;

            if (elevator.Direction == Direction.Up)
            {
                elevator.Position = Math.Min(elevator.Position + step, destination);
                elevator.CurrentFloor = (int)Math.Floor(elevator.Position);
            }
            else
            {
                elevator.Position = Math.Max(elevator.Position - step, destination);
                elevator.CurrentFloor = (int)Math.Ceiling(elevator.Position);
            }

            if (elevator.Position == destination)
            {
                elevator.CurrentFloor = destination;
                elevator.RaiseEvent(ElevatorEventType.Arrived, "Arrived at floor " + destination);
                elevator.Direction = Direction.Idle;
                elevator.Destination = null;
                elevator.ChangeState(DoorsOpeningState.Instance);
            }
        }

        public double SecondsUntilDoorsClosed(IElevatorInfo elevator)
        {
            return 0; // the doors are already closed while travelling
        }
    }
}
