﻿#region Copyright information
// <copyright file="ParentChangedNotifierHelper.cs">
//     Licensed under Microsoft Public License (Ms-PL)
//     http://wpflocalizeextension.codeplex.com/license
// </copyright>
// <author>Uwe Mayer</author>
#endregion

#if SILVERLIGHT
namespace SLLocalizeExtension.Providers
#else
namespace WPFLocalizeExtension.Providers
#endif
{
    using System;
    using System.Windows;
    using System.Windows.Media;
    using System.Collections.Generic;
    using XAMLMarkupExtensions.Base;

    /// <summary>
    /// Extension methods for <see cref="DependencyObject"/> in conjunction with the <see cref="ParentChangedNotifier"/>.
    /// </summary>
    public static class ParentChangedNotifierHelper
    {
        /// <summary>
        /// Tries to get a value that is stored somewhere in the visual tree above this <see cref="DependencyObject"/>.
        /// If this is not available, it will register a <see cref="ParentChangedNotifier"/> on the last element.
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <param name="target">The <see cref="DependencyObject"/>.</param>
        /// <param name="GetFunction">The function that gets the value from a <see cref="DependencyObject"/>.</param>
        /// <param name="ParentChangedAction">The notification action on the change event of the Parent property.</param>
        /// <param name="parentNotifiers">A dictionary of already registered notifiers.</param>
        /// <returns>The value, if possible.</returns>
        public static T GetValueOrRegisterParentNotifier<T>(this DependencyObject target, Func<DependencyObject, T> GetFunction, Action<DependencyObject> ParentChangedAction, Dictionary<DependencyObject, ParentChangedNotifier> parentNotifiers)
        {
            var ret = default(T);

            if (target != null)
            {
                var depObj = target;

                while (ret == null)
                {
                    // Try to get the value using the provided GetFunction.
                    ret = GetFunction(depObj);

                    // Try to get the parent using the visual tree helper. This may fail on some occations.
                    DependencyObject depObjParent = null;
                    try { depObjParent = VisualTreeHelper.GetParent(depObj); }
                    catch { }
                    // If this failed, try again using the Parent property (sometimes this is not covered by the VisualTreeHelper class :-P.
                    if (depObjParent == null && depObj is FrameworkElement)
                        depObjParent = ((FrameworkElement)depObj).Parent;

                    if (ret == null && depObjParent == null)
                    {
                        // Try to establish a notification on changes of the Parent property of dp.
                        if (depObj is FrameworkElement && !parentNotifiers.ContainsKey(target))
                        {
                            parentNotifiers.Add(target, new ParentChangedNotifier((FrameworkElement)depObj, () =>
                            {
                                ParentChangedAction(target);
                            }));
                        }
                        break;
                    }

                    // Assign the parent to the current DependencyObject and start the next iteration.
                    depObj = depObjParent;
                }
            }

            return ret;
        }

        /// <summary>
        /// Tries to get a value from a <see cref="DependencyProperty"/> that is stored somewhere in the visual tree above this <see cref="DependencyObject"/>.
        /// If this is not available, it will register a <see cref="ParentChangedNotifier"/> on the last element.
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <param name="target">The <see cref="DependencyObject"/>.</param>
        /// <param name="property">A <see cref="DependencyProperty"/> that will be read out.</param>
        /// <param name="ParentChangedAction">The notification action on the change event of the Parent property.</param>
        /// <param name="parentNotifiers">A dictionary of already registered notifiers.</param>
        /// <returns>The value, if possible.</returns>
        public static T GetValueOrRegisterParentNotifier<T>(this DependencyObject target, DependencyProperty property, Action<DependencyObject> ParentChangedAction, Dictionary<DependencyObject, ParentChangedNotifier> parentNotifiers)
        {
            return target.GetValueOrRegisterParentNotifier<T>((depObj) =>
            {
                var value = depObj.GetValue(property);
                if (value is T)
                    return (T)value;
                return default(T);
            }, ParentChangedAction, parentNotifiers);
        }
    }
}
