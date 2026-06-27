using CommunityToolkit.Mvvm.Input;
using StrikeBox.ViewModels;
using Wpf.Ui.Controls;

namespace StrikeBox.Views.Dialogs;

public partial class EditToolDialog : FluentWindow
{
    private readonly EditToolDialogViewModel _viewModel;

    public EditToolDialog(EditToolDialogViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
    }

    private async void SaveButton_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        if (_viewModel.SubmitCommand is IAsyncRelayCommand asyncCmd)
            await asyncCmd.ExecuteAsync(null);
        else
            _viewModel.SubmitCommand.Execute(null);

        if (_viewModel.Result != null)
        {
            DialogResult = true;
            Close();
        }
    }

    private void CancelButton_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        _viewModel.CancelCommand.Execute(null);
        DialogResult = false;
        Close();
    }
}
