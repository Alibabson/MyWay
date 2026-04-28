using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using MyWay.Models;

namespace MyWay.Services
{
    public class DatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService()
        {
            var folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "MyWay");
            Directory.CreateDirectory(folder);
            var dbPath = Path.Combine(folder, "myway.db");
            _connectionString = $"Data Source={dbPath}";
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();

            conn.ExecuteNonQuery(@"
                CREATE TABLE IF NOT EXISTS TaskItems (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Title TEXT NOT NULL,
                    Difficulty INTEGER NOT NULL DEFAULT 1,
                    DueDate TEXT NOT NULL,
                    IsCompleted INTEGER NOT NULL DEFAULT 0,
                    TimeSpentSeconds INTEGER NOT NULL DEFAULT 0
                );");

            conn.ExecuteNonQuery(@"
                CREATE TABLE IF NOT EXISTS Habits (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Title TEXT NOT NULL,
                    CurrentStreak INTEGER NOT NULL DEFAULT 0,
                    BestStreak INTEGER NOT NULL DEFAULT 0,
                    LastCompletedDate TEXT
                );");

            conn.ExecuteNonQuery(@"
                CREATE TABLE IF NOT EXISTS DailyRecords (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Date TEXT NOT NULL UNIQUE,
                    MoodScore INTEGER NOT NULL DEFAULT 3,
                    ExtraPoints INTEGER NOT NULL DEFAULT 0,
                    QuoteOfTheDay TEXT,
                    TaskPoints INTEGER NOT NULL DEFAULT 0
                );");
        }

        // ── TASKS ──────────────────────────────────────────────────────────

        public async Task<List<TaskItem>> GetTasksAsync()
        {
            var list = new List<TaskItem>();
            await using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM TaskItems ORDER BY DueDate ASC, Difficulty DESC";
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var task = new TaskItem
                {
                    Id = reader.GetInt32(0),
                    Title = reader.GetString(1),
                    Difficulty = reader.GetInt32(2),
                    DueDate = DateTime.Parse(reader.GetString(3)),
                    IsCompleted = reader.GetInt32(4) == 1,
                    TimeSpentSeconds = reader.GetInt32(5)
                };
                task.UpdateOverdue();
                list.Add(task);
            }
            return list;
        }

        public async Task<int> AddTaskAsync(TaskItem task)
        {
            await using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO TaskItems (Title, Difficulty, DueDate, IsCompleted, TimeSpentSeconds)
                VALUES ($title, $diff, $due, $done, $time);
                SELECT last_insert_rowid();";
            cmd.Parameters.AddWithValue("$title", task.Title);
            cmd.Parameters.AddWithValue("$diff", task.Difficulty);
            cmd.Parameters.AddWithValue("$due", task.DueDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("$done", task.IsCompleted ? 1 : 0);
            cmd.Parameters.AddWithValue("$time", task.TimeSpentSeconds);
            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task UpdateTaskAsync(TaskItem task)
        {
            await using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                UPDATE TaskItems SET
                    Title=$title, Difficulty=$diff, DueDate=$due,
                    IsCompleted=$done, TimeSpentSeconds=$time
                WHERE Id=$id";
            cmd.Parameters.AddWithValue("$title", task.Title);
            cmd.Parameters.AddWithValue("$diff", task.Difficulty);
            cmd.Parameters.AddWithValue("$due", task.DueDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("$done", task.IsCompleted ? 1 : 0);
            cmd.Parameters.AddWithValue("$time", task.TimeSpentSeconds);
            cmd.Parameters.AddWithValue("$id", task.Id);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task DeleteTaskAsync(int id)
        {
            await using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM TaskItems WHERE Id=$id";
            cmd.Parameters.AddWithValue("$id", id);
            await cmd.ExecuteNonQueryAsync();
        }

        // ── HABITS ─────────────────────────────────────────────────────────

        public async Task<List<Habit>> GetHabitsAsync()
        {
            var list = new List<Habit>();
            await using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM Habits ORDER BY Title";
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var habit = new Habit
                {
                    Id = reader.GetInt32(0),
                    Title = reader.GetString(1),
                    CurrentStreak = reader.GetInt32(2),
                    BestStreak = reader.GetInt32(3),
                    LastCompletedDate = reader.IsDBNull(4) ? null : DateTime.Parse(reader.GetString(4))
                };
                habit.CheckAndResetStreak();
                list.Add(habit);
            }
            return list;
        }

        public async Task<int> AddHabitAsync(Habit habit)
        {
            await using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO Habits (Title, CurrentStreak, BestStreak, LastCompletedDate)
                VALUES ($title, 0, 0, NULL);
                SELECT last_insert_rowid();";
            cmd.Parameters.AddWithValue("$title", habit.Title);
            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task UpdateHabitAsync(Habit habit)
        {
            await using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                UPDATE Habits SET
                    Title=$title, CurrentStreak=$cur, BestStreak=$best, LastCompletedDate=$last
                WHERE Id=$id";
            cmd.Parameters.AddWithValue("$title", habit.Title);
            cmd.Parameters.AddWithValue("$cur", habit.CurrentStreak);
            cmd.Parameters.AddWithValue("$best", habit.BestStreak);
            cmd.Parameters.AddWithValue("$last",
                habit.LastCompletedDate.HasValue
                    ? habit.LastCompletedDate.Value.ToString("yyyy-MM-dd")
                    : (object)DBNull.Value);
            cmd.Parameters.AddWithValue("$id", habit.Id);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task DeleteHabitAsync(int id)
        {
            await using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM Habits WHERE Id=$id";
            cmd.Parameters.AddWithValue("$id", id);
            await cmd.ExecuteNonQueryAsync();
        }

        // ── DAILY RECORDS ──────────────────────────────────────────────────

        public async Task<DailyRecord?> GetTodayRecordAsync()
        {
            await using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM DailyRecords WHERE Date=$date";
            cmd.Parameters.AddWithValue("$date", DateTime.Today.ToString("yyyy-MM-dd"));
            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new DailyRecord
                {
                    Id = reader.GetInt32(0),
                    Date = DateTime.Parse(reader.GetString(1)),
                    MoodScore = reader.GetInt32(2),
                    ExtraPoints = reader.GetInt32(3),
                    QuoteOfTheDay = reader.IsDBNull(4) ? "" : reader.GetString(4),
                    TaskPoints = reader.GetInt32(5)
                };
            }
            return null;
        }

        public async Task<List<DailyRecord>> GetRecordsForPeriodAsync(DateTime from, DateTime to)
        {
            var list = new List<DailyRecord>();
            await using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM DailyRecords WHERE Date >= $from AND Date <= $to ORDER BY Date";
            cmd.Parameters.AddWithValue("$from", from.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("$to", to.ToString("yyyy-MM-dd"));
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new DailyRecord
                {
                    Id = reader.GetInt32(0),
                    Date = DateTime.Parse(reader.GetString(1)),
                    MoodScore = reader.GetInt32(2),
                    ExtraPoints = reader.GetInt32(3),
                    QuoteOfTheDay = reader.IsDBNull(4) ? "" : reader.GetString(4),
                    TaskPoints = reader.GetInt32(5)
                });
            }
            return list;
        }

        public async Task UpsertDailyRecordAsync(DailyRecord record)
        {
            await using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO DailyRecords (Date, MoodScore, ExtraPoints, QuoteOfTheDay, TaskPoints)
                VALUES ($date, $mood, $extra, $quote, $pts)
                ON CONFLICT(Date) DO UPDATE SET
                    MoodScore=$mood, ExtraPoints=$extra,
                    QuoteOfTheDay=$quote, TaskPoints=$pts";
            cmd.Parameters.AddWithValue("$date", record.Date.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("$mood", record.MoodScore);
            cmd.Parameters.AddWithValue("$extra", record.ExtraPoints);
            cmd.Parameters.AddWithValue("$quote", record.QuoteOfTheDay ?? "");
            cmd.Parameters.AddWithValue("$pts", record.TaskPoints);
            await cmd.ExecuteNonQueryAsync();
        }
    }

    internal static class SqliteExtensions
    {
        public static void ExecuteNonQuery(this SqliteConnection conn, string sql)
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();
        }
    }
}
