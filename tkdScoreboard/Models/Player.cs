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
        private int _points;
        private int _penalties;

        public string Name { get; set; }


        public int Points
        {
            get => _points;
            private set
            {
                _points = value;
                OnPropertyChanged();
            }
        }

        public int Penalties
        {
            get => _penalties;
            private set
            {
                _penalties = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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

        public void AddPenalty()
        {
            Penalties++;
        }

        public void RemovePenalty()
        {
            Penalties = Math.Max(0, Penalties - 1);
        }

        public void ResetScore()
        {
            Points = 0;
            Penalties = 0;
        }

    }
}
