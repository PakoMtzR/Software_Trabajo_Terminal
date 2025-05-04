using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace tkdScoreboard.Models
{
    public class Match : INotifyPropertyChanged
    {
        // Enumeración para el estado del combate
        public enum MatchStateEnum
        {
            Pausa,
            Combate,
            Descanso
        }

        //  Evento para notificar cambios en las propiedades
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Atributos privados
        private Timer _timer;
        private int _round = 1;
        private MatchStateEnum _matchState = MatchStateEnum.Pausa;
        private int _roundTime = 5; // 2 minutos por defecto
        private int _restTime = 3;  // segundos de descanso por defecto

        public Player Player1 { get; }
        public Player Player2 { get; }

        // Propiedades públicas
        public int Round
        {
            get => _round;
            set
            {
                if (_round != value)
                {
                    _round = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(RoundDisplay));    // Notificar cambio en la visualización
                }
            }
        }

        public int RoundTime
        {
            get => _roundTime;
            set
            {
                if (_roundTime != value)
                {
                    _roundTime = value;
                    OnPropertyChanged();
                }
            }
        }

        public int RestTime
        {
            get => _restTime;
            set
            {
                if (_restTime != value)
                {
                    _restTime = value;
                    OnPropertyChanged();
                }
            }
        }

        public MatchStateEnum MatchState
        {
            get => _matchState;
            set
            {
                if (_matchState != value)
                {
                    _matchState = value;
                    OnPropertyChanged();
                }
            }
        }

        // Propiedades de visualización
        public string RoundDisplay => $"R{Round}";
        public string TimerDisplay =>
            $"{_timer?.RemainingTime / 60:00}:{_timer?.RemainingTime % 60:00}";

        // Propiedad para verificar si el temporizador está en ejecución
        public bool IsTimerRunning => _timer?.IsRunning ?? false;

        // Constructor
        public Match()
        {
            Player1 = new Player { Name = "CHUNG" };
            Player2 = new Player { Name = "HONG" };
            _timer = new Timer(RoundTime);
            _timer.TimeElapsed += (sender, e) =>
            {
                OnPropertyChanged(nameof(TimerDisplay));
                if (_timer.RemainingTime <= 0)
                {
                    if (MatchState == MatchStateEnum.Combate)
                    {
                        SelectRoundWinner();
                        if (Round < 3 && ( Player1.WonRounds < 1 || Player2.WonRounds < 1) )
                        {
                            StartRest();
                        }
                        else
                        {
                            PauseRound();
                            // Aquí podemos manejar el final del combate
                            System.Windows.MessageBox.Show("Fin del combate");
                        }
                    }
                    else
                    {
                        NextRound();
                    }
                }
            };
        }

        public void SelectRoundWinner()
        {
            if (Player1.Points > Player2.Points)
            {
                Player1.WonRounds++;
            }
            else if (Player2.Points > Player1.Points)
            {
                Player2.WonRounds++;
            }
            else
            {
                // Empate, no se incrementa el contador de rondas ganadas
            }
        }

        public void PenalizePlayer(Player player1, Player player2)
        {
            player1.AddPenalty();
            player2.AddPoints(1);
        }

        public void ResetRound()
        {
            _timer.Reset(RoundTime);
            Player1.ResetScore();
            Player2.ResetScore();
            PauseRound();
            OnPropertyChanged(nameof(TimerDisplay));
        }
        public void PauseRound()
        {
            _timer.Pause();
            MatchState = MatchStateEnum.Pausa;
            // OnPropertyChanged(nameof(IsTimerRunning));
        }
        public void ResumeRound()
        {
            _timer.Start();
            MatchState = MatchStateEnum.Combate;
            // OnPropertyChanged(nameof(IsTimerRunning));
        }
        public void NextRound()
        {
            if (Round < 3)
            {
                Round++;
                ResetRound();
            }
            else
            {
                // Aquí podemos manejar el final del combate
                System.Windows.MessageBox.Show("Fin del combate");
            }
        }
        public void ResetMatch()
        {
            Round = 1;
            Player1.Reset();
            Player2.Reset();
            ResetRound();
        }
        public void StartRest()
        {
            _timer.Reset(RestTime);
            MatchState = MatchStateEnum.Descanso;
            _timer.Start();
        }
    }
}
