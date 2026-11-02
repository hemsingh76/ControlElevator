using System;
using ElevatorSimulation.Core;
using ElevatorSimulation.Models;

namespace ElevatorSimulation.Patterns.States
{
    /// <summary>Doors fully open; passengers get in or out. After a while the doors close.</summary>
    public class DoorsOpenState : IElevatorState
    {
        public static readonly DoorsOpenState Instance = new DoorsOpenState();

        private DoorsOpenState() { }

        public string Name { get { return "Doors open"; } }

        public void Enter(IElevatorContext elevator)
        {
            elevator.RaiseEvent(ElevatorEventType.DoorsOpened, "Doors fully open at floor " + elevator.CurrentFloor);
        }

        public void Tick(IElevatorContext elevator, double elapsedSeconds)
        {
            // elevator.StateSeconds counts how long we have been in this state.
            if (elevator.StateSeconds >= SimulationSettings.DoorOpenSeconds)
            {
                elevator.ChangeState(DoorsClosingState.Instance);
            }
        }

        public double SecondsUntilDoorsClosed(IElevatorInfo elevator)
        {
            double remainingWait = Math.Max(0, SimulationSettings.DoorOpenSeconds - elevator.StateSeconds);
            return remainingWait + SimulationSettings.DoorMoveSeconds;
        }
    }
}
