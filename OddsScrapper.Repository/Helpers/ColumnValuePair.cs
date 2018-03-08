using System;
using System.Data;

namespace OddsScrapper.Repository.Helpers
{
    public class ColumnValuePair
    {
        public static ColumnValuePair CreateName(string name)
        {
            return Create("Name", name);
        }

        public static ColumnValuePair Create(string column, DateTime? date)
        {
            return Create(column, date, DbType.DateTime);
        }

        public static ColumnValuePair Create(string column, bool value)
        {
            return Create(column, value, DbType.Boolean);
        }

        public static ColumnValuePair Create(string column, int id)
        {
            return Create(column, id, DbType.Int32);
        }

        public static ColumnValuePair Create(string column, double value)
        {
            return Create(column, value, DbType.Double);
        }

        public static ColumnValuePair Create(string column, string value)
        {
            return Create(column, value, DbType.String);
        }

        public static ColumnValuePair Create(string column, object value, DbType dbType)
        {
            return new ColumnValuePair(column, column.ToLower(), value, dbType);
        }

        private ColumnValuePair(string column, string valueName, object value, DbType dbType)
        {
            Column = column;
            ValueName = valueName;
            Value = value;
            ValueType = dbType;
        }

        public string Column { get; }
        public string ValueName { get; }
        public object Value { get; }
        public DbType ValueType { get; }
    }
}
