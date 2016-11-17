using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace Ruler
{
    public sealed class MainForm : Form, IRulerInfo
    {
        #region ResizeRegion enum

        private enum ResizeRegion
        {
            None, N, NE, E, SE, S, SW, W, NW
        }

        #endregion ResizeRegion enum

        private readonly ToolTip _toolTip = new ToolTip();
        private Point _offset;
        private Rectangle _mouseDownRect;
        private readonly int _resizeBorderWidth = 5;
        private bool _crossLineVisible;
        private Font _crossLineFont;
        private Point _mouseDownPoint;
        private ResizeRegion _resizeRegion = ResizeRegion.None;
        private readonly ContextMenu _menu = new ContextMenu();
        private MenuItem _verticalMenuItem;
        private MenuItem _toolTipMenuItem;
        private MenuItem _lockedMenuItem;

        public MainForm()
        {
            var rulerInfo = RulerInfo.GetDefaultRulerInfo();
            Init(rulerInfo);
        }

        public MainForm(RulerInfo rulerInfo)
        {
            Init(rulerInfo);
        }

        public bool IsVertical
        {
            get { return _verticalMenuItem.Checked; }
            set { _verticalMenuItem.Checked = value; }
        }

        public bool IsLocked { get; set; }

        public bool ShowToolTip
        {
            get
            {
                return _toolTipMenuItem.Checked;
            }
            set
            {
                _toolTipMenuItem.Checked = value;
                if (value)
                    SetToolTip();
            }
        }

        private void Init(RulerInfo rulerInfo)
        {
            SetStyle(ControlStyles.ResizeRedraw, true);
            UpdateStyles();

            Icon = Icon.ExtractAssociatedIcon(Process.GetCurrentProcess().MainModule.FileName);
//            var resources = new ResourceManager(typeof(MainForm));
//            Icon = (Icon)resources.GetObject("$this.Icon");
            FormBorderStyle = FormBorderStyle.None;
            Text = "Ruler";
            BackColor = Color.DarkOrange;

            SetUpMenu(rulerInfo);
            ContextMenu = _menu;
            Font = new Font("Verdana", 8);
            _crossLineFont = new Font("Verdana", 7, FontStyle.Bold);

            SetStyle(ControlStyles.DoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);

            rulerInfo.CopyInto(this);
        }

        private RulerInfo GetRulerInfo()
        {
            var rulerInfo = new RulerInfo();
            this.CopyInto(rulerInfo);
            return rulerInfo;
        }

        private void SetUpMenu(RulerInfo rulerInfo)
        {
            var _topMost = AddMenuItem("Stay On Top", Shortcut.CtrlT);
            _topMost.Checked = rulerInfo.TopMost;
            _verticalMenuItem = AddMenuItem("Vertical", Shortcut.CtrlV);
            _toolTipMenuItem = AddMenuItem("Tool Tip");
            var opacityMenuItem = AddMenuItem("Opacity");
            _lockedMenuItem = AddMenuItem("Lock resizing", Shortcut.None, LockHandler);
            AddMenuItem("Set size...", Shortcut.CtrlS, SetWidthHeightHandler);
            AddMenuItem("Duplicate", Shortcut.CtrlD, DuplicateHandler);
            AddMenuItem("-");
            AddMenuItem("About...");
            AddMenuItem("-");
            AddMenuItem("Exit");

            for (var i = 10; i <= 100; i += 10)
            {
                var subMenu = new MenuItem(i + "%") { Checked = i == (int)Math.Round(rulerInfo.Opacity * 100) };
                subMenu.Click += OpacityMenuHandler;
                opacityMenuItem.MenuItems.Add(subMenu);
            }
        }

        private void SetWidthHeightHandler(object sender, EventArgs e)
        {
            var form = new SetSizeForm(Width, Height);
            if (TopMost)
                form.TopMost = true;
            if (form.ShowDialog() != DialogResult.OK) return;
            var size = form.GetNewSize();
            Width = size.Width;
            Height = size.Height;
        }

        private void LockHandler(object sender, EventArgs e)
        {
            IsLocked = !IsLocked;
            _lockedMenuItem.Checked = IsLocked;
        }

        private void DuplicateHandler(object sender, EventArgs e)
        {
            var exe = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var rulerInfo = GetRulerInfo();
            var startInfo = new ProcessStartInfo(exe, rulerInfo.ConvertToParameters());
            var process = new Process { StartInfo = startInfo };
            process.Start();
        }

        private MenuItem AddMenuItem(string text, Shortcut shortcut = Shortcut.None)
        {
            return AddMenuItem(text, shortcut, MenuHandler);
        }

        private MenuItem AddMenuItem(string text, Shortcut shortcut, EventHandler handler)
        {
            var mi = new MenuItem(text);
            mi.Click += handler;
            mi.Shortcut = shortcut;
            _menu.MenuItems.Add(mi);
            return mi;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            _offset = new Point(MousePosition.X - Location.X, MousePosition.Y - Location.Y);
            _mouseDownPoint = MousePosition;
            _mouseDownRect = ClientRectangle;

            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            _resizeRegion = ResizeRegion.None;
            base.OnMouseUp(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (_resizeRegion != ResizeRegion.None)
            {
                HandleResize();
                return;
            }

            var clientCursorPos = PointToClient(MousePosition);
            var resizeInnerRect = ClientRectangle;
            resizeInnerRect.Inflate(-_resizeBorderWidth, -_resizeBorderWidth);

            var inResizableArea = ClientRectangle.Contains(clientCursorPos) && !resizeInnerRect.Contains(clientCursorPos);
            if (inResizableArea)
            {
                var resizeRegion = GetResizeRegion(clientCursorPos);
                SetResizeCursor(resizeRegion);

                if (e.Button == MouseButtons.Left)
                {
                    _resizeRegion = resizeRegion;
                    HandleResize();
                }
            }
            else
            {
                Cursor = Cursors.Default;
                if (e.Button == MouseButtons.Left)
                    Location = new Point(MousePosition.X - _offset.X, MousePosition.Y - _offset.Y);
                else
                    Invalidate();
            }
            base.OnMouseMove(e);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            _crossLineVisible = true;
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _crossLineVisible = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnResize(EventArgs e)
        {
            if (ShowToolTip)
                SetToolTip();
            base.OnResize(e);
        }

        private void SetToolTip()
        {
            _toolTip.SetToolTip(this, string.Format("Width: {0} pixels\nHeight: {1} pixels", Width, Height));
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Right:
                case Keys.Left:
                case Keys.Up:
                case Keys.Down:
                    HandleMoveResizeKeystroke(e);
                    break;

                case Keys.Space:
                    ChangeOrientation();
                    break;
            }
            base.OnKeyDown(e);
        }

        private void HandleMoveResizeKeystroke(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Right:
                    if (e.Control)
                    {
                        if (e.Shift)
                            Width += 1;
                        else
                            Left += 1;
                    }
                    else
                        Left += 5;
                    break;
                case Keys.Left:
                    if (e.Control)
                    {
                        if (e.Shift)
                            Width -= 1;
                        else
                            Left -= 1;
                    }
                    else
                        Left -= 5;
                    break;
                case Keys.Up:
                    if (e.Control)
                    {
                        if (e.Shift)
                            Height -= 1;
                        else
                            Top -= 1;
                    }
                    else
                        Top -= 5;
                    break;
                case Keys.Down:
                    if (e.Control)
                    {
                        if (e.Shift)
                            Height += 1;
                        else
                            Top += 1;
                    }
                    else
                        Top += 5;
                    break;
            }
        }

        private void HandleResize()
        {
            if (IsLocked)
                return;
            switch (_resizeRegion)
            {
                case ResizeRegion.E:
                {
                    var diff = MousePosition.X - _mouseDownPoint.X;
                    Width = _mouseDownRect.Width + diff;
                    break;
                }
                case ResizeRegion.S:
                {
                    var diff = MousePosition.Y - _mouseDownPoint.Y;
                    Height = _mouseDownRect.Height + diff;
                    break;
                }
                case ResizeRegion.SE:
                {
                    Width = _mouseDownRect.Width + MousePosition.X - _mouseDownPoint.X;
                    Height = _mouseDownRect.Height + MousePosition.Y - _mouseDownPoint.Y;
                    break;
                }
            }
        }

        private void SetResizeCursor(ResizeRegion region)
        {
            switch (region)
            {
                case ResizeRegion.N:
                case ResizeRegion.S:
                    Cursor = Cursors.SizeNS;
                    break;
                case ResizeRegion.E:
                case ResizeRegion.W:
                    Cursor = Cursors.SizeWE;
                    break;
                case ResizeRegion.NW:
                case ResizeRegion.SE:
                    Cursor = Cursors.SizeNWSE;
                    break;
                default:
                    Cursor = Cursors.SizeNESW;
                    break;
            }
        }

        private ResizeRegion GetResizeRegion(Point clientCursorPos)
        {
            if (clientCursorPos.Y <= _resizeBorderWidth)
            {
                if (clientCursorPos.X <= _resizeBorderWidth) return ResizeRegion.NW;
                if (clientCursorPos.X >= Width - _resizeBorderWidth) return ResizeRegion.NE;
                return ResizeRegion.N;
            }
            if (clientCursorPos.Y >= Height - _resizeBorderWidth)
            {
                if (clientCursorPos.X <= _resizeBorderWidth) return ResizeRegion.SW;
                if (clientCursorPos.X >= Width - _resizeBorderWidth) return ResizeRegion.SE;
                return ResizeRegion.S;
            }
            if (clientCursorPos.X <= _resizeBorderWidth) return ResizeRegion.W;
            return ResizeRegion.E;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var graphics = e.Graphics;
            var height = Height;
            var width = Width;
            if (IsVertical)
            {
                graphics.RotateTransform(90);
                graphics.TranslateTransform(0, -Width + 1);
                height = Width;
                width = Height;
            }
            DrawRuler(graphics, width, height);
            DrawCrossLine(graphics, width, height);
            base.OnPaint(e);
        }

        private void DrawRuler(Graphics g, int formWidth, int formHeight)
        {
            // Border
            g.DrawRectangle(Pens.Black, 0, 0, formWidth - 1, formHeight - 1);
            // Width
            g.DrawString(string.Format("{0}/{1} px", formWidth, formHeight), Font, Brushes.Black, 10, formHeight/2 - Font.Height/2);
            // Ticks
            for (var i = 0; i < formWidth; i++)
            {
                if (i%2 != 0) continue;
                int tickHeight;
                if (i%100 == 0)
                {
                    tickHeight = 15;
                    DrawTickLabel(g, i.ToString(), i, formHeight, tickHeight);
                }
                else if (i%10 == 0)
                {
                    tickHeight = 10;
                }
                else
                {
                    tickHeight = 5;
                }
                DrawTick(g, i, formHeight, tickHeight);
            }
        }

        private void DrawCrossLine(Graphics g, int formWidth, int formHeight)
        {
            var pos = PointToClient(MousePosition);
            if (!_crossLineVisible) return;
            if (IsVertical)
            {
                var tmp = pos.X;
                pos.X = pos.Y;
                pos.Y = formHeight - tmp;
            }
            g.DrawLine(Pens.Red, new Point(pos.X, 0), new Point(pos.X, formHeight));
            g.DrawLine(Pens.Red, new Point(0, pos.Y), new Point(formWidth, pos.Y));
            g.DrawString(string.Format("{0}/{1} px", pos.X, pos.Y), _crossLineFont, Brushes.Red, pos.X, pos.Y - (IsVertical ? 0 : 12));
        }

        private static void DrawTick(Graphics g, int xPos, int formHeight, int tickHeight)
        {
            // Top
            g.DrawLine(Pens.Black, xPos, 0, xPos, tickHeight);
            // Bottom
            g.DrawLine(Pens.Black, xPos, formHeight, xPos, formHeight - tickHeight);
        }

        private void DrawTickLabel(Graphics g, string text, int xPos, int formHeight, int height)
        {
            // Top
            g.DrawString(text, Font, Brushes.Black, xPos, height);
            // Bottom
            g.DrawString(text, Font, Brushes.Black, xPos, formHeight - height - Font.Height);
        }

        private static void Main(params string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var mainForm = args.Length == 0 ? new MainForm() : new MainForm(RulerInfo.ConvertToRulerInfo(args));
            Application.Run(mainForm);
        }

        private void OpacityMenuHandler(object sender, EventArgs e)
        {
            var mi = (MenuItem)sender;
            UncheckMenuItem(mi.Parent);
            mi.Checked = true;
            Opacity = double.Parse(mi.Text.Replace("%", "")) / 100;
        }

        private void UncheckMenuItem(Menu parent)
        {
            if (parent == null)
                return;
            for (var i = 0; i < parent.MenuItems.Count; i++)
            {
                if (parent.MenuItems[i].Checked)
                    parent.MenuItems[i].Checked = false;
            }
        }

        private void MenuHandler(object sender, EventArgs e)
        {
            var mi = (MenuItem)sender;
            switch (mi.Text)
            {
                case "Exit":
                    Close();
                    break;
                case "Tool Tip":
                    ShowToolTip = !ShowToolTip;
                    break;
                case "Vertical":
                    ChangeOrientation();
                    break;
                case "Stay On Top":
                    mi.Checked = !mi.Checked;
                    TopMost = mi.Checked;
                    break;
                case "About...":
                    var message = string.Format("Ruler v{0}\nby Sergiy Yegoshyn (egoshin.sergey@gmail.com)", Application.ProductVersion);
                    MessageBox.Show(message, "About Ruler", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
                default:
                    MessageBox.Show("Unknown menu item.");
                    break;
            }
        }

        private void ChangeOrientation()
        {
            IsVertical = !IsVertical;
            var width = Width;
            Width = Height;
            Height = width;
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // MainForm
            // 
            this.ClientSize = new System.Drawing.Size(300, 300);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "MainForm";
            this.ResumeLayout(false);

        }
    }
}