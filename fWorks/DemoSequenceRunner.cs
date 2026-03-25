using System;
using System.Collections.Generic;
using System.Linq;

namespace fWorks
{
    public sealed class DemoSequenceRunner
    {
        private readonly FireworkSimulation _simulation;
        private readonly CryptoRandom _random = new CryptoRandom();
        private readonly Queue<ScheduledLaunch> _pending = new Queue<ScheduledLaunch>();
        private readonly HashSet<FireworkType> _fastShellBlacklist = new HashSet<FireworkType>
        {
            FireworkType.FallingLeaves,
            FireworkType.Floral,
            FireworkType.Willow
        };

		private readonly HashSet<FireworkType> _shellBlacklist = new HashSet<FireworkType>
		{
			FireworkType.FallingLeaves,
			FireworkType.Willow
		};

		private bool _isFirstSequence = true;
        private DateTime _lastSmallBarrage = DateTime.MinValue;
        private double _timeUntilNextSequence;

        public DemoSequenceRunner(FireworkSimulation simulation)
        {
            _simulation = simulation;
        }

        public bool IsRunning { get; private set; }

        public void Start()
        {
            IsRunning = true;
            _isFirstSequence = true;
            _timeUntilNextSequence = 0;
            _pending.Clear();
        }

        public void Stop()
        {
            IsRunning = false;
            _pending.Clear();
        }

        public void Update(double deltaMilliseconds)
        {
            if (!IsRunning)
            {
                return;
            }

            _timeUntilNextSequence -= deltaMilliseconds;

            while (_pending.Count > 0)
            {
                ScheduledLaunch launch = _pending.Peek();
                launch.RemainingDelay -= deltaMilliseconds;
                if (launch.RemainingDelay > 0)
                {
                    break;
                }

                _pending.Dequeue();
                _simulation.AddNewFirework(launch.Type, launch.Size, launch.X, launch.Height);
            }

            if (_timeUntilNextSequence <= 0)
            {
                _timeUntilNextSequence = StartSequence();
            }
        }

        private int StartSequence()
        {
            if (_isFirstSequence)
            {
                //_isFirstSequence = false;
                //_simulation.AddNewFirework(FireworkType.Crysanthemum, px: 0.5, py: 0.5, shellSize: 2);

				SequencePyramid();
				return 5000;
            }

            double rand = _random.NextDouble();
            if (rand < 0.08 && (DateTime.Now - _lastSmallBarrage).TotalMilliseconds > 10000)
            {
                return SequenceSmallBarrage();
            }

            if (rand < 0.10)
            {
                return SequencePyramid();
            }

            if (rand < 0.60)
            {
                return SequenceRandomShell();
            }

            if (rand < 0.80)
            {
                return SequenceTwoRandom();
            }

            return SequenceTriple();
        }

        private RandomShellConfig GetRandomShellConfig()
        {
            double baseSize = 2;
            double maxVariance = 1;
            double variance = _random.NextDouble() * maxVariance;
            double size = baseSize - variance;
            double height = maxVariance == 0 ? _random.NextDouble() : 1 - (variance / maxVariance);
            double centerOffset = _random.NextDouble() * (1 - height * 0.65) * 0.5;
            double x = _random.NextDouble() < 0.5 ? 0.5 - centerOffset : 0.5 + centerOffset;

            return new RandomShellConfig
            {
                X = x,
                Size = size,
                Height = height
            };
        }

        private FireworkType RandomFastShell()
        {
            FireworkType[] values = Enum.GetValues(typeof(FireworkType)).Cast<FireworkType>().ToArray();
            FireworkType type = values[_random.Next(values.Length)];
            while (_fastShellBlacklist.Contains(type))
            {
                type = values[_random.Next(values.Length)];
            }
            return type;
        }

        private int SequenceRandomShell()
        {
            RandomShellConfig config = GetRandomShellConfig();
            FireworkType[] values = Enum.GetValues(typeof(FireworkType)).Cast<FireworkType>().ToArray();
            FireworkType type = values[_random.Next(values.Length)];
			while (_shellBlacklist.Contains(type)) {
				type = values[_random.Next(values.Length)];
			}
			Firework firework = _simulation.AddNewFirework(type, px: config.X, py: config.Height, shellSize: config.Size);
            double extraDelay = firework._FireworkType == FireworkType.FallingLeaves ? 4600 : firework._StarLife;
            return (int)(1200 + _random.NextDouble() * 600 + extraDelay);
        }

