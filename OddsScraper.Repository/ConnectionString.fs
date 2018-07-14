namespace OddsScraper.Repository

module ConnectionString =
    open System.IO
    open System.Data.SQLite

    let createConnectionString path =
        let file = new FileInfo(path)
        let builder = new SQLiteConnectionStringBuilder()
        builder.DataSource <- file.FullName
        builder.ConnectionString

