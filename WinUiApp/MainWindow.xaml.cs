using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.IO;
using Windows.ApplicationModel;

namespace WinUiApp
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
            SetWindowIcon();

            var v = Package.Current.Id.Version;
            string versionString = $"{v.Major}.{v.Minor}.{v.Build}.{v.Revision}";
            this.AppWindow.Title = $"DF_m@ster v{versionString}";

            RootFrame.PointerPressed += MainWindow_PointerPressed;  // 마우스 핸들링
        }

        public Frame RootFrameControl => RootFrame;

        private void SetWindowIcon()  // 아이콘 설정
        {
            string iconPath = Path.Combine(
                AppContext.BaseDirectory,
                "Assets\\Icons",
                "DF_m@ster.ico");

            this.AppWindow.SetIcon(iconPath);

            // 시작 페이지 로드
            RootFrame.Navigate(typeof(Pages.StartPage));
        }

        private void MainWindow_PointerPressed(object sender, PointerRoutedEventArgs e)  // 마우스 핸들링
        {
            var props = e.GetCurrentPoint(null).Properties;

            if (props.IsXButton1Pressed)  // 마우스 "뒤로 가기" 핸들링
            {
                if (RootFrame.CanGoBack)
                {
                    RootFrame.GoBack();
                    e.Handled = true;
                }
            }

            else if (props.IsXButton2Pressed)  // 마우스 "앞으로 가기" 핸들링
            {
                if (RootFrame.CanGoForward)
                {
                    RootFrame.GoForward();
                    e.Handled = true;
                }
            }
        }
    }
}
