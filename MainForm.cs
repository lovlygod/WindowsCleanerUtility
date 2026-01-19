using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using WindowsCleanerUtility.Services;
using WindowsCleanerUtility.Settings;

namespace WindowsCleanerUtility
{
    public partial class MainForm : Form
    {
        private CancellationTokenSource? _cancellationTokenSource;
                private readonly CleaningManager _cleaningManager;
                private readonly CleaningOptions _cleaningOptions;
                private readonly ILoggerService _logger;
                private readonly ISchedulerService _schedulerService;
                private readonly IReportService _reportService;
                private UserSettings _userSettings;
                
                private TextBox? _logTextBox;
                private ProgressBar? _progressBar;
                private Button? _btnClean;
                private Button? _btnCancel;
                private Button? _btnExit;
                private Button? _btnReports;
                private CheckBox? chkTempFiles;
                private CheckBox? chkLogFiles;
                private CheckBox? chkEventLogs;
                private CheckBox? chkOldFiles;
                private CheckBox? chkHistory;
                private CheckBox? chkCookies;
                private CheckBox? chkDNSCache;
        
                public MainForm()
                {
                    _userSettings = UserSettings.Load();
                    
                    var services = new ServiceCollection();
                    services.AddCleanerServices(_userSettings);
                    var serviceProvider = services.BuildServiceProvider();
                    
                    _logger = serviceProvider.GetRequiredService<ILoggerService>();
                    _cleaningManager = serviceProvider.GetRequiredService<CleaningManager>();
                    _schedulerService = serviceProvider.GetRequiredService<ISchedulerService>();
                    _reportService = serviceProvider.GetRequiredService<IReportService>();
                    
                    _cleaningOptions = CreateCleaningOptionsFromSettings();
                    
                    InitializeComponent();
                    SetupLogging();
                }
        
                private CleaningOptions CreateCleaningOptionsFromSettings()
                {
                    return new CleaningOptions
                    {
                        IncludeTemporaryFiles = _userSettings.IncludeTemporaryFiles,
                        IncludeLogFiles = _userSettings.IncludeLogFiles,
                        IncludeEventLogs = _userSettings.IncludeEventLogs,
                        IncludeOldFiles = _userSettings.IncludeOldFiles,
                        IncludeBrowserHistory = _userSettings.IncludeBrowserHistory,
                        IncludeBrowserCookies = _userSettings.IncludeBrowserCookies,
                        IncludeDNSTempFiles = _userSettings.IncludeDNSTempFiles,
                        DaysForOldFiles = _userSettings.DaysForOldFiles,
                        MoveToRecycleBin = _userSettings.MoveToRecycleBin,
                        ShowProgress = _userSettings.ShowProgress
                    };
                }
        
                private void InitializeComponent()
                {
                    this.Text = "Windows Cleaner Utility";
                    this.Size = new System.Drawing.Size(1000, 720);
                    this.StartPosition = FormStartPosition.CenterScreen;
                    this.FormBorderStyle = FormBorderStyle.FixedSingle;
                    this.MaximizeBox = false;
                    this.MinimizeBox = false;
                    this.BackColor = Color.FromArgb(45, 45, 48);
                    
                    try
                    {
                        this.Icon = new Icon(@"image\icon-WCU.ico");
                    }
                    catch
                    {
                    }
                    
                    CreateControls();
                }
        
