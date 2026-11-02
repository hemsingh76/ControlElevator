using System;
using System.Collections.Generic;
using ElevatorSimulation.Patterns.States;

namespace ElevatorSimulation.Core
{
    public class ShortestResponseTimeScheduler : IRequestScheduler
    {
        public IElevator ChooseElevator(IList<IElevator> elevators, int floor, out double estimatedSeconds)
        {
            IElevator best = null;
            double bestSeconds = double.MaxValue;
            int bestQueueLength = int.MaxValue;

            foreach (IElevator elevator in elevators)
            {
                double seconds = EstimateSeconds(elevator, floor);
                int queueLength = elevator.GetPendingStops().Length;

                bool clearlyFaster = seconds < bestSeconds - 0.001;
                bool tieButShorterQueue = Math.Abs(seconds - bestSeconds) <= 0.001 && queueLength < bestQueueLength;

                if (best == null || clearlyFaster || tieButShorterQueue)
                {
                    best = elevator;
                    bestSeconds = seconds;
                    bestQueueLength = queueLength;
                }
            }

            estimatedSeconds = bestSeconds;
            return best;
        }


        public double EstimateSeconds(IElevatorInfo elevator, int floor)
        {
            // Doors are already opening/open on that floor: passengers can step in right now.
            if (elevator.CurrentFloor == floor &&
                (elevator.State is DoorsOpeningState || elevator.State is DoorsOpenState))
            {
                return 0;
            }

            double time = elevator.State.SecondsUntilDoorsClosed(elevator);
            double position = elevator.Position;

            // The trip in progress comes first, then the queue (first come, first served).
            var route = new List<int>();
            if (elevator.Destination.HasValue)
            {
                route.Add(elevator.Destination.Value);
            }
            route.AddRange(elevator.GetPendingStops());

            foreach (int stop in route)
            {
                time += Math.Abs(position - stop) * SimulationSettings.SecondsPerFloor;
                if (stop == floor)
                {
                    return time; // already on its route
                }
                position = stop;
                time += SimulationSettings.DoorCycleSeconds;
            }

            return time + Math.Abs(position - floor) * SimulationSettings.SecondsPerFloor;
        }
    }
}
