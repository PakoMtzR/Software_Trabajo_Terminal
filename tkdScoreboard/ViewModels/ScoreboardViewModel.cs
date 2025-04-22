using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Input;
using tkdScoreboard.Commands;
using tkdScoreboard.Models;

namespace tkdScoreboard.ViewModels
{
    internal class ScoreboardViewModel : BaseViewModel
    {
        public Match CurrentMatch { get; }

        public ICommand saludarCommand { get; }

        public ScoreboardViewModel() 
        { 
            // CurrentMatch = new Match();

            saludarCommand = new RelayCommand(Saludar);
        }

        private void Saludar()
        {
            Console.WriteLine("hola");
            System.Windows.MessageBox.Show("Hola");
        }
    }
}
