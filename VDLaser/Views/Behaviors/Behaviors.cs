using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace VDLaser.Views.Behaviors
{
    public static class DataGridAutoScrollBehavior
    {
        // -------------------------------
        // EnableAutoScroll
        // -------------------------------
        public static bool GetEnableAutoScroll(DependencyObject obj)
            => (bool)obj.GetValue(EnableAutoScrollProperty);

        public static void SetEnableAutoScroll(DependencyObject obj, bool value)
            => obj.SetValue(EnableAutoScrollProperty, value);

        public static readonly DependencyProperty EnableAutoScrollProperty =
            DependencyProperty.RegisterAttached(
                "EnableAutoScroll",
                typeof(bool),
                typeof(DataGridAutoScrollBehavior),
                new PropertyMetadata(false, OnEnableAutoScrollChanged));

        // -------------------------------
        // IsPaused
        // -------------------------------
        public static bool GetIsPaused(DependencyObject obj)
            => (bool)obj.GetValue(IsPausedProperty);

        public static void SetIsPaused(DependencyObject obj, bool value)
            => obj.SetValue(IsPausedProperty, value);

        public static readonly DependencyProperty IsPausedProperty =
            DependencyProperty.RegisterAttached(
                "IsPaused",
                typeof(bool),
                typeof(DataGridAutoScrollBehavior),
                new PropertyMetadata(false));

        private static void OnEnableAutoScrollChanged(
            DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DataGrid dataGrid)
                return;

            if ((bool)e.NewValue)
            {
                dataGrid.Loaded += DataGrid_Loaded;
                dataGrid.PreviewMouseWheel += OnUserScroll;
            }
            else
            {
                dataGrid.Loaded -= DataGrid_Loaded;
                dataGrid.PreviewMouseWheel -= OnUserScroll;
            }
        }

        private static void DataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is not DataGrid dataGrid)
                return;

            if (dataGrid.ItemsSource is INotifyCollectionChanged collection)
            {
                collection.CollectionChanged += (_, args) =>
                {
                    if (args.Action != NotifyCollectionChangedAction.Add)
                        return;

                    if (GetIsPaused(dataGrid))
                        return;

                    if (dataGrid.Items.Count > 0)
                    {
                        dataGrid.ScrollIntoView(
                            dataGrid.Items[dataGrid.Items.Count - 1]);
                    }
                };
            }
        }

        // Pause automatique si l’utilisateur scrolle
        private static void OnUserScroll(object sender, MouseWheelEventArgs e)
        {
            if (sender is DataGrid dataGrid)
            {
                SetIsPaused(dataGrid, true);
            }
        }
    }
}
