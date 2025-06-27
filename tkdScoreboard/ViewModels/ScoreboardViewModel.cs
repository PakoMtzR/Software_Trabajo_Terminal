using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using tkdScoreboard.Commands;
using tkdScoreboard.Models;
using tkdScoreboard.Services;
using tkdScoreboard.Services.Interfaces;

namespace tkdScoreboard.ViewModels
{
    public class ScoreboardViewModel : BaseViewModel
    {
        public Match CurrentMatch { get; }
        private SerialConnection _serialConnection;
        public string ReceivedData { get; set; }
        private readonly IDialogService _dialogService;

        // Commandos para abrir ventanas flotantes
        public ICommand OpenSettingsCommand { get; }

        // Propiedades expuestas para la vista
        public string TimerDisplay => CurrentMatch.TimerDisplay;
        public string RoundDisplay => CurrentMatch.RoundDisplay;

        // Comandos del timer
        public ICommand ResumeRoundCommand { get; }
        public ICommand PauseRoundCommand { get; }
        public ICommand NextRoundCommand { get; }
        public ICommand ResetMatchCommand { get; }
        public ICommand FinishRoundCommand { get; }


        // Comandos atajos para el jugador 1 (Azul)
        // ----------------------------------------------------------
        // Comandos para agregar puntos al jugador 1
        public ICommand Add1PointToPlayer1Command { get; }
        public ICommand Add2PointsToPlayer1Command { get; }
        public ICommand Add3PointsToPlayer1Command { get; }
        public ICommand Add4PointsToPlayer1Command { get; }
        public ICommand Add5PointsToPlayer1Command { get; }
        // Comandos para quitar puntos al jugador 1
        public ICommand Remove1PointToPlayer1Command { get; }
        public ICommand Remove2PointsToPlayer1Command { get; }
        public ICommand Remove3PointsToPlayer1Command { get; }
        public ICommand Remove4PointsToPlayer1Command { get; }
        public ICommand Remove5PointsToPlayer1Command { get; }
        // Comandos para agregar/quitar penalizaciones al jugador 1
        public ICommand AddPenaltyToPlayer1Command { get; }
        public ICommand RemovePenaltyToPlayer1Command { get; }


        // Comandos atajos para el jugador 2 (Rojo)
        // ----------------------------------------------------------
        public ICommand Add1PointToPlayer2Command { get; }
        public ICommand Add2PointsToPlayer2Command { get; }
        public ICommand Add3PointsToPlayer2Command { get; }
        public ICommand Add4PointsToPlayer2Command { get; }
        public ICommand Add5PointsToPlayer2Command { get; }
        // Comandos para quitar puntos al jugador 2
        public ICommand Remove1PointToPlayer2Command { get; }
        public ICommand Remove2PointsToPlayer2Command { get; }
        public ICommand Remove3PointsToPlayer2Command { get; }
        public ICommand Remove4PointsToPlayer2Command { get; }
        public ICommand Remove5PointsToPlayer2Command { get; }
        // Comandos para agregar/quitar penalizaciones al jugador 2
        public ICommand AddPenaltyToPlayer2Command { get; }
        public ICommand RemovePenaltyToPlayer2Command { get; }

        // Comandos para la conexión serial
        public ICommand ConnectSerialCommand { get; }
        public ICommand DisconnectSerialCommand { get; }

