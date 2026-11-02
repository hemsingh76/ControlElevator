using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace ElevatorSimulation.Core
{
 
    public delegate void AnimationTickHandler(double elapsedSeconds);

    public class ElevatorAnimator : IDisposable
    {
        private const double MaxStepSeconds = 0.1; // protects against a big jump after the window was blocked

        private readonly Timer _timer = new Timer();
        private readonly Stopwatch _stopwatch = new Stopwatch();
        private readonly AnimationTickHandler _onTick;
        private double _lastSeconds;

        public ElevatorAnimator(AnimationTickHandler onTick, int intervalMilliseconds)
        {
            _onTick = onTick;
            _timer.Interval = intervalMilliseconds;
            _timer.Tick += OnTimerTick;
        }

        public void Start()
        {
            _stopwatch.Restart();
            _lastSeconds = 0;
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
            _stopwatch.Stop();
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            double now = _stopwatch.Elapsed.TotalSeconds;
            double elapsed = Math.Min(now - _lastSeconds, MaxStepSeconds);
            _lastSeconds = now;

            _onTick(elapsed); // invoke the delegate
        }

        public void Dispose()
        {
            _timer.Stop();
            _timer.Dispose();
        }
    }
}
