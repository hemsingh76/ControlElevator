using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using ElevatorSimulation.Models;

namespace ElevatorSimulation.Data
{
    /// Stores and reads elevator events in the ElevatorLog table (all queries are parameterized)
    public class AccessEventRepository : IEventRepository
    {
        private const string InsertSql =
            "INSERT INTO ElevatorLog (ElevatorId, CurrentFloor, DestinationFloor, EventDescription, EventTime) " +
            "VALUES (?, ?, ?, ?, ?)";

        private const string SelectSql =
            "SELECT TOP 500 LogId AS [Log ID], ElevatorId AS [Elevator], CurrentFloor AS [Current Floor], " +
            "DestinationFloor AS [Destination], EventDescription AS [Event Description], EventTime AS [Event Time] " +
            "FROM ElevatorLog ORDER BY LogId DESC";

        private const string DeleteSql = "DELETE FROM ElevatorLog";

        private readonly AccessDatabase _database;

        public AccessEventRepository(AccessDatabase database)
        {
            _database = database;
        }

        public void Insert(ElevatorEvent elevatorEvent)
        {
            InsertMany(new[] { elevatorEvent });
        }

        /// Disconnected model: the events are added to an in-memory DataTable first, then
        /// DataAdapter.Update() (inside AccessDatabase.SaveNewRows) writes them to Access.
      
        public void InsertMany(IEnumerable<ElevatorEvent> elevatorEvents)
        {
            DataTable table = CreateEmptyLogTable();
            foreach (ElevatorEvent e in elevatorEvents)
            {
                table.Rows.Add(CreateRow(e));   // new rows get RowState = Added
            }

            if (table.Rows.Count == 0)
            {
                return;
            }
            _database.SaveNewRows(table, InsertSql, CreateInsertParameters());
        }

        public DataTable GetAll()
        {
            return _database.ExecuteQuery(SelectSql);
        }

        public void DeleteAll()
        {
            _database.ExecuteNonQuery(DeleteSql);
        }

        /// An empty in-memory copy of the table's columns (no database connection needed)
        private static DataTable CreateEmptyLogTable()
        {
            var table = new DataTable("ElevatorLog");
            table.Columns.Add("ElevatorId", typeof(int));
            table.Columns.Add("CurrentFloor", typeof(int));
            table.Columns.Add("DestinationFloor", typeof(int));
            table.Columns.Add("EventDescription", typeof(string));
            table.Columns.Add("EventTime", typeof(DateTime));
            return table;
        }

        /// <summary>One DataTable row for one event. The order matches the columns above.</summary>
        private static object[] CreateRow(ElevatorEvent e)
        {
            string text = e.Description ?? string.Empty;
            if (text.Length > 255)
            {
                text = text.Substring(0, 255);
            }

            // Access stores whole seconds, so cut the milliseconds off to avoid conversion problems.
            DateTime time = e.Timestamp;
            time = new DateTime(time.Ticks - (time.Ticks % TimeSpan.TicksPerSecond));

            object destination = e.Destination.HasValue ? (object)e.Destination.Value : DBNull.Value;
            return new object[] { e.ElevatorId, e.CurrentFloor, destination, text, time };
        }


        /// OleDb parameters are positional: the order here must match the ? marks in InsertSql.

   
        private static OleDbParameter[] CreateInsertParameters()
        {
            return new[]
            {
                new OleDbParameter("@ElevatorId", OleDbType.Integer, 0, "ElevatorId"),
                new OleDbParameter("@CurrentFloor", OleDbType.Integer, 0, "CurrentFloor"),
                new OleDbParameter("@DestinationFloor", OleDbType.Integer, 0, "DestinationFloor"),
                new OleDbParameter("@EventDescription", OleDbType.VarWChar, 255, "EventDescription"),
                new OleDbParameter("@EventTime", OleDbType.Date, 0, "EventTime")
            };
        }
    }
}
