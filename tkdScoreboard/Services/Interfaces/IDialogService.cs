using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tkdScoreboard.ViewModels;

namespace tkdScoreboard.Services.Interfaces
{
    public interface IDialogService
    {
        bool ShowSettingsDialog(SettingsViewModel settingsViewModel);
        bool ShowEditScoreboardDialog(EditScoreboardViewModel editScoreboardViewModel);
    }

}
