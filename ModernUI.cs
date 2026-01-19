using System;
using System.Drawing;
using System.Windows.Forms;

namespace WindowsCleanerUtility
{
    public class ModernUI
    {
        // Цвета в стиле Microsoft Fluent Design
        public static Color PrimaryColor = Color.FromArgb(0, 120, 215); // Синий цвет Microsoft
        public static Color SecondaryColor = Color.FromArgb(232, 17, 35); // Красный цвет Microsoft
        public static Color DarkBackgroundColor = Color.FromArgb(32, 32, 32);
        public static Color MediumBackgroundColor = Color.FromArgb(45, 45, 45);
        public static Color LightBackgroundColor = Color.FromArgb(64, 64, 64);
        public static Color TextColor = Color.FromArgb(255, 255, 255);
        public static Color SecondaryTextColor = Color.FromArgb(220, 220, 220);
        
        public static Font DefaultFont = new Font("Segoe UI", 9F);
        public static Font HeaderFont = new Font("Segoe UI", 16F, FontStyle.Bold);
        
        public static Button CreateModernButton(string text, Color backColor, EventHandler? clickHandler = null)
        {
            Button button = new Button();
            button.Text = text;
            button.BackColor = backColor;
            button.ForeColor = TextColor;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.Font = DefaultFont;
            button.Cursor = Cursors.Hand;
            
            if(clickHandler != null)
            {
                button.Click += clickHandler;
            }
            
            // Эффект наведения
            button.MouseEnter += (sender, e) => {
                Button btn = sender as Button;
                btn.BackColor = ControlPaint.Light(btn.BackColor, 0.2f);
            };
            button.MouseLeave += (sender, e) => {
                Button btn = sender as Button;
                btn.BackColor = backColor;
            };
            
            return button;
        }
        
        public static Panel CreateModernPanel(DockStyle dock = DockStyle.Fill)
        {
            Panel panel = new Panel();
            panel.BackColor = MediumBackgroundColor;
            panel.Dock = dock;
            return panel;
        }
        
        public static TextBox CreateModernLogBox()
        {
            TextBox textBox = new TextBox();
            textBox.BackColor = Color.FromArgb(15, 15, 15);
            textBox.ForeColor = SecondaryTextColor;
            textBox.Font = new Font("Consolas", 9F);
            textBox.Multiline = true;
            textBox.ScrollBars = ScrollBars.Vertical;
            textBox.ReadOnly = true;
            textBox.BorderStyle = BorderStyle.FixedSingle;
            return textBox;
        }
        
        public static Label CreateModernLabel(string text, Font? font = null, Color? textColor = null)
        {
            Label label = new Label();
            label.Text = text;
            label.ForeColor = textColor ?? TextColor;
            label.Font = font ?? DefaultFont;
            return label;
        }
    }
}