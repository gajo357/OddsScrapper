namespace OddsScrapper.Repository.DbBuilder
{
    public interface ITableColumn
    {
        string CreateSql();
        string CreateForeignKeySql();
    }
}
