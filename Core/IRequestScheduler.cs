using System.Collections.Generic;

namespace ElevatorSimulation.Core
{
    public interface IRequestScheduler
    {
        IElevator ChooseElevator(IList<IElevator> elevators, int floor, out double estimatedSeconds);
    }
}