        private int SequenceTwoRandom()
        {
            RandomShellConfig left = GetRandomShellConfig();
            RandomShellConfig right = GetRandomShellConfig();
            FireworkType[] values = Enum.GetValues(typeof(FireworkType)).Cast<FireworkType>().ToArray();

            FireworkType type1 = values[_random.Next(values.Length)];
			while (_shellBlacklist.Contains(type1)) {
				type1 = values[_random.Next(values.Length)];
			}
			FireworkType type2 = values[_random.Next(values.Length)];
			while (_shellBlacklist.Contains(type2)) {
				type2 = values[_random.Next(values.Length)];
			}
			double leftOffset = _random.NextDouble() * 0.2 - 0.1;
            double rightOffset = _random.NextDouble() * 0.2 - 0.1;

            Firework first = _simulation.AddNewFirework(type1, px: 0.3 + leftOffset, py: left.Height, shellSize: left.Size);
            Firework second = _simulation.AddNewFirework(type2, px: 0.7 + rightOffset, py: right.Height, shellSize: right.Size);

            double extraDelay = first._FireworkType == FireworkType.FallingLeaves || second._FireworkType == FireworkType.FallingLeaves
                ? 4600
                : Math.Max(first._StarLife, second._StarLife);

            return (int)(1200 + _random.NextDouble() * 600 + extraDelay);
        }

        private int SequenceTriple()
        {
            FireworkType centerType = RandomFastShell();
            _simulation.AddNewFirework(centerType, px: 0.5 + (_random.NextDouble() * 0.08 - 0.04), py: 0.7, shellSize: 2);

            ScheduleLaunch(1000 + _random.NextDouble() * 400, RandomFastShell(), 0.2 + (_random.NextDouble() * 0.08 - 0.04), 0.1, 2);
            ScheduleLaunch(1000 + _random.NextDouble() * 400, RandomFastShell(), 0.8 + (_random.NextDouble() * 0.08 - 0.04), 0.1, 2);
            return 4100;
        }

        private int SequencePyramid()
        {
            double barrageCountHalf = 5;
            FireworkType main = _random.NextDouble() < 0.78 ? FireworkType.Crysanthemum : FireworkType.Ring;
            FireworkType side = RandomFastShell();
            int count = 0;
            double delay = 0;

            while (count <= barrageCountHalf)
            {
                if (count == barrageCountHalf)
                {
                    ScheduleLaunch(delay, main, 0.5, 0.75, 2);
                }
                else
                {
                    double offset = count / barrageCountHalf * 0.5;
                    double leftX = offset;
                    double rightX = 1 - offset;
                    double leftHeight = (leftX <= 0.5 ? leftX / 0.5 : (1 - leftX) / 0.5) * 0.42;
                    double rightHeight = (rightX <= 0.5 ? rightX / 0.5 : (1 - rightX) / 0.5) * 0.42;
                    ScheduleLaunch(delay, side, leftX, leftHeight, 1);
                    ScheduleLaunch(delay, side, rightX, rightHeight, 1);
                }

                count++;
                delay += 200;
            }

            return (int)(3500 + barrageCountHalf * 250);
        }

        private int SequenceSmallBarrage()
        {
            _lastSmallBarrage = DateTime.Now;
            double barrageCount = 7;
            double shellSize = 2;
            FireworkType main = _random.NextDouble() < 0.78 ? FireworkType.Crysanthemum : FireworkType.Ring;
            FireworkType side = RandomFastShell();
            int count = 0;
            double delay = 0;

            while (count < barrageCount)
            {
                if (count == 0)
                {
                    double height = (Math.Cos(0.5 * 5 * Math.PI + Math.PI / 2) + 1) / 2;
                    _simulation.AddNewFirework(main, px: 0.5, py: height * 0.75, shellSize: shellSize);
                    count += 1;
                }
                else
                {
                    double offset = (count + 1) / barrageCount / 2;
                    double rightX = 0.5 + offset;
                    double leftX = 0.5 - offset;
                    double rightHeight = (Math.Cos(rightX * 5 * Math.PI + Math.PI / 2) + 1) / 2;
                    double leftHeight = (Math.Cos(leftX * 5 * Math.PI + Math.PI / 2) + 1) / 2;
                    ScheduleLaunch(delay, side, rightX, rightHeight * 0.75, shellSize);
                    ScheduleLaunch(delay, side, leftX, leftHeight * 0.75, shellSize);
                    count += 2;
                }

                delay += 200;
            }

            return (int)(3400 + barrageCount * 120);
        }

        private void ScheduleLaunch(double delay, FireworkType type, double x, double height, double size)
        {
            List<ScheduledLaunch> launches = _pending.ToList();
            launches.Add(new ScheduledLaunch
            {
                RemainingDelay = delay,
                Type = type,
                X = x,
                Height = height,
                Size = size
            });

            launches.Sort((a, b) => a.RemainingDelay.CompareTo(b.RemainingDelay));
            _pending.Clear();
            foreach (ScheduledLaunch launch in launches)
            {
                _pending.Enqueue(launch);
            }
        }

        private sealed class RandomShellConfig
        {
            public double Size;
            public double X;
            public double Height;
        }

        private sealed class ScheduledLaunch
        {
            public double RemainingDelay;
            public FireworkType Type;
            public double X;
            public double Height;
            public double Size;
        }
    }
}
