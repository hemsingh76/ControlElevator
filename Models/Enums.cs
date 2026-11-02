namespace ElevatorSimulation.Models
{
    /// <summary>Direction the elevator is currently travelling.</summary>
    public enum Direction
    {
        Idle,
        Up,
        Down
    }

    /// <summary>Where a request came from.</summary>
    public enum RequestSource
    {
        /// <summary>Button on the control panel inside an elevator car.</summary>
        Internal,
        /// <summary>Request button on a floor.</summary>
        External
    }

    /// <summary>Kinds of things an elevator can report to its observers.</summary>
    public enum ElevatorEventType
    {
        Started,
        RequestQueued,
        Departed,
        Arrived,
        DoorsOpening,
        DoorsOpened,
        DoorsClosing,
        DoorsClosed,
        Idle
    }
}
