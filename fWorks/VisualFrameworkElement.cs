using System.Windows;
using System.Windows.Media;

namespace fWorks
{
    public sealed class VisualFrameworkElement : FrameworkElement
    {
        public VisualCollection Visuals { get; }

        public VisualFrameworkElement()
        {
            Visuals = new VisualCollection(this);
        }

        protected override int VisualChildrenCount => Visuals.Count;

        protected override Visual GetVisualChild(int index)
        {
            return Visuals[index];
        }
    }
}
