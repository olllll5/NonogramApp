using NonogramApp.Models;
using NonogramApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls;

namespace NonogramApp.Pages;

[QueryProperty(nameof(SelectedLevel), "SelectedLevel")]
public partial class GamePage : ContentPage
{
    private readonly DatabaseService _dbService;
    private Level _selectedLevel = null!;
    private List<LevelLayer> _layers = new();
    private int _currentLayerIndex = 0;
    private int[,] _playerBoard = null!;
    private int _errorsCount = 0;
    private const int MaxErrors = 5;
    private bool _isEyeMode = false;

    public GamePage(DatabaseService dbService)
    {
        InitializeComponent();
        _dbService = dbService;
    }

    public Level SelectedLevel
    {
        get => _selectedLevel;
        set => _selectedLevel = value;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        if (_selectedLevel == null)
        {
            await DisplayAlert("Ошибка", "Уровень не выбран!", "ОК");
            await Shell.Current.GoToAsync("..");
            return;
        }
        
        await LoadLayerData();
    }

    private async Task LoadLayerData()
    {
        try
        {
            _layers = await _dbService.GetLevelLayers(_selectedLevel.Id);

            if (_layers != null && _layers.Any())
            {
                _currentLayerIndex = 0;
                _errorsCount = 0;
                UpdateErrorsDisplay();
                GenerateBoard(_selectedLevel.Width, _selectedLevel.Height, _layers[0].GridData);
            }
            else
            {
                await DisplayAlert("Ошибка", $"В базе данных нет слоев для уровня '{_selectedLevel.Name}'!", "ОК");
                await Shell.Current.GoToAsync("..");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка сети", $"Не удалось загрузить слои: {ex.Message}", "ОК");
            await Shell.Current.GoToAsync("..");
        }
    }

    private void GenerateBoard(int width, int height, List<List<int>> solution)
    {
        GameGrid.Children.Clear();
        GameGrid.RowDefinitions.Clear();
        GameGrid.ColumnDefinitions.Clear();

        _playerBoard = new int[height, width];
        double cellSize = 35;

        GameGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        for (int i = 0; i < height; i++) 
            GameGrid.RowDefinitions.Add(new RowDefinition { Height = cellSize });

        GameGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        for (int j = 0; j < width; j++) 
            GameGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = cellSize });

        // Подсказки строк
        for (int r = 0; r < height; r++)
        {
            var hints = CalculateHints(solution[r]);
            var label = new Label
            {
                Text = string.Join(" ", hints),
                VerticalTextAlignment = TextAlignment.Center,
                HorizontalTextAlignment = TextAlignment.End,
                Margin = new Thickness(5, 0, 5, 0),
                FontSize = 12,
                TextColor = Colors.Black,
                FontAttributes = FontAttributes.Bold
            };
            Grid.SetRow(label, r + 1);
            Grid.SetColumn(label, 0);
            GameGrid.Children.Add(label);
        }

        // Подсказки столбцов
        for (int c = 0; c < width; c++)
        {
            var colData = new List<int>();
            for (int r = 0; r < height; r++) 
                colData.Add(solution[r][c]);

            var hints = CalculateHints(colData);
            var label = new Label
            {
                Text = string.Join("\n", hints),
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.End,
                Margin = new Thickness(0, 5, 0, 5),
                FontSize = 12,
                TextColor = Colors.Black,
                FontAttributes = FontAttributes.Bold
            };
            Grid.SetRow(label, 0);
            Grid.SetColumn(label, c + 1);
            GameGrid.Children.Add(label);
        }

        // Клетки
        for (int r = 0; r < height; r++)
        {
            for (int c = 0; c < width; c++)
            {
                var box = new Border
                {
                    WidthRequest = cellSize,
                    HeightRequest = cellSize,
                    BackgroundColor = Colors.White,
                    Stroke = Colors.Gray,
                    StrokeThickness = 1
                };

                int row = r, col = c;
                var tap = new TapGestureRecognizer();
                tap.Tapped += (s, e) => OnCellTapped(row, col, box, solution);
                box.GestureRecognizers.Add(tap);
                
                if (_isEyeMode && solution[r][c] == 1)
                {
                    box.BackgroundColor = Colors.LightGreen;
                    _playerBoard[row, col] = 1;
                }

                Grid.SetRow(box, r + 1);
                Grid.SetColumn(box, c + 1);
                GameGrid.Children.Add(box);
            }
        }
        
