using ElevatorSimulation.Models;

namespace ElevatorSimulation.Patterns.Observers
{
    /// <summary>
    /// Observer pattern (1 of 2): for anything that wants to SHOW the elevator (the GUI).
    /// The elevator calls this every time something visible changed (position, doors, lights, state...).
    /// </summary>
    public interface IElevatorChangeObserver
    {
        void ElevatorChanged(ElevatorSnapshot snapshot);
    }
}
