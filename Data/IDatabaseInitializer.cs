namespace ElevatorSimulation.Data
{
    /// <summary>Makes sure the database file and its tables exist.</summary>
    public interface IDatabaseInitializer
    {
        /// <exception cref="DatabaseException">The database could not be created or opened.</exception>
        void Initialize();
    }
}
