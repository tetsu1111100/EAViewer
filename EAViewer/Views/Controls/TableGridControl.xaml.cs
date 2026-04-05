using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using EAViewer.ViewModels;

namespace EAViewer.Views.Controls;

public partial class TableGridControl : UserControl
{
    private bool _isDragging;
    private Point _dragStartPoint;
    private double _originX, _originY;

    // Predefined color palette for right-click menu
    private static readonly (string Name, string Rgb)[] ColorPalette = new[]
    {
        ("🔴 紅色", "rgb(255, 99, 99)"),
        ("🟠 橘色", "rgb(255, 165, 0)"),
        ("🟡 黃色", "rgb(255, 191, 0)"),
        ("🟢 綠色", "rgb(144, 238, 144)"),
        ("🔵 藍色", "rgb(135, 206, 250)"),
        ("🟣 紫色", "rgb(200, 162, 255)"),
        ("⚪ 灰色", "rgb(211, 211, 211)"),
        ("⬜ 清除", ""),
    };

    public TableGridControl()
    {
        InitializeComponent();
    }

    #region Title Bar Drag

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is not TableGridViewModel vm) return;

        vm.BringToFront();

        _isDragging = true;
        _dragStartPoint = e.GetPosition(this.Parent as UIElement);
        _originX = vm.CanvasX;
        _originY = vm.CanvasY;

        ((UIElement)sender).CaptureMouse();
        e.Handled = true;
    }

    private void TitleBar_MouseMove(object sender, MouseEventArgs e)
    {
        if (!_isDragging || DataContext is not TableGridViewModel vm) return;

        var currentPos = e.GetPosition(this.Parent as UIElement);
        double newX = _originX + (currentPos.X - _dragStartPoint.X);
        double newY = _originY + (currentPos.Y - _dragStartPoint.Y);

        // Prevent dragging out of the top/left bounds so the title bar isn't lost
        vm.CanvasX = System.Math.Max(0, newX);
        vm.CanvasY = System.Math.Max(0, newY);
    }

    private void TitleBar_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        _isDragging = false;
        ((UIElement)sender).ReleaseMouseCapture();
    }

    #endregion

    #region Resize Thumb Drag

    private void ResizeThumb_DragDelta(object sender, DragDeltaEventArgs e)
    {
        double newWidth = this.ActualWidth + e.HorizontalChange;
        double newHeight = this.ActualHeight + e.VerticalChange;
        
        if (newWidth > this.MinWidth)
            this.Width = newWidth;
            
        if (newHeight > 100)
            this.Height = newHeight;
    }

    #endregion

    #region Row Selection Fix

    private void Cell_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is Border border)
        {
            var row = FindParent<DataGridRow>(border);
            if (row != null && row.DataContext != null)
            {
                if (!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl))
                {
                    row.IsSelected = true;
                }
                else
                {
                    row.IsSelected = !row.IsSelected;
                    e.Handled = true;
                }
            }
        }
    }

    #endregion

    #region Column Header Click → Search Popup

    private void DetailGrid_Loaded(object sender, RoutedEventArgs e)
    {
        // Hook column header click events after grid is loaded
    }

    #endregion

    #region Right-Click Context Menu

    private void Cell_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is TextBlock tb)
        {
            // Select the row
            var row = FindParent<DataGridRow>(tb);
            if (row != null)
            {
                DetailGrid.SelectedItem = row.DataContext;
            }
        }
    }

    private void ContextMenu_Opened(object sender, RoutedEventArgs e)
    {
        if (sender is not ContextMenu menu) return;

        menu.Items.Clear();

        var targetColumn = menu.Tag?.ToString() ?? string.Empty;

        // Get the data context (the row)
        var textBlock = menu.PlacementTarget as TextBlock;
        var row = textBlock?.DataContext as TableDetailRow;
        if (row == null) return;

        // Get the cell text for copy
        string cellText = targetColumn switch
        {
            "ColName" => row.DisplayColName,
            "ColDesc" => row.ColDesc ?? string.Empty,
            "Remark" => row.Remark ?? string.Empty,
            _ => string.Empty
        };

        // Copy menu item
        var copyItem = new MenuItem
        {
            Header = "複製",
            Icon = new Wpf.Ui.Controls.SymbolIcon { Symbol = Wpf.Ui.Controls.SymbolRegular.Copy24 }
        };
        copyItem.Click += (s, args) =>
        {
            try { Clipboard.SetText(cellText); } catch { }
        };
        menu.Items.Add(copyItem);

        menu.Items.Add(new Separator());

        // Color palette
        var vm = DataContext as TableGridViewModel;
        foreach (var (name, rgb) in ColorPalette)
        {
            var colorItem = new MenuItem { Header = name };

            if (!string.IsNullOrEmpty(rgb))
            {
                var brush = TableGridViewModel.ParseRgbBrush(rgb);
                colorItem.Icon = new System.Windows.Shapes.Rectangle
                {
                    Width = 16,
                    Height = 16,
                    Fill = brush,
                    RadiusX = 3,
                    RadiusY = 3,
                    Stroke = new SolidColorBrush(Colors.Gray),
                    StrokeThickness = 0.5
                };
            }

            string capturedRgb = rgb;
            colorItem.Click += (s, args) =>
            {
                if (string.IsNullOrEmpty(capturedRgb))
                {
                    // Clear color
                    switch (targetColumn)
                    {
                        case "ColName": row.ColNameBackground = null; break;
                        case "ColDesc": row.ColDescBackground = null; break;
                        case "Remark": row.RemarkBackground = null; break;
                    }
                }
                else
                {
                    vm?.SetCellColor(row, targetColumn, capturedRgb);
                }
            };

            menu.Items.Add(colorItem);
        }
    }

    #endregion

    #region Column Header Click for Search

    protected override void OnInitialized(EventArgs e)
    {
        base.OnInitialized(e);
        DetailGrid.Loaded += (s, args) =>
        {
            // Attach click handlers to column headers
            foreach (var column in DetailGrid.Columns)
            {
                var header = GetColumnHeader(DetailGrid, column);
                if (header != null)
                {
                    header.PreviewMouseLeftButtonUp += ColumnHeader_Click;
                }
            }
        };
    }

    private void ColumnHeader_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is not DataGridColumnHeader header) return;
        if (DataContext is not TableGridViewModel vm) return;

        string columnName = header.Content?.ToString() ?? string.Empty;
        string targetProp = columnName switch
        {
            "欄位" => "ColName",
            "說明" => "ColDesc",
            "備註" => "Remark",
            _ => string.Empty
        };

        if (string.IsNullOrEmpty(targetProp)) return;

        var searchWindow = new ColumnSearchWindow(vm, targetProp, columnName, DetailGrid)
        {
            Owner = Window.GetWindow(this)
        };
        searchWindow.Show();
    }

    private static DataGridColumnHeader? GetColumnHeader(DataGrid grid, DataGridColumn column)
    {
        var headersPresenter = FindVisualChild<DataGridColumnHeadersPresenter>(grid);
        if (headersPresenter == null) return null;

        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(headersPresenter); i++)
        {
            var child = VisualTreeHelper.GetChild(headersPresenter, i);
            if (child is DataGridColumnHeader h && h.Column == column)
                return h;
        }
        return null;
    }

    #endregion

    #region Visual Tree Helpers

    private static T? FindParent<T>(DependencyObject child) where T : DependencyObject
    {
        var parent = VisualTreeHelper.GetParent(child);
        while (parent != null)
        {
            if (parent is T t) return t;
            parent = VisualTreeHelper.GetParent(parent);
        }
        return null;
    }

    private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T t) return t;
            var result = FindVisualChild<T>(child);
            if (result != null) return result;
        }
        return null;
    }

    #endregion
}
