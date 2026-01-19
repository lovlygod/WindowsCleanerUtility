# Руководство по тестированию WindowsCleanerUtility

## Общие положения

Проект следует принципам модульного тестирования, где каждый компонент тестируется изолированно. Благодаря использованию интерфейсов и внедрения зависимостей, компоненты легко поддаются тестированию с использованием mock-объектов.

## Структура тестов

Рекомендуется создать отдельный проект для тестов:
```
WindowsCleanerUtility.Tests/
├── Unit/
│   ├── Services/
│   │   ├── TemporaryFilesCleanerTests.cs
│   │   ├── BrowserDataCleanerTests.cs
│   │   ├── SystemLogsCleanerTests.cs
│   │   ├── OldFilesCleanerTests.cs
│   │   └── DNSCacheCleanerTests.cs
│   ├── Core/
│   │   ├── LoggerServiceTests.cs
│   │   ├── FileOperationsServiceTests.cs
│   │   └── CleaningManagerTests.cs
│   └── Utils/
│       └── UserSettingsTests.cs
└── Integration/
    └── EndToEndTests.cs
```

## Примеры unit-тестов

### Тестирование сервиса очистки

```csharp
[TestClass]
public class TemporaryFilesCleanerTests
{
    private Mock<IFileOperations> _mockFileOps;
    private Mock<ILoggerService> _mockLogger;
    private TemporaryFilesCleaner _cleaner;

    [TestInitialize]
    public void Setup()
    {
        _mockFileOps = new Mock<IFileOperations>();
        _mockLogger = new Mock<ILoggerService>();
        _cleaner = new TemporaryFilesCleaner(_mockFileOps.Object, _mockLogger.Object);
    }

    [TestMethod]
    public async Task CleanAsync_WhenCalled_ReturnsTrue()
    {
        // Arrange
        var cts = new CancellationTokenSource();

        // Act
        var result = await _cleaner.CleanAsync(cts.Token);

        // Assert
        Assert.IsTrue(result);
        _mockLogger.Verify(l => l.LogInfo(It.IsAny<string>()), Times.AtLeastOnce);
    }

    [TestMethod]
    public async Task CleanAsync_WhenCancelled_ThrowsOperationCanceledException()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsExceptionAsync<OperationCanceledException>(
            () => _cleaner.CleanAsync(cts.Token));
    }
}
```

### Тестирование сервиса операций с файлами

```csharp
[TestClass]
public class FileOperationsServiceTests
{
    private Mock<ILoggerService> _mockLogger;
    private FileOperationsService _service;

    [TestInitialize]
    public void Setup()
    {
        _mockLogger = new Mock<ILoggerService>();
        _service = new FileOperationsService(_mockLogger.Object);
    }

    [TestMethod]
    public async Task MoveToRecycleBinAsync_WhenFileExists_ReturnsTrue()
    {
        // Arrange
        var testFile = Path.GetTempFileName();

        // Act
        var result = await _service.MoveToRecycleBinAsync(testFile);

        // Assert
        Assert.IsTrue(result);
        
        // Cleanup
        if(File.Exists(testFile))
        {
            File.Delete(testFile);
        }
    }

    [TestMethod]
    public async Task IsFileInUseAsync_WhenFileIsFree_ReturnsFalse()
    {
        // Arrange
        var testFile = Path.GetTempFileName();

        // Act
        var result = await _service.IsFileInUseAsync(testFile);

        // Assert
        Assert.IsFalse(result);

        // Cleanup
        if(File.Exists(testFile))
        {
            File.Delete(testFile);
        }
    }
}
```

### Тестирование менеджера очистки

```csharp
[TestClass]
public class CleaningManagerTests
{
    private Mock<ILoggerService> _mockLogger;
    private Mock<ICleanerService> _mockCleaner1;
    private Mock<ICleanerService> _mockCleaner2;
    private CleaningManager _manager;

    [TestInitialize]
    public void Setup()
    {
        _mockLogger = new Mock<ILoggerService>();
        _mockCleaner1 = new Mock<ICleanerService>();
        _mockCleaner2 = new Mock<ICleanerService>();
        
        var cleaners = new[] { _mockCleaner1.Object, _mockCleaner2.Object };
        _manager = new CleaningManager(cleaners, _mockLogger.Object);
    }

    [TestMethod]
    public async Task PerformCleaningAsync_WithValidOptions_CallsAllServices()
    {
        // Arrange
        var options = new CleaningOptions();
        var cts = new CancellationTokenSource();
        
        _mockCleaner1.Setup(c => c.CleanAsync(It.IsAny<CancellationToken>()))
                     .ReturnsAsync(true);
        _mockCleaner2.Setup(c => c.CleanAsync(It.IsAny<CancellationToken>()))
                     .ReturnsAsync(true);

        // Act
        var result = await _manager.PerformCleaningAsync(options, cts.Token);

        // Assert
        _mockCleaner1.Verify(c => c.CleanAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockCleaner2.Verify(c => c.CleanAsync(It.IsAny<CancellationToken>()), Times.Once);
        Assert.IsTrue(result);
    }
}
```

## Рекомендации по тестированию

### 1. Модульное тестирование
- Покрывайте все публичные методы и свойства
- Тестируйте как успешные сценарии, так и ошибочные
- Используйте mock-объекты для зависимостей
- Проверяйте взаимодействие с зависимостями с помощью Verify

### 2. Интеграционное тестирование
- Тестируйте взаимодействие между несколькими компонентами
- Проверяйте работу с реальной файловой системой в контролируемой среде
- Тестируйте загрузку и сохранение настроек

### 3. Тестирование исключений
- Проверяйте корректную обработку OperationCanceledException
- Тестируйте ситуации, когда файлы заняты другими процессами
- Проверяйте поведение при отсутствии необходимых прав доступа

### 4. Параметризованные тесты
Для тестирования различных комбинаций настроек:

```csharp
[TestMethod]
[DataRow(true, true, true)]
[DataRow(false, true, false)]
[DataRow(true, false, true)]
public async Task PerformCleaningAsync_WithOptions_ReturnsExpectedResult(
    bool includeTempFiles, 
    bool includeLogFiles, 
    bool expectedResult)
{
    // Arrange
    var options = new CleaningOptions
    {
        IncludeTemporaryFiles = includeTempFiles,
        IncludeLogFiles = includeLogFiles
    };

    // Act
    var result = await _manager.PerformCleaningAsync(options);

    // Assert
    Assert.AreEqual(expectedResult, result);
}
```

## Покрытие кода

Целью должно быть достижение не менее 80% покрытия кода для критических компонентов. Используйте инструменты анализа покрытия, такие как:
- Visual Studio Test Explorer
- Coverlet
- ReportGenerator

## Запуск тестов

Для запуска тестов используйте:
```
dotnet test
```

Для генерации отчета о покрытии:
```
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

## Заключение

Благодаря модульной архитектуре и использованию интерфейсов, приложение легко поддается тестированию. Регулярное тестирование обеспечивает стабильность и надежность работы приложения.