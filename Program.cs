using System.ComponentModel;
using System.Drawing.Imaging;
using System.Text;

namespace WindowPin;

internal static class Program
{
    private static PinForm? _greenPin;
    private static PinForm? _redPin;
    private static NotifyIcon? _trayIcon;
    private static HotkeyHost? _hotkeyHost;

    private const int PIN_SIZE = 56;
    private const int GAP = 6;

    public static readonly string LogPath =
        Path.Combine(Path.GetDirectoryName(Application.ExecutablePath)!, "debug.log");

    public static void Log(string msg)
    {
        try { File.AppendAllText(LogPath, $"{DateTime.Now:HH:mm:ss.fff} {msg}\n"); }
        catch { }
    }

    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.SetHighDpiMode(HighDpiMode.DpiUnaware);

        try { File.WriteAllText(LogPath, ""); } catch { }
        Log("=== 钉子启动 ===");

        var screen = Screen.PrimaryScreen!;
        int startX = screen.WorkingArea.Right - PIN_SIZE - 10;
        int startY = screen.WorkingArea.Bottom - PIN_SIZE * 2 - GAP - 10;

        _greenPin = new PinForm(PinForm.Mode.Pin) { Location = new Point(startX, startY) };
        _greenPin.Show();
        Log($"绿钉子: {_greenPin.Handle} at ({startX},{startY})");

        _redPin = new PinForm(PinForm.Mode.Unpin) { Location = new Point(startX, startY + PIN_SIZE + GAP) };
        _redPin.Show();
        Log($"红钉子: {_redPin.Handle}");

        _trayIcon = new NotifyIcon
        {
            Icon = CreateTrayIcon(),
            Text = "钉子",
            Visible = true,
            ContextMenuStrip = new ContextMenuStrip()
        };
        var menu = _trayIcon.ContextMenuStrip;
        menu.Items.Add("置顶当前窗口 (Ctrl+Shift+T)", null, (_, _) => PinForeground());
        menu.Items.Add("取消置顶当前窗口", null, (_, _) => UnpinForeground());
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("显示/隐藏钉子", null, (_, _) =>
        {
            _greenPin!.Visible = !_greenPin.Visible;
            _redPin!.Visible = !_redPin.Visible;
        });
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("退出", null, (_, _) => ExitApp());

        _trayIcon.MouseClick += (_, e) =>
        {
            if (e.Button == MouseButtons.Left)
            {
                _greenPin!.Visible = !_greenPin.Visible;
                _redPin!.Visible = !_redPin.Visible;
            }
        };

        _hotkeyHost = new HotkeyHost(_greenPin);

        try { Application.Run(_hotkeyHost); }
        finally { ExitApp(); }
    }

    private static void PinForeground()
    {
        var hWnd = NativeMethods.GetForegroundWindow();
        Log($"托盘置顶: hWnd={hWnd}");
        _greenPin!.ApplyTopMost(hWnd);
    }
    private static void UnpinForeground()
    {
        var hWnd = NativeMethods.GetForegroundWindow();
        Log($"托盘取消: hWnd={hWnd}");
        _redPin!.ApplyTopMost(hWnd);
    }
    private static void ExitApp()
    {
        Log("退出");
        _hotkeyHost?.Unregister();
        _trayIcon?.Dispose();
        _greenPin?.Close();
        _redPin?.Close();
        Application.Exit();
    }

    private static Icon CreateTrayIcon()
    {
        using var bmp = new Bitmap(32, 32);
        using var g = Graphics.FromImage(bmp);
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        g.Clear(Color.Transparent);
        using var bg = new SolidBrush(Color.FromArgb(66, 133, 244));
        g.FillEllipse(bg, 1, 1, 30, 30);
        using var bp = new Pen(Color.White, 2f);
        g.DrawEllipse(bp, 1, 1, 30, 30);
        using var f = new Font("Segoe UI Emoji", 15f);
        var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        g.DrawString("📌", f, Brushes.White, new RectangleF(0, 0, 32, 32), sf);
        return Icon.FromHandle(bmp.GetHicon());
    }
}

internal class HotkeyHost : Form
{
    private const int HOTKEY_ID = 1;
    private readonly PinForm _pinForm;

    public HotkeyHost(PinForm pinForm)
    {
        _pinForm = pinForm;
        Size = new Size(0, 0);
        FormBorderStyle = FormBorderStyle.None;
        ShowInTaskbar = false;
        WindowState = FormWindowState.Minimized;
        ControlBox = false;
        Text = string.Empty;
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        _ = NativeMethods.RegisterHotKey(Handle, HOTKEY_ID,
            NativeMethods.MOD_CONTROL | NativeMethods.MOD_SHIFT | NativeMethods.MOD_NOREPEAT, 0x54);
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == NativeMethods.WM_HOTKEY && m.WParam == HOTKEY_ID)
        {
            var hWnd = NativeMethods.GetForegroundWindow();
            Program.Log($"热键: hWnd={hWnd}");
            _pinForm.ApplyTopMost(hWnd);
        }
        base.WndProc(ref m);
    }

    public void Unregister()
    {
        if (IsHandleCreated)
            _ = NativeMethods.UnregisterHotKey(Handle, HOTKEY_ID);
    }
}

