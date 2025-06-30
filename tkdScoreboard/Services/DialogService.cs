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

        public bool ShowEditScoreboardDialog(EditScoreboardViewModel editScoreViewModel)
        {
            if (editScoreViewModel == null) return false;

            var editScoreboardWindow = new EditScoreboardWindow();
            bool dialogResult = false;

            // Creamos nuevo ViewModel con los mismos parámetros más el callback
            var vm = new EditScoreboardViewModel(
                editScoreViewModel.Player1Points,
                editScoreViewModel.Player1Penalties,
                editScoreViewModel.Player2Points,
                editScoreViewModel.Player2Penalties,
                editScoreViewModel.CurrentTime,
                result => {
                    dialogResult = result;
                    editScoreboardWindow.Close();
                });

            editScoreboardWindow.DataContext = vm;
            editScoreboardWindow.ShowDialog();

            // Copiamos los valores de vuelta si se aceptó
            if (dialogResult)
            {
                editScoreViewModel.Player1Points = vm.Player1Points;
                editScoreViewModel.Player1Penalties = vm.Player1Penalties;
                editScoreViewModel.Player2Points = vm.Player2Points;
                editScoreViewModel.Player2Penalties = vm.Player2Penalties;
                editScoreViewModel.CurrentTime = vm.CurrentTime;
            }

            return dialogResult;
        }
    }

}
