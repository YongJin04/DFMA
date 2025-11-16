using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

using WinUiApp;
using WinUiApp.Pages.CaseAnalysis;

namespace WinUiApp.Pages
{
    public sealed partial class CaseAnalysisPage : Page
    {
        public CaseAnalysisPage()
        {
            this.InitializeComponent();

            // 페이지 처음 로드 시 "케이스 생성" 페이지 로드
            this.Loaded += CreateCasePage_Loaded;
        }

        // CreateCasePage 페이지 기본 로드
        private void CreateCasePage_Loaded(object sender, RoutedEventArgs e)
        {
            NavigationViewItem? caseItem = null;

            foreach (var item in nvSample.MenuItems.OfType<NavigationViewItem>())
            {
                // Tag 이름을 XAML과 동일하게 "CreateCasePage"
                caseItem = FindNavigationViewItemByTagRecursive(item, "CreateCasePage");
                if (caseItem != null)
                    break;
            }

            if (caseItem != null)
            {
                nvSample.SelectedItem = caseItem;
                contentFrame.Navigate(typeof(CreateCasePage));
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

            switch (tag)
            {
                case "CreateCasePage":
                    contentFrame.Navigate(typeof(CreateCasePage));
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
            if (App.MainWindowInstance is MainWindow window)
            {
                window.RootFrameControl.Navigate(typeof(StartPage));
            }
        }
    }
}
