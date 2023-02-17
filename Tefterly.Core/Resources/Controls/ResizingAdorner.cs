using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Tefterly.Core.Resources.Controls
{
    // Credit: https://social.msdn.microsoft.com/Forums/vstudio/en-US/41229dc6-6171-453b-82bc-f742f629e4f1/richtextbox-resizing-adorner
    public class ResizingAdorner : Adorner
    {
        public delegate void Notify();
        public event Notify AdornerResized;

        // Resizing adorner uses thumbs for visual elements   
        private readonly Thumb _topLeft;
        private readonly Thumb _topRight;
        private readonly Thumb _bottomLeft;
        private readonly Thumb _bottomRight;
        private readonly Rectangle _adornerBorder;

        private readonly double _parentWidth;
        private readonly double _parentHeight;

        private static readonly double THUMB_SIZE = 11;
        private static readonly double ADORNER_MIN_SIZE = 70;

        // To store and manage the adorner's visual children 
        private readonly VisualCollection _adornerVisualChildren;

        // Initialize the ResizingAdorner
        public ResizingAdorner(UIElement adornedElement, double parentWidth, double parentHeight) : base(adornedElement)
        {
            _parentWidth = parentWidth;
            _parentHeight = parentHeight;

            _adornerVisualChildren = new VisualCollection(this);

            // Build adorner border
            BuildAdornerBorder(ref _adornerBorder);

            // Call a helper method to initialize the thumbs with a customized cursors
            BuildAdornerCorner(ref _topLeft, Cursors.SizeNWSE);
            BuildAdornerCorner(ref _topRight, Cursors.SizeNESW);
            BuildAdornerCorner(ref _bottomLeft, Cursors.SizeNESW);
            BuildAdornerCorner(ref _bottomRight, Cursors.SizeNWSE);

            // Add handlers for resizing
            _topLeft.DragDelta += new DragDeltaEventHandler(HandleTopLeft);
            _topRight.DragDelta += new DragDeltaEventHandler(HandleTopRight);
            _bottomLeft.DragDelta += new DragDeltaEventHandler(HandleBottomLeft);
            _bottomRight.DragDelta += new DragDeltaEventHandler(HandleBottomRight);
        }

        private void BuildAdornerBorder(ref Rectangle adornerBorder)
        {
            adornerBorder = new Rectangle();
            adornerBorder.StrokeDashArray.Add(7);
            adornerBorder.Stroke = Utilities.GetColorBrushFromString(SettingsManager.Instance.Settings.Notes.HyperlinkForegroundColor);
            adornerBorder.StrokeThickness = 1;

            _adornerVisualChildren.Add(adornerBorder);
        }

        private void BuildAdornerCorner(ref Thumb cornerThumb, Cursor cursor)
        {
            if (cornerThumb != null)
                return;

            cornerThumb = new Thumb()
            {
                Width = THUMB_SIZE,
                Height = THUMB_SIZE,
                Cursor = cursor,
                Template = new ControlTemplate(typeof(Thumb))
                {
                    VisualTree = GetThumbTemplate(new SolidColorBrush(Colors.White))
                }
            };

            _adornerVisualChildren.Add(cornerThumb);
        }

        private void HandleTopLeft(object sender, DragDeltaEventArgs args)
        {
            if (AdornedElement is not FrameworkElement adornedElement || sender is not Thumb hitThumb)
                return;

            // Ensure that the width and height are properly initialized after the resize
            EnforceSize(adornedElement);

            // Change the size by the amount the user drags the mouse, as long as it's larger  
            // than the width or height of an adorner, respectively
            adornedElement.Width = Math.Max(adornedElement.Width - args.HorizontalChange, hitThumb.DesiredSize.Width);
            adornedElement.Height = Math.Max(adornedElement.Height - args.VerticalChange, hitThumb.DesiredSize.Height);

            // Adjust adorner minimal size based on the predefined constraints
            AdjustAdornerMinSize(adornedElement);
        }

        private void HandleBottomRight(object sender, DragDeltaEventArgs args)
        {
            if (AdornedElement is not FrameworkElement adornedElement || sender is not Thumb hitThumb)
                return;

            // Ensure that the width and height are properly initialized after the resize
            EnforceSize(adornedElement);

            // Change the size by the amount the user drags the mouse, as long as it's larger  
            // than the width or height of an adorner, respectively
            adornedElement.Width = Math.Max(adornedElement.Width + args.HorizontalChange, hitThumb.DesiredSize.Width);
            adornedElement.Height = Math.Max(adornedElement.Height + args.VerticalChange, hitThumb.DesiredSize.Height);

            // Adjust adorner minimal size based on the predefined constraints
            AdjustAdornerMinSize(adornedElement);
        }

        private void HandleBottomLeft(object sender, DragDeltaEventArgs args)
        {
            if (AdornedElement is not FrameworkElement adornedElement || sender is not Thumb hitThumb)
                return;

            // Ensure that the width and height are properly initialized after the resize
            EnforceSize(adornedElement);

            // Change the size by the amount the user drags the mouse, as long as it's larger  
            // than the width or height of an adorner, respectively
            adornedElement.Width = Math.Max(adornedElement.Width - args.HorizontalChange, hitThumb.DesiredSize.Width);
            adornedElement.Height = Math.Max(adornedElement.Height + args.VerticalChange, hitThumb.DesiredSize.Height);

            // Adjust adorner minimal size based on the predefined constraints
            AdjustAdornerMinSize(adornedElement);
        }

        private void HandleTopRight(object sender, DragDeltaEventArgs args)
        {
            if (AdornedElement is not FrameworkElement adornedElement || sender is not Thumb hitThumb)
                return;

            // Ensure that the width and height are properly initialized after the resize
            EnforceSize(adornedElement);

            // Change the size by the amount the user drags the mouse, as long as it's larger  
            // than the width or height of an adorner, respectively
            adornedElement.Width = Math.Max(adornedElement.Width + args.HorizontalChange, hitThumb.DesiredSize.Width);
            adornedElement.Height = Math.Max(adornedElement.Height - args.VerticalChange, hitThumb.DesiredSize.Height);

            // Adjust adorner minimal size based on the predefined constraints
            AdjustAdornerMinSize(adornedElement);
        }

        private void EnforceSize(FrameworkElement adornedElement)
        {
            // Ensures that the Widths and Heights are initialized. Sizing to content produces 
            // Width and Height values of Double.NaN. Because this adorner explicitly resizes, the Width and Height 
            // need to be set first. It also sets the maximum size of the adorned element

            if (adornedElement.Width.Equals(Double.NaN))
                adornedElement.Width = adornedElement.RenderSize.Width;
            if (adornedElement.Height.Equals(Double.NaN))
                adornedElement.Height = adornedElement.RenderSize.Height;

            adornedElement.MaxWidth = _parentWidth;
            adornedElement.MaxHeight = _parentHeight;
        }

        private void AdjustAdornerMinSize(FrameworkElement adornedElement)
        {
            // notify any subscribers when the adorner is resized
            AdornerResized?.Invoke();

            if (adornedElement.Width < ADORNER_MIN_SIZE)
            {
                adornedElement.Width = ADORNER_MIN_SIZE;
                return;
            }

            if (adornedElement.Height < ADORNER_MIN_SIZE)
            {
                adornedElement.Height = ADORNER_MIN_SIZE;
                return;
            }
        }

        private static FrameworkElementFactory GetThumbTemplate(Brush adornerBrush)
        {
            FrameworkElementFactory frameworkElement = new FrameworkElementFactory(typeof(Ellipse));
            frameworkElement.SetValue(Shape.FillProperty, adornerBrush);
            frameworkElement.SetValue(Shape.StrokeProperty, Utilities.GetColorBrushFromString(SettingsManager.Instance.Settings.Notes.HyperlinkForegroundColor));
            frameworkElement.SetValue(Shape.StrokeThicknessProperty, (double)2);

            return frameworkElement;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            // desiredWidth and desiredHeight are the width and height of the element that's being adorned.  
            // These will be used to place the ResizingAdorner at the corners of the adorned element
            double desiredWidth = AdornedElement.RenderSize.Width;
            double desiredHeight = AdornedElement.RenderSize.Height;

            // adornerWidth & adornerHeight are used for placement as well
            double adornerWidth = RenderSize.Width;
            double adornerHeight = RenderSize.Height;

            _adornerBorder.Arrange(new Rect(0, 0, adornerWidth, adornerHeight));

            _topLeft.Arrange(new Rect(-adornerWidth / 2, -adornerHeight / 2, adornerWidth, adornerHeight));
            _topRight.Arrange(new Rect(desiredWidth - adornerWidth / 2, -adornerHeight / 2, adornerWidth, adornerHeight));
            _bottomLeft.Arrange(new Rect(-adornerWidth / 2, desiredHeight - adornerHeight / 2, adornerWidth, adornerHeight));
            _bottomRight.Arrange(new Rect(desiredWidth - adornerWidth / 2, desiredHeight - adornerHeight / 2, adornerWidth, adornerHeight));

            return finalSize;
        }

        protected override int VisualChildrenCount
        {
            get { return _adornerVisualChildren.Count; }
        }

        protected override Visual GetVisualChild(int index)
        {
            return _adornerVisualChildren[index];
        }
    }
}