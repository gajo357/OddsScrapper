using System;

namespace OddsScrapper.Repository.DbBuilder
{
    class ForegnKeyTableColumn : SimpleTableColumn
    {
        private string ForeignTable { get; }

        public ForegnKeyTableColumn(string ownerTable, string foreignTable, string columnName)
            : base($"FK_{ownerTable}_{foreignTable}_{columnName}", "INTEGER")
        {
            ForeignTable = foreignTable;
        }

        public override string CreateForeignKeySql()
        {
            return $"FOREIGN KEY ({ColumnName}) REFERENCES {ForeignTable}(Id)";
        }
    }
}
