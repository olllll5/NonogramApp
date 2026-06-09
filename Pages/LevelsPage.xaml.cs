using NonogramApp.Models;
using NonogramApp.Services;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls;

namespace NonogramApp.Pages;

public partial class LevelsPage : ContentPage
{
    private readonly DatabaseService _dbService;
    private List<Level> _allLevels = new();

    public LevelsPage(DatabaseService dbService)
    {
        InitializeComponent();
        _dbService = dbService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadLevels();
    }

    private async Task LoadLevels()
    {
        try
        {
            _allLevels = await _dbService.GetLevels();
            var completedLevels = await _dbService.GetCompletedLevels();
            
            foreach (var level in _allLevels)
            {
                level.IsCompleted = completedLevels.ContainsKey(level.Id) && completedLevels[level.Id];
            }
            
            LevelsListView.ItemsSource = _allLevels;
        }
        catch (System.Exception ex)
        {
            await DisplayAlert("Ошибка", $"База данных недоступна: {ex.Message}", "ОК");
        }
    }

    private async void OnLevelSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Level selectedLevel)
        {
            ((CollectionView)sender).SelectedItem = null;
            
            var navParam = new Dictionary<string, object>
            {
                { "SelectedLevel", selectedLevel }
            };
            
            await Shell.Current.GoToAsync(nameof(GamePage), navParam);
        }
    }
}
