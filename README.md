# <img src="image/icon-WCU.ico" alt="Windows Cleaner Utility" width="40" height="40" style="vertical-align: middle; margin-right: 10px;"> Windows Cleaner Utility

Windows Cleaner Utility - это мощное приложение для очистки системы Windows, которое помогает освободить место на диске, удалить временные файлы, логи, кэш браузеров и другие ненужные данные.

## Особенности

- Удаление временных файлов
- Очистка логов системы
- Очистка кэша DNS
- Удаление старых файлов
- Очистка данных браузеров
- Перемещение файлов в корзину
- Генерация отчетов об очистке
- Планировщик задач для автоматической очистки

## Требования

- .NET 6.0 Runtime
- Windows 7 или выше

## Установка

1. Клонируйте репозиторий:
   ```bash
   git clone https://github.com/lovlygod/Windows-Cleaner-Utility.git
   cd Windows-Cleaner-Utility
   ```
2. Восстановите зависимости:
   ```bash
   dotnet restore
   ```
3. Соберите проект:
   ```bash
   dotnet build --configuration Release
   ```
4. Запустите приложение:
   ```bash
   dotnet run --project WindowsCleanerUtility/WindowsCleanerUtility.csproj
   ```

## Требования для разработки

- .NET 6.0 SDK (для сборки и разработки)
- .NET 6.0 Runtime (для запуска)
- Windows 7 или выше

## Лицензия

Этот проект распространяется под лицензией GNU GENERAL PUBLIC LICENSE Version 3. Подробности см. в файле [LICENSE](LICENSE).

## Вклад в проект

Если вы хотите внести свой вклад в проект, пожалуйста, создайте fork и отправьте Pull Request.
