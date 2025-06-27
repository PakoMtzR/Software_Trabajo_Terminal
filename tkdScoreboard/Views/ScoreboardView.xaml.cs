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
using tkdScoreboard.ViewModels;

namespace tkdScoreboard.Views
{
    /// <summary>
    /// Lógica de interacción para ScoreboardView.xaml
    /// </summary>
    public partial class ScoreboardView : UserControl
    {
        public ScoreboardView(ScoreboardViewModel scoreboardViewModel)
        {
            InitializeComponent();
            DataContext = scoreboardViewModel;

            // Enfoque en el teclado para los comandos
            Loaded += (s, e) =>
            {
                Focus();
                Keyboard.Focus(this);
            };

            //this.KeyDown += (s, e) =>
            //{
            //    MessageBox.Show($"Tecla presionada: {e.Key}");
            //};
        }
    }
}
