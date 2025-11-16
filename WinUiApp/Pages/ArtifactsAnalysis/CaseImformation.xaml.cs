using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinUiApp.Interop;

namespace WinUiApp.Pages.ArtifactsAnalysis
{
    public sealed partial class CaseImformation : Page
    {
        public CaseImformation()
        {
            this.InitializeComponent();
        }

        private void LibewfLoadButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // dll\EwfTools 폴더 기준으로 libewf.dll을 찾고 로드
                var resolvedPath = NativeDllManager.LoadNativeLibrary(
                    "libewf.dll",
                    @"dll\EwfTools");

                // 로드된 DLL 내부 함수 호출
                var version = NativeDllManager.GetFileVersionFromPath(resolvedPath);

                if (string.IsNullOrWhiteSpace(version))
                {
                    LibewfStatusTextBlock.Text =
                        $"libewf.dll 로드 성공\r\n" +
                        $"파일 버전 정보를 가져올 수 없습니다.";
                }
                else
                {
                    LibewfStatusTextBlock.Text =
                        $"libewf.dll 로드 성공\r\n" +
                        $"파일 버전: {version}";
                }
            }
            catch (NativeDllManager.NativeDllLoadException ex)
            {
                LibewfStatusTextBlock.Text =
                    "libewf.dll 로드 실패\r\n" +
                    ex.Message;
            }
            catch (Exception ex)
            {
                LibewfStatusTextBlock.Text =
                    "libewf.dll 로드 중 예기치 못한 오류가 발생했습니다.\r\n" +
                    ex.Message;
            }
        }
    }
}
