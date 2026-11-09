public class CreatePollForm : Form
{
    RoundInput title, desc, opts, pass; CheckBox priv, multi; Label err;
    public CreatePollForm()
    {
        Text = "Новое голосование"; ClientSize = new Size(480, 640); StartPosition = FormStartPosition.CenterParent; BackColor = Theme.Bg; Font = Theme.F(10);
        FormBorderStyle = FormBorderStyle.FixedDialog; MaximizeBox = false; MinimizeBox = false;
        var c = new RoundPanel { Radius = 22, Left = 15, Top = 15, Width = 450, Height = 610 };
        c.Controls.Add(Theme.Lbl("Создать голосование", 15, true, Theme.Text, 25, 18));
        c.Controls.Add(Theme.Lbl("Вопрос", 9, false, Theme.Muted, 25, 65)); title = new RoundInput { Left = 25, Top = 87, Width = 400 };
        c.Controls.Add(Theme.Lbl("Описание (необязательно)", 9, false, Theme.Muted, 25, 140)); desc = new RoundInput(42) { Left = 25, Top = 162, Width = 400 };
        c.Controls.Add(Theme.Lbl("Варианты ответа — каждый с новой строки (минимум 2)", 9, false, Theme.Muted, 25, 215)); opts = new RoundInput(120, true) { Left = 25, Top = 237, Width = 400 };
        priv = new CheckBox { Text = "Закрытое (не показывать в списке, только по коду)", Left = 25, Top = 372, Width = 400, BackColor = Color.White, Font = Theme.F(10) };
        multi = new CheckBox { Text = "Можно выбрать несколько вариантов", Left = 25, Top = 402, Width = 400, BackColor = Color.White, Font = Theme.F(10) };
        c.Controls.Add(Theme.Lbl("Пароль (необязательно)", 9, false, Theme.Muted, 25, 440)); pass = new RoundInput(42, false, true) { Left = 25, Top = 462, Width = 400 };
        err = Theme.Lbl("", 9, false, Theme.Danger, 25, 512);
        var ok = new RoundButton { Text = "Создать", Left = 25, Top = 540, Width = 400, Height = 46 }; ok.Click += (_, _) => Create();
        c.Controls.AddRange(new Control[] { title, desc, opts, priv, multi, pass, err, ok }); Controls.Add(c);
    }
    void Create()
    {
        var o = opts.Value.Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).Where(s => s != "").Distinct().ToList();
        if (title.Value.Trim() == "") { err.Text = "Введите вопрос"; return; }
        if (o.Count < 2) { err.Text = "Нужно минимум 2 разных варианта"; return; }
        var p = new Poll { Code = Store.NewCode(), Title = title.Value.Trim(), Description = desc.Value.Trim(), Owner = Store.Current.Name, IsPrivate = priv.Checked, Multiple = multi.Checked, Options = o.Select(x => new Option { Text = x }).ToList() };
        if (pass.Value != "") { p.Salt = Store.NewSalt(); p.PassHash = Store.Hash(pass.Value, p.Salt); }
        Store.Load(); Store.Data.Polls.Add(p); Store.Save();
        string note = "";
        try { Clipboard.SetText(p.Code); note = "\n(скопирован в буфер обмена)"; } catch { }
        MessageBox.Show($"Голосование создано!\nКод: {p.Code}{note}", "Готово");
        DialogResult = DialogResult.OK;
    }
}
