using System;

namespace ElevatorSimulation.Models
{

    public class ElevatorEvent
    {
        public int ElevatorId { get; }
        public ElevatorEventType EventType { get; }
        public int CurrentFloor { get; }
        /// <summary>Floor the elevator is heading to, or null when it is not travelling.</summary>
        public int? Destination { get; }
        public string Description { get; }
        public DateTime Timestamp { get; }

        public ElevatorEvent(int elevatorId, ElevatorEventType eventType, int currentFloor, int? destination, string description)
        {
            ElevatorId = elevatorId;
            EventType = eventType;
            CurrentFloor = currentFloor;
            Destination = destination;
            Description = description;
            Timestamp = DateTime.Now;
        }
    }
}
