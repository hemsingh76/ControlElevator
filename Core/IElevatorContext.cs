using ElevatorSimulation.Models;
using ElevatorSimulation.Patterns.States;

namespace ElevatorSimulation.Core
{

    public interface IElevatorContext : IElevatorInfo
    {
        // The same values as in IElevatorInfo, but states may also change them.
        new int CurrentFloor { get; set; }
        new double Position { get; set; }
        new int? Destination { get; set; }
        new Direction Direction { get; set; }
        new double DoorOpenness { get; set; }

        bool HasPendingStops { get; }
        int PeekNextStop();
        int TakeNextStop();
        void ClearPanelLight(int floor);

        void ChangeState(IElevatorState newState);

        void RaiseEvent(ElevatorEventType type, string description);
    }
}
