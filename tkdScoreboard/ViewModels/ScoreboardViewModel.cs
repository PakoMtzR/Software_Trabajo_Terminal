using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using tkdScoreboard.Commands;
using tkdScoreboard.Models;
using tkdScoreboard.Services;

namespace tkdScoreboard.ViewModels
{
    internal class ScoreboardViewModel : BaseViewModel
    {
        public Match CurrentMatch { get; }
        private SerialConnection _serialConnection;
        public string ReceivedData { get; set; }

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


        // Constructor
        public ScoreboardViewModel()
        {
            // Inicializamos el modelo
            CurrentMatch = new Match();
            _serialConnection = new SerialConnection("COM3", 115200);
            _serialConnection.DataReceived += OnDataReceived;

            try
            {
                _serialConnection.Open();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al abrir el puerto serial: {ex.Message}");
            }

            // Configuramos los comandos del timer
            ResumeRoundCommand = new RelayCommand(CurrentMatch.ResumeRound, CanResumeRound);
            PauseRoundCommand = new RelayCommand(CurrentMatch.PauseRound, CanPauseRound);
            NextRoundCommand = new RelayCommand(CurrentMatch.NextRound, CanNextRound);
            ResetMatchCommand = new RelayCommand(CurrentMatch.ResetMatch);
            FinishRoundCommand = new RelayCommand(CurrentMatch.FinishRound);

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

        private void OnDataReceived(string data)
        {
            // Actualiza la propiedad con los datos recibidos
            ReceivedData = data;
            try
            {
                int dataValue = int.Parse(data.Trim());
                if (dataValue <= 5)
                {
                    CurrentMatch.Player1.AddPoints(dataValue);
                }
                else
                {
                    CurrentMatch.Player2.AddPoints(dataValue - 5); // Asumiendo que los valores mayores a 5 son para el jugador 2
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al procesar los datos recibidos: {ex.Message}");
            }
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
