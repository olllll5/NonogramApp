using NonogramApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NonogramApp.Services;

public class DatabaseService
{
    private readonly Supabase.Client _client;
    private string _currentUserId = "test-user-id"; // Временный ID для теста

    public DatabaseService(Supabase.Client client)
    {
        _client = client;
    }
    
    public void SetCurrentUser(string userId)
    {
        _currentUserId = userId;
    }
    
    public async Task<List<Level>> GetLevels()
    {
        var response = await _client.From<Level>().Get();
        return response.Models;
    }
    
    public async Task<List<LevelLayer>> GetLevelLayers(long levelId)
    {
        var response = await _client.From<LevelLayer>()
            .Where(x => x.LevelId == levelId)
            .Order(x => x.LayerIndex)
            .Get();

        return response.Models;
    }
    
    public async Task<List<UserProgress>> GetUserProgress()
    {
        if (string.IsNullOrEmpty(_currentUserId))
            return new List<UserProgress>();
            
        var response = await _client.From<UserProgress>()
            .Where(x => x.UserId == _currentUserId)
            .Get();
            
        return response.Models;
    }
    
    public async Task<Dictionary<long, bool>> GetCompletedLevels()
    {
        var result = new Dictionary<long, bool>();
        
        if (string.IsNullOrEmpty(_currentUserId))
            return result;
            
        var progress = await GetUserProgress();
        
        foreach (var p in progress)
        {
            result[p.LevelId] = p.IsCompleted;
        }
        
        return result;
    }
    
    public async Task<bool> SaveLevelProgress(long levelId)
    {
        if (string.IsNullOrEmpty(_currentUserId))
            return false;
        
        try
        {
            var existing = await _client.From<UserProgress>()
                .Where(x => x.UserId == _currentUserId && x.LevelId == levelId)
                .Get();
            
            if (existing.Models.Any())
            {
                var current = existing.Models.First();
                if (!current.IsCompleted)
                {
                    current.IsCompleted = true;
                    current.LastPlayed = DateTime.UtcNow;
                    
                    await _client.From<UserProgress>()
                        .Where(x => x.UserId == _currentUserId && x.LevelId == levelId)
                        .Update(current);
                }
            }
            else
            {
                var progress = new UserProgress
                {
                    UserId = _currentUserId,
                    LevelId = levelId,
                    IsCompleted = true,
                    LastPlayed = DateTime.UtcNow
                };
                
                await _client.From<UserProgress>().Insert(progress);
            }
            
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка сохранения прогресса: {ex.Message}");
            return false;
        }
    }
}
