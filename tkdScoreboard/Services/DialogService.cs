using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tkdScoreboard.Services.Interfaces;
using tkdScoreboard.ViewModels;
using tkdScoreboard.Views;

namespace tkdScoreboard.Services
{
    public class DialogService : IDialogService
    {
        public bool ShowSettingsDialog(SettingsViewModel settingsViewModel)
        {
            if (settingsViewModel == null) return false;

            var settingsWindow = new SettingsWindow();
            bool dialogResult = false;

            // Creamos nuevo ViewModel con los mismos parámetros más el callback
            var vm = new SettingsViewModel(
                settingsViewModel.Player1Name,
                settingsViewModel.Player2Name,
                settingsViewModel.RoundTime,
                settingsViewModel.RestTime,
                settingsViewModel.PenaltyLimit,
                result => {
                    dialogResult = result;
                    settingsWindow.Close();
                });

            settingsWindow.DataContext = vm;
            settingsWindow.ShowDialog();

            // Copiamos los valores de vuelta si se aceptó
            if (dialogResult)
            {
                settingsViewModel.Player1Name = vm.Player1Name;
                settingsViewModel.Player2Name = vm.Player2Name;
                settingsViewModel.RoundTime = vm.RoundTime;
                settingsViewModel.RestTime = vm.RestTime;
                settingsViewModel.PenaltyLimit = vm.PenaltyLimit;
            }

            return dialogResult;
        }
    }

}
