using ProVoiceLedger.Core.Models;
using SQLite;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
namespace ProVoiceLedger.Core.Services
{
    public class SessionDatabase
    {
        private readonly SQLiteAsyncConnection _database;

        public SessionDatabase(string dbPath)
        {
            _database = new SQLiteAsyncConnection(dbPath);
            _ = InitializeAsync(); // fire-and-forget setup
        }

        private async Task InitializeAsync()
        {
            await _database.CreateTableAsync<Session>();
        }

        // 🔍 Get all sessions, newest first
        public async Task<List<Session>> GetSessionsAsync()
        {
            return await _database
                .Table<Session>()
                .OrderByDescending(s => s.StartTime)
                .ToListAsync();
        }

        // 🎯 Get a specific session by ID
        public async Task<Session?> GetSessionAsync(int id)
        {
            return await _database
                .Table<Session>()
                .Where(s => s.Id == id)
                .FirstOrDefaultAsync();
        }

        // 💾 Save a session (insert or update)
        public async Task<int> SaveSessionAsync(Session session)
        {
            if (session.Id != 0)
                return await _database.UpdateAsync(session);
            else
                return await _database.InsertAsync(session);
        }

        // 🗑️ Delete a session by ID
        public async Task<int> DeleteSessionAsync(int id)
        {
            // Defensive: ensure session exists first (optional)
            var sessionToDelete = await GetSessionAsync(id);
            if (sessionToDelete != null)
            {
                return await _database.DeleteAsync(sessionToDelete);
            }
            return 0;
        }

        // 📆 Optional: get sessions within a date range
        public async Task<List<Session>> GetSessionsInRangeAsync(DateTime from, DateTime to)
        {
            return await _database
                .Table<Session>()
                .Where(s => s.StartTime >= from && s.StartTime <= to)
                .OrderByDescending(s => s.StartTime)
                .ToListAsync();
        }
    }
}
