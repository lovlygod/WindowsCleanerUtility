using System;
using System.Windows.Forms;
using WindowsCleanerUtility.Services;

namespace WindowsCleanerUtility
{
    public partial class ReportsForm : Form
    {
        private readonly IReportService _reportService;
        private ComboBox _cmbReportFormat;
        private Button _btnGenerate;
        private Button _btnSave;
        private TextBox _txtReportPreview;
        private Button _btnClose;

        public ReportsForm(IReportService reportService)
        {
            _reportService = reportService;

            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Отчеты о очистке";
            this.Size = new System.Drawing.Size(700, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Label для выбора формата
            var lblFormat = new Label();
            lblFormat.Text = "Формат отчета:";
            lblFormat.Location = new System.Drawing.Point(20, 20);
            lblFormat.Size = new System.Drawing.Size(100, 20);
            this.Controls.Add(lblFormat);

            // ComboBox для выбора формата отчета
            _cmbReportFormat = new ComboBox();
            _cmbReportFormat.Location = new System.Drawing.Point(130, 20);
            _cmbReportFormat.Size = new System.Drawing.Size(120, 20);
            _cmbReportFormat.Items.Add("JSON");
            _cmbReportFormat.Items.Add("XML");
            _cmbReportFormat.Items.Add("CSV");
            _cmbReportFormat.Items.Add("HTML");
            _cmbReportFormat.SelectedIndex = 0;
            this.Controls.Add(_cmbReportFormat);

            // Кнопка "Сгенерировать"
            _btnGenerate = new Button();
            _btnGenerate.Text = "Сгенерировать";
            _btnGenerate.Location = new System.Drawing.Point(270, 20);
            _btnGenerate.Size = new System.Drawing.Size(100, 30);
            _btnGenerate.Click += BtnGenerate_Click;
            this.Controls.Add(_btnGenerate);

            // TextBox для предпросмотра отчета
            _txtReportPreview = new TextBox();
            _txtReportPreview.Location = new System.Drawing.Point(20, 70);
            _txtReportPreview.Size = new System.Drawing.Size(650, 300);
            _txtReportPreview.Multiline = true;
            _txtReportPreview.ScrollBars = ScrollBars.Vertical;
            _txtReportPreview.ReadOnly = true;
            _txtReportPreview.Font = new System.Drawing.Font("Consolas", 8);
            this.Controls.Add(_txtReportPreview);

            // Кнопка "Сохранить"
            _btnSave = new Button();
            _btnSave.Text = "Сохранить";
            _btnSave.Location = new System.Drawing.Point(20, 390);
            _btnSave.Size = new System.Drawing.Size(80, 30);
            _btnSave.Click += BtnSave_Click;
            _btnSave.Enabled = false; // Пока не сгенерирован отчет
            this.Controls.Add(_btnSave);

            // Кнопка "Закрыть"
            _btnClose = new Button();
            _btnClose.Text = "Закрыть";
            _btnClose.Location = new System.Drawing.Point(600, 420);
            _btnClose.Size = new System.Drawing.Size(80, 30);
            _btnClose.Click += BtnClose_Click;
            this.Controls.Add(_btnClose);
        }

        private async void BtnGenerate_Click(object sender, EventArgs e)
        {
            try
            {
                // Здесь будет использоваться реальный результат очистки
                // Для демонстрации создадим фиктивный результат
                var fakeResult = new CleaningResult
                {
                    StartTime = DateTime.Now.AddHours(-1),
                    EndTime = DateTime.Now,
                    Duration = TimeSpan.FromMinutes(5),
                    TotalFilesProcessed = 150,
                    TotalSpaceFreed = 1024 * 1024 * 50, // 50 MB
                    ServiceResults = new System.Collections.Generic.List<ServiceResult>
                    {
                        new ServiceResult { ServiceName = "TemporaryFilesCleaner", Success = true, FilesProcessed = 100, SpaceFreed = 1024 * 1024 * 30, StartTime = DateTime.Now.AddMinutes(-5), EndTime = DateTime.Now.AddMinutes(-3) },
                        new ServiceResult { ServiceName = "BrowserDataCleaner", Success = true, FilesProcessed = 30, SpaceFreed = 1024 * 1024 * 15, StartTime = DateTime.Now.AddMinutes(-3), EndTime = DateTime.Now.AddMinutes(-1) },
                        new ServiceResult { ServiceName = "SystemLogsCleaner", Success = true, FilesProcessed = 20, SpaceFreed = 1024 * 1024 * 5, StartTime = DateTime.Now.AddMinutes(-1), EndTime = DateTime.Now }
                    }
                };

                var format = (ReportFormat)Enum.Parse(typeof(ReportFormat), _cmbReportFormat.SelectedItem.ToString());
                var reportContent = await _reportService.GenerateReportAsync(fakeResult, format);

                _txtReportPreview.Text = reportContent;
                _btnSave.Enabled = true;

                MessageBox.Show("Отчет успешно сгенерирован.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при генерации отчета: {ex.Message}", 
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_txtReportPreview.Text))
            {
                MessageBox.Show("Сначала сгенерируйте отчет.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var saveFileDialog = new SaveFileDialog())
            {
                var format = _cmbReportFormat.SelectedItem.ToString().ToLower();
                saveFileDialog.Filter = $"{format} files (*.{format})|*.{format}|All files (*.*)|*.*";
                saveFileDialog.DefaultExt = format;
                saveFileDialog.FileName = $"cleaning_report_{DateTime.Now:yyyyMMdd_HHmmss}.{format}";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        System.IO.File.WriteAllText(saveFileDialog.FileName, _txtReportPreview.Text);
                        MessageBox.Show("Отчет успешно сохранен.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при сохранении отчета: {ex.Message}", 
                            "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}