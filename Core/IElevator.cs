using ElevatorSimulation.Models;
using ElevatorSimulation.Patterns.Observers;

namespace ElevatorSimulation.Core
{

    public interface IElevator : IElevatorInfo
    {
        // Observer pattern: the elevator is the "subject" that observers attach to.
        void AttachChangeObserver(IElevatorChangeObserver observer);
        void DetachChangeObserver(IElevatorChangeObserver observer);
        void AttachEventObserver(IElevatorEventObserver observer);
        void DetachEventObserver(IElevatorEventObserver observer);



        void AddStop(int floor, RequestSource source, string note);

        void Start();

        void Tick(double elapsedSeconds);
    }
}
