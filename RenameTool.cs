using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace FileRenamer
{
    // 主题调色板（浅色 / 深色）
    static class Palette
    {
        public static bool Dark;
        public static Color Accent, AccentHover, AccentPressed, AccentText, AccentTint;
        public static Color Bg, Card, Border, TextPrimary, TextSecondary;
        public static Color HoverGray, PressGray, HeaderBg, TitleBarBg, TrackOff;

        public static void SetDark(bool dark)
        {
            Dark = dark;
            if (dark)
            {
                Accent = Color.FromArgb(0x0F, 0x6C, 0xBD);
                AccentHover = Color.FromArgb(0x11, 0x61, 0xA8);
                AccentPressed = Color.FromArgb(0x0D, 0x55, 0x93);
                AccentText = Color.FromArgb(0x5C, 0xA9, 0xE6);
                AccentTint = Color.FromArgb(0x17, 0x32, 0x4D);
                Bg = Color.FromArgb(0x20, 0x20, 0x20);
                Card = Color.FromArgb(0x2B, 0x2B, 0x2B);
                Border = Color.FromArgb(0x3A, 0x3A, 0x3A);
                TextPrimary = Color.FromArgb(0xFF, 0xFF, 0xFF);
                TextSecondary = Color.FromArgb(0xA8, 0xA8, 0xA8);
                HoverGray = Color.FromArgb(0x35, 0x35, 0x35);
                PressGray = Color.FromArgb(0x2A, 0x2A, 0x2A);
                HeaderBg = Color.FromArgb(0x26, 0x26, 0x26);
                TitleBarBg = Color.FromArgb(0x2B, 0x2B, 0x2B);
                TrackOff = Color.FromArgb(0x5A, 0x5A, 0x5A);
            }
            else
            {
                Accent = Color.FromArgb(0x0F, 0x6C, 0xBD);
                AccentHover = Color.FromArgb(0x0B, 0x5A, 0xA6);
                AccentPressed = Color.FromArgb(0x09, 0x4E, 0x8F);
                AccentText = Color.FromArgb(0x0F, 0x6C, 0xBD);
                AccentTint = Color.FromArgb(0xE5, 0xF1, 0xFB);
                Bg = Color.FromArgb(0xF3, 0xF3, 0xF3);
                Card = Color.White;
                Border = Color.FromArgb(0xE5, 0xE5, 0xE5);
                TextPrimary = Color.FromArgb(0x1B, 0x1B, 0x1B);
                TextSecondary = Color.FromArgb(0x61, 0x61, 0x61);
                HoverGray = Color.FromArgb(0xF5, 0xF5, 0xF5);
                PressGray = Color.FromArgb(0xE8, 0xE8, 0xE8);
                HeaderBg = Color.FromArgb(0xF7, 0xF7, 0xF7);
                TitleBarBg = Color.White;
                TrackOff = Color.FromArgb(0xC9, 0xCD, 0xD4);
            }
        }
    }

    // 通用绘制辅助
    static class Shapes
    {
        public static GraphicsPath Rounded(Rectangle r, int radius)
        {
            if (radius <= 0) radius = 1;
            int d = radius * 2;
            var path = new GraphicsPath();
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }

    // 现代圆角按钮（含悬停 / 按压状态）
    public class RoundedButton : Control
    {
        public string ButtonText { get; set; }
        public Color BaseColor { get; set; }
        public Color HoverColor { get; set; }
        public Color PressColor { get; set; }
        public Color BorderColor { get; set; }
        public Color HoverForeColor { get; set; }
        public int CornerRadius { get; set; }

        private bool hovered = false;
        private bool pressed = false;

        public RoundedButton()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            Cursor = Cursors.Hand;
            ForeColor = Color.White;
            Font = new Font("Microsoft YaHei UI", 9F);
            ButtonText = "";
            BaseColor = Color.Transparent;
            HoverColor = Color.Transparent;
            PressColor = Color.Transparent;
            BorderColor = Color.Transparent;
            HoverForeColor = Color.Empty;
            CornerRadius = 8;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Color fill = pressed ? PressColor : (hovered ? HoverColor : BaseColor);
            Rectangle r = new Rectangle(0, 0, Width - 1, Height - 1);
            using (var path = Shapes.Rounded(r, CornerRadius))
            {
                if (fill.A > 0)
                {
                    using (var b = new SolidBrush(fill)) e.Graphics.FillPath(b, path);
                }
                if (BorderColor.A > 0)
                {
                    using (var p = new Pen(BorderColor)) e.Graphics.DrawPath(p, path);
                }
            }
            Color tc = ForeColor;
            if (hovered && HoverForeColor != Color.Empty) tc = HoverForeColor;
            TextRenderer.DrawText(e.Graphics, ButtonText, Font, r, tc,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        protected override void OnMouseEnter(EventArgs e) { hovered = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { hovered = false; pressed = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) { pressed = true; Invalidate(); }
            base.OnMouseDown(e);
        }
        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (pressed) { pressed = false; Invalidate(); }
            base.OnMouseUp(e);
        }
    }

    // 现代开关控件
    public class ToggleSwitch : Control
    {
        public bool Checked { get; set; }
        public Color Accent { get; set; }
        public Color TrackOff { get; set; }
        public Color TextColor { get; set; }
        public string Label { get; set; }
        public event EventHandler CheckedChanged;

        private const int TrackW = 42, TrackH = 22, Thumb = 16;

        public ToggleSwitch()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            Cursor = Cursors.Hand;
            Font = new Font("Microsoft YaHei UI", 9F);
            Height = TrackH;
            Accent = Color.FromArgb(0x0F, 0x6C, 0xBD);
            TrackOff = Color.FromArgb(0xC9, 0xCD, 0xD4);
            TextColor = Color.FromArgb(0x1B, 0x1B, 0x1B);
            Label = "";
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            var track = new Rectangle(0, (Height - TrackH) / 2, TrackW, TrackH);
            Color trackColor = Checked ? Accent : TrackOff;
            using (var path = Shapes.Rounded(track, TrackH / 2))
            using (var b = new SolidBrush(trackColor)) e.Graphics.FillPath(b, path);

            int thumbX = Checked ? track.Right - Thumb - 3 : track.X + 3;
            var thumb = new Rectangle(thumbX, track.Y + (TrackH - Thumb) / 2, Thumb, Thumb);
            using (var b = new SolidBrush(Color.White)) e.Graphics.FillEllipse(b, thumb);

            var textRect = new Rectangle(track.Right + 10, 0, Width - track.Right - 10, Height);
            TextRenderer.DrawText(e.Graphics, Label, Font, textRect, TextColor,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }

        protected override void OnClick(EventArgs e)
        {
            Checked = !Checked;
            Invalidate();
            if (CheckedChanged != null) CheckedChanged(this, EventArgs.Empty);
            base.OnClick(e);
        }
    }

    // 圆角面板
    public class RoundedPanel : Panel
    {
        public int CornerRadius { get; set; }
        public Color BorderColor { get; set; }
        public Color FillColor { get; set; }

        public RoundedPanel()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            BackColor = Color.Transparent;
            CornerRadius = 10;
            BorderColor = Color.FromArgb(0xE5, 0xE5, 0xE5);
            FillColor = Color.White;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle r = new Rectangle(0, 0, Width - 1, Height - 1);
            using (var path = Shapes.Rounded(r, CornerRadius))
            {
                using (var b = new SolidBrush(FillColor)) e.Graphics.FillPath(b, path);
                using (var p = new Pen(BorderColor)) e.Graphics.DrawPath(p, path);
            }
        }
    }

    public class MainForm : Form
    {
        static readonly Color CloseRed = Color.FromArgb(0xE8, 0x11, 0x23);
        static readonly Color CloseRedPress = Color.FromArgb(0xC4, 0x0F, 0x21);

        private TextBox txtFolder;
        private ListView listView;
        private ComboBox cmbSort;
        private RoundedButton btnMin, btnClose, btnTheme, btnBrowse, btnDesc, btnAsc, btnRename;
        private ToggleSwitch swPad;
        private RoundedPanel inputCard, listCard;
        private Label lblFolder, lblSort, lblDir, lblStatus, lblHint;

        private string currentDir = "";
        private List<FileInfo> currentFiles = new List<FileInfo>();
        private int hoverIndex = -1;
        private bool descending = true;

        private readonly string ThemeFile = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "BatchRenamer", "theme.txt");

        // DPI 缩放
        private float S = 1f;
        private int P(float v) { return (int)Math.Round(v * S); }

        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
        private static extern int StrCmpLogicalW(string x, string y);

        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern uint GetDpiForSystem();

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int value, int size);

        [DllImport("uxtheme.dll", EntryPoint = "#135", SetLastError = true)]
        private static extern int SetPreferredAppMode(int appMode);

        const int WM_NCLBUTTONDOWN = 0xA1;
        const int HTCAPTION = 2;
        const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

        public MainForm()
        {
            S = GetDpiForSystem() / 96f;

            FormBorderStyle = FormBorderStyle.None;
            ClientSize = new Size(P(720), P(580));
            StartPosition = FormStartPosition.CenterScreen;
            MaximizeBox = false;

            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            KeyPreview = true;
            KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) Close(); };

            Palette.SetDark(false);
            LoadTheme();

            BuildUi();
            ApplyTheme();

            EnableDrop(this);
            LoadFolder(AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\'));
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ClassStyle |= 0x00020000; // CS_DROPSHADOW
                return cp;
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            SetNativeDarkMode(Palette.Dark);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (WindowState != FormWindowState.Minimized)
            {
                using (var path = Shapes.Rounded(new Rectangle(0, 0, Width, Height), P(12)))
                    Region = new Region(path);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            int th = P(44);
            using (var b = new SolidBrush(Palette.TitleBarBg)) e.Graphics.FillRectangle(b, 0, 0, Width, th);
            using (var p = new Pen(Palette.Border)) e.Graphics.DrawLine(p, 0, th - 1, Width, th - 1);
            TextRenderer.DrawText(e.Graphics, "批量重命名工具",
                new Font("Microsoft YaHei UI", 9.5F),
                new Rectangle(P(20), 0, P(320), th), Palette.TextPrimary,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left && e.Y < P(44))
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, (IntPtr)HTCAPTION, IntPtr.Zero);
            }
        }

        private void BuildUi()
        {
            Font baseFont = new Font("Microsoft YaHei UI", 9F);
            int tbBtnH = P(32);
            int tbBtnY = (P(44) - tbBtnH) / 2;

            // 标题栏按钮：主题 / 最小化 / 关闭
            btnTheme = new RoundedButton();
            btnTheme.Font = new Font("Segoe UI Symbol", 11F);
            btnTheme.Location = new Point(Width - P(120), tbBtnY);
            btnTheme.Size = new Size(P(40), tbBtnH);
            btnTheme.CornerRadius = 0;
            btnTheme.Click += (s, e) => ToggleTheme();

            btnMin = new RoundedButton();
            btnMin.ButtonText = "—";
            btnMin.Font = new Font("Microsoft YaHei UI", 10F);
            btnMin.Location = new Point(Width - P(80), tbBtnY);
            btnMin.Size = new Size(P(40), tbBtnH);
            btnMin.CornerRadius = 0;
            btnMin.Click += (s, e) => { WindowState = FormWindowState.Minimized; };

            btnClose = new RoundedButton();
            btnClose.ButtonText = "×";
            btnClose.Font = new Font("Microsoft YaHei UI", 12F);
            btnClose.Location = new Point(Width - P(40), tbBtnY);
            btnClose.Size = new Size(P(40), tbBtnH);
            btnClose.CornerRadius = 0;
            btnClose.Click += (s, e) => Close();

            // 文件夹标题
            lblFolder = new Label();
            lblFolder.Text = "文件夹";
            lblFolder.Font = new Font("Microsoft YaHei UI", 9.5F, FontStyle.Bold);
            lblFolder.AutoSize = true;
            lblFolder.Location = new Point(P(24), P(60));

            // 输入框卡片 + 文本框
            inputCard = new RoundedPanel();
            inputCard.Location = new Point(P(24), P(86));
            inputCard.Size = new Size(P(540), P(40));
            inputCard.CornerRadius = P(8);

            txtFolder = new TextBox();
            txtFolder.BorderStyle = BorderStyle.None;
            txtFolder.Font = baseFont;
            txtFolder.Location = new Point(P(12), P(12));
            txtFolder.Size = new Size(P(516), P(18));
            txtFolder.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) LoadFolder(txtFolder.Text.Trim()); };
            inputCard.Controls.Add(txtFolder);

            btnBrowse = new RoundedButton();
            btnBrowse.ButtonText = "浏览";
            btnBrowse.Location = new Point(P(576), P(86));
            btnBrowse.Size = new Size(P(120), P(40));
            btnBrowse.CornerRadius = P(8);
            btnBrowse.Click += (s, e) => Browse();

            // 列表卡片 + 列表
            listCard = new RoundedPanel();
            listCard.Location = new Point(P(24), P(140));
            listCard.Size = new Size(P(672), P(296));
            listCard.CornerRadius = P(10);

            listView = new ListView();
            listView.Location = new Point(P(2), P(2));
            listView.Size = new Size(P(668), P(292));
            listView.View = View.Details;
            listView.OwnerDraw = true;
            listView.FullRowSelect = true;
            listView.MultiSelect = false;
            listView.HideSelection = false;
            listView.BorderStyle = BorderStyle.None;
            listView.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            listView.Columns.Add("原文件名", P(320));
            listView.Columns.Add("新文件名", P(340));
            listView.DrawColumnHeader += listView_DrawColumnHeader;
            listView.DrawItem += listView_DrawItem;
            listView.DrawSubItem += listView_DrawSubItem;
            listView.MouseMove += (s, e) =>
            {
                var item = listView.GetItemAt(e.X, e.Y);
                int idx = item == null ? -1 : item.Index;
                if (idx != hoverIndex) { hoverIndex = idx; listView.Invalidate(); }
            };
            listView.MouseLeave += (s, e) => { if (hoverIndex != -1) { hoverIndex = -1; listView.Invalidate(); } };
            listCard.Controls.Add(listView);

            // 排序方式
            lblSort = new Label();
            lblSort.Text = "排序方式";
            lblSort.Font = baseFont;
            lblSort.AutoSize = true;
            lblSort.Location = new Point(P(24), P(461));

            cmbSort = new ComboBox();
            cmbSort.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbSort.FlatStyle = FlatStyle.Flat;
            cmbSort.Font = baseFont;
            cmbSort.Items.AddRange(new object[] { "名称", "修改时间", "创建时间", "大小", "类型" });
            cmbSort.SelectedIndex = 0;
            cmbSort.Location = new Point(P(88), P(456));
            cmbSort.Size = new Size(P(150), P(30));
            cmbSort.SelectedIndexChanged += (s, e) => { ApplySort(); RefreshPreview(); };

            // 编号方向
            lblDir = new Label();
            lblDir.Text = "编号";
            lblDir.Font = baseFont;
            lblDir.AutoSize = true;
            lblDir.Location = new Point(P(254), P(461));

            btnDesc = new RoundedButton();
            btnDesc.ButtonText = "从大到小";
            btnDesc.Font = baseFont;
            btnDesc.Location = new Point(P(298), P(456));
            btnDesc.Size = new Size(P(84), P(30));
            btnDesc.CornerRadius = P(6);
            btnDesc.Click += (s, e) => SetDirection(true);

            btnAsc = new RoundedButton();
            btnAsc.ButtonText = "从小到大";
            btnAsc.Font = baseFont;
            btnAsc.Location = new Point(P(386), P(456));
            btnAsc.Size = new Size(P(84), P(30));
            btnAsc.CornerRadius = P(6);
            btnAsc.Click += (s, e) => SetDirection(false);

            // 补齐位数
            swPad = new ToggleSwitch();
            swPad.Label = "补齐位数";
            swPad.Location = new Point(P(500), P(459));
            swPad.Size = new Size(P(140), P(24));
            swPad.CheckedChanged += (s, e) => RefreshPreview();

            // 底部
            lblStatus = new Label();
            lblStatus.AutoSize = true;
            lblStatus.Font = new Font("Microsoft YaHei UI", 8F);
            lblStatus.Location = new Point(P(24), P(526));

            lblHint = new Label();
            lblHint.Text = "提示：可直接把文件夹拖进窗口";
            lblHint.AutoSize = true;
            lblHint.Font = new Font("Microsoft YaHei UI", 8F);
            lblHint.Location = new Point(P(300), P(526));

            btnRename = new RoundedButton();
            btnRename.ButtonText = "开始重命名";
            btnRename.Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold);
            btnRename.Location = new Point(P(524), P(508));
            btnRename.Size = new Size(P(172), P(44));
            btnRename.CornerRadius = P(8);
            btnRename.Click += (s, e) => DoRename();

            Controls.Add(btnMin);
            Controls.Add(btnClose);
            Controls.Add(btnTheme);
            Controls.Add(lblFolder);
            Controls.Add(inputCard);
            Controls.Add(btnBrowse);
            Controls.Add(listCard);
            Controls.Add(lblSort);
            Controls.Add(cmbSort);
            Controls.Add(lblDir);
            Controls.Add(btnDesc);
            Controls.Add(btnAsc);
            Controls.Add(swPad);
            Controls.Add(lblStatus);
            Controls.Add(lblHint);
            Controls.Add(btnRename);
        }

        private void ApplyTheme()
        {
            BackColor = Palette.Bg;

            btnTheme.ButtonText = Palette.Dark ? "☀" : "☾";
            btnTheme.ForeColor = Palette.TextSecondary;
            btnTheme.HoverForeColor = Palette.TextPrimary;
            btnTheme.HoverColor = Palette.HoverGray;
            btnTheme.PressColor = Palette.PressGray;

            btnMin.ForeColor = Palette.TextSecondary;
            btnMin.HoverForeColor = Palette.TextPrimary;
            btnMin.HoverColor = Palette.HoverGray;
            btnMin.PressColor = Palette.PressGray;

            btnClose.ForeColor = Palette.TextSecondary;
            btnClose.HoverForeColor = Color.White;
            btnClose.HoverColor = CloseRed;
            btnClose.PressColor = CloseRedPress;

            inputCard.FillColor = Palette.Card;
            inputCard.BorderColor = Palette.Border;
            listCard.FillColor = Palette.Card;
            listCard.BorderColor = Palette.Border;

            txtFolder.BackColor = Palette.Card;
            txtFolder.ForeColor = Palette.TextPrimary;

            btnBrowse.BaseColor = Palette.Card;
            btnBrowse.HoverColor = Palette.HoverGray;
            btnBrowse.PressColor = Palette.PressGray;
            btnBrowse.BorderColor = Palette.Border;
            btnBrowse.ForeColor = Palette.TextPrimary;

            listView.BackColor = Palette.Card;

            lblFolder.ForeColor = Palette.TextPrimary;
            lblSort.ForeColor = Palette.TextPrimary;
            lblDir.ForeColor = Palette.TextPrimary;

            cmbSort.BackColor = Palette.Card;
            cmbSort.ForeColor = Palette.TextPrimary;

            swPad.Accent = Palette.Accent;
            swPad.TrackOff = Palette.TrackOff;
            swPad.TextColor = Palette.TextPrimary;

            lblStatus.ForeColor = Palette.TextSecondary;
            lblHint.ForeColor = Palette.TextSecondary;

            btnRename.BaseColor = Palette.Accent;
            btnRename.HoverColor = Palette.AccentHover;
            btnRename.PressColor = Palette.AccentPressed;
            btnRename.ForeColor = Color.White;

            SetDirection(descending);

            Invalidate(true);
            if (listView != null) listView.Invalidate();

            SetNativeDarkMode(Palette.Dark);
        }

        private void SetNativeDarkMode(bool dark)
        {
            try { SetPreferredAppMode(dark ? 1 : 0); } catch { }
            if (IsHandleCreated)
            {
                int v = dark ? 1 : 0;
                try { DwmSetWindowAttribute(Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref v, 4); } catch { }
            }
        }

        private void ToggleTheme()
        {
            Palette.SetDark(!Palette.Dark);
            ApplyTheme();
            SaveTheme();
        }

        private void LoadTheme()
        {
            try
            {
                if (File.Exists(ThemeFile))
                {
                    Palette.SetDark(File.ReadAllText(ThemeFile).Trim().ToLower() == "dark");
                }
            }
            catch { }
        }

        private void SaveTheme()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(ThemeFile));
                File.WriteAllText(ThemeFile, Palette.Dark ? "dark" : "light");
            }
            catch { }
        }

        private void SetDirection(bool desc)
        {
            descending = desc;
            if (btnDesc == null || btnAsc == null) return;

            btnDesc.BaseColor = desc ? Palette.Accent : Palette.Card;
            btnDesc.ForeColor = desc ? Color.White : Palette.TextPrimary;
            btnDesc.BorderColor = desc ? Color.Transparent : Palette.Border;
            btnDesc.HoverColor = desc ? Palette.AccentHover : Palette.HoverGray;
            btnDesc.PressColor = desc ? Palette.AccentPressed : Palette.PressGray;

            btnAsc.BaseColor = desc ? Palette.Card : Palette.Accent;
            btnAsc.ForeColor = desc ? Palette.TextPrimary : Color.White;
            btnAsc.BorderColor = desc ? Palette.Border : Color.Transparent;
            btnAsc.HoverColor = desc ? Palette.HoverGray : Palette.AccentHover;
            btnAsc.PressColor = desc ? Palette.PressGray : Palette.AccentPressed;

            btnDesc.Invalidate();
            btnAsc.Invalidate();
        }

        private void EnableDrop(Control root)
        {
            if (root is TextBox || root is ComboBox) return;
            root.AllowDrop = true;
            root.DragEnter += OnDragEnter;
            root.DragDrop += OnDragDrop;
            foreach (Control c in root.Controls) EnableDrop(c);
        }

        private void OnDragEnter(object sender, DragEventArgs e)
        {
            e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop)
                ? DragDropEffects.Copy : DragDropEffects.None;
        }

        private void OnDragDrop(object sender, DragEventArgs e)
        {
            string[] paths = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (paths == null || paths.Length == 0) return;
            string path = paths[0];
            if (Directory.Exists(path)) LoadFolder(path);
            else if (File.Exists(path)) LoadFolder(Path.GetDirectoryName(path));
            else MessageBox.Show("只能拖入文件夹或文件。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void Browse()
        {
            using (FolderBrowserDialog dlg = new FolderBrowserDialog())
            {
                dlg.Description = "请选择要重命名的文件夹";
                if (!string.IsNullOrEmpty(currentDir)) dlg.SelectedPath = currentDir;
                if (dlg.ShowDialog() == DialogResult.OK) LoadFolder(dlg.SelectedPath);
            }
        }

        private void LoadFolder(string dir)
        {
            currentDir = dir;
            txtFolder.Text = dir;
            currentFiles.Clear();

            if (!Directory.Exists(dir))
            {
                lblStatus.Text = "文件夹不存在";
                btnRename.Enabled = false;
                listView.Items.Clear();
                return;
            }

            FileInfo[] files;
            try { files = new DirectoryInfo(dir).GetFiles(); }
            catch (Exception ex)
            {
                lblStatus.Text = "读取失败：" + ex.Message;
                btnRename.Enabled = false;
                listView.Items.Clear();
                return;
            }

            string exePath = Application.ExecutablePath;
            var list = new List<FileInfo>();
            foreach (var f in files)
            {
                if (string.Equals(f.FullName, exePath, StringComparison.OrdinalIgnoreCase)) continue;
                list.Add(f);
            }

            currentFiles = list;
            ApplySort();
            RefreshPreview();

            int n = currentFiles.Count;
            btnRename.Enabled = n > 0;
            lblStatus.Text = n > 0 ? "共 " + n + " 个文件" : "该文件夹没有文件";
        }

        private void ApplySort()
        {
            string key = cmbSort.SelectedItem == null ? "名称" : cmbSort.SelectedItem.ToString();
            switch (key)
            {
                case "修改时间":
                    currentFiles.Sort((a, b) => b.LastWriteTime.CompareTo(a.LastWriteTime));
                    break;
                case "创建时间":
                    currentFiles.Sort((a, b) => b.CreationTime.CompareTo(a.CreationTime));
                    break;
                case "大小":
                    currentFiles.Sort((a, b) => b.Length.CompareTo(a.Length));
                    break;
                case "类型":
                    currentFiles.Sort((a, b) =>
                    {
                        int c = string.Compare(Path.GetExtension(a.Name), Path.GetExtension(b.Name), true);
                        return c != 0 ? c : StrCmpLogicalW(a.Name, b.Name);
                    });
                    break;
                default:
                    currentFiles.Sort((a, b) => StrCmpLogicalW(a.Name, b.Name));
                    break;
            }
        }

        private void RefreshPreview()
        {
            listView.Items.Clear();
            int n = currentFiles.Count;
            int digits = n.ToString().Length;

            for (int i = 0; i < n; i++)
            {
                int number = descending ? (n - i) : (i + 1);
                string numStr = swPad.Checked ? number.ToString("D" + digits) : number.ToString();
                string newName = numStr + currentFiles[i].Extension;

                ListViewItem item = new ListViewItem(currentFiles[i].Name);
                item.SubItems.Add(newName);
                listView.Items.Add(item);
            }
        }

        private void listView_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            using (var b = new SolidBrush(Palette.HeaderBg))
                e.Graphics.FillRectangle(b, e.Bounds);
            var rect = new Rectangle(e.Bounds.X + P(12), e.Bounds.Y, e.Bounds.Width - P(12), e.Bounds.Height);
            TextRenderer.DrawText(e.Graphics, e.Header.Text, new Font("Microsoft YaHei UI", 9F, FontStyle.Bold),
                rect, Palette.TextSecondary, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
            using (var p = new Pen(Palette.Border))
                e.Graphics.DrawLine(p, e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);
        }

        private void listView_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            bool hot = (e.ItemIndex == hoverIndex) || e.Item.Selected;
            Color bg = hot ? Palette.AccentTint : Palette.Card;
            using (var b = new SolidBrush(bg)) e.Graphics.FillRectangle(b, e.Bounds);
        }

        private void listView_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            Color fg = e.ColumnIndex == 0 ? Palette.TextPrimary : Palette.AccentText;
            var rect = new Rectangle(e.Bounds.X + P(12), e.Bounds.Y, e.Bounds.Width - P(12), e.Bounds.Height);
            TextRenderer.DrawText(e.Graphics, e.SubItem.Text, new Font("Microsoft YaHei UI", 9F), rect, fg,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }

        private void DoRename()
        {
            int n = currentFiles.Count;
            if (n == 0) { MessageBox.Show("没有文件可以重命名。"); return; }

            var result = MessageBox.Show(
                "即将把 " + n + " 个文件重命名为数字，\n原文件顺序保持不变。\n\n确定继续吗？",
                "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result != DialogResult.Yes) return;

            int digits = n.ToString().Length;
            string tmpPrefix = "~rename_" + Guid.NewGuid().ToString("N") + "_";

            List<string> tmpPaths = new List<string>();
            List<string> origPaths = new List<string>();
            for (int i = 0; i < n; i++) origPaths.Add(currentFiles[i].FullName);

            try
            {
                for (int i = 0; i < n; i++)
                {
                    string tmp = Path.Combine(currentDir, tmpPrefix + i + currentFiles[i].Extension);
                    File.Move(currentFiles[i].FullName, tmp);
                    tmpPaths.Add(tmp);
                }
                for (int i = 0; i < n; i++)
                {
                    int number = descending ? (n - i) : (i + 1);
                    string numStr = swPad.Checked ? number.ToString("D" + digits) : number.ToString();
                    string final = Path.Combine(currentDir, numStr + currentFiles[i].Extension);
                    File.Move(tmpPaths[i], final);
                }
            }
            catch (Exception ex)
            {
                for (int i = 0; i < n; i++)
                {
                    if (i < tmpPaths.Count && File.Exists(tmpPaths[i]))
                    {
                        try { File.Move(tmpPaths[i], origPaths[i]); } catch { }
                    }
                }
                MessageBox.Show("重命名出错：" + ex.Message + "\n\n已尝试恢复原文件名。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LoadFolder(currentDir);
                return;
            }

            LoadFolder(currentDir);
            lblStatus.Text = "完成！已重命名 " + n + " 个文件。";
            MessageBox.Show("重命名完成！", "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    static class Program
    {
        [DllImport("user32.dll")]
        static extern bool SetProcessDpiAwarenessContext(IntPtr value);

        [DllImport("user32.dll")]
        static extern bool SetProcessDPIAware();

        [STAThread]
        static void Main()
        {
            try
            {
                if (!SetProcessDpiAwarenessContext(new IntPtr(-4)))
                {
                    SetProcessDPIAware();
                }
            }
            catch { SetProcessDPIAware(); }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
