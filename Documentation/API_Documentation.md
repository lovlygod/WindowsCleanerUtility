# Документация API WindowsCleanerUtility

## Интерфейсы

### ILoggerService
Интерфейс для ведения логов с различными уровнями.

#### Методы:
- `void Log(LogLevel level, string message)` - Записывает сообщение с указанным уровнем
- `void LogTrace(string message)` - Записывает трассировочное сообщение
- `void LogDebug(string message)` - Записывает отладочное сообщение
- `void LogInfo(string message)` - Записывает информационное сообщение
- `void LogWarning(string message)` - Записывает предупреждение
- `void LogError(string message)` - Записывает сообщение об ошибке
- `void LogFatal(string message)` - Записывает фатальное сообщение
- `void LogException(Exception exception, string message = null)` - Записывает исключение

#### Перечисление LogLevel:
- Trace
- Debug
- Info
- Warning
- Error
- Fatal

### IFileOperations
Интерфейс для операций с файлами.

#### Методы:
- `Task<bool> MoveToRecycleBinAsync(string filePath)` - Перемещает файл в корзину
- `Task<bool> DeletePermanentlyAsync(string filePath)` - Удаляет файл без возможности восстановления
- `Task<bool> IsFileInUseAsync(string filePath)` - Проверяет, используется ли файл другим процессом

### ICleanerService
Интерфейс для сервисов очистки.

#### Свойства:
- `string Name` - Имя сервиса
- `string Description` - Описание сервиса

#### Методы:
- `Task<bool> CleanAsync(CancellationToken cancellationToken = default)` - Выполняет процесс очистки

## Классы

### CleaningOptions
Класс, содержащий настройки для процесса очистки.

#### Свойства:
- `bool IncludeTemporaryFiles` - Включить ли удаление временных файлов
- `bool IncludeLogFiles` - Включить ли удаление лог-файлов
- `bool IncludeEventLogs` - Включить ли очистку журналов событий
- `bool IncludeOldFiles` - Включить ли удаление старых файлов
- `bool IncludeBrowserHistory` - Включить ли очистку истории браузеров
- `bool IncludeBrowserCookies` - Включить ли очистку cookies
- `bool IncludeDNSTempFiles` - Включить ли очистку DNS-кэша
- `int DaysForOldFiles` - Количество дней для определения "старых" файлов
- `bool MoveToRecycleBin` - Перемещать ли файлы в корзину
- `bool ShowProgress` - Показывать ли прогресс

### CleaningManager
Класс, управляющий процессом очистки.

#### Методы:
- `Task<bool> PerformCleaningAsync(CleaningOptions options, CancellationToken cancellationToken = default)` - Запускает процесс очистки с указанными настройками

### UserSettings
Класс для хранения пользовательских настроек.

#### Свойства:
- `bool IncludeTemporaryFiles` - Включить ли удаление временных файлов
- `bool IncludeLogFiles` - Включить ли удаление лог-файлов
- `bool IncludeEventLogs` - Включить ли очистку журналов событий
- `bool IncludeOldFiles` - Включить ли удаление старых файлов
- `bool IncludeBrowserHistory` - Включить ли очистку истории браузеров
- `bool IncludeBrowserCookies` - Включить ли очистку cookies
- `bool IncludeDNSTempFiles` - Включить ли очистку DNS-кэша
- `int DaysForOldFiles` - Количество дней для определения "старых" файлов
- `bool MoveToRecycleBin` - Перемещать ли файлы в корзину
- `bool ShowProgress` - Показывать ли прогресс
- `string Theme` - Тема интерфейса ("Dark", "Light", "System")
- `string Language` - Язык интерфейса

#### Методы:
- `static UserSettings Load()` - Загружает настройки из файла
- `void Save()` - Сохраняет настройки в файл

## Сервисы очистки

### TemporaryFilesCleaner
Сервис для удаления временных файлов из различных системных местоположений.

### BrowserDataCleaner
Сервис для очистки данных браузеров (история, cookies).

### SystemLogsCleaner
Сервис для удаления системных логов и журналов событий.

### OldFilesCleaner
Сервис для удаления старых файлов, основываясь на дате создания/изменения.

### DNSCacheCleaner
Сервис для очистки DNS-кэша.

## Примеры использования

### Создание и использование сервиса очистки
```csharp
var logger = new LoggerService();
var fileOps = new FileOperationsService(logger);
var cleaner = new TemporaryFilesCleaner(fileOps, logger);

var result = await cleaner.CleanAsync();
```

### Использование менеджера очистки
```csharp
var options = new CleaningOptions
{
    IncludeTemporaryFiles = true,
    IncludeLogFiles = true,
    IncludeBrowserHistory = true
};

var manager = new CleaningManager(cleanerServices, logger);
var success = await manager.PerformCleaningAsync(options);