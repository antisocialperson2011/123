public static class Theme
{
    public static Color Bg = Color.FromArgb(243, 245, 250), Card = Color.White, Accent = Color.FromArgb(79, 70, 229),
        Text = Color.FromArgb(30, 41, 59), Muted = Color.FromArgb(100, 116, 139), Line = Color.FromArgb(226, 232, 240), Danger = Color.FromArgb(220, 38, 38);
    public static Font F(float s, bool b = false) => new("Segoe UI", s, b ? FontStyle.Bold : FontStyle.Regular);
    public static GraphicsPath Round(Rectangle r, int d)
    {
        d = Math.Max(1, Math.Min(d * 2, Math.Min(r.Width, r.Height))); var p = new GraphicsPath();
        p.AddArc(r.X, r.Y, d, d, 180, 90); p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
        p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90); p.AddArc(r.X, r.Bottom - d, d, d, 90, 90); p.CloseFigure(); return p;
    }
    public static Label Lbl(string t, float s, bool b, Color c, int x, int y, int w = 0) =>
        new() { Text = t, Font = F(s, b), ForeColor = c, BackColor = Color.Transparent, Left = x, Top = y, AutoSize = w == 0, Width = w, Height = w == 0 ? 0 : (int)(s * 2.2) };
    public static Color Behind(Control c)
    {
        for (var p = c.Parent; p != null; p = p.Parent) { if (p is RoundPanel rp) return rp.Fill; if (p.BackColor.A == 255) return p.BackColor; }
        return Bg;
    }
    public static void Copy(RoundButton b, string text)
    {
        try { Clipboard.SetDataObject(text, true, 10, 100); }   // 10 попыток с паузой 100 мс
        catch { InputDialog.Ask("Скопируйте код вручную (Ctrl+C)", false, text); return; }
        var old = b.Text; b.Text = "Скопировано ✓"; b.Invalidate();
        var t = new System.Windows.Forms.Timer { Interval = 1500 };
        t.Tick += (_, _) => { t.Stop(); t.Dispose(); if (!b.IsDisposed) { b.Text = old; b.Invalidate(); } };
        t.Start();
    }
}

public class RoundPanel : Panel
{
    public int Radius = 16; public Color Fill = Theme.Card; public bool Border;
    public RoundPanel()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);
        BackColor = Color.Transparent;
    }
    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
        using var path = Theme.Round(new Rectangle(0, 0, Width - 1, Height - 1), Radius);
        using var b = new SolidBrush(Fill); g.FillPath(b, path);
        if (Border) { using var pen = new Pen(Theme.Line); g.DrawPath(pen, path); }
    }
}

public class RoundButton : Button
{
    public Color Fill = Theme.Accent, TextColor = Color.White; public int Radius = 12; bool hov;
    public RoundButton() { FlatStyle = FlatStyle.Flat; FlatAppearance.BorderSize = 0; Cursor = Cursors.Hand; Font = Theme.F(10, true); Height = 42; }
    protected override void OnMouseEnter(EventArgs e) { hov = true; Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { hov = false; Invalidate(); base.OnMouseLeave(e); }
    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics; g.Clear(Theme.Behind(this)); g.SmoothingMode = SmoothingMode.AntiAlias;
        var c = hov ? ControlPaint.Dark(Fill, 0.1f) : Fill;
        using var path = Theme.Round(new Rectangle(0, 0, Width - 1, Height - 1), Radius); using var b = new SolidBrush(c); g.FillPath(b, path);
        TextRenderer.DrawText(g, Text, Font, ClientRectangle, TextColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
    }
}

public class RoundInput : RoundPanel
{
    public TextBox Box = new() { BorderStyle = BorderStyle.None, Font = Theme.F(11) };
    public RoundInput(int height = 42, bool multiline = false, bool password = false)
    {
        Radius = 12; Border = true; Fill = Color.White; Height = height; Box.Multiline = multiline; Box.UseSystemPasswordChar = password;
        Box.Location = new Point(12, multiline ? 10 : (height - Box.Height) / 2); Box.Width = Width - 24;
        if (multiline) Box.Height = height - 20; Box.Anchor = AnchorStyles.Left | AnchorStyles.Right | (multiline ? AnchorStyles.Top | AnchorStyles.Bottom : 0);
        Controls.Add(Box);
    }
    public string Value => Box.Text;
}

public static class InputDialog
{
    public static string Ask(string title, bool password = true, string initial = "")
    {
        var f = new Form { Text = title, Size = new Size(380, 200), StartPosition = FormStartPosition.CenterParent, FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false, MinimizeBox = false, BackColor = Theme.Bg };
        var l = Theme.Lbl(title, 11, true, Theme.Text, 20, 20); var inp = new RoundInput(42, false, password) { Left = 20, Top = 55, Width = 325 };
        inp.Box.Text = initial;
        var ok = new RoundButton { Text = "OK", Left = 245, Top = 108, Width = 100, DialogResult = DialogResult.OK };
        f.Controls.AddRange(new Control[] { l, inp, ok }); f.AcceptButton = ok;
        f.Shown += (_, _) => { inp.Box.Focus(); inp.Box.SelectAll(); };
        return f.ShowDialog() == DialogResult.OK ? inp.Value : null;
    }
}
