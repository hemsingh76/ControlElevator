using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;

namespace ElevatorSimulation.Data
{

    public class AccessDatabase
    {
        private readonly DatabaseConfig _config;

        public AccessDatabase(DatabaseConfig config)
        {
            _config = config;
        }

        public DatabaseConfig Config { get { return _config; } }

        /// <summary>INSERT / UPDATE / DELETE / DDL. Returns the number of affected rows.</summary>
        public int ExecuteNonQuery(string sql, params OleDbParameter[] parameters)
        {
            try
            {
                using (var connection = new OleDbConnection(_config.ConnectionString))
                using (var command = new OleDbCommand(sql, connection))
                {
                    command.Parameters.AddRange(parameters);
                    connection.Open();
                    return command.ExecuteNonQuery();
                }
            }
            catch (Exception ex) when (IsDatabaseError(ex))
            {
                throw new DatabaseException("Database command failed: " + ex.Message, ex);
            }
        }

        /// <summary>SELECT that returns a table.</summary>
        public DataTable ExecuteQuery(string sql, params OleDbParameter[] parameters)
        {
            try
            {
                using (var connection = new OleDbConnection(_config.ConnectionString))
                using (var command = new OleDbCommand(sql, connection))
                using (var adapter = new OleDbDataAdapter(command))
                {
                    command.Parameters.AddRange(parameters);
                    var table = new DataTable();
                    adapter.Fill(table);
                    return table;
                }
            }
            catch (Exception ex) when (IsDatabaseError(ex))
            {
                throw new DatabaseException("Database query failed: " + ex.Message, ex);
            }
        }

     
        public int SaveNewRows(DataTable table, string insertSql, params OleDbParameter[] insertParameters)
        {
            try
            {
                using (var connection = new OleDbConnection(_config.ConnectionString))
                using (var insertCommand = new OleDbCommand(insertSql, connection))
                using (var adapter = new OleDbDataAdapter())
                {
                    insertCommand.Parameters.AddRange(insertParameters);
                    insertCommand.UpdatedRowSource = UpdateRowSource.None;
                    adapter.InsertCommand = insertCommand;

                    connection.Open();
                    using (OleDbTransaction transaction = connection.BeginTransaction())
                    {
                        insertCommand.Transaction = transaction;
                        int saved = adapter.Update(table);   // sends every Added row to the database
                        transaction.Commit();
                        table.AcceptChanges();
                        return saved;
                    }
                }
            }
            catch (Exception ex) when (IsDatabaseError(ex))
            {
                throw new DatabaseException("Saving the changes failed: " + ex.Message, ex);
            }
        }

        /// <summary>True if a table with this name exists in the Access file.</summary>
        public bool TableExists(string tableName)
        {
            try
            {
                using (var connection = new OleDbConnection(_config.ConnectionString))
                {
                    connection.Open();
                    DataTable tables = connection.GetOleDbSchemaTable(
                        OleDbSchemaGuid.Tables, new object[] { null, null, tableName, "TABLE" });
                    return tables != null && tables.Rows.Count > 0;
                }
            }
            catch (Exception ex) when (IsDatabaseError(ex))
            {
                throw new DatabaseException("Could not read the database structure: " + ex.Message, ex);
            }
        }

        private static bool IsDatabaseError(Exception ex)
        {
            // InvalidOperationException is what OleDb throws when the provider is not installed.
            return ex is OleDbException || ex is InvalidOperationException || ex is IOException;
        }
    }
}
