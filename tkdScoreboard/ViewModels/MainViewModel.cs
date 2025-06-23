using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using tkdScoreboard.Commands;

namespace tkdScoreboard.ViewModels
{
    internal class MainViewModel : BaseViewModel
    {
        private BaseViewModel _currentViewModel;

        public BaseViewModel CurrentViewModel
        {
            get => _currentViewModel;
            set
            {
                if (_currentViewModel != value)
                {
                    CurrentViewModel = value;
                    OnPropertyChanged();
                }
            }
        }
        
        // Comandos para la navegacion
        public ICommand ShowScoreboardViewCommand { get; }
        public ICommand ShowSerialConnectionViewCommand { get; }
        public ICommand ShowSettingsViewCommand { get; }
        public ICommand ShowCamaraViewCommand { get; }
        public ICommand ShowAboutViewCommand { get; }

        public MainViewModel()
        {
            // Inicializar los comandos
            ShowScoreboardViewCommand = new RelayCommand(() => CurrentViewModel = new ScoreboardViewModel());
            //ShowSerialConnectionViewCommand = new RelayCommand(() => CurrentViewModel = new SerialConnectionViewModel());
            //ShowSettingsViewCommand = new RelayCommand(() => CurrentViewModel = new SettingsViewModel());
            //ShowCamaraViewCommand = new RelayCommand(() => CurrentViewModel = new CamaraViewModel());

            // Inicializar el modelo de vista actual
            CurrentViewModel = new ScoreboardViewModel();
        }
    }
}
