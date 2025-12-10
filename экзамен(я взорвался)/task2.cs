using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;

public static void CompareInsertSpeed(string connectionString)
{
    const int recordCount = 10_000;

    using SqlConnection connection = new SqlConnection(connectionString);
    connection.Open();

    Stopwatch insertWatch = Stopwatch.StartNew();

    for (int i = 0; i < recordCount; i++)
    {
        using SqlCommand cmd = new SqlCommand(
            "INSERT INTO TargetTable (Id, Name, Age) VALUES (@Id, @Name, @Age)",
            connection);

        cmd.Parameters.AddWithValue("@Id", i);
        cmd.Parameters.AddWithValue("@Name", "User" + i);
        cmd.Parameters.AddWithValue("@Age", 20 + (i % 30));

        cmd.ExecuteNonQuery();
    }

    insertWatch.Stop();

    new SqlCommand("TRUNCATE TABLE TargetTable", connection).ExecuteNonQuery();

    DataTable table = new DataTable();
    table.Columns.Add("Id", typeof(int));
    table.Columns.Add("Name", typeof(string));
    table.Columns.Add("Age", typeof(int));

    for (int i = 0; i < recordCount; i++)
        table.Rows.Add(i, "User" + i, 20 + (i % 30));

    Stopwatch bulkWatch = Stopwatch.StartNew();

    using SqlBulkCopy bulkCopy = new SqlBulkCopy(connection);
    bulkCopy.DestinationTableName = "TargetTable";
    bulkCopy.BatchSize = recordCount;
    bulkCopy.WriteToServer(table);

    bulkWatch.Stop();

    Console.WriteLine($"INSERT time: {insertWatch.ElapsedMilliseconds} ms");
    Console.WriteLine($"SqlBulkCopy time: {bulkWatch.ElapsedMilliseconds} ms");
}
