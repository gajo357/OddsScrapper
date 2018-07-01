using System;

namespace OddsScrapper.Repository.DbBuilder
{
    public class ForegnKeyTableColumn : SimpleTableColumn
    {
        private string ForeignTable { get; }
        private bool OnDeleteCascade { get; }

        public ForegnKeyTableColumn(string ownerTable, string foreignTable, string columnName, bool onDeleteCascade = false)
            : base($"FK_{ownerTable}_{foreignTable}_{columnName}", "INTEGER")
        {
            ForeignTable = foreignTable;
            OnDeleteCascade = onDeleteCascade;
        }

        public override string CreateForeignKeySql()
        {
            return $"FOREIGN KEY ({ColumnName}) REFERENCES {ForeignTable}(Id) ON DELETE {(OnDeleteCascade ? "CASCADE" : "SET NULL")}";
        }
    }
}
