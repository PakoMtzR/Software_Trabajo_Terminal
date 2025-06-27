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
    public class SettingsViewModel : BaseViewModel
    {
        public string Player1Name { get; set; }
        public string Player2Name { get; set; }
        public int RoundTime { get; set; }
        public int RestTime { get; set; }
        public int PenaltyLimit { get; set; }

        private readonly Action<bool> _closeAction;

        // public bool? DialogResult { get; private set; }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public SettingsViewModel(string player1Name, string player2Name, int roundTime, int restTime, int penaltyLimit, Action<bool> closeAction)
        {
            Player1Name = player1Name;
            Player2Name = player2Name;
            RoundTime = roundTime;
            RestTime = restTime;
            PenaltyLimit = penaltyLimit;

            SaveCommand = new RelayCommand(Save);
            CancelCommand = new RelayCommand(Cancel);
            _closeAction = closeAction;
        }

        private void Save()
        {
            _closeAction?.Invoke(true);
            // DialogResult = true; // Indica que se guardaron los cambios
            // CloseWindow();
        }

        private void Cancel()
        {
            _closeAction?.Invoke(false);
            // DialogResult = false; // Indica que se cancelaron los cambios
            // CloseWindow();
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
