using Microsoft.Maui.Controls;

namespace NonogramApp.Pages;

public partial class ProfilePage : ContentPage
{
    public ProfilePage()
    {
        InitializeComponent();
    }
    
    private async void OnSaveClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Инфо", "Авторизация будет добавлена позже. Пока вы в демо-режиме.", "ОК");
    }
    
    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Выход", "Вы уверены, что хотите выйти?", "Да", "Нет");
        if (confirm)
        {
            await Shell.Current.GoToAsync("//MainPage");
        }
    }
}
