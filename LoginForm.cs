public class LoginForm : Form
{
    bool reg; Label title, err, sw; RoundInput user, pass, pass2; RoundButton go;
    public LoginForm()
    {
        Text = "Голосования — вход"; ClientSize = new Size(440, 560); StartPosition = FormStartPosition.CenterScreen; BackColor = Theme.Bg; Font = Theme.F(10);
        FormBorderStyle = FormBorderStyle.FixedSingle; MaximizeBox = false;
        var card = new RoundPanel { Radius = 24, Left = 30, Top = 30, Width = 380, Height = 500 };
        title = Theme.Lbl("Добро пожаловать", 18, true, Theme.Text, 30, 30);
        card.Controls.Add(Theme.Lbl("🗳  Голосования", 11, true, Theme.Accent, 30, 0 + 8));
        card.Controls.Add(title);
        card.Controls.Add(Theme.Lbl("Логин", 9, false, Theme.Muted, 30, 100)); user = new RoundInput { Left = 30, Top = 122, Width = 320 };
        card.Controls.Add(Theme.Lbl("Пароль", 9, false, Theme.Muted, 30, 180)); pass = new RoundInput(42, false, true) { Left = 30, Top = 202, Width = 320 };
        card.Controls.Add(Theme.Lbl("Повторите пароль", 9, false, Theme.Muted, 30, 260)); pass2 = new RoundInput(42, false, true) { Left = 30, Top = 282, Width = 320 };
        err = Theme.Lbl("", 9, false, Theme.Danger, 30, 335);
        go = new RoundButton { Left = 30, Top = 370, Width = 320, Height = 46 };
        sw = Theme.Lbl("", 10, true, Theme.Accent, 30, 440); sw.Cursor = Cursors.Hand; sw.Click += (_, _) => { reg = !reg; Apply(); };
        go.Click += (_, _) => Submit();
        card.Controls.AddRange(new Control[] { user, pass, pass2, err, go, sw }); Controls.Add(card);
        AcceptButton = go; Apply();
    }
    void Apply()
    {
        title.Text = reg ? "Регистрация" : "Вход"; go.Text = reg ? "Создать аккаунт" : "Войти";
        sw.Text = reg ? "Уже есть аккаунт? Войти" : "Нет аккаунта? Зарегистрироваться";
        pass2.Visible = reg; foreach (Control c in pass2.Parent.Controls) if (c is Label l && l.Text == "Повторите пароль") l.Visible = reg; err.Text = "";
    }
    void Submit()
    {
        if (reg && pass.Value != pass2.Value) { err.Text = "Пароли не совпадают"; return; }
        var r = reg ? Store.Register(user.Value, pass.Value) : Store.Login(user.Value, pass.Value);
        if (r != null) err.Text = r; else Close();
    }
}
