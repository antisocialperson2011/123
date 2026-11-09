public class AdminForm : Form
{
    bool polls = true; FlowLayoutPanel list; RoundButton tabP, tabU; Label stats;
    public AdminForm()
    {
        Text = "Админ-панель"; ClientSize = new Size(780, 620); MinimumSize = new Size(700, 400); StartPosition = FormStartPosition.CenterParent; BackColor = Theme.Bg; Font = Theme.F(10);
        var top = new Panel { Dock = DockStyle.Top, Height = 110 };
        top.Controls.Add(Theme.Lbl("⚙ Админ-панель", 15, true, Theme.Danger, 20, 12));
        stats = Theme.Lbl("", 9, false, Theme.Muted, 22, 48);
        tabP = new RoundButton { Text = "Голосования", Left = 20, Top = 66, Width = 140, Height = 36 };
        tabU = new RoundButton { Text = "Пользователи", Left = 168, Top = 66, Width = 140, Height = 36 };
        tabP.Click += (_, _) => { polls = true; Fill(); }; tabU.Click += (_, _) => { polls = false; Fill(); };
        top.Controls.AddRange(new Control[] { stats, tabP, tabU });
        list = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, FlowDirection = FlowDirection.TopDown, WrapContents = false, Padding = new Padding(20, 5, 20, 10) };
        list.SizeChanged += (_, _) => { foreach (Control c in list.Controls) c.Width = Math.Max(300, list.ClientSize.Width - 50); };
        Controls.Add(list); Controls.Add(top); Fill();
    }

    void Fill()
    {
        Store.Load();
        tabP.Fill = polls ? Theme.Accent : Theme.Line; tabP.TextColor = polls ? Color.White : Theme.Text;
        tabU.Fill = polls ? Theme.Line : Theme.Accent; tabU.TextColor = polls ? Theme.Text : Color.White; tabP.Invalidate(); tabU.Invalidate();
        stats.Text = $"Пользователей: {Store.Data.Users.Count}  •  Голосований: {Store.Data.Polls.Count}  •  Всего голосов: {Store.Data.Polls.Sum(p => p.Total)}";
        list.SuspendLayout(); list.Controls.Clear();
        if (polls) foreach (var p in Store.Data.Polls.AsEnumerable().Reverse()) list.Controls.Add(PollCard(p));
        else foreach (var u in Store.Data.Users) list.Controls.Add(UserCard(u));
        list.ResumeLayout();
    }

    RoundPanel Card() => new() { Height = 96, Width = Math.Max(300, list.ClientSize.Width - 50), Margin = new Padding(0, 0, 0, 12) };
    static RoundButton Btn(string t, int x, Color fill, Color tc) => new() { Text = t, Left = x, Top = 52, Width = 140, Height = 32, Radius = 10, Fill = fill, TextColor = tc, Font = Theme.F(9, true) };
    static bool Confirm(string t) => MessageBox.Show(t, "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;

    Control PollCard(Poll p)
    {
        var c = Card();
        c.Controls.Add(Theme.Lbl(p.Title, 11, true, Theme.Text, 20, 10));
        c.Controls.Add(Theme.Lbl($"Автор: {p.Owner}  •  Код: {p.Code}  •  Голосов: {p.Total}" + (p.IsPrivate ? "  •  🔒" : "") + (p.PassHash != "" ? "  •  🔑" : "") + (p.Closed ? "  •  завершено" : ""), 9, false, Theme.Muted, 20, 32));
        var cp = Btn("Копировать код", 20, Theme.Line, Theme.Text); cp.Click += (_, _) => Theme.Copy(cp, p.Code);
        var cl = Btn(p.Closed ? "Возобновить" : "Завершить", 170, Theme.Muted, Color.White); cl.Click += (_, _) => { p.Closed = !p.Closed; Store.Save(); Fill(); };
        var del = Btn("Удалить", 320, Theme.Danger, Color.White);
        del.Click += (_, _) => { if (Confirm($"Удалить голосование «{p.Title}»?")) { Store.Data.Polls.Remove(p); Store.Save(); Fill(); } };
        c.Controls.AddRange(new Control[] { cp, cl, del }); return c;
    }

    Control UserCard(User u)
    {
        var c = Card(); bool adm = u.Name == "admin";
        c.Controls.Add(Theme.Lbl("👤 " + u.Name + (adm ? "  (администратор)" : ""), 11, true, Theme.Text, 20, 10));
        c.Controls.Add(Theme.Lbl($"Создано голосований: {Store.Data.Polls.Count(p => p.Owner == u.Name)}", 9, false, Theme.Muted, 20, 32));
        var pw = Btn("Новый пароль", 20, Theme.Accent, Color.White);
        pw.Click += (_, _) =>
        {
            var s = InputDialog.Ask($"Новый пароль для {u.Name}"); if (s == null) return;
            if (s.Length < 4) { MessageBox.Show("Минимум 4 символа"); return; }
            u.Salt = Store.NewSalt(); u.Hash = Store.Hash(s, u.Salt); Store.Save(); MessageBox.Show("Пароль изменён");
        };
        c.Controls.Add(pw);
        if (!adm)
        {
            var del = Btn("Удалить", 170, Theme.Danger, Color.White);
            del.Click += (_, _) =>
            {
                if (!Confirm($"Удалить пользователя «{u.Name}» и все его голосования?")) return;
                Store.Data.Polls.RemoveAll(p => p.Owner == u.Name); Store.Data.Users.Remove(u); Store.Save(); Fill();
            };
            c.Controls.Add(del);
        }
        return c;
    }
}
