using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

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
        private int _penaltiesLimit = 5;

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

        public int PenaltiesLimit
        {
            get => _penaltiesLimit;
            set
            {
                if(_penaltiesLimit != value)
                {
                    _penaltiesLimit = value;
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
                    _timer.Stop();
                    if (MatchState == MatchStateEnum.Combate)
                    {
                        SelectRoundWinner();
                        if (Player1.WonRounds >= 2 || Player2.WonRounds >= 2) 
                        {
                            PauseRound();
                            // Aquí podemos manejar el final del combate
                            System.Windows.MessageBox.Show("Fin del combate");
                        }
                        else
                        {
                            StartRest();
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
                return;
            }
            if (Player2.Points > Player1.Points)
            {
                Player2.WonRounds++;
                return;
            }
            if (Player1.Penalties > Player2.Penalties)
            {
                Player2.WonRounds++;
                return;
            }
            if (Player2.Penalties > Player1.Penalties)
            {
                Player1.WonRounds++;
                return;
            }

            // Empate total, pedir decisión al usuario
            DialogResult result = System.Windows.Forms.MessageBox.Show(
                    "Azul - Yes]\nRojo - No]",
                    "Selecciona Ganador del Round",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

            // Procesar la respuesta
            if (result == System.Windows.Forms.DialogResult.Yes)
            {
                System.Windows.Forms.MessageBox.Show("Gana Azul por decisión");
                Player1.WonRounds++;
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("Gana Rojo por decisión");
                Player2.WonRounds++;
            }
        }

        public void PenalizePlayer1()
        {
            Player1.AddPenalty();
            Player2.AddPoints(1);
        }
        public void DespenalizePlayer1()
        {
            Player1.RemovePenalty();
            Player2.RemovePoints(1);
        }

        public void PenalizePlayer2()
        {
            Player2.AddPenalty();
            Player1.AddPoints(1);
        }
        public void DespenalizePlayer2()
        {
            Player2.RemovePenalty();
            Player1.RemovePoints(1);
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
        public void FinishRound()
        {
            if (MatchState != MatchStateEnum.Descanso)
                _timer.RemainingTime = 0;
        }
        public void NextRound()
        {
            Round++;
            ResetRound();
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
