using System;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WindowsCleanerUtility.Services;
using WindowsCleanerUtility.Settings;

namespace WindowsCleanerUtility
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            var host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    var userSettings = UserSettings.Load();
                    services.AddCleanerServices(userSettings);
                })
                .Build();

            var mainForm = ActivatorUtilities.CreateInstance<MainForm>(host.Services);
            
            Application.Run(mainForm);
        }
    }
}