                private void CreateControls()
                {
                    Label titleLabel = RoundedControls.CreateRoundedLabel("Windows Cleaner Utility", new Font("Segoe UI", 18F, FontStyle.Bold));
                    titleLabel.ForeColor = Color.White;
                    titleLabel.TextAlign = ContentAlignment.MiddleCenter;
                    titleLabel.Location = new Point(0, 10);
                    titleLabel.Size = new Size(1000, 30);
                    titleLabel.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
                    
                    this.Controls.Add(titleLabel);
                    
                    Panel optionsPanel = RoundedControls.CreateRoundedPanel();
                    optionsPanel.Location = new Point(20, 50);
                    optionsPanel.Size = new Size(960, 200);
                    optionsPanel.BackColor = Color.FromArgb(60, 60, 60);
                    optionsPanel.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
                    
                    Label optionsLabel = RoundedControls.CreateRoundedLabel("Выберите действия для очистки:", new Font("Segoe UI", 10F, FontStyle.Bold));
                    optionsLabel.Location = new Point(10, 5);
                    optionsLabel.Size = new Size(250, 30);
                    optionsLabel.ForeColor = Color.White;
                    optionsPanel.Controls.Add(optionsLabel);
                    RoundedControls.ApplyRoundedCorners(optionsLabel, 5);
                    
                    var chkTempFiles = CreateCheckBox("Удалить временные файлы", 10, 30, "chkTempFiles", _userSettings.IncludeTemporaryFiles);
                    optionsPanel.Controls.Add(chkTempFiles);
                    
                    var chkLogFiles = CreateCheckBox("Удалить логи", 10, 60, "chkLogFiles", _userSettings.IncludeLogFiles);
                    optionsPanel.Controls.Add(chkLogFiles);
                    
                    var chkEventLogs = CreateCheckBox("Очистить журнал событий", 10, 90, "chkEventLogs", _userSettings.IncludeEventLogs);
                    optionsPanel.Controls.Add(chkEventLogs);
                    
                    var chkOldFiles = CreateCheckBox("Удалить старые файлы", 10, 120, "chkOldFiles", _userSettings.IncludeOldFiles);
                    optionsPanel.Controls.Add(chkOldFiles);
                    
                    var chkHistory = CreateCheckBox("Очистить историю браузеров", 280, 30, "chkHistory", _userSettings.IncludeBrowserHistory);
                    optionsPanel.Controls.Add(chkHistory);
                    
                    var chkCookies = CreateCheckBox("Очистить cookies", 280, 60, "chkCookies", _userSettings.IncludeBrowserCookies);
                    optionsPanel.Controls.Add(chkCookies);
                    
                    var chkDNSCache = CreateCheckBox("Сбросить DNS-кэш", 280, 90, "chkDNSCache", _userSettings.IncludeDNSTempFiles);
                    optionsPanel.Controls.Add(chkDNSCache);
                    
                    this.chkTempFiles = chkTempFiles;
                    this.chkLogFiles = chkLogFiles;
                    this.chkEventLogs = chkEventLogs;
                    this.chkOldFiles = chkOldFiles;
                    this.chkHistory = chkHistory;
                    this.chkCookies = chkCookies;
                    this.chkDNSCache = chkDNSCache;
                    
                    this.Controls.Add(optionsPanel);
                    
                    _logTextBox = RoundedControls.CreateRoundedLogBox();
                    _logTextBox.Location = new Point(20, 260);
                    _logTextBox.Size = new Size(960, 330);
                    _logTextBox.BackColor = Color.FromArgb(30, 30, 30);
                    _logTextBox.ForeColor = Color.FromArgb(204, 204, 204);
                    _logTextBox.Multiline = true;
                    _logTextBox.ScrollBars = ScrollBars.Vertical;
                    _logTextBox.ReadOnly = true;
                    _logTextBox.Font = new Font("Consolas", 9F);
                    _logTextBox.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;
                    
                    this.Controls.Add(_logTextBox);
                    _logTextBox.Name = "logTextBox";
                    
                    _btnClean = RoundedControls.CreateRoundedButton("Очистить", RoundedControls.PrimaryColor, BtnClean_Click);
                    _btnClean.Location = new Point(580, 630);
                    _btnClean.Size = new Size(120, 40);
                    _btnClean.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
                    
                    this.Controls.Add(_btnClean);
                    
                    _btnCancel = RoundedControls.CreateRoundedButton("Отмена", RoundedControls.SecondaryColor, BtnCancel_Click);
                    _btnCancel.Location = new Point(440, 630);
                    _btnCancel.Size = new Size(120, 40);
                    _btnCancel.Enabled = false;
                    _btnCancel.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
                    
                    this.Controls.Add(_btnCancel);
                    
                    _btnReports = RoundedControls.CreateRoundedButton("Отчеты", RoundedControls.SecondaryColor, BtnReports_Click);
                    _btnReports.Location = new Point(300, 630);
                    _btnReports.Size = new Size(120, 40);
                    _btnReports.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
                    
                    this.Controls.Add(_btnReports);
                    
                    _btnExit = RoundedControls.CreateRoundedButton("Выход", RoundedControls.DangerColor, BtnExit_Click);
                    _btnExit.Location = new Point(20, 630);
                    _btnExit.Size = new Size(120, 40);
                    _btnExit.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
                    
                    this.Controls.Add(_btnExit);
                }
                
                
        
