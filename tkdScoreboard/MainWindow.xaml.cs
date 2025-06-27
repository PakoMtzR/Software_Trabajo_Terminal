using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using tkdScoreboard.Services;
using tkdScoreboard.ViewModels;
using tkdScoreboard.Views;

namespace tkdScoreboard
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // Crear instancia del servicio de diálogos
            var dialogService = new DialogService();

            // Crear instancia del ViewModel y pasar el servicio de diálogos
            var scoreboardViewModel = new ScoreboardViewModel(dialogService);

            // Crear instancia del View y pasar el ViewModel
            var scoreboardView = new ScoreboardView(scoreboardViewModel);

            // Mostrar el View en el MainWindow
            Content = scoreboardView;
        }
    }
}
