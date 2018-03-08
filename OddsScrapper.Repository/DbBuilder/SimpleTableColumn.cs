namespace OddsScrapper.Repository.DbBuilder
{
    class SimpleTableColumn : ITableColumn
    {
        public string ColumnName { get; }
        protected string ColumnType { get; }

        public SimpleTableColumn(string columnName, string columnType)
        {
            ColumnName = columnName;
            ColumnType = columnType;
        }

        public virtual string CreateSql()
        {
            return $"{ColumnName} {ColumnType}";
        }

        public virtual string CreateForeignKeySql()
        {
            return string.Empty;
        }
    }
}
