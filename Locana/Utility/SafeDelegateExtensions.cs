using System;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Kazyx.Uwpmm.Utility
{
    public static class EventHandlerExtensions
    {
        public static void Raise<T>(this EventHandler<T> @event, object sender, T e) where T : EventArgs
        {
            if (@event != null)
            {
                @event(sender, e);
            }
        }

        public static void Raise(this EventHandler @event, object sender, EventArgs e)
        {
            if (@event != null)
            {
                @event(sender, e);
            }
        }

        public static void Raise(this PropertyChangedEventHandler @event, object sender, PropertyChangedEventArgs e)
        {
            if (@event != null)
            {
                @event(sender, e);
            }
        }

        public static void Raise(this NotifyCollectionChangedEventHandler @event, object sender, NotifyCollectionChangedEventArgs e)
        {
            if (@event != null)
            {
                @event(sender, e);
            }
        }
    }

    public static class ActionExtensions
    {
        public static void Raise(this Action action)
        {
            if (action != null)
            {
                action();
            }
        }

        public static void Raise<T>(this Action<T> action, T p)
        {
            if (action != null)
            {
                action(p);
            }
        }

        public static void Raise<T1, T2>(this Action<T1, T2> action, T1 p1, T2 p2)
        {
            if (action != null)
            {
                action(p1, p2);
            }
        }

        public static void Raise<T1, T2, T3>(this Action<T1, T2, T3> action, T1 p1, T2 p2, T3 p3)
        {
            if (action != null)
            {
                action(p1, p2, p3);
            }
        }
    }
}
