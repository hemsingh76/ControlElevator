namespace ElevatorSimulation.Models
{
 
    public class ElevatorSnapshot
    {
        public int ElevatorId { get; }
        /// <summary>Exact position in floors (0.0 = floor 0, 1.0 = floor 1, 0.5 = half way).</summary>
        public double Position { get; }
        public int CurrentFloor { get; }
        public int? Destination { get; }
        public Direction Direction { get; }
        public string StateName { get; }
        /// <summary>0 = doors closed, 100 = doors fully open.</summary>
        public double DoorOpenness { get; }
        /// <summary>Lit state of the floor buttons on the car's control panel.</summary>
        public bool[] PanelLights { get; }
        /// <summary>Floors waiting in this elevator's queue.</summary>
        public int[] PendingStops { get; }

        public ElevatorSnapshot(int elevatorId, double position, int currentFloor, int? destination,
            Direction direction, string stateName, double doorOpenness, bool[] panelLights, int[] pendingStops)
        {
            ElevatorId = elevatorId;
            Position = position;
            CurrentFloor = currentFloor;
            Destination = destination;
            Direction = direction;
            StateName = stateName;
            DoorOpenness = doorOpenness;
            PanelLights = panelLights;
            PendingStops = pendingStops;
        }
    }
}
