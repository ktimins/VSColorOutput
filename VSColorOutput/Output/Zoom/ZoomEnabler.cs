using System;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace VSColorOutput.Output.Zoom
{
    [Export(typeof(IWpfTextViewCreationListener))]
    [ContentType("output")]
    [TextViewRole(PredefinedTextViewRoles.Interactive)]
    internal sealed class OutputZoomTextViewCreationListener : IWpfTextViewCreationListener
    {
        public void TextViewCreated(IWpfTextView textView)
        {
            ZoomEnabler.EnableZoom(textView?.VisualElement);
        }
    }

    public static class ZoomEnabler
    {
        private static readonly DependencyProperty ZoomEnabledProperty = DependencyProperty.RegisterAttached(
            "ZoomEnabled",
            typeof(bool),
            typeof(ZoomEnabler),
            new PropertyMetadata(false));

        public static ScaleTransform EnableZoom(FrameworkElement element)
        {
            if (element == null)
            {
                return null;
            }

            if ((bool)element.GetValue(ZoomEnabledProperty))
            {
                return element.LayoutTransform as ScaleTransform;
            }

            element.SetValue(ZoomEnabledProperty, true);

            var parms = new ZoomEnablerParms();

            var zoomTransform = element.LayoutTransform as ScaleTransform;
            if (zoomTransform == null)
            {
                zoomTransform = new ScaleTransform(parms.InitialZoom, parms.InitialZoom);
                element.LayoutTransform = zoomTransform;
            }
            else
            {
                zoomTransform.ScaleX = parms.InitialZoom;
                zoomTransform.ScaleY = parms.InitialZoom;
            }

            var currentZoom = parms.InitialZoom;

            void Handler(object sender, MouseWheelEventArgs e)
            {
                if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                {
                    e.Handled = true;

                    currentZoom += e.Delta > 0 ? parms.ZoomStep : -parms.ZoomStep;
                    if (currentZoom < parms.MinZoom)
                    {
                        currentZoom = parms.MinZoom;
                    }

                    if (currentZoom > parms.MaxZoom)
                    {
                        currentZoom = parms.MaxZoom;
                    }

                    zoomTransform.ScaleX = currentZoom;
                    zoomTransform.ScaleY = currentZoom;

                    parms.OnStatusUpdate?.Invoke($"Zoom: {Math.Round(currentZoom * 100)}%");
                }
                else
                {
                    var parentScroll = FindParentScrollViewer(element);
                    if (parentScroll != null)
                    {
                        var newArgs = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
                        {
                            RoutedEvent = UIElement.MouseWheelEvent,
                            Source = sender
                        };
                        parentScroll.RaiseEvent(newArgs);
                    }
                }
            }

            element.PreviewMouseWheel += Handler;

            RoutedEventHandler unloadHandler = null;
            unloadHandler = (sender, args) =>
            {
                element.PreviewMouseWheel -= Handler;
                element.Unloaded -= unloadHandler;
            };
            element.Unloaded += unloadHandler;

            return zoomTransform;
        }

        private static ScrollViewer FindParentScrollViewer(DependencyObject child)
        {
            while (child != null && !(child is ScrollViewer))
            {
                child = VisualTreeHelper.GetParent(child);
            }

            return child as ScrollViewer;
        }

        private class ZoomEnablerParms
        {
            private double _initialZoom = 1.0;
            private double _zoomStep = 0.1;
            private double _minZoom = 0.1;
            private double _maxZoom = 5.0;

            public double InitialZoom
            {
                get => _initialZoom;
                set => _initialZoom = value > 0 ? value : 1.0;
            }

            public double ZoomStep
            {
                get => _zoomStep;
                set => _zoomStep = value > 0 ? value : 0.1;
            }

            public double MinZoom
            {
                get => _minZoom;
                set => _minZoom = value > 0 ? value : 0.1;
            }

            public double MaxZoom
            {
                get => _maxZoom;
                set => _maxZoom = value > 0 ? value : 5.0;
            }

            public Action<string> OnStatusUpdate { get; set; }
        }
    }
}