                private CheckBox CreateCheckBox(string text, int x, int y, string name, bool isChecked)
                {
                    CheckBox checkBox = new CheckBox();
                    checkBox.Text = text;
                    checkBox.Location = new Point(x, y);
                    checkBox.Size = new Size(250, 25);
                    checkBox.ForeColor = Color.White;
                    checkBox.BackColor = Color.Transparent;
                    checkBox.Name = name;
                    checkBox.Checked = isChecked;
                    checkBox.CheckedChanged += OnOptionsChanged;
                    checkBox.Font = new Font("Segoe UI", 9F);
                    return checkBox;
                }
        
                private void OnOptionsChanged(object sender, EventArgs e)
                {
                    _userSettings.IncludeTemporaryFiles = chkTempFiles?.Checked ?? false;
                    _userSettings.IncludeLogFiles = chkLogFiles?.Checked ?? false;
                    _userSettings.IncludeEventLogs = chkEventLogs?.Checked ?? false;
                    _userSettings.IncludeOldFiles = chkOldFiles?.Checked ?? false;
                    _userSettings.IncludeBrowserHistory = chkHistory?.Checked ?? false;
                    _userSettings.IncludeBrowserCookies = chkCookies?.Checked ?? false;
                    _userSettings.IncludeDNSTempFiles = chkDNSCache?.Checked ?? false;
                }
        
                private void SetupLogging()
                {
                }
        
