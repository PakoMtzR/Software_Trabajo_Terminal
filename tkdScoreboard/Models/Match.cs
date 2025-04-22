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
        private Timer _timer;
        private int _roundTime = 120; // 2 minutos por defecto

        public Player Player1 { get; }
        public Player Player2 { get; }
        public int Round { get; set; } = 1;

        public int RoundTime
        {
            get => _roundTime;
            set
            {
                _roundTime = value;
                OnPropertyChanged();
            }
        }

        public string TimerDisplay =>
            $"{_timer?.RemainingTime / 60:00}:{_timer?.RemainingTime % 60:00}";

        public bool IsTimerRunning => _timer?.IsRunning ?? false;

        public event PropertyChangedEventHandler PropertyChanged;

        public Match()
        {
            Player1 = new Player { Name = "Jugador Rojo" };
            Player2 = new Player { Name = "Jugador Azul" };
            _timer = new Timer(RoundTime);
            _timer.TimeElapsed += (sender, e) => OnPropertyChanged(nameof(TimerDisplay));
        }

        // Métodos del temporizador
        public void StartTimer()
        {
            _timer.Start();
            OnPropertyChanged(nameof(IsTimerRunning));
        }

        public void StopTimer()
        {
            _timer.Stop();
            OnPropertyChanged(nameof(IsTimerRunning));
        }

        public void ResetTimer()
        {
            _timer.Reset(RoundTime);
            OnPropertyChanged(nameof(TimerDisplay));
            OnPropertyChanged(nameof(IsTimerRunning));
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
