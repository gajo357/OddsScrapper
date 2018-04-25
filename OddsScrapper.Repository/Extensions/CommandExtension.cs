using OddsScrapper.Repository.DbBuilder;
using OddsScrapper.Repository.Helpers;
using System.Data.Common;
using System.Data.SQLite;
using System.Text;

namespace OddsScrapper.Repository.Extensions
{
    public static class CommandExtension
    {
        public static void BuildInsertCommand(this DbCommand command, string tableName, params ColumnValuePair[] columnValuePairs)
        {
            var first = true;
            var columnNameList = new StringBuilder();
            var columnValueList = new StringBuilder();
            foreach (var columnValue in columnValuePairs)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    columnNameList.Append(",");
                    columnValueList.Append(",");
                }

                columnNameList.Append(columnValue.Column);
                columnValueList.Append("?");

                command.Parameters.Add(columnValue.ToParameter());
            }

            command.CommandText = $"INSERT INTO {tableName} ({columnNameList}) VALUES ({columnValueList});";

            //var agg = columnValuePairs.Aggregate((Tuple<string, string>)null, (result, current) => {
            //    if (result == null)
            //    {
            //        return Tuple.Create(current.Column, $"'{current.Value}'");
            //    }

            //    return Tuple.Create($"{result.Item1},{current.Column}", $"{result.Item2},'{current.Value}'");
            //});
            //return $"INSERT INTO {tableName} ({agg.Item1}) VALUES ({agg.Item2});";
        }

        public static void BuildUpdateCommand(this DbCommand command, string tableName, int id, params ColumnValuePair[] columnValuePairs)
        {
            var first = true;
            var setValueList = new StringBuilder();
            foreach (var columnValue in columnValuePairs)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    setValueList.Append(",");
                }

                setValueList.Append($"{columnValue.Column}=?");

                command.Parameters.Add(columnValue.ToParameter());
            }

            // add the id parameter as the last one
            command.Parameters.Add(new SQLiteParameter("Id",  id));

            command.CommandText = $"UPDATE {tableName} SET {setValueList.ToString()} WHERE Id=?;";
        }

        public static void BuildSelectIdCommand(this DbCommand command, string tableName, params ColumnValuePair[] columnValuePairs)
        {
            var whereClause = command.BuildWhereClause(columnValuePairs);

            command.CommandText = $"SELECT Id FROM {tableName} WHERE {whereClause};";

            //var agg = columnValuePairs.Aggregate((Tuple<string, string>)null, (result, current) => {
            //    if (result == null)
            //    {
            //        return Tuple.Create(current.Column, $"'{current.Value}'");
            //    }

            //    return Tuple.Create($"{result.Item1},{current.Column}", $"{result.Item2},'{current.Value}'");
            //});
            //return $"INSERT INTO {tableName} ({agg.Item1}) VALUES ({agg.Item2});";
        }

        public static void BuildDeleteCommand(this DbCommand command, string tableName, params ColumnValuePair[] columnValuePairs)
        {
            var whereClause = command.BuildWhereClause(columnValuePairs);

            command.CommandText = $"DELETE FROM {tableName} WHERE {whereClause};";
        }

        private static string BuildWhereClause(this DbCommand command, params ColumnValuePair[] columnValuePairs)
        {
            var first = true;
            var whereClause = new StringBuilder();
            foreach (var columnValue in columnValuePairs)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    whereClause.Append(" AND ");
                }

                whereClause.Append($"{columnValue.Column}=?");

                command.Parameters.Add(columnValue.ToParameter());
            }

            return whereClause.ToString();
        }

        public static void BuildCreateCommand(this DbCommand command, Table table)
        {
            var first = true;

            var createTable = new StringBuilder();
            var foreignKeys = new StringBuilder();

            createTable.AppendLine($"CREATE TABLE {table.TableName} (");
            foreach (var column in table.Columns)
            {
                var prepend = first ? "" : ", ";
                createTable.AppendLine($"{prepend}{column.CreateSql()}");

                var foreignKey = column.CreateForeignKeySql();
                if (!string.IsNullOrEmpty(foreignKey))
                    foreignKeys.AppendLine($", {foreignKey}");

                first = false;
            }

            createTable.AppendLine(foreignKeys.ToString());

            createTable.AppendLine(");");
            command.CommandText = createTable.ToString();
        }
    }
}
