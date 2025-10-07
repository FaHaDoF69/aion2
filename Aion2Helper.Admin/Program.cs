namespace Aion2Helper.Admin;

static class Program
{
    /// <summary>
    ///  管理端应用程序入口点
    /// </summary>
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new AdminMainForm());
    }    
}