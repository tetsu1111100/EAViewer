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

    private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        // Fix 2: Focus search box when clicking on empty canvas area
        SearchBox.Focus();
    }
}