        LayerInfoLabel.Text = $"Слой {_currentLayerIndex + 1}/{_layers.Count}";
        EyeButton.IsVisible = _errorsCount > 0;
    }

    private void OnCellTapped(int row, int col, Border box, List<List<int>> solution)
    {
        if (_errorsCount >= MaxErrors) return;
        
        int newValue = _playerBoard[row, col] == 0 ? 1 : 0;
        
        if (!_isEyeMode && newValue == 1 && solution[row][col] == 0)
        {
            _errorsCount++;
            UpdateErrorsDisplay();
            
            if (_errorsCount >= MaxErrors)
            {
                DisplayAlert("Поражение", $"Вы превысили лимит ошибок ({MaxErrors})!", "ОК");
                return;
            }
            
            box.BackgroundColor = Colors.Red;
            Task.Delay(200).ContinueWith(_ => 
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (_playerBoard[row, col] == 0)
                        box.BackgroundColor = Colors.White;
                });
            });
            return;
        }
        
        _playerBoard[row, col] = newValue;
        box.BackgroundColor = newValue == 1 ? Colors.Black : Colors.White;
    }

    private List<int> CalculateHints(List<int> line)
    {
        var hints = new List<int>();
        int count = 0;
        foreach (var cell in line)
        {
            if (cell == 1) count++;
            else if (count > 0) { hints.Add(count); count = 0; }
        }
        if (count > 0) hints.Add(count);
        return hints.Count > 0 ? hints : new List<int> { 0 };
    }

    private void UpdateErrorsDisplay()
    {
        ErrorsLabel.Text = $"{_errorsCount}/{MaxErrors}";
        EyeButton.IsVisible = _errorsCount > 0;
    }

    private void OnNextLayerClicked()
    {
        if (_currentLayerIndex < _layers.Count - 1) 
        { 
            _currentLayerIndex++; 
            _errorsCount = 0;
            UpdateErrorsDisplay();
            UpdateLayer(); 
        }
    }

    private void UpdateLayer()
    {
        if (_layers != null && _layers.Count > _currentLayerIndex)
        {
            _isEyeMode = false;
            EyeButton.BackgroundColor = Colors.White;
            EyeButton.Text = "👁";
            GenerateBoard(_selectedLevel.Width, _selectedLevel.Height, _layers[_currentLayerIndex].GridData);
        }
    }

    private async void OnCheckResultClicked(object sender, EventArgs e)
    {
        if (_playerBoard == null || !_layers.Any()) return;
        
        var solution = _layers[_currentLayerIndex].GridData;
        bool isCorrect = true;

        for (int r = 0; r < _selectedLevel.Height; r++)
        {
            for (int c = 0; c < _selectedLevel.Width; c++)
            {
                if (_playerBoard[r, c] != solution[r][c]) 
                { 
                    isCorrect = false; 
                    break; 
                }
            }
            if (!isCorrect) break;
        }

        if (isCorrect)
        {
            if (_currentLayerIndex == _layers.Count - 1)
            {
                await _dbService.SaveLevelProgress(_selectedLevel.Id);
                
                string starsMessage = _errorsCount == 0 ? "⭐️⭐️⭐️ ИДЕАЛЬНО! 3 звезды!" : 
                                     (_errorsCount <= 2 ? "⭐⭐ Хорошо! 2 звезды!" : "⭐ Неплохо! 1 звезда!");
                
                bool goToMenu = await DisplayAlert("ПОЗДРАВЛЯЕМ!", 
                    $"Вы полностью собрали 3D модель!\n\n{starsMessage}", 
                    "В меню", "Играть снова");
                
                if (goToMenu)
                    await Shell.Current.GoToAsync("..");
                else
                {
                    _errorsCount = 0;
                    _currentLayerIndex = 0;
                    _isEyeMode = false;
                    UpdateErrorsDisplay();
                    UpdateLayer();
                }
            }
            else
            {
                bool next = await DisplayAlert("Верно!", 
                    $"Слой {_currentLayerIndex + 1} решен! Переходим к следующему?", 
                    "Да", "Нет");
                
                if (next)
                    OnNextLayerClicked();
            }
        }
        else
        {
            await DisplayAlert("Ошибка", "Картинка не совпадает с решением. Попробуйте еще раз!", "ОК");
        }
    }

    private async void OnToggleEyeClicked(object sender, EventArgs e)
    {
        _isEyeMode = !_isEyeMode;
        
        if (_isEyeMode)
        {
            EyeButton.BackgroundColor = Colors.Gold;
            EyeButton.Text = "👁‍🗨";
            
            await DisplayAlert("Режим подсказки", "Зеленые клетки показывают правильное решение на 10 секунд", "ОК");
            
            UpdateLayerWithHint();
            
            await Task.Delay(10000);
            
            if (_isEyeMode)
            {
                _isEyeMode = false;
                EyeButton.BackgroundColor = Colors.White;
                EyeButton.Text = "👁";
                UpdateLayer();
            }
        }
        else
        {
            EyeButton.BackgroundColor = Colors.White;
            EyeButton.Text = "👁";
            UpdateLayer();
        }
    }
    
    private void UpdateLayerWithHint()
    {
        var solution = _layers[_currentLayerIndex].GridData;
        
        for (int r = 0; r < _selectedLevel.Height; r++)
        {
            for (int c = 0; c < _selectedLevel.Width; c++)
            {
                int index = (r * _selectedLevel.Width + c) + 1 + _selectedLevel.Width;
                if (index < GameGrid.Children.Count)
                {
                    var border = GameGrid.Children[index] as Border;
                    if (border != null && solution[r][c] == 1)
                    {
                        border.BackgroundColor = Colors.LightGreen;
                        _playerBoard[r, c] = 1;
                    }
                }
            }
        }
    }

    private async void OnMenuClicked(object sender, EventArgs e)
    {
        string action = await DisplayActionSheet("Меню", "Отмена", null, 
            "Выйти в меню уровней", "Перезапустить уровень", "Правила игры");
        
        switch (action)
        {
            case "Выйти в меню уровней":
                await Shell.Current.GoToAsync("..");
                break;
            case "Перезапустить уровень":
                _errorsCount = 0;
                _isEyeMode = false;
                UpdateErrorsDisplay();
                UpdateLayer();
                break;
            case "Правила игры":
                await DisplayAlert("Правила игры", 
                    "Нонограмма - это головоломка, где нужно закрасить клетки так, " +
                    "чтобы получилось изображение.\n\n" +
                    "• Цифры слева и сверху показывают, сколько подряд закрашенных клеток\n" +
                    "• Нажмите на клетку, чтобы закрасить/очистить её\n" +
                    "• У вас есть 5 попыток на ошибку\n" +
                    "• Кнопка 👁 показывает правильное решение на 10 секунд\n" +
                    "• Собирайте все слои, чтобы завершить уровень!",
                    "Понятно");
                break;
        }
    }
}
