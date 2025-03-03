using System.Windows;
using System.Windows.Controls;

namespace PowerBIOptimizer.UI.Controls
{
    public partial class QueryListControl : UserControl
    {
        public event EventHandler<object>? ItemSelected;
        public event EventHandler<object>? OptimizeRequested;
        public event EventHandler<object>? ViewDetailsRequested;
        public event EventHandler<object>? CopyRequested;

        public QueryListControl()
        {
            InitializeComponent();
        }

        public void SetItems(IEnumerable<object> items)
        {
            ItemsList.ItemsSource = items;
        }

        private void ItemsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ItemsList.SelectedItem != null)
            {
                ItemSelected?.Invoke(this, ItemsList.SelectedItem);
            }
        }

        private void Optimize_Click(object sender, RoutedEventArgs e)
        {
            if (ItemsList.SelectedItem != null)
            {
                OptimizeRequested?.Invoke(this, ItemsList.SelectedItem);
            }
        }

        private void ViewDetails_Click(object sender, RoutedEventArgs e)
        {
            if (ItemsList.SelectedItem != null)
            {
                ViewDetailsRequested?.Invoke(this, ItemsList.SelectedItem);
            }
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            if (ItemsList.SelectedItem != null)
            {
                CopyRequested?.Invoke(this, ItemsList.SelectedItem);
            }
        }

        public object? SelectedItem => ItemsList.SelectedItem;
    }
}