        // Constructor
        public ScoreboardViewModel(IDialogService dialogService)
        {
            // Inicializamos el modelo
            CurrentMatch = new Match();
            _dialogService = dialogService;
            _serialConnection = new SerialConnection("COM3", 115200);
            _serialConnection.DataReceived += OnDataReceived;

            // Configuramos los comandos para abrir ventanas flotantes
            OpenSettingsCommand = new RelayCommand(OpenSettings);

            // Configuramos los comandos del timer
            ResumeRoundCommand = new RelayCommand(CurrentMatch.ResumeRound, CanResumeRound);
            PauseRoundCommand = new RelayCommand(CurrentMatch.PauseRound, CanPauseRound);
            NextRoundCommand = new RelayCommand(CurrentMatch.NextRound, CanNextRound);
            ResetMatchCommand = new RelayCommand(CurrentMatch.ResetMatch);
            FinishRoundCommand = new RelayCommand(CurrentMatch.FinishRound);

            // Comandos para la conexión serial
            ConnectSerialCommand = new RelayCommand(_serialConnection.Open, () => !_serialConnection.IsOpen);
            DisconnectSerialCommand = new RelayCommand(_serialConnection.Close, () => _serialConnection.IsOpen);

            // Comandos para el jugador 1
            Add1PointToPlayer1Command = new RelayCommand(() => CurrentMatch.Player1.AddPoints(1));
            Add2PointsToPlayer1Command = new RelayCommand(() => CurrentMatch.Player1.AddPoints(2));
            Add3PointsToPlayer1Command = new RelayCommand(() => CurrentMatch.Player1.AddPoints(3));
            Add4PointsToPlayer1Command = new RelayCommand(() => CurrentMatch.Player1.AddPoints(4));
            Add5PointsToPlayer1Command = new RelayCommand(() => CurrentMatch.Player1.AddPoints(5));
            Remove1PointToPlayer1Command = new RelayCommand(() => CurrentMatch.Player1.RemovePoints(1));
            Remove2PointsToPlayer1Command = new RelayCommand(() => CurrentMatch.Player1.RemovePoints(2));
            Remove3PointsToPlayer1Command = new RelayCommand(() => CurrentMatch.Player1.RemovePoints(3));
            Remove4PointsToPlayer1Command = new RelayCommand(() => CurrentMatch.Player1.RemovePoints(4));
            Remove5PointsToPlayer1Command = new RelayCommand(() => CurrentMatch.Player1.RemovePoints(5));
            AddPenaltyToPlayer1Command = new RelayCommand(() => CurrentMatch.PenalizePlayer1());
            RemovePenaltyToPlayer1Command = new RelayCommand(() => CurrentMatch.DespenalizePlayer1());

            // Comandos para el jugador 2
            Add1PointToPlayer2Command = new RelayCommand(() => CurrentMatch.Player2.AddPoints(1));
            Add2PointsToPlayer2Command = new RelayCommand(() => CurrentMatch.Player2.AddPoints(2));
            Add3PointsToPlayer2Command = new RelayCommand(() => CurrentMatch.Player2.AddPoints(3));
            Add4PointsToPlayer2Command = new RelayCommand(() => CurrentMatch.Player2.AddPoints(4));
            Add5PointsToPlayer2Command = new RelayCommand(() => CurrentMatch.Player2.AddPoints(5));
            Remove1PointToPlayer2Command = new RelayCommand(() => CurrentMatch.Player2.RemovePoints(1));
            Remove2PointsToPlayer2Command = new RelayCommand(() => CurrentMatch.Player2.RemovePoints(2));
            Remove3PointsToPlayer2Command = new RelayCommand(() => CurrentMatch.Player2.RemovePoints(3));
            Remove4PointsToPlayer2Command = new RelayCommand(() => CurrentMatch.Player2.RemovePoints(4));
            Remove5PointsToPlayer2Command = new RelayCommand(() => CurrentMatch.Player2.RemovePoints(5));
            AddPenaltyToPlayer2Command = new RelayCommand(() => CurrentMatch.PenalizePlayer2());
            RemovePenaltyToPlayer2Command = new RelayCommand(() => CurrentMatch.DespenalizePlayer2());


            CurrentMatch.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(CurrentMatch.TimerDisplay))
                    OnPropertyChanged(nameof(TimerDisplay));
                if (e.PropertyName == nameof(CurrentMatch.RoundDisplay))
                    OnPropertyChanged(nameof(RoundDisplay));
            };
        }

        private void OpenSettings()
        {
            var settingsViewModel = new SettingsViewModel(
            CurrentMatch.Player1.Name,
            CurrentMatch.Player2.Name,
            CurrentMatch.RoundTime,
            CurrentMatch.RestTime,
            CurrentMatch.PenaltiesLimit,
            null); // El callback lo pone DialogService

            if (_dialogService.ShowSettingsDialog(settingsViewModel))
            {
                // Aquí los valores ya están actualizados por DialogService
                CurrentMatch.Player1.Name = settingsViewModel.Player1Name;
                CurrentMatch.Player2.Name = settingsViewModel.Player2Name;
                CurrentMatch.RoundTime = settingsViewModel.RoundTime;
                CurrentMatch.RestTime = settingsViewModel.RestTime;
                CurrentMatch.PenaltiesLimit = settingsViewModel.PenaltyLimit;
            }
        }

        private void OnDataReceived(string data)
        {
            if (CurrentMatch.MatchState != Match.MatchStateEnum.Combate) 
                return;

            // Actualiza la propiedad con los datos recibidos
            ReceivedData = data;
            if (int.TryParse(data.Trim(), out int dataValue)) 
            { 
                if (dataValue <= 5) CurrentMatch.Player1.AddPoints(dataValue);
                else CurrentMatch.Player2.AddPoints(dataValue - 5); // Asumiendo que los valores mayores a 5 son para el jugador 2
            }
            else Console.WriteLine("Error al procesar los datos recibidos.");
            OnPropertyChanged(nameof(ReceivedData));
        }

        private bool CanResumeRound()
        {
            return CurrentMatch.MatchState == Match.MatchStateEnum.Pausa;
        }
        private bool CanPauseRound()
        {
            return CurrentMatch.MatchState == Match.MatchStateEnum.Combate;
        }
        private bool CanNextRound()
        {
            return CurrentMatch.MatchState == Match.MatchStateEnum.Descanso;
        }

        ~ScoreboardViewModel()
        {
            _serialConnection.Close();
        }
    }
}
