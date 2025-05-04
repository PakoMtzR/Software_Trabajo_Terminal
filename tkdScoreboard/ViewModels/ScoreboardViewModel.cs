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

        // Propiedades expuestas para la vista
        public string TimerDisplay => CurrentMatch.TimerDisplay;
        public string RoundDisplay => CurrentMatch.RoundDisplay;

        // Comandos
        public ICommand ResumeRoundCommand { get; }
        public ICommand PauseRoundCommand { get; }
        public ICommand NextRoundCommand { get; }
        public ICommand ResetMatchCommand { get; }

        public ICommand Add1PointToPlayer1Command { get; }
        public ICommand Add2PointsToPlayer1Command { get; }
        public ICommand Add3PointsToPlayer1Command { get; }
        public ICommand Add4PointsToPlayer1Command { get; }
        public ICommand Add5PointsToPlayer1Command { get; }

        public ICommand Add1PointToPlayer2Command { get; }
        public ICommand Add2PointsToPlayer2Command { get; }
        public ICommand Add3PointsToPlayer2Command { get; }
        public ICommand Add4PointsToPlayer2Command { get; }
        public ICommand Add5PointsToPlayer2Command { get; }



        public ScoreboardViewModel()
        {
            CurrentMatch = new Match();

            // Configuramos los comandos
            ResumeRoundCommand = new RelayCommand(CurrentMatch.ResumeRound, CanResumeRound);
            PauseRoundCommand = new RelayCommand(CurrentMatch.PauseRound, CanPauseRound);
            NextRoundCommand = new RelayCommand(CurrentMatch.NextRound, CanNextRound);
            ResetMatchCommand = new RelayCommand(CurrentMatch.ResetMatch);

            Add1PointToPlayer1Command = new RelayCommand(() => CurrentMatch.Player1.AddPoints(1));
            Add2PointsToPlayer1Command = new RelayCommand(() => CurrentMatch.Player1.AddPoints(2));
            Add3PointsToPlayer1Command = new RelayCommand(() => CurrentMatch.Player1.AddPoints(3));
            Add4PointsToPlayer1Command = new RelayCommand(() => CurrentMatch.Player1.AddPoints(4));
            Add5PointsToPlayer1Command = new RelayCommand(() => CurrentMatch.Player1.AddPoints(5));

            Add1PointToPlayer2Command = new RelayCommand(() => CurrentMatch.Player2.AddPoints(1));
            Add2PointsToPlayer2Command = new RelayCommand(() => CurrentMatch.Player2.AddPoints(2));
            Add3PointsToPlayer2Command = new RelayCommand(() => CurrentMatch.Player2.AddPoints(3));
            Add4PointsToPlayer2Command = new RelayCommand(() => CurrentMatch.Player2.AddPoints(4));
            Add5PointsToPlayer2Command = new RelayCommand(() => CurrentMatch.Player2.AddPoints(5));

            CurrentMatch.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(CurrentMatch.TimerDisplay))
                    OnPropertyChanged(nameof(TimerDisplay));
                if (e.PropertyName == nameof(CurrentMatch.RoundDisplay))
                    OnPropertyChanged(nameof(RoundDisplay));
            };
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


        // Version vieja
        private bool CanStartTimer()
        {
            return !CurrentMatch.IsTimerRunning;
        }
        private bool CanStopTimer()
        {
            return CurrentMatch.IsTimerRunning;
        }
    }
}
