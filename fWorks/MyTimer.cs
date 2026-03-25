using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace fWorks
{
    public sealed class PerformanceStopwatch
    {
        private long _start;
        private long _end;

        public void StampBegin()
        {
            _start = Stopwatch.GetTimestamp();
            _end = _start;
        }

        public void StampEnd()
        {
            _end = Stopwatch.GetTimestamp();
        }

        public long ElapsedNs => ((_end - _start) * (long)1E9) / Stopwatch.Frequency;
        public long ElapsedUs => ((_end - _start) * (long)1E6) / Stopwatch.Frequency;
        public long ElapsedMs => ((_end - _start) * (long)1E3) / Stopwatch.Frequency;
    }

    public sealed class AccurateTimer : IDisposable
    {
        public delegate void DLTMMTimer(long elapsedMs);
        public DLTMMTimer EVTMMTimer;

        private readonly NativeMethods.TimeSetEventCallback _callback;
        private bool _disposed;
        private uint _timerId;
        private uint _timerStartTime;

        public AccurateTimer()
        {
            _callback = Callback;
        }

        public void Start(uint milliseconds, bool repeat)
        {
            NativeMethods.TimerEventType flags = NativeMethods.TimerEventType.TIME_CALLBACK_FUNCTION |
                                                 (repeat ? NativeMethods.TimerEventType.TIME_PERIODIC : NativeMethods.TimerEventType.TIME_ONESHOT);

            Stop();
            _timerStartTime = NativeMethods.timeGetTime();
            _timerId = NativeMethods.timeSetEvent(milliseconds, 10, _callback, UIntPtr.Zero, (uint)flags);
            if (_timerId == 0)
            {
                throw new InvalidOperationException("timeSetEvent error");
            }
        }

        public void Stop()
        {
            if (_timerId == 0)
            {
                return;
            }

            NativeMethods.timeKillEvent(_timerId);
            _timerId = 0;
        }

        private void Callback(uint timerId, uint msg, UIntPtr user, UIntPtr dw1, UIntPtr dw2)
        {
            EVTMMTimer?.BeginInvoke(NativeMethods.timeGetTime() - _timerStartTime, null, null);
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            Stop();
            _disposed = true;
            GC.SuppressFinalize(this);
        }

        ~AccurateTimer()
        {
            Dispose();
        }
    }

    internal static class NativeMethods
    {
        [Flags]
        public enum TimerEventType
        {
            TIME_ONESHOT = 0x0000,
            TIME_PERIODIC = 0x0001,
            TIME_CALLBACK_FUNCTION = 0x0000
        }

        public delegate void TimeSetEventCallback(uint timerId, uint message, UIntPtr user, UIntPtr dw1, UIntPtr dw2);

        [DllImport("winmm.dll")]
        public static extern uint timeSetEvent(uint delay, uint resolution, TimeSetEventCallback callback, UIntPtr user, uint eventType);

        [DllImport("winmm.dll")]
        public static extern uint timeKillEvent(uint timerId);

        [DllImport("winmm.dll")]
        public static extern uint timeGetTime();
    }
}
