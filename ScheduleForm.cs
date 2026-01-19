using System;
using System.Windows.Forms;
using WindowsCleanerUtility.Services;
using WindowsCleanerUtility.Settings;

namespace WindowsCleanerUtility
{
    public partial class ScheduleForm : Form
    {
        private readonly ISchedulerService _schedulerService;
        private readonly UserSettings _userSettings;
        private NumericUpDown _numericUpDownHours;
        private Button _btnSchedule;
        private Button _btnCancel;
        private CheckBox _chkEnableSchedule;

        public ScheduleForm(ISchedulerService schedulerService, UserSettings userSettings)
        {
            _schedulerService = schedulerService;
            _userSettings = userSettings;

            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Планировщик очистки";
            this.Size = new System.Drawing.Size(400, 200);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // CheckBox для включения планировщика
            _chkEnableSchedule = new CheckBox();
            _chkEnableSchedule.Text = "Включить автоматическую очистку";
            _chkEnableSchedule.Location = new System.Drawing.Point(20, 20);
            _chkEnableSchedule.Size = new System.Drawing.Size(300, 20);
            _chkEnableSchedule.Checked = _schedulerService.IsScheduled;
            this.Controls.Add(_chkEnableSchedule);

            // Label для интервала
            var lblInterval = new Label();
            lblInterval.Text = "Интервал (часы):";
            lblInterval.Location = new System.Drawing.Point(20, 60);
            lblInterval.Size = new System.Drawing.Size(100, 20);
            this.Controls.Add(lblInterval);

            // NumericUpDown для выбора интервала
            _numericUpDownHours = new NumericUpDown();
            _numericUpDownHours.Location = new System.Drawing.Point(130, 60);
            _numericUpDownHours.Size = new System.Drawing.Size(100, 20);
            _numericUpDownHours.Minimum = 1;
            _numericUpDownHours.Maximum = 168; // Максимум 1 неделя
            _numericUpDownHours.Value = 24; // По умолчанию 24 часа
            this.Controls.Add(_numericUpDownHours);

            // Кнопка "Назначить"
            _btnSchedule = new Button();
            _btnSchedule.Text = "Назначить";
            _btnSchedule.Location = new System.Drawing.Point(150, 120);
            _btnSchedule.Size = new System.Drawing.Size(75, 30);
            _btnSchedule.Click += BtnSchedule_Click;
            this.Controls.Add(_btnSchedule);

            // Кнопка "Отмена"
            _btnCancel = new Button();
            _btnCancel.Text = "Отмена";
            _btnCancel.Location = new System.Drawing.Point(240, 120);
            _btnCancel.Size = new System.Drawing.Size(75, 30);
            _btnCancel.Click += BtnCancel_Click;
            this.Controls.Add(_btnCancel);
        }

        private void BtnSchedule_Click(object sender, EventArgs e)
        {
            try
            {
                if (_chkEnableSchedule.Checked)
                {
                    // Включаем планировщик
                    _schedulerService.ScheduleCleanup(new CleaningOptions(), (int)_numericUpDownHours.Value);
                    MessageBox.Show($"Планировщик включен. Очистка будет выполняться каждые {_numericUpDownHours.Value} часов.", 
                        "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    // Отменяем планировщик
                    _schedulerService.CancelSchedule();
                    MessageBox.Show("Планировщик отключен.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при настройке планировщика: {ex.Message}", 
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}