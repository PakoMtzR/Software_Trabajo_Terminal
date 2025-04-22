using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace tkdScoreboard.Models
{
    public class Timer
    {
        private readonly System.Timers.Timer _timer;

        public int RemainingTime { get; private set; }
        public bool IsRunning { get; private set; }
        public event ElapsedEventHandler TimeElapsed;

        public Timer(int seconds)
        {
            RemainingTime = seconds;
            _timer = new System.Timers.Timer(1000);
            _timer.Elapsed += OnTimerElapsed;
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            RemainingTime--;
            TimeElapsed?.Invoke(this, e);

            if (RemainingTime <= 0)
                Stop();
        }

        public void Start()
        {
            _timer.Start();
            IsRunning = true;
        }

        public void Stop()
        {
            _timer.Stop();
            IsRunning = false;
        }

        public void Reset(int seconds)
        {
            Stop();
            RemainingTime = seconds;
        }
    }
}
