namespace ElevatorSimulation.Patterns.Observers
{
    /// <summary>Observer for the floor request (call) buttons and their built-in lights.</summary>
    public interface IRequestPanelObserver
    {
        /// <summary>lights[floor] is true when that call button of the given elevator is lit.</summary>
        void ExternalLightsChanged(int elevatorId, bool[] lights);
    }
}
