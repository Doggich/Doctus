using System.Windows;
using System.Windows.Controls;

namespace MarkdownEditorDiploma
{
    public partial class MainWindow
    {
        private enum ViewMode { EditorOnly, PreviewOnly, Both }
        private ViewMode _currentViewMode = ViewMode.Both;

        private void UpdateViewLayout()
        {
            var dockPanel = this.Content as DockPanel;
            if (dockPanel == null) return;

            var mainGrid = dockPanel.Children[1] as Grid;
            if (mainGrid == null) return;

            switch (_currentViewMode)
            {
                case ViewMode.EditorOnly:
                    mainGrid.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
                    mainGrid.ColumnDefinitions[1].Width = new GridLength(0);
                    mainGrid.ColumnDefinitions[2].Width = new GridLength(0);
                    break;
                case ViewMode.PreviewOnly:
                    mainGrid.ColumnDefinitions[0].Width = new GridLength(0);
                    mainGrid.ColumnDefinitions[1].Width = new GridLength(0);
                    mainGrid.ColumnDefinitions[2].Width = new GridLength(1, GridUnitType.Star);
                    break;
                case ViewMode.Both:
                    mainGrid.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
                    mainGrid.ColumnDefinitions[1].Width = new GridLength(6);
                    mainGrid.ColumnDefinitions[2].Width = new GridLength(1, GridUnitType.Star);
                    break;
            }
        }

        private void SetViewMode(ViewMode mode)
        {
            _currentViewMode = mode;
            UpdateViewLayout();
            UpdateMenuCheckmarks();
        }

        private void UpdateMenuCheckmarks()
        {
            var menuBar = this.FindName("MenuBar") as Menu;
            if (menuBar == null) return;

            MenuItem viewMenu = null;
            foreach (var item in menuBar.Items)
            {
                var menuItem = item as MenuItem;
                if (menuItem != null && menuItem.Header.ToString() == "_Вид")
                {
                    viewMenu = menuItem;
                    break;
                }
            }

            if (viewMenu == null) return;

            foreach (MenuItem item in viewMenu.Items)
            {
                if (item.Header.ToString() == "Только редактор")
                    item.IsChecked = (_currentViewMode == ViewMode.EditorOnly);
                else if (item.Header.ToString() == "Только просмотр")
                    item.IsChecked = (_currentViewMode == ViewMode.PreviewOnly);
                else if (item.Header.ToString() == "Вместе")
                    item.IsChecked = (_currentViewMode == ViewMode.Both);
            }
        }

        private void ViewEditorOnly_Click(object sender, RoutedEventArgs e) => SetViewMode(ViewMode.EditorOnly);
        private void ViewPreviewOnly_Click(object sender, RoutedEventArgs e) => SetViewMode(ViewMode.PreviewOnly);
        private void ViewBoth_Click(object sender, RoutedEventArgs e) => SetViewMode(ViewMode.Both);
    }
}