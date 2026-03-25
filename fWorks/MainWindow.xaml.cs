using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace fWorks
{
    public partial class MainWindow : Window
    {
        private readonly Stopwatch _stopwatch = new Stopwatch();
        private readonly FireworkSimulation _simulation = new FireworkSimulation();
        private readonly DemoSequenceRunner _demoSequenceRunner;
        private readonly FrameStatistics _frameStatistics = new FrameStatistics();
        private bool _isLoaded;
        private long _lastTimestamp;

        public MainWindow()
        {
            InitializeComponent();
            _demoSequenceRunner = new DemoSequenceRunner(_simulation);

            foreach (FireworkType type in Enum.GetValues(typeof(FireworkType)))
            {
                ComboBoxItem item = new ComboBoxItem { Content = type };
                myCombobox.Items.Add(item);
            }

            myCombobox.SelectedIndex = 4;
            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isLoaded)
            {
                return;
            }

            _isLoaded = true;
            SimulationContext.Current.StageWidth = _MyCanvas.ActualWidth;
            SimulationContext.Current.StageHeight = _MyCanvas.ActualHeight;

            _stopwatch.Restart();
            _lastTimestamp = 0;
            CompositionTarget.Rendering -= CompositionTarget_Rendering;
            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            _demoSequenceRunner.Stop();
            CompositionTarget.Rendering -= CompositionTarget_Rendering;
        }

        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            long currentTimestamp = _stopwatch.ElapsedMilliseconds;
            long delta = currentTimestamp - _lastTimestamp;
            if (delta <= 0)
            {
                return;
            }

            _lastTimestamp = currentTimestamp;
            double lag = delta / 16.6667;

            _demoSequenceRunner.Update(delta);
            _simulation.Update(delta, lag);

            DrawingVisual visual = _simulation.Render();
            visual.CacheMode = new BitmapCache();
            _MyDraw.Visuals.Add(visual);

            if (_MyDraw.Visuals.Count > 10)
            {
                _MyDraw.Visuals.RemoveAt(0);
            }

            UpdateStatistics(delta);
        }

        private void UpdateStatistics(long delta)
        {
            _frameStatistics.Add(delta, _simulation.StarList.Count, _simulation.SparkList.Count);
            myTextbox1.Text = $"ms mean: {_frameStatistics.MeanMs:0.0}";
            myTextbox2.Text = $"ms max: {_frameStatistics.MaxMs}";
            myTextbox3.Text = $"star: {_simulation.StarList.Count}";
            myTextbox4.Text = $"max star: {_frameStatistics.MaxStars}";
            myTextbox5.Text = $"spark: {_simulation.SparkList.Count}";
            myTextbox6.Text = $"max spark: {_frameStatistics.MaxSparks}";
        }

        private void _LaunchFirework(object sender, RoutedEventArgs e)
        {
            _demoSequenceRunner.Stop();
            FireworkType type = (FireworkType)((ComboBoxItem)myCombobox.SelectedItem).Content;
            _simulation.AddNewFirework(type, shellSize: 2);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (_demoSequenceRunner.IsRunning)
            {
                _demoSequenceRunner.Stop();
            }
            else
            {
                _demoSequenceRunner.Start();
            }
        }
    }
}
