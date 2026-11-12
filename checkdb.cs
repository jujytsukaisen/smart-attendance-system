using System;
using Microsoft.Data.Sqlite;
class Program {
    static void Main() {
        using var connection = new SqliteConnection("Data Source=c:\Users\MSI CYBORG 15\Downloads\smart-attendance-system\backend\src\SmartAttendance.API\smart_attendance.db");
        connection.Open();
        var command = connection.CreateCommand();
        command.CommandText = "SELECT Name, StartTime, EndTime FROM Shifts WHERE Name LIKE '%????%' LIMIT 1";
        using var reader = command.ExecuteReader();
        while (reader.Read()) {
            Console.WriteLine("Shift: " + reader.GetString(0) + " Start: " + reader.GetString(1) + " End: " + reader.GetString(2));
        }
    }
}
