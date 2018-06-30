using OddsScrapper.Repository.DbBuilder;
using OddsScrapper.Repository.Helpers;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SQLite;
using System.Linq;
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

        public static async Task<T> GetByIdAsync<T>(this DbConnection connection, string tableName, int id, Func<DbDataReader, Task<T>> dataCreator)
        {
            return await connection.GetSingleAsync(tableName, new[] { ColumnValuePair.CreateId(id) }, dataCreator);
        }

        public static async Task<T> GetSingleAsync<T>(this DbConnection connection, string tableName, ColumnValuePair[] whereColumns, Func<DbDataReader, Task<T>> dataCreator)
        {
            return (await connection.GetAllAsync(tableName, whereColumns, dataCreator)).FirstOrDefault();
        }

        public static async Task<IEnumerable<T>> GetAllAsync<T>(this DbConnection connection, string tableName, ColumnValuePair[] whereColumns, Func<DbDataReader, Task<T>> dataCreator)
        {
            var results = new List<T>();
            using (var command = connection.CreateCommand())
            {
                command.BuildSelectCommand(tableName, whereColumns);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    // Always call Read before accessing data.
                    while (await reader.ReadAsync())
                    {
                        results.Add(await dataCreator(reader));
                    }
                }
            }

            return results;
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

        public static T GetById<T>(this DbConnection connection, string tableName, int id, Func<DbDataReader, T> dataCreator)
        {
            return connection.GetSingle(tableName, new[] { ColumnValuePair.CreateId(id) }, dataCreator);
        }

        public static T GetSingle<T>(this DbConnection connection, string tableName, ColumnValuePair[] whereColumns, Func<DbDataReader, T> dataCreator)
        {
            return (connection.GetAll(tableName, whereColumns, dataCreator)).FirstOrDefault();
        }

        public static IEnumerable<T> GetAll<T>(this DbConnection connection, string tableName, ColumnValuePair[] whereColumns, Func<DbDataReader, T> dataCreator)
        {
            using (var command = connection.CreateCommand())
            {
                command.BuildSelectCommand(tableName, whereColumns);

                using (var reader = command.ExecuteReader())
                {
                    // Always call Read before accessing data.
                    while (reader.Read())
                    {
                        yield return dataCreator(reader);
                    }
                }
            }
        }


    }
}
