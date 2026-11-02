using System.Collections.Generic;
using System.Data;
using ElevatorSimulation.Models;

namespace ElevatorSimulation.Data
{
    /// <summary>What the rest of the program needs from the database. The UI depends on this, not on Access.</summary>
    public interface IEventRepository
    {
        void Insert(ElevatorEvent elevatorEvent);
        void InsertMany(IEnumerable<ElevatorEvent> elevatorEvents);
        DataTable GetAll();
        void DeleteAll();
    }
}
