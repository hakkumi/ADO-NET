using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;

class Program
{
    static void Main()
    {
        string connectionString = "Server=.;Database=TestDb;Trusted_Connection=True;";
        string csvFilePath = @"C:\data\data.csv";

        using SqlConnection connection = new SqlConnection(connectionString);
        connection.Open();

        using SqlTransaction transaction = connection.BeginTransaction();

        using SqlBulkCopy bulkCopy = new SqlBulkCopy(
            connection,
            SqlBulkCopyOptions.TableLock,
            transaction);

        bulkCopy.DestinationTableName = "dbo.TargetTable";
        bulkCopy.BatchSize = 100_000;
        bulkCopy.BulkCopyTimeout = 0;

        DataTable table = new DataTable();
        table.Columns.Add("Id", typeof(int));
        table.Columns.Add("Name", typeof(string));
        table.Columns.Add("Age", typeof(int));

        using StreamReader reader = new StreamReader(csvFilePath);
        reader.ReadLine(); // пропуск заголовка

        while (!reader.EndOfStream)
        {
            string[] values = reader.ReadLine().Split(',');
            table.Rows.Add(
                int.Parse(values[0]),
                values[1],
                int.Parse(values[2]));
        }

        bulkCopy.WriteToServer(table);
        transaction.Commit();
    }
}
