ApplicationConfiguration.Initialize();
while (true)
{
    Store.Current = null;
    Application.Run(new LoginForm());
    if (Store.Current == null) break;      // окно входа закрыли крестиком
    var main = new MainForm();
    Application.Run(main);
    if (!main.Logout) break;               // выход из приложения
}
