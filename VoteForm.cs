public class VoteForm : Form
{
    Poll p; Panel root; List<ButtonBase> checks = new();
    bool IsOwner => p.Owner == Store.Current.Name || Store.IsAdmin;
    public VoteForm(Poll poll)
    {
        p = poll; Text = p.Title; ClientSize = new Size(560, 680); StartPosition = FormStartPosition.CenterParent; BackColor = Theme.Bg; Font = Theme.F(10);
        FormBorderStyle = FormBorderStyle.FixedDialog; MaximizeBox = false; MinimizeBox = false;
        root = new Panel { Dock = DockStyle.Fill, AutoScroll = true }; Controls.Add(root); Build();
    }
    void Build()
    {
        Store.Load(); p = Store.Find(p.Code) ?? p; root.Controls.Clear(); checks.Clear();
        bool voted = p.Voted.Contains(Store.Current.Name), canVote = !p.Closed && !voted;
        int y = 15;
        var card = new RoundPanel { Radius = 22, Left = 15, Top = y, Width = 510 };
        card.Controls.Add(Theme.Lbl(p.Title, 15, true, Theme.Text, 22, 18, 466));
        int cy = 62;
        if (p.Description != "") { var d = Theme.Lbl(p.Description, 10, false, Theme.Muted, 22, cy, 466); d.Height = 40; card.Controls.Add(d); cy += 46; }
        card.Controls.Add(Theme.Lbl($"Автор: {p.Owner}" + (p.Closed ? "  •  завершено" : ""), 9, false, Theme.Muted, 22, cy)); cy += 26;
        card.Controls.Add(Theme.Lbl("Код: " + p.Code, 11, true, Theme.Accent, 22, cy + 4));
        var cp = new RoundButton { Text = "Копировать код", Left = 340, Top = cy, Width = 148, Height = 32, Radius = 10, Fill = Theme.Line, TextColor = Theme.Text, Font = Theme.F(9, true) };
        var codeText = p.Code; cp.Click += (_, _) => Theme.Copy(cp, codeText); card.Controls.Add(cp); cy += 44;

        if (canVote)
        {
            card.Controls.Add(Theme.Lbl(p.Multiple ? "Выберите один или несколько вариантов:" : "Выберите вариант:", 10, true, Theme.Text, 22, cy)); cy += 32;
            var box = new Panel { Left = 22, Top = cy, Width = 466, Height = p.Options.Count * 36, BackColor = Color.White };
            for (int i = 0; i < p.Options.Count; i++)
            {
                ButtonBase b = p.Multiple ? new CheckBox() : new RadioButton();
                b.Text = p.Options[i].Text; b.Tag = i; b.Left = 0; b.Top = i * 36; b.Width = 466; b.Height = 32; b.Font = Theme.F(11); b.Cursor = Cursors.Hand; box.Controls.Add(b); checks.Add(b);
            }
            card.Controls.Add(box); cy += box.Height + 10;
            var go = new RoundButton { Text = "Проголосовать", Left = 22, Top = cy, Width = 466, Height = 46 }; go.Click += (_, _) => Vote(); card.Controls.Add(go); cy += 56;
        }
        if (!canVote || IsOwner)
        {
            card.Controls.Add(Theme.Lbl(voted ? "Ваш голос учтён. Результаты:" : "Результаты:", 10, true, Theme.Text, 22, cy)); cy += 34;
            foreach (var o in p.Options)
            {
                double pct = p.Total == 0 ? 0 : 100.0 * o.Votes / p.Total;
                card.Controls.Add(Theme.Lbl($"{o.Text}", 10, false, Theme.Text, 22, cy)); card.Controls.Add(new Label { Text = $"{o.Votes} ({pct:0}%)", Font = Theme.F(10, true), ForeColor = Theme.Accent, BackColor = Color.Transparent, AutoSize = false, TextAlign = ContentAlignment.TopRight, Left = 340, Top = cy, Width = 148, Height = 22 });
                cy += 26; var track = new RoundPanel { Radius = 7, Fill = Theme.Line, Left = 22, Top = cy, Width = 466, Height = 14 }; card.Controls.Add(track);
                if (o.Votes > 0) track.Controls.Add(new RoundPanel { Radius = 7, Fill = Theme.Accent, Left = 0, Top = 0, Height = 14, Width = Math.Max(14, (int)(466 * pct / 100)) });
                cy += 32;
            }
            card.Controls.Add(Theme.Lbl($"Всего голосов: {p.Total}", 9, false, Theme.Muted, 22, cy)); cy += 30;
        }
        if (IsOwner)
        {
            var close = new RoundButton { Text = p.Closed ? "Возобновить" : "Завершить", Left = 22, Top = cy, Width = 227, Height = 40, Fill = Theme.Muted };
            close.Click += (_, _) => { p.Closed = !p.Closed; Store.Save(); Build(); };
            var del = new RoundButton { Text = "Удалить", Left = 261, Top = cy, Width = 227, Height = 40, Fill = Theme.Danger };
            del.Click += (_, _) => { if (MessageBox.Show("Удалить голосование?", "Подтверждение", MessageBoxButtons.YesNo) == DialogResult.Yes) { Store.Data.Polls.RemoveAll(x => x.Code == p.Code); Store.Save(); Close(); } };
            card.Controls.AddRange(new Control[] { close, del }); cy += 52;
        }
        card.Height = cy + 10; root.Controls.Add(card);
    }
    void Vote()
    {
        var sel = checks.Where(c => c is CheckBox cb ? cb.Checked : ((RadioButton)c).Checked).Select(c => (int)c.Tag).ToList();
        if (sel.Count == 0) { MessageBox.Show("Выберите вариант"); return; }
        Store.Load(); var cur = Store.Find(p.Code);
        if (cur == null || cur.Closed || cur.Voted.Contains(Store.Current.Name)) { Build(); return; }
        foreach (var i in sel) cur.Options[i].Votes++;
        cur.Voted.Add(Store.Current.Name); Store.Save(); Build();
    }
}
