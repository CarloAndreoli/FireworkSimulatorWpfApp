using System.Collections.Generic;
using System.Linq;

namespace fWorks
{
    public sealed class FrameStatistics
    {
        private readonly int _capacity;
        private readonly Queue<long> _frameTimes;
        private readonly Queue<long> _starCounts;
        private readonly Queue<long> _sparkCounts;

        public FrameStatistics(int capacity = 120)
        {
            _capacity = capacity;
            _frameTimes = new Queue<long>(capacity);
            _starCounts = new Queue<long>(capacity);
            _sparkCounts = new Queue<long>(capacity);
        }

        public void Add(long frameTime, int starCount, int sparkCount)
        {
            _frameTimes.Enqueue(frameTime);
            _starCounts.Enqueue(starCount);
            _sparkCounts.Enqueue(sparkCount);

            while (_frameTimes.Count > _capacity)
            {
                _frameTimes.Dequeue();
                _starCounts.Dequeue();
                _sparkCounts.Dequeue();
            }
        }

        public double MeanMs => _frameTimes.Count == 0 ? 0 : _frameTimes.Average();
        public long MaxMs => _frameTimes.Count == 0 ? 0 : _frameTimes.Max();
        public int MaxStars => _starCounts.Count == 0 ? 0 : (int)_starCounts.Max();
        public int MaxSparks => _sparkCounts.Count == 0 ? 0 : (int)_sparkCounts.Max();
    }
}
