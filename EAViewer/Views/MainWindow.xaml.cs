using System.Windows;
using System.Windows.Input;
using EAViewer.Core.Services;
using EAViewer.ViewModels;

namespace EAViewer.Views;

public partial class MainWindow : Wpf.Ui.Controls.FluentWindow
{
    public MainWindow(TableService tableService)
    {
        DataContext = new MainWindowViewModel(tableService);
        InitializeComponent();
    }

    private void SearchBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            var vm = DataContext as MainWindowViewModel;
            if (vm?.SearchCommand.CanExecute(null) == true)
            {
                vm.SearchCommand.Execute(null);
            }
        }
    }

    private Point? _lastDragPoint;

    private void Canvas_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        var originalSource = e.OriginalSource as DependencyObject;
        bool isTableGrid = IsInControl<Controls.TableGridControl>(originalSource);
        bool isScrollBar = IsInControl<System.Windows.Controls.Primitives.ScrollBar>(originalSource);

        if (!isTableGrid && !isScrollBar)
        {
            // Focus SearchBox (use Dispatcher to ensure it runs after UI layout/events complete)
            Application.Current.Dispatcher.BeginInvoke(new Action(() => 
            {
                SearchBox.Focus();
                Keyboard.Focus(SearchBox);
            }), System.Windows.Threading.DispatcherPriority.Input);

            // Start dragging the canvas when clicking on blank area
            _lastDragPoint = e.GetPosition(CanvasScrollViewer);
            CanvasScrollViewer.CaptureMouse();
            CanvasScrollViewer.Cursor = Cursors.ScrollAll; // Changed to dragging icon
            e.Handled = true;
        }
    }

    private void Canvas_PreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (_lastDragPoint.HasValue && CanvasScrollViewer.IsMouseCaptured)
        {
            Point posOnScrollViewer = e.GetPosition(CanvasScrollViewer);
            
            double deltaX = posOnScrollViewer.X - _lastDragPoint.Value.X;
            double deltaY = posOnScrollViewer.Y - _lastDragPoint.Value.Y;

            CanvasScrollViewer.ScrollToHorizontalOffset(CanvasScrollViewer.HorizontalOffset - deltaX);
            CanvasScrollViewer.ScrollToVerticalOffset(CanvasScrollViewer.VerticalOffset - deltaY);

            _lastDragPoint = posOnScrollViewer;
        }
    }

    private void Canvas_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (CanvasScrollViewer.IsMouseCaptured)
        {
            CanvasScrollViewer.ReleaseMouseCapture();
            CanvasScrollViewer.Cursor = Cursors.Arrow; // Reset cursor
            _lastDragPoint = null;
        }
    }

    private static bool IsInControl<T>(DependencyObject? current) where T : DependencyObject
    {
        while (current != null)
        {
            if (current is T)
                return true;
            
            if (current is System.Windows.Media.Visual or System.Windows.Media.Media3D.Visual3D)
            {
                current = System.Windows.Media.VisualTreeHelper.GetParent(current) ?? System.Windows.LogicalTreeHelper.GetParent(current);
            }
            else
            {
                current = System.Windows.LogicalTreeHelper.GetParent(current);
            }
        }
        return false;
    }
}
