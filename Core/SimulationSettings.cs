namespace ElevatorSimulation.Core
{
    ///All the tunable numbers of the simulation in one place.
    public static class SimulationSettings
    {
        public const int FloorCount = 2;
        public const int ElevatorCount = 2;

        /// >How often the animation timers fire (milliseconds).
        public const int TickMilliseconds = 30;

        ///secnds the car needs to travel one floor
        public const double SecondsPerFloor = 2.5;

        /// Seconds the doors need to open (or to close).
        public const double DoorMoveSeconds = 1.0;

        /// Seconds the doors stay fully open.
        public const double DoorOpenSeconds = 3.0;

        /// Opening + waiting + closing.
        public const double DoorCycleSeconds = DoorMoveSeconds * 2 + DoorOpenSeconds;
    }
}
