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

            // 페이지 처음 로드 시 "케이스 정보" 페이지 로드
            this.Loaded += ArtifactsAnalysisPage_Loaded;
        }

        // CaseImformation 페이지 기본 로드
        private void ArtifactsAnalysisPage_Loaded(object sender, RoutedEventArgs e)
        {
            NavigationViewItem? caseItem = null;

            foreach (var item in nvSample.MenuItems.OfType<NavigationViewItem>())
            {
                caseItem = FindNavigationViewItemByTagRecursive(item, "CaseImformation");
                if (caseItem != null)
                    break;
            }

            if (caseItem != null)
            {
                nvSample.SelectedItem = caseItem;
                contentFrame.Navigate(typeof(CaseImformation));
            }
        }

        // 네비게이션 내부 페이지 로드
        private void NavigationView_SelectionChanged(
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

        // 네비게이션 트리 내부 Tag 서치 메서드
        private NavigationViewItem? FindNavigationViewItemByTagRecursive(
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

        // 뒤로가기 버튼 호출
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
