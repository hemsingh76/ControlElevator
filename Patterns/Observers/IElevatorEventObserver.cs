using ElevatorSimulation.Models;

namespace ElevatorSimulation.Patterns.Observers
{
    /// <summary>
    /// Observer pattern (2 of 2): for anything that wants to RECORD or REACT to what happened
    /// (the database logger, the controller). Called when something noteworthy happened
    /// (arrived, doors opened, request queued...).
    /// </summary>
    public interface IElevatorEventObserver
    {
        void ElevatorEventOccurred(ElevatorEvent elevatorEvent);
    }
}
