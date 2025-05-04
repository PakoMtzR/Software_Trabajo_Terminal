using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace tkdScoreboard.Models
{
    public class Player : INotifyPropertyChanged
    {
        //  Evento para notificar cambios en las propiedades
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Atributos privados
        private string _name;
        private int _points = 0;
        private int _penalties = 0;
        private int _wonRounds = 0;

        // Propiedades públicas
        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged();
                }
            }
        }

        public int Points
        {
            get => _points;
            private set
            {
                if (_points != value)
                {
                    _points = value;
                    OnPropertyChanged();
                }
            }
        }

        public int Penalties
        {
            get => _penalties;
            private set
            {
                if (_penalties != value)
                {
                    _penalties = value;
                    OnPropertyChanged();
                }
            }
        }

        public int WonRounds
        {
            get => _wonRounds;
            set
            {
                if (_wonRounds != value)
                {
                    _wonRounds = value;
                    OnPropertyChanged();
                }
            }
        }

        // Métodos para modificar puntuación
        public void AddPoints(int points)
        {
            if (points > 0)
                Points += points;
        }

        public void DeductPoints(int points)
        {
            if (points > 0)
                Points = Math.Max(0, Points - points);
        }

        // Metodos para modficar penalizaciones
        public void AddPenalty()
        {
            Penalties++;
        }

        public void RemovePenalty()
        {
            Penalties = Math.Max(0, Penalties - 1);
        }

        // Metodo para reiniciar el puntaje y las penalizaciones
        public void Reset()
        {
            WonRounds = 0;
            ResetScore();
        }
        public void ResetScore()
        {
            Points = 0;
            Penalties = 0;
        }
    }
}
