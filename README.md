# NonogramApp - MAUI Nonogram Game

.NET MAUI приложение для решения нонограмм (head-scratching puzzles) с поддержкой многослойных уровней.

## Структура проекта

```
NonogramApp/
├── Models/
│   ├── BaseModel.cs
│   ├── Level.cs
│   ├── LevelLayer.cs
│   └── UserProgress.cs
├── Services/
│   └── DatabaseService.cs
├── Pages/
│   ├── MainPage.xaml / .cs
│   ├── LevelsPage.xaml / .cs
│   ├── GamePage.xaml / .cs
│   └── ProfilePage.xaml / .cs
├── App.xaml / .cs
├── AppShell.xaml / .cs
└── MauiProgram.cs
```

## Возможности

- 🎮 Решение нонограмм с подсказками
- 📊 Многослойные 3D уровни
- 👁 Режим подсказки (Eye mode)
- ⭐ Система звезд за выполнение
- 💾 Сохранение прогресса в Supabase
- 👤 Личный кабинет пользователя

## Установка

1. Убедитесь, что у вас установлен .NET 8+ и MAUI
2. Клонируйте репозиторий
3. Установите NuGet пакет: `dotnet add package Supabase`
4. Соберите проект: `dotnet build`
5. Запустите приложение

## Конфигурация Supabase

Данные для подключения к Supabase уже настроены в `MauiProgram.cs`:
- URL: `https://xphcflocpgrxthgkumxz.supabase.co`
- API Key: встроенный в код

## Разработка

Проект готов к расширению:
- Добавить аутентификацию пользователей
- Реализовать онлайн лидербоард
- Добавить редактор уровней
- Поддержка темного режима
