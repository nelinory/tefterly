using System;
using System.Diagnostics;

namespace Tefterly.Core
{
    public class DisposableStopwatch : IDisposable
    {
        private readonly Stopwatch _stopwatch;
        private readonly Action<TimeSpan> _actionHandler;

        public DisposableStopwatch(Action<TimeSpan> actionHandler)
        {
            _actionHandler = actionHandler;
            _stopwatch = Stopwatch.StartNew();
        }

        public void Dispose()
        {
            _stopwatch.Stop();
            _actionHandler(_stopwatch.Elapsed);
        }
    }
}
