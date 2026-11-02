using ElevatorSimulation.Models;
using ElevatorSimulation.Patterns.States;

namespace ElevatorSimulation.Core
{
    public interface IElevatorInfo
    {
        int Id { get; }

        double Position { get; }

        int CurrentFloor { get; }

        int? Destination { get; }

        Direction Direction { get; }

        double DoorOpenness { get; }

        double StateSeconds { get; }

        IElevatorState State { get; }

        int[] GetPendingStops();
    }
}
