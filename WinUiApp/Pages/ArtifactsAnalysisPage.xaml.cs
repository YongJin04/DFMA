using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

using WinUiApp.Pages.ArtifactsAnalysis;

namespace WinUiApp.Pages
{
    public sealed partial class ArtifactsAnalysisPage : Page
    {
        public ArtifactsAnalysisPage()
        {
            this.InitializeComponent();
        }


        private void NavigationView_SelectionChanged(    // 네비게이션 내부 페이지 로드
            NavigationView sender,
            NavigationViewSelectionChangedEventArgs args)
        {
            var selectedItem = args.SelectedItemContainer as NavigationViewItem;
            if (selectedItem?.Tag is not string tag)
            {
                return;
            }

            // Tag 별 페이지 로드
            switch (tag)
            {
                case "CaseImformation":
                    contentFrame.Navigate(typeof(CaseImformation));
                    break;

                default:
                    contentFrame.Content = null;
                    break;
            }
        }

        private NavigationViewItem? FindNavigationViewItemByTagRecursive(  // 네비게이션 트리 내부 Tag 서치 메서드
            NavigationViewItem parent,
            string tag)
        {
            if (parent.Tag is string current && current == tag)
                return parent;

            foreach (var child in parent.MenuItems.OfType<NavigationViewItem>())
            {
                var found = FindNavigationViewItemByTagRecursive(child, tag);
                if (found != null)
                    return found;
            }

            return null;
        }

        private void NavigationView_BackRequested(  // 뒤로가기 버튼 호출
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
