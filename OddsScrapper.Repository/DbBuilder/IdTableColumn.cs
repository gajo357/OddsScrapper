﻿namespace OddsScrapper.Repository.DbBuilder
{
    public class IdTableColumn : SimpleTableColumn
    {
        public IdTableColumn() 
            : base("Id", "INTEGER")
        {
        }

        public override string CreateSql()
        {
            return $"{base.CreateSql()} PRIMARY KEY AUTOINCREMENT NOT NULL";
        }
    }
}
