using OddsScrapper.Repository.Helpers;
using System.Data.Common;
using System.Data.SQLite;

namespace OddsScrapper.Repository.Extensions
{
    public static class ColumnValueExtensions
    {
        public static DbParameter ToParameter(this ColumnValuePair columnValuePair)
        {
            return new SQLiteParameter(columnValuePair.ValueType, columnValuePair.Value);
        }
    }
}
