using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace tkdScoreboard
{
    /// <summary>
    /// Lógica de interacción para App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // Configurar el manejador global para la tecla ESC
            EventManager.RegisterClassHandler(typeof(Window),
                Window.KeyDownEvent,
                new KeyEventHandler(HandleKeyEvents), true);

            base.OnStartup(e);

            MainWindow = new MainWindow();
            MainWindow.Show();
        }

        private void HandleKeyEvents(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape && sender is Window window)
            {
                window.WindowState = WindowState.Normal;
                window.WindowStyle = WindowStyle.SingleBorderWindow;
                e.Handled = true;
            }
        }
    }
}
