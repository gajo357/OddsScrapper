namespace OddsScrapper.Repository.DbBuilder
{
    public interface ITableColumn
    {
        string ColumnName { get; }
        string CreateSql();
        string CreateForeignKeySql();
    }
}
