using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Text;

namespace WindowPin;

public class PinForm : Form
{
    public enum Mode { Pin, Unpin }

    private readonly Mode _mode;
    private bool _dragging;
    private Point _dragStart;
    private int _feedbackType;
    private System.Windows.Forms.Timer _feedbackTimer = null!;
    private const int SIZE = 56;

    private static PinForm? _activePicker;
    private static IntPtr _hookId;
    private static NativeMethods.LowLevelMouseProc? _hookProc;
    private static readonly HashSet<IntPtr> PinHandles = new();
    private Cursor? _oldCursor;
    private double _oldOpacity;

    private static readonly Color GreenMain  = Color.FromArgb(76, 175, 80);
    private static readonly Color RedMain    = Color.FromArgb(229, 57, 53);
    private static readonly Color GoldFlash  = Color.FromArgb(255, 235, 59);
    private static readonly Color OrangeFlash = Color.FromArgb(255, 138, 101);
    private static readonly Color GrayFlash  = Color.FromArgb(160, 160, 160);

    private Color FeedbackColor => _feedbackType switch
    {
        1 => _mode == Mode.Pin ? GoldFlash : OrangeFlash,
        2 => GrayFlash,
        _ => BackColor
    };

    private string LogTag => _mode == Mode.Pin ? "[绿]" : "[红]";

    protected override bool ShowWithoutActivation => true;

    protected override CreateParams CreateParams
    {
        get
        {
            var cp = base.CreateParams;
            cp.ExStyle |= 0x08000000 | (int)NativeMethods.WS_EX_TOOLWINDOW;
            return cp;
        }
    }

    public PinForm(Mode mode)
    {
        _mode = mode;
        Size = new Size(SIZE, SIZE);
        FormBorderStyle = FormBorderStyle.None;
        ShowInTaskbar = false;
        TopMost = true;
        BackColor = mode == Mode.Pin ? GreenMain : RedMain;
        DoubleBuffered = true;
        _feedbackTimer = new System.Windows.Forms.Timer { Interval = 700 };
        _feedbackTimer.Tick += (_, _) => { _feedbackType = 0; _feedbackTimer.Stop(); Invalidate(); };
    }

