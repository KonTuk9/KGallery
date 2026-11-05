using System.ComponentModel;
using System.Windows;
using Wpf.Ui.Controls;

namespace KGallery.Controls
{
    public class VirtualizingUiWrapPanel : VirtualizingWrapPanel
    {
        public static readonly DependencyProperty ScrollAmountProperty = DependencyProperty.Register(nameof(ScrollAmount), typeof(double), typeof(VirtualizingUiWrapPanel), new PropertyMetadata(40d));

        [Bindable(true)]
        [Category("Scroll viewer")]
        [Description("Specifies how many pixels mouse wheel scrolls")]
        public double ScrollAmount
        {
            get { return (double)GetValue(ScrollAmountProperty); }
            set { SetValue(ScrollAmountProperty, value); }
        }

        protected override double GetMouseWheelDownScrollAmount()
        {
            return ScrollAmount;
        }

        protected override double GetMouseWheelUpScrollAmount()
        {
            return -ScrollAmount;
        }
    }
}
