using System.Collections;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

//
// This code based on code available here:
//
//  http://www.codeproject.com/KB/WPF/WPFJoshSmith.aspx
//
namespace Tauron.Application.Localizer.Core.UI
{
    //
    // This class is an adorner that allows a FrameworkElement derived class to adorn another FrameworkElement.
    //
    public class FrameworkElementAdorner : Adorner
    {
        //
        // The framework element that is the adorner. 
        //
        private readonly FrameworkElement _child;

        //
        // Placement of the child.
        //
        private readonly AdornerPlacement _horizontalAdornerPlacement = AdornerPlacement.Inside;

        //
        // Offset of the child.
        //
        private readonly double _offsetX;
        private readonly double _offsetY;
        private readonly AdornerPlacement _verticalAdornerPlacement = AdornerPlacement.Inside;

        public FrameworkElementAdorner(FrameworkElement adornerChildElement, FrameworkElement adornedElement)
            : base(adornedElement)
        {
            _child = adornerChildElement;

            AddLogicalChild(adornerChildElement);
            AddVisualChild(adornerChildElement);
        }

        public FrameworkElementAdorner(FrameworkElement adornerChildElement, FrameworkElement adornedElement,
            AdornerPlacement horizontalAdornerPlacement, AdornerPlacement verticalAdornerPlacement,
            double offsetX, double offsetY)
            : base(adornedElement)
        {
            _child = adornerChildElement;
            _horizontalAdornerPlacement = horizontalAdornerPlacement;
            _verticalAdornerPlacement = verticalAdornerPlacement;
            _offsetX = offsetX;
            _offsetY = offsetY;

            adornedElement.SizeChanged += adornedElement_SizeChanged;

            AddLogicalChild(adornerChildElement);
            AddVisualChild(adornerChildElement);
        }

        //
        // Position of the child (when not set to NaN).
        //
        public double PositionX { get; set; } = double.NaN;

        public double PositionY { get; set; } = double.NaN;

        protected override int VisualChildrenCount => 1;

        protected override IEnumerator LogicalChildren
        {
            get
            {
                ArrayList list = new ArrayList {_child};
                return list.GetEnumerator();
            }
        }

        /// <summary>
        ///     Override AdornedElement from base class for less type-checking.
        /// </summary>
        public new FrameworkElement AdornedElement => (FrameworkElement) base.AdornedElement;

        /// <summary>
        ///     Event raised when the adorned control's size has changed.
        /// </summary>
        private void adornedElement_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            InvalidateMeasure();
        }

        protected override Size MeasureOverride(Size constraint)
        {
            _child.Measure(constraint);
            return _child.DesiredSize;
        }

        /// <summary>
        ///     Determine the X coordinate of the child.
        /// </summary>
        private double DetermineX()
        {
            switch (_child.HorizontalAlignment)
            {
                case HorizontalAlignment.Left:
                {
                    if (_horizontalAdornerPlacement == AdornerPlacement.Outside)
                        return -_child.DesiredSize.Width + _offsetX;

                    return _offsetX;
                }
                case HorizontalAlignment.Right:
                {
                    if (_horizontalAdornerPlacement == AdornerPlacement.Outside)
                    {
                        var adornedWidth = AdornedElement.ActualWidth;
                        return adornedWidth + _offsetX;
                    }
                    else
                    {
                        var adornerWidth = _child.DesiredSize.Width;
                        var adornedWidth = AdornedElement.ActualWidth;
                        var x = adornedWidth - adornerWidth;
                        return x + _offsetX;
                    }
                }
                case HorizontalAlignment.Center:
                {
                    var adornerWidth = _child.DesiredSize.Width;
                    var adornedWidth = AdornedElement.ActualWidth;
                    var x = adornedWidth / 2 - adornerWidth / 2;
                    return x + _offsetX;
                }
                case HorizontalAlignment.Stretch:
                    return 0.0;
                default:
                    return 0;
            }
        }

        /// <summary>
        ///     Determine the Y coordinate of the child.
        /// </summary>
        private double DetermineY()
        {
            switch (_child.VerticalAlignment)
            {
                case VerticalAlignment.Top:
                {
                    if (_verticalAdornerPlacement == AdornerPlacement.Outside)
                        return -_child.DesiredSize.Height + _offsetY;
                    return _offsetY;
                }
                case VerticalAlignment.Bottom:
                {
                    if (_verticalAdornerPlacement == AdornerPlacement.Outside)
                    {
                        var adornedHeight = AdornedElement.ActualHeight;
                        return adornedHeight + _offsetY;
                    }
                    else
                    {
                        var adornerHeight = _child.DesiredSize.Height;
                        var adornedHeight = AdornedElement.ActualHeight;
                        var x = adornedHeight - adornerHeight;
                        return x + _offsetY;
                    }
                }
                case VerticalAlignment.Center:
                {
                    var adornerHeight = _child.DesiredSize.Height;
                    var adornedHeight = AdornedElement.ActualHeight;
                    var x = adornedHeight / 2 - adornerHeight / 2;
                    return x + _offsetY;
                }
                case VerticalAlignment.Stretch:
                {
                    return 0.0;
                }
                default:
                    return 0;
            }
        }

        /// <summary>
        ///     Determine the width of the child.
        /// </summary>
        private double DetermineWidth()
        {
            if (!double.IsNaN(PositionX)) return _child.DesiredSize.Width;

            return _child.HorizontalAlignment switch
            {
                HorizontalAlignment.Left => _child.DesiredSize.Width,
                HorizontalAlignment.Right => _child.DesiredSize.Width,
                HorizontalAlignment.Center => _child.DesiredSize.Width,
                HorizontalAlignment.Stretch => AdornedElement.ActualWidth,
                _ => 0.0
            };
        }

        /// <summary>
        ///     Determine the height of the child.
        /// </summary>
        private double DetermineHeight()
        {
            if (!double.IsNaN(PositionY)) return _child.DesiredSize.Height;

            return _child.VerticalAlignment switch
            {
                VerticalAlignment.Top => _child.DesiredSize.Height,
                VerticalAlignment.Bottom => _child.DesiredSize.Height,
                VerticalAlignment.Center => _child.DesiredSize.Height,
                VerticalAlignment.Stretch => AdornedElement.ActualHeight,
                _ => 0.0
            };
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var x = PositionX;
            if (double.IsNaN(x)) x = DetermineX();
            var y = PositionY;
            if (double.IsNaN(y)) y = DetermineY();
            var adornerWidth = DetermineWidth();
            var adornerHeight = DetermineHeight();
            _child.Arrange(new Rect(x, y, adornerWidth, adornerHeight));
            return finalSize;
        }

        protected override Visual GetVisualChild(int index)
        {
            return _child;
        }

        /// <summary>
        ///     Disconnect the child element from the visual tree so that it may be reused later.
        /// </summary>
        public void DisconnectChild()
        {
            RemoveLogicalChild(_child);
            RemoveVisualChild(_child);
        }
    }
}