    protected override void OnHandleCreated(EventArgs e) { base.OnHandleCreated(e); PinHandles.Add(Handle); }
    protected override void OnHandleDestroyed(EventArgs e) { PinHandles.Remove(Handle); base.OnHandleDestroyed(e); }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        using var path = new GraphicsPath();
        path.AddEllipse(0, 0, SIZE - 1, SIZE - 1);
        Region = new Region(path);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var bg = _feedbackType > 0 ? FeedbackColor : BackColor;
        int m = 2;
        using (var sb = new SolidBrush(Color.FromArgb(30, 0, 0, 0)))
            g.FillEllipse(sb, m + 2, m + 2, SIZE - 5, SIZE - 5);
        using (var bb = new SolidBrush(bg))
            g.FillEllipse(bb, m, m, SIZE - 5, SIZE - 5);
        using var bp = new Pen(Color.White, 2.5f);
        g.DrawEllipse(bp, m + 1, m + 1, SIZE - 7, SIZE - 7);
        var icon = _mode == Mode.Pin ? "📌" : "📍";
        using var f = new Font("Segoe UI Emoji", 22f);
        var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        g.DrawString(icon, f, Brushes.White, new RectangleF(0, 0, SIZE, SIZE), sf);
        if (_feedbackType > 0)
        {
            using var hp = new Pen(_feedbackType == 2 ? Color.Gray : Color.White, 3f);
            g.DrawEllipse(hp, m + 3, m + 3, SIZE - 11, SIZE - 11);
        }
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.Button != MouseButtons.Left || _activePicker != null) return;
        _dragging = true;
        _dragStart = e.Location;
    }
    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (_dragging)
            Location = new Point(Location.X + e.X - _dragStart.X, Location.Y + e.Y - _dragStart.Y);
    }
    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        if (!_dragging) return;
        _dragging = false;
        if (Math.Abs(e.X - _dragStart.X) + Math.Abs(e.Y - _dragStart.Y) < 4) EnterPickMode();
    }

    private void EnterPickMode()
    {
        if (_activePicker != null) return;
        _activePicker = this;
        _oldCursor = Cursor; _oldOpacity = Opacity;
        Cursor = Cursors.Cross; Opacity = 0.6;
        _hookProc = HookCallback;
        _hookId = NativeMethods.SetWindowsHookEx(NativeMethods.WH_MOUSE_LL, _hookProc,
            NativeMethods.GetModuleHandle(null!), 0);
    }
    private void ExitPickMode()
    {
        if (_hookId != IntPtr.Zero) { NativeMethods.UnhookWindowsHookEx(_hookId); _hookId = IntPtr.Zero; }
        _hookProc = null;
        Cursor = _oldCursor ?? Cursors.Default; Opacity = _oldOpacity;
        _activePicker = null;
    }

    private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && wParam == (IntPtr)NativeMethods.WM_LBUTTONDOWN)
        {
            var hs = Marshal.PtrToStructure<NativeMethods.MSLLHOOKSTRUCT>(lParam);
            var hWnd = NativeMethods.FindWindowAtPoint(hs.pt.x, hs.pt.y, PinHandles);
            var picker = _activePicker;
            if (picker != null && hWnd != IntPtr.Zero)
                picker.BeginInvoke(() => { picker.ExitPickMode(); picker.ProcessPick(hWnd); });
            else
                picker?.BeginInvoke(() => picker.ExitPickMode());
            return (IntPtr)1;
        }
        return NativeMethods.CallNextHookEx(_hookId, nCode, wParam, lParam);
    }

    private void ProcessPick(IntPtr hWnd)
    {
        var clsSb = new StringBuilder(256);
        NativeMethods.GetClassName(hWnd, clsSb, 256);
        Program.Log($"{LogTag} 目标: hWnd={hWnd} [{clsSb}]");

        if (hWnd == IntPtr.Zero || PinHandles.Contains(hWnd)) return;
        if (!NativeMethods.IsWindowVisible(hWnd)) return;
        if (NativeMethods.IsSystemWindow(hWnd)) return;

        DoTopMost(hWnd);
    }

    public void ApplyTopMost(IntPtr hWnd)
    {
        if (hWnd == IntPtr.Zero || PinHandles.Contains(hWnd)) return;
        if (!NativeMethods.IsWindowVisible(hWnd)) return;
        if (NativeMethods.IsSystemWindow(hWnd)) return;
        DoTopMost(hWnd);
    }

    /// <summary>★ 用 SetWindowLong 直接改 WS_EX_TOPMOST 位</summary>
    private void DoTopMost(IntPtr hWnd)
    {
        long ex = NativeMethods.GetWindowLongPtr(hWnd, NativeMethods.GWL_EXSTYLE).ToInt64();
        bool isTop = (ex & NativeMethods.WS_EX_TOPMOST) != 0;
        var clsSb = new StringBuilder(256);
        NativeMethods.GetClassName(hWnd, clsSb, 256);
        Program.Log($"{LogTag} DoTopMost: hWnd={hWnd} [{clsSb}] isTop={isTop}");

        if (_mode == Mode.Pin)
        {
            if (!isTop)
            {
                bool ok = NativeMethods.SetTopMost(hWnd, true);
                Program.Log($"{LogTag} SetTopMost(true) → {(ok?"ok":"FAIL")}");
                _feedbackType = 1;
            }
            else
            {
                Program.Log($"{LogTag} 已置顶");
                _feedbackType = 2;
            }
        }
        else
        {
            if (isTop)
            {
                bool ok = NativeMethods.SetTopMost(hWnd, false);
                Program.Log($"{LogTag} SetTopMost(false) → {(ok?"ok":"FAIL")}");
                _feedbackType = 1;
            }
            else
            {
                Program.Log($"{LogTag} 未置顶");
                _feedbackType = 2;
            }
        }

        // 钉子自己保持最上
        _ = NativeMethods.SetWindowPos(Handle, NativeMethods.HWND_TOPMOST,
            0, 0, 0, 0, NativeMethods.SWP_NOSIZE | NativeMethods.SWP_NOMOVE);

        _feedbackTimer.Stop();
        _feedbackTimer.Start();
        Invalidate();
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        if (_activePicker == this) ExitPickMode();
        _feedbackTimer.Stop(); _feedbackTimer.Dispose();
        base.OnClosing(e);
    }
}

