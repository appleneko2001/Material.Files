using System;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace Material.Files.Gestures
{
    public static class MouseGestures
    {
        public static readonly RoutedEvent<RoutedEventArgs> MouseBackPressedEvent = RoutedEvent.Register<RoutedEventArgs>(
            "MouseBackPressed",
            RoutingStrategies.Bubble,
            typeof(MouseGestures));

        static MouseGestures()
        {
            InputElement.PointerPressedEvent.RouteFinished.Subscribe(PointerPressed);
            InputElement.PointerReleasedEvent.RouteFinished.Subscribe(PointerReleased);
        }
        
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        private static WeakReference<IInteractive> s_lastPress = new WeakReference<IInteractive>(null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        
        private static void PointerPressed(RoutedEventArgs ev)
        {
            if (ev.Source is null)
            {
                return;
            }

            if (ev.Route == RoutingStrategies.Bubble)
            {
                var e = (PointerPressedEventArgs)ev;
                var visual = (IVisual)ev.Source;

                if (e.ClickCount <= 1)
                {
                    s_lastPress = new WeakReference<IInteractive>(ev.Source);
                }
            }
        }

        private static void PointerReleased(RoutedEventArgs ev)
        {
            if (ev.Route == RoutingStrategies.Bubble)
            {
                var e = (PointerReleasedEventArgs)ev;

                if (s_lastPress.TryGetTarget(out var target) && target == e.Source)
                {
                    if (e.InitialPressMouseButton == MouseButton.XButton1)
                    {
                        e.Source.RaiseEvent(new RoutedEventArgs(MouseBackPressedEvent));
                    }
                }
            }
        }
    }
}