using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace WindowsCleanerUtility
{
    public class RoundedControls
    {
        public static Color PrimaryColor = Color.FromArgb(0, 120, 215); // Синий цвет Microsoft
        public static Color SecondaryColor = Color.FromArgb(100, 100, 100); // Серый цвет
        public static Color DangerColor = Color.FromArgb(232, 17, 35); // Красный цвет Microsoft
        public static Color DarkBackgroundColor = Color.FromArgb(32, 32, 32);
        public static Color MediumBackgroundColor = Color.FromArgb(45, 45, 45);
        public static Color LightBackgroundColor = Color.FromArgb(64, 64, 64);
        public static Color TextColor = Color.FromArgb(255, 255, 255);
        public static Color SecondaryTextColor = Color.FromArgb(220, 220, 220);
        
        public static Font DefaultFont = new Font("Segoe UI", 9F);
        public static Font HeaderFont = new Font("Segoe UI", 16F, FontStyle.Bold);
        
        public static Button CreateRoundedButton(string text, Color backColor, EventHandler? clickHandler = null)
        {
            Button button = new Button();
            button.Text = text;
            button.BackColor = backColor;
            button.ForeColor = TextColor;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.Font = DefaultFont;
            button.Cursor = Cursors.Hand;
            button.Size = new Size(120, 40);
            
            // Добавляем поддержку закругленных краев
            button.Paint += (sender, e) => {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                
                // Рисуем закругленный прямоугольник
                using (GraphicsPath path = new GraphicsPath())
                {
                    int radius = 10; // Радиус закругления
                    Rectangle rect = new Rectangle(0, 0, button.Width, button.Height);
                    
                    path.StartFigure();
                    path.AddArc(rect.X, rect.Y, radius, radius, 180, 90); // Левый верхний угол
                    path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90); // Правый верхний угол
                    path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90); // Правый нижний угол
                    path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90); // Левый нижний угол
                    path.CloseFigure();
                    
                    button.Region = new Region(path);
                    
                    // Рисуем кнопку с градиентом или просто заливкой
                    using (SolidBrush brush = new SolidBrush(button.BackColor))
                    {
                        g.FillPath(brush, path);
                    }
                    
                    // Рисуем текст по центру кнопки
                    TextRenderer.DrawText(g, button.Text, button.Font, rect, button.ForeColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak);
                }
            };
            
            if(clickHandler != null)
            {
                button.Click += clickHandler;
            }
            
            // Эффект наведения
            button.MouseEnter += (sender, e) => {
                Button btn = sender as Button;
                btn.BackColor = ControlPaint.Light(btn.BackColor, 0.2f);
                btn.Invalidate(); // Перерисовать для обновления закругленных краев
            };
            button.MouseLeave += (sender, e) => {
                Button btn = sender as Button;
                btn.BackColor = backColor;
                btn.Invalidate(); // Перерисовать для обновления закругленных краев
            };
            
            return button;
        }
        
        public static Panel CreateRoundedPanel(DockStyle dock = DockStyle.Fill)
        {
            Panel panel = new RoundedPanel();
            panel.BackColor = MediumBackgroundColor;
            panel.Dock = dock;
            return panel;
        }
        
        // Класс панели с закругленными углами
        private class RoundedPanel : Panel
        {
            protected override void OnPaint(PaintEventArgs e)
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                
                using (GraphicsPath path = new GraphicsPath())
                {
                    int radius = 10;
                    Rectangle rect = new Rectangle(0, 0, Width, Height);
                    
                    path.StartFigure();
                    path.AddArc(rect.X, rect.Y, radius, radius, 180, 90); // Левый верхний угол
                    path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90); // Правый верхний угол
                    path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90); // Правый нижний угол
                    path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90); // Левый нижний угол
                    path.CloseFigure();
                    
                    this.Region = new Region(path);
                    
                    using (SolidBrush brush = new SolidBrush(this.BackColor))
                    {
                        g.FillPath(brush, path);
                    }
                }
            }
        }
        
        public static TextBox CreateRoundedLogBox()
        {
            RoundedTextBox textBox = new RoundedTextBox();
            textBox.BackColor = Color.FromArgb(15, 15, 15);
            textBox.ForeColor = SecondaryTextColor;
            textBox.Font = new Font("Consolas", 9F);
            textBox.Multiline = true;
            textBox.ScrollBars = ScrollBars.Vertical;
            textBox.ReadOnly = true;
            textBox.BorderStyle = BorderStyle.None;
            return textBox;
        }
        
        // Класс текстбокса с закругленными углами
        private class RoundedTextBox : TextBox
        {
            protected override void OnPaint(PaintEventArgs e)
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                
                using (GraphicsPath path = new GraphicsPath())
                {
                    int radius = 10;
                    Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);
                    
                    path.StartFigure();
                    path.AddArc(rect.X, rect.Y, radius, radius, 180, 90); // Левый верхний угол
                    path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90); // Правый верхний угол
                    path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90); // Правый нижний угол
                    path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90); // Левый нижний угол
                    path.CloseFigure();
                    
                    this.Region = new Region(path);
                    
                    using (Pen pen = new Pen(Color.FromArgb(100, 100, 100), 1)) // Цвет границы
                    {
                        g.DrawPath(pen, path);
                    }
                }
            }
        }
        
        public static Label CreateRoundedLabel(string text, Font? font = null, Color? textColor = null)
        {
            Label label = new Label();
            label.Text = text;
            label.ForeColor = textColor ?? TextColor;
            label.Font = font ?? DefaultFont;
            return label;
        }
        
        // Метод для установки закругленных углов у обычной метки (при необходимости)
        public static void ApplyRoundedCorners(Control control, int radius = 10)
        {
            GraphicsPath path = new GraphicsPath();
            Rectangle rect = new Rectangle(0, 0, control.Width, control.Height);
            
            path.StartFigure();
            path.AddArc(rect.X, rect.Y, radius, radius, 180, 90); // Левый верхний угол
            path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90); // Правый верхний угол
            path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90); // Правый нижний угол
            path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90); // Левый нижний угол
            path.CloseFigure();
            
            control.Region = new Region(path);
        }
    }
}