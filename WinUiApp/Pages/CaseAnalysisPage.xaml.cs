using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

using WinUiApp;

namespace WinUiApp.Pages
{
    public sealed partial class CaseAnalysisPage : Page
    {
        public CaseAnalysisPage()
        {
            InitializeComponent();
        }

        private void NavigationView_SelectionChanged(
            NavigationView sender,
            NavigationViewSelectionChangedEventArgs args)
        {
        }

        private void NavigationView_BackRequested(
            NavigationView sender,
            NavigationViewBackRequestedEventArgs args)
        {
            var window = App.MainWindowInstance as MainWindow;

            if (window != null)
            {
                window.RootFrameControl.Navigate(typeof(StartPage));
            }
        }
    }
}