                private async void BtnClean_Click(object sender, EventArgs e)
                {
                    bool hasAnySelection =
                        (chkTempFiles?.Checked ?? false) ||
                        (chkLogFiles?.Checked ?? false) ||
                        (chkEventLogs?.Checked ?? false) ||
                        (chkOldFiles?.Checked ?? false) ||
                        (chkHistory?.Checked ?? false) ||
                        (chkCookies?.Checked ?? false) ||
                        (chkDNSCache?.Checked ?? false);
        
                    if (!hasAnySelection)
                    {
                        MessageBox.Show(
                            "Пожалуйста, выберите хотя бы один элемент для очистки.",
                            "Нет выбранных действий",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning
                        );
                        return;
                    }
        
                    _userSettings.Save();
        
                    if (_logTextBox != null)
                    {
                        _logTextBox.AppendText($"[{DateTime.Now:HH:mm:ss}] Начата процедура очистки...\r\n");
                    }
        
                    _btnClean.Enabled = false;
                                _btnCancel.Enabled = true;
                                
                                // Disable all checkboxes during cleaning
                                if (chkTempFiles != null) chkTempFiles.Enabled = false;
                                if (chkLogFiles != null) chkLogFiles.Enabled = false;
                                if (chkEventLogs != null) chkEventLogs.Enabled = false;
                                if (chkOldFiles != null) chkOldFiles.Enabled = false;
                                if (chkHistory != null) chkHistory.Enabled = false;
                                if (chkCookies != null) chkCookies.Enabled = false;
                                if (chkDNSCache != null) chkDNSCache.Enabled = false;
                                

            _cancellationTokenSource = new CancellationTokenSource();
            
                        try
                        {
                            var result = await _cleaningManager.PerformCleaningWithReportAsync(_cleaningOptions, _cancellationTokenSource.Token);
                            
                            if (result.TotalSpaceFreed > 0)
                            {
                                if (_logTextBox != null)
                                {
                                    _logTextBox.AppendText($"[{DateTime.Now:HH:mm:ss}] Процедура очистки завершена.\r\n");
                                    _logTextBox.AppendText($"[{DateTime.Now:HH:mm:ss}] Освобождено: {FormatBytes(result.TotalSpaceFreed)}\r\n");
                                }
                                
                                MessageBox.Show(
                                    result.ServiceResults.All(r => r.Success) ?
                                        $"Очистка завершена успешно! Файлы перемещены в корзину.\nОсвобождено: {FormatBytes(result.TotalSpaceFreed)}" :
                                        "Очистка завершена с ошибками. Проверьте лог для деталей.",
                                    "Информация",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Information
                                );
                            }
                            else
                            {
                                if (_logTextBox != null)
                                {
                                    _logTextBox.AppendText($"[{DateTime.Now:HH:mm:ss}] Процедура очистки завершена.\r\n");
                                    _logTextBox.AppendText($"[{DateTime.Now:HH:mm:ss}] Нечего удалять, все временные файлы отсутствуют.\r\n");
                                }
                                
                                MessageBox.Show(
                                    "Нечего удалять, все временные файлы отсутствуют.",
                                    "Информация",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Information
                                );
                            }
                        }
                        catch (OperationCanceledException)
                        {
                            if (_logTextBox != null)
                            {
                                _logTextBox.AppendText($"[{DateTime.Now:HH:mm:ss}] Операция очистки была отменена пользователем.\r\n");
                            }
                        }
                        catch (Exception ex)
                        {
                            if (_logTextBox != null)
                            {
                                _logTextBox.AppendText($"[{DateTime.Now:HH:mm:ss}] Ошибка при выполнении очистки: {ex.Message}\r\n");
                            }
                            
                            MessageBox.Show($"Ошибка при выполнении очистки: {ex.Message}",
                                "Ошибка",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error
                            );
                        }
                        finally
                                    {
                                        _cancellationTokenSource?.Dispose();
                                        _cancellationTokenSource = null;
                                        
                                        _btnClean.Enabled = true;
                                        _btnCancel.Enabled = false;
                                        
                                        // Enable all checkboxes after cleaning is done
                                        if (chkTempFiles != null) chkTempFiles.Enabled = true;
                                        if (chkLogFiles != null) chkLogFiles.Enabled = true;
                                        if (chkEventLogs != null) chkEventLogs.Enabled = true;
                                        if (chkOldFiles != null) chkOldFiles.Enabled = true;
                                        if (chkHistory != null) chkHistory.Enabled = true;
                                        if (chkCookies != null) chkCookies.Enabled = true;
                                        if (chkDNSCache != null) chkDNSCache.Enabled = true;
                                    }
                                }
                        
                                private void BtnCancel_Click(object sender, EventArgs e)
                                {
                                    _cancellationTokenSource?.Cancel();
                                }
                        

        private void BtnReports_Click(object sender, EventArgs e)
        {
            // Открыть диалог просмотра отчетов
            var reportsForm = new ReportsForm(_reportService);
            reportsForm.ShowDialog();
        }

        private void BtnExit_Click(object sender, EventArgs e)
        {
            // Сохраняем настройки перед выходом
            _userSettings.Save();
            this.Close();
        }

        private string FormatBytes(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            
            return $"{len:0.##} {sizes[order]}";
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _cancellationTokenSource?.Dispose();
                // Сохраняем настройки при уничтожении формы
                _userSettings.Save();
            }
            base.Dispose(disposing);
        }
    }
}