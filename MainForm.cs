public class MainForm : Form
{
    public bool Logout; bool mine; FlowLayoutPanel list; RoundInput code; RoundButton tabAll, tabMine;
    public MainForm()
    {
        Text = "Голосование"; Size = new Size(920, 700); MinimumSize = new Size(760, 520); StartPosition = FormStartPosition.CenterScreen; BackColor = Theme.Bg; Font = Theme.F(10);
        var head = new Panel { Dock = DockStyle.Top, Height = 64, BackColor = Color.White };
        head.Controls.Add(Theme.Lbl("Голосование", 15, true, Theme.Accent, 20, 16));

        var exit = new RoundButton { Text = "Выйти", Width = 90, Height = 36, Fill = Theme.Line, TextColor = Theme.Text, Top = 14 };
        exit.Click += (_, _) => { Logout = true; Close(); };

        var me = Theme.Lbl("👤 " + Store.Current.Name, 10, true, Theme.Text, 0, 21);

        RoundButton adm = null;
        if (Store.IsAdmin)
        {
            adm = new RoundButton { Text = "⚙ Админ-панель", Width = 150, Height = 36, Fill = Theme.Danger, Top = 14 };
            adm.Click += (_, _) => { new AdminForm().ShowDialog(this); Fill(); };
            head.Controls.Add(adm);
        }
        head.Controls.AddRange(new Control[] { exit, me });

        void Place()   // расставляем кнопки от правого края при каждом изменении размера
        {
            exit.Left = head.ClientSize.Width - exit.Width - 20;
            me.Left = exit.Left - me.PreferredWidth - 20;
            if (adm != null) adm.Left = me.Left - adm.Width - 15;
        }
        head.Resize += (_, _) => Place();
        Place();

        var bar = new Panel { Dock = DockStyle.Top, Height = 70 };
        tabAll = new RoundButton { Text = "Открытые", Left = 20, Top = 14, Width = 120, Height = 40 }; tabMine = new RoundButton { Text = "Мои", Left = 148, Top = 14, Width = 100, Height = 40 };
        tabAll.Click += (_, _) => { mine = false; Fill(); }; tabMine.Click += (_, _) => { mine = true; Fill(); };
        code = new RoundInput(40) { Left = 290, Top = 14, Width = 150 }; code.Box.CharacterCasing = CharacterCasing.Upper; code.Box.MaxLength = 6; code.Box.PlaceholderText = "КОД";
        var find = new RoundButton { Text = "Найти по коду", Left = 450, Top = 14, Width = 140, Height = 40 }; find.Click += (_, _) => FindByCode();
        var create = new RoundButton { Text = "+ Создать", Left = 610, Top = 14, Width = 130, Height = 40, Fill = Color.FromArgb(22, 163, 74) };
        create.Click += (_, _) => { using var f = new CreatePollForm(); if (f.ShowDialog(this) == DialogResult.OK) { mine = true; Fill(); } };
        bar.Controls.AddRange(new Control[] { tabAll, tabMine, code, find, create });
        AcceptButton = find;

        list = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, FlowDirection = FlowDirection.TopDown, WrapContents = false, Padding = new Padding(20, 5, 20, 10) };
        list.SizeChanged += (_, _) => { foreach (Control c in list.Controls) c.Width = Math.Max(300, list.ClientSize.Width - 50); };
        Controls.Add(list); Controls.Add(bar); Controls.Add(head); Fill();
    }
    void FindByCode()
    {
        Store.Load(); var p = Store.Find(code.Value);
        if (p == null) { MessageBox.Show("Голосование с таким кодом не найдено", "Поиск"); return; }
        Open(p);
    }
    void Open(Poll p)
    {
        if (p.PassHash != "" && p.Owner != Store.Current.Name && !Store.IsAdmin)
        {
            var pw = InputDialog.Ask("Пароль голосования"); if (pw == null) return;
            if (!Store.CheckPass(p, pw)) { MessageBox.Show("Неверный пароль", "Доступ закрыт"); return; }
        }
        new VoteForm(p).ShowDialog(this); Fill();
    }
    void Fill()
    {
        Store.Load(); tabAll.Fill = mine ? Theme.Line : Theme.Accent; tabAll.TextColor = mine ? Theme.Text : Color.White;
        tabMine.Fill = mine ? Theme.Accent : Theme.Line; tabMine.TextColor = mine ? Color.White : Theme.Text; tabAll.Invalidate(); tabMine.Invalidate();
        list.SuspendLayout(); list.Controls.Clear();
        var items = Store.Data.Polls.Where(p => mine ? p.Owner == Store.Current.Name : !p.IsPrivate).Reverse().ToList();
        if (items.Count == 0) list.Controls.Add(Theme.Lbl(mine ? "Вы пока ничего не создавали" : "Открытых голосований пока нет", 11, false, Theme.Muted, 0, 0));
        foreach (var p in items)
        {
            var c = new RoundPanel { Height = 92, Width = Math.Max(300, list.ClientSize.Width - 50), Margin = new Padding(0, 0, 0, 12), Cursor = Cursors.Hand };
            var t = Theme.Lbl(p.Title, 12, true, Theme.Text, 20, 14);
            var meta = Theme.Lbl($"Автор: {p.Owner}  •  Голосов: {p.Total}  •  Код: {p.Code}" + (p.IsPrivate ? "  •  🔒 закрытое" : "") + (p.PassHash != "" ? "  •  🔑 пароль" : "") + (p.Closed ? "  •  завершено" : ""), 9, false, Theme.Muted, 20, 50);
            c.Controls.AddRange(new Control[] { t, meta });
            var cp = new RoundButton { Text = "Копировать код", Width = 140, Height = 32, Radius = 10, Fill = Theme.Line, TextColor = Theme.Text, Font = Theme.F(9, true), Top = 14, Anchor = AnchorStyles.Top | AnchorStyles.Right };
            cp.Left = c.Width - 160; var cd = p.Code; cp.Click += (_, _) => Theme.Copy(cp, cd);
            c.Controls.Add(cp);
            var pp = p; c.Click += (_, _) => Open(pp); t.Click += (_, _) => Open(pp); meta.Click += (_, _) => Open(pp);
            list.Controls.Add(c);
        }
        list.ResumeLayout();
    }
}
