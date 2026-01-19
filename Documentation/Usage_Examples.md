# Примеры использования WindowsCleanerUtility

## Базовое использование

### Запуск полной очистки
```csharp
// Загрузка настроек пользователя
var userSettings = UserSettings.Load();

// Создание контейнера зависимостей
var container = new ServiceContainer();
var logger = new LoggerService();
container.Register<ILoggerService>(logger);

var fileOps = new FileOperationsService(logger);
container.Register<IFileOperations>(fileOps);

// Создание сервисов очистки
var tempCleaner = new TemporaryFilesCleaner(fileOps, logger);
var browserCleaner = new BrowserDataCleaner(fileOps, logger);
var systemLogsCleaner = new SystemLogsCleaner(fileOps, logger);
var oldFilesCleaner = new OldFilesCleaner(fileOps, logger, userSettings.DaysForOldFiles);
var dnsCleaner = new DNSCacheCleaner(logger);

// Создание менеджера очистки
var cleaningManager = new CleaningManager(
    new ICleanerService[] { 
        tempCleaner, 
        browserCleaner, 
        systemLogsCleaner, 
        oldFilesCleaner, 
        dnsCleaner 
    }, 
    logger
);

// Создание опций очистки на основе пользовательских настроек
var cleaningOptions = new CleaningOptions
{
    IncludeTemporaryFiles = userSettings.IncludeTemporaryFiles,
    IncludeLogFiles = userSettings.IncludeLogFiles,
    IncludeEventLogs = userSettings.IncludeEventLogs,
    IncludeOldFiles = userSettings.IncludeOldFiles,
    IncludeBrowserHistory = userSettings.IncludeBrowserHistory,
    IncludeBrowserCookies = userSettings.IncludeBrowserCookies,
    IncludeDNSTempFiles = userSettings.IncludeDNSTempFiles
};

// Выполнение очистки
var cancellationTokenSource = new CancellationTokenSource();
try
{
    bool success = await cleaningManager.PerformCleaningAsync(cleaningOptions, cancellationTokenSource.Token);
    Console.WriteLine($"Очистка завершена {(success ? "успешно" : "с ошибками")}");
}
catch (OperationCanceledException)
{
    Console.WriteLine("Очистка была отменена пользователем");
}
```

### Выборочная очистка
```csharp
// Очистка только временных файлов
var tempOnlyOptions = new CleaningOptions
{
    IncludeTemporaryFiles = true,
    IncludeLogFiles = false,
    IncludeEventLogs = false,
    IncludeOldFiles = false,
    IncludeBrowserHistory = false,
    IncludeBrowserCookies = false,
    IncludeDNSTempFiles = false
};

var success = await cleaningManager.PerformCleaningAsync(tempOnlyOptions);
```

### Проверка прав администратора
```csharp
using System.Security.Principal;

public static bool IsAdministrator()
{
    try
    {
        var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }
    catch
    {
        return false;
    }
}

// Использование в приложении
if (!IsAdministrator())
{
    MessageBox.Show(
        "Для полноценной очистки рекомендуется запустить приложение с правами администратора", 
        "Предупреждение", 
        MessageBoxButtons.OK, 
        MessageBoxIcon.Warning
    );
}
```

### Интеграция с планировщиком задач
```csharp
// Пример создания задачи для планировщика (требуется дополнительная библиотека)
using Microsoft.Win32.TaskScheduler;

public static void CreateScheduledTask()
{
    using (var taskService = new TaskService())
    {
        var task = taskService.NewTask();
        task.RegistrationInfo.Description = "Периодическая очистка системы Windows";
        
        task.Triggers.Add(new WeeklyTrigger
        {
            DaysOfWeek = DaysOfTheWeek.Sunday,
            StartBoundary = DateTime.Today.AddHours(2) // 2 AM
        });
        
        task.Actions.Add(new ExecAction(
            @"C:\Path\To\WindowsCleanerUtility.exe", 
            "--scheduled", 
            @"C:\Path\To\"
        ));
        
        taskService.RootFolder.RegisterTaskDefinition(
            "Windows Cleaner Utility Auto-Clean", 
            task
        );
    }
}
```

### Обработка исключений и логирование
```csharp
public async Task<bool> SafeCleanAsync(ICleanerService service, ILoggerService logger)
{
    try
    {
        logger.LogInfo($"Запуск очистки: {service.Name}");
        var result = await service.CleanAsync();
        logger.LogInfo($"Очистка завершена: {service.Name}, успех: {result}");
        return result;
    }
    catch (OperationCanceledException)
    {
        logger.LogWarning($"Очистка отменена: {service.Name}");
        throw; // Перебрасываем исключение отмены
    }
    catch (Exception ex)
    {
        logger.LogError($"Ошибка при очистке {service.Name}: {ex.Message}");
        logger.LogException(ex);
        return false;
    }
}
```

## Расширение функциональности

### Добавление нового сервиса очистки
```csharp
// 1. Реализуйте интерфейс ICleanerService
public class CustomCleaner : ICleanerService
{
    public string Name => "Custom Cleaner";
    public string Description => "Описание вашего сервиса очистки";

    private readonly IFileOperations _fileOperations;
    private readonly ILoggerService _logger;

    public CustomCleaner(IFileOperations fileOperations, ILoggerService logger)
    {
        _fileOperations = fileOperations;
        _logger = logger;
    }

    public async Task<bool> CleanAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInfo("Запуск пользовательской очистки");
        
        // Ваш код очистки
        
        _logger.LogInfo("Пользовательская очистка завершена");
        return true;
    }
}

// 2. Зарегистрируйте его в контейнере
container.Register<ICleanerService>(new CustomCleaner(fileOps, logger));
```

### Добавление поддержки нового языка
```csharp
// Создайте файл ресурсов Localizations/ru-RU.json
{
  "AppName": "Утилита очистки Windows",
  "CleanButton": "Очистить",
  "CancelButton": "Отмена",
  "ExitButton": "Выход",
  "CleaningOptions": "Параметры очистки",
  "IncludeTempFiles": "Удалить временные файлы",
  "IncludeLogFiles": "Удалить логи",
  // ... другие переводы
}

// Используйте в приложении
public class LocalizationService
{
    private readonly Dictionary<string, string> _translations;
    
    public LocalizationService(string languageCode = "ru-RU")
    {
        var resourcePath = $"Localizations/{languageCode}.json";
        var json = File.ReadAllText(resourcePath);
        _translations = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
    }
    
    public string GetString(string key)
    {
        return _translations.ContainsKey(key) ? _translations[key] : key;
    }
}
```

Эти примеры демонстрируют гибкость и расширяемость новой архитектуры приложения.