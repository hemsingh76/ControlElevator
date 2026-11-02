using System;

namespace ElevatorSimulation.Data
{
    /// <summary>Thrown by the data layer so the rest of the program does not depend on OleDb exception types.</summary>
    public class DatabaseException : Exception
    {
        public DatabaseException(string message) : base(message) { }

        public DatabaseException(string message, Exception inner) : base(message, inner) { }
    }
}
