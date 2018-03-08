using OddsScrapper.Repository.DbBuilder;
using OddsScrapper.Repository.Helpers;
using System;
using System.Data.Common;
using System.Data.SQLite;
using System.Threading.Tasks;

namespace OddsScrapper.Repository.Extensions
{
    public static class ConnectionExtension
    {
        public static async Task<int> InsertAsync(this SQLiteConnection connection, string tableName, params ColumnValuePair[] columnValuePairs)
        {
            var id = -1;
            using (var transaction = connection.BeginTransaction())
            {
                using (var command = connection.CreateCommand())
                {
                    command.Transaction = transaction;

                    command.BuildInsertCommand(tableName, columnValuePairs);

                    await command.ExecuteNonQueryAsync();
                }

                id = Convert.ToInt32(connection.LastInsertRowId);

                transaction.Commit();
            }

            return id; // await connection.GetIdAsync("SELECT LAST_INSERT_ROWID();");
        }

        public static async Task<int> GetIdAsync(this DbConnection connection, string tableName, params ColumnValuePair[] columnValuePairs)
        {
            int id = -1;
            using (var command = connection.CreateCommand())
            {
                command.BuildSelectIdCommand(tableName, columnValuePairs);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    // Always call Read before accessing data.
                    while (await reader.ReadAsync())
                    {
                        id = reader.GetInt32(0);

                        return id;
                    }
                }
            }

            return id;
        }

        public static async Task<int> DeleteAsync(this DbConnection connection, string tableName, params ColumnValuePair[] columnValuePairs)
        {
            int id = -1;
            using (var transaction = connection.BeginTransaction())
            {
                using (var command = connection.CreateCommand())
                {
                    command.Transaction = transaction;

                    command.BuildDeleteCommand(tableName, columnValuePairs);

                    id = await command.ExecuteNonQueryAsync();
                }

                transaction.Commit();
            }

            return id;
        }

        public static async Task<int> UpdateAsync(this DbConnection connection, string tableName, int id, params ColumnValuePair[] columnValuePairs)
        {
            using (var transaction = connection.BeginTransaction())
            {
                using (var command = connection.CreateCommand())
                {
                    command.Transaction = transaction;

                    command.BuildUpdateCommand(tableName, id, columnValuePairs);

                    id = await command.ExecuteNonQueryAsync();
                }

                transaction.Commit();
            }

            return id;
        }

        public static void Create(this DbConnection connection, params Table[] tables)
        {
            using (var transaction = connection.BeginTransaction())
            {
                foreach (var table in tables)
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.Transaction = transaction;

                        command.BuildCreateCommand(table);

                        command.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
            }
        }
    }
}
