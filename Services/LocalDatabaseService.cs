using SQLite;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using RoutineKeeper.Models;

namespace RoutineKeeper.Services;

public class LocalDatabaseService
{
    private SQLiteAsyncConnection? _database;

    private async Task InitAsync()
    {
        if (_database != null)
            return;

        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "RoutineKeeper.db3");
        _database = new SQLiteAsyncConnection(dbPath);
        
        await _database.CreateTableAsync<ActivityItem>();
        await _database.CreateTableAsync<NoteItem>();
        await _database.CreateTableAsync<ChatMessage>();
    }

    // --- ACTIVITY CRUD ---

    public async Task<List<ActivityItem>> GetActivitiesAsync()
    {
        await InitAsync();
        return await _database!.Table<ActivityItem>().ToListAsync();
    }

    public async Task<List<ActivityItem>> GetActivitiesForDateAsync(System.DateTime date)
    {
        await InitAsync();
        // Ponieważ przechowujemy pełne Date, możemy pobrać wszystkie na dany dzień w pamięci, 
        // lub zdefiniować przedział jeśli optymalizacja będzie konieczna.
        var all = await _database!.Table<ActivityItem>().ToListAsync();
        return all.Where(a => a.Date.Date == date.Date).ToList();
    }

    public async Task<int> SaveActivityAsync(ActivityItem item)
    {
        await InitAsync();
        if (item.Id != 0)
        {
            return await _database!.UpdateAsync(item);
        }
        else
        {
            return await _database!.InsertAsync(item);
        }
    }

    public async Task<int> DeleteActivityAsync(ActivityItem item)
    {
        await InitAsync();
        return await _database!.DeleteAsync(item);
    }

    // --- NOTE CRUD ---

    public async Task<List<NoteItem>> GetNotesAsync()
    {
        await InitAsync();
        return await _database!.Table<NoteItem>().ToListAsync();
    }

    public async Task<int> SaveNoteAsync(NoteItem item)
    {
        await InitAsync();
        if (item.Id != 0)
        {
            return await _database!.UpdateAsync(item);
        }
        else
        {
            return await _database!.InsertAsync(item);
        }
    }

    public async Task<int> DeleteNoteAsync(NoteItem item)
    {
        await InitAsync();
        return await _database!.DeleteAsync(item);
    }

    // --- CHAT MESSAGE CRUD ---

    public async Task<List<ChatMessage>> GetChatMessagesForDateAsync(System.DateTime date)
    {
        await InitAsync();
        var all = await _database!.Table<ChatMessage>().ToListAsync();
        return all.Where(m => m.Date.Date == date.Date).OrderBy(m => m.Timestamp).ToList();
    }

    public async Task<int> SaveChatMessageAsync(ChatMessage item)
    {
        await InitAsync();
        if (item.Id != 0)
        {
            return await _database!.UpdateAsync(item);
        }
        else
        {
            return await _database!.InsertAsync(item);
        }
    }

    public async Task<int> DeleteChatMessageAsync(ChatMessage item)
    {
        await InitAsync();
        return await _database!.DeleteAsync(item);
    }
}
