using ElevatorSimulation.Core;

namespace ElevatorSimulation.Patterns.States
{
    /// <summary>
    /// State pattern: every situation an elevator can be in is its own class.
    /// The elevator simply forwards each timer tick to its current state object.
    /// </summary>
    public interface IElevatorState
    {
        string Name { get; }

        /// <summary>Runs once when the elevator switches into this state.</summary>
        void Enter(IElevatorContext elevator);

        /// <summary>Runs on every animation tick while the elevator is in this state.</summary>
        void Tick(IElevatorContext elevator, double elapsedSeconds);

        /// <summary>How many seconds until the doors are closed again (0 if they already are). Used for scheduling.</summary>
        double SecondsUntilDoorsClosed(IElevatorInfo elevator);
    }
}
