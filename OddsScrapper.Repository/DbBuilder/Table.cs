using System.Collections.Generic;

namespace OddsScrapper.Repository.DbBuilder
{
    public class Table
    {
        public string TableName { get; }
        public Table(string name)
        {
            TableName = name;
        }

        private IList<ITableColumn> _columns = new List<ITableColumn>();
        public IEnumerable<ITableColumn> Columns => _columns;

        public Table AddIdColumn()
        {
            _columns.Add(new IdTableColumn());

            return this;
        }

        public Table AddForeignKeyColumn(Table foreignTable, string columnName = "Id", bool onDeleteCascade = false)
        {
            _columns.Add(new ForegnKeyTableColumn(TableName, foreignTable.TableName, columnName, onDeleteCascade));

            return this;
        }

        public Table AddDoubleColumn(string columnName)
        {
            _columns.Add(new SimpleTableColumn(columnName, "DOUBLE"));

            return this;
        }

        public Table AddBoolColumn(string columnName)
        {
            _columns.Add(new SimpleTableColumn(columnName, "BOOL"));

            return this;
        }

        public Table AddDatetimeColumn(string columnName)
        {
            _columns.Add(new SimpleTableColumn(columnName, "DATETIME"));

            return this;
        }

        public Table AddIntegerColumn(string columnName)
        {
            _columns.Add(new SimpleTableColumn(columnName, "INTEGER"));

            return this;
        }

        public Table AddNameColumn()
        {
            return AddTextColumn("Name");
        }

        public Table AddTextColumn(string columnName)
        {
            _columns.Add(new SimpleTableColumn(columnName, "TEXT"));

            return this;
        }
    }
}
