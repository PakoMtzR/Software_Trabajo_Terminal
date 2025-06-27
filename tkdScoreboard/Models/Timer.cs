using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Timers;

namespace tkdScoreboard.Models
{
    public class Timer : INotifyPropertyChanged
    {
        // Atributos privados
        private readonly System.Timers.Timer _timer;
        private int _remainingTime;
        private bool _isRunning;

        //  Evento para notificar cambios en las propiedades
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event ElapsedEventHandler TimeElapsed;

        // Propiedades públicas
        public int RemainingTime
        {
            get => _remainingTime;
            set
            {
                if (_remainingTime != value)
                {
                    _remainingTime = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsRunning
        {
            get => _isRunning;
            private set
            {
                if (_isRunning != value)
                {
                    _isRunning = value ? true : false;
                    OnPropertyChanged();
                }
            }
        }

        // Constructor
        public Timer(int seconds)
        {
            if (seconds < 0)
                throw new ArgumentOutOfRangeException(nameof(seconds), "El tiempo no puede ser negativo.");

            RemainingTime = seconds;
            _timer = new System.Timers.Timer(1000);     
            _timer.Elapsed += OnTimerElapsed;           // Se ejecuta cada segundo
        }

        // Método que se ejecuta cada segundo
        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (RemainingTime > 0)
            {
                RemainingTime--;
                TimeElapsed?.Invoke(this, e);
            }
            else Stop();
        }

        // Métodos para iniciar, detener, pausar y reiniciar el temporizador
        public void Start()
        {
            if (RemainingTime > 0)
            {
                _timer.Start();
                IsRunning = true;
            }
        }

        public void Stop()
        {
            _timer.Stop();
            IsRunning = false;
        }

        public void Pause()
        {
            Stop();
        }

        public void Reset(int seconds)
        {
            if (seconds < 0)
                throw new ArgumentOutOfRangeException(nameof(seconds), "El tiempo no puede ser negativo.");

            Stop();
            RemainingTime = seconds;
        }
    }
}
