using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using tkdScoreboard.Commands;

namespace tkdScoreboard.ViewModels
{
    public class EditScoreboardViewModel : BaseViewModel
    {
        public int Player1Points { get; set; }
        public int Player1Penalties { get; set; }
        public int Player2Points { get; set; }
        public int Player2Penalties { get; set; }
        public int CurrentTime { get; set; }

        private readonly Action<bool> _closeAction;

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public EditScoreboardViewModel(int player1Points, int player1Penalties, int player2Points, int player2Penalties, int currentTime, Action<bool> closeAction)
        {
            Player1Points = player1Points;
            Player1Penalties = player1Penalties;
            Player2Points = player2Points;
            Player2Penalties = player2Penalties;
            CurrentTime = currentTime;
            SaveCommand = new RelayCommand(Save);
            CancelCommand = new RelayCommand(Cancel);
            _closeAction = closeAction;
        }

        private void Save()
        {
            _closeAction?.Invoke(true);
        }

        private void Cancel()
        {
            _closeAction?.Invoke(false);
        }

        private void CloseWindow()
        {
            // Cierra la ventana asociada al ViewModel
            if (System.Windows.Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.DataContext == this) is Window window)
            {
                window.Close();
            }
        }

    }
}
