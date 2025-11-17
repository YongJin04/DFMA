using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Pickers;
using WinRT.Interop;
using WinUiApp.Pages.ArtifactsAnalysis;

namespace WinUiApp.Pages.CaseAnalysis.EvidenceSource
{
    public sealed partial class StaticImage : Page
    {
        // 페이지 이동 후에도 값 유지용 static 상태
        private static string? _savedImagePath;

        // 동적으로 만드는 증거 칸 번호용
        private int _evidenceIndex = 1;

        public StaticImage()
        {
            this.InitializeComponent();
            LoadSavedState();
            HookTextChangedForState();
        }

        // 저장 상태 불러오기
        private void LoadSavedState()
        {
            if (_savedImagePath is null)
            {
                CaseFolderPathTextBox.Text = string.Empty;
            }
            else
            {
                CaseFolderPathTextBox.Text = _savedImagePath;
            }
        }

        // TextBox 변경 시 자동 저장
        private void HookTextChangedForState()
        {
            CaseFolderPathTextBox.TextChanged += (_, __) => SaveState();
        }

        private void SaveState()
        {
            _savedImagePath = CaseFolderPathTextBox.Text;
        }

        private void DeleteEvidenceButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                if (button.Tag is Grid rowGrid)
                {
                    // 동적으로 추가된 줄 삭제
                    EvidenceStackPanel.Children.Remove(rowGrid);
                }
                else
                {
                    // 첫 번째(기본) 칸 삭제 버튼은 자기 줄(Grid)의 Parent 제거
                    if (button.Parent is Grid firstRow)
                    {
                        EvidenceStackPanel.Children.Remove(firstRow);
                    }
                }
            }
        }

        // "증거 추가" 버튼 클릭
        private void AddEvidenceButton_Click(object sender, RoutedEventArgs e)
        {
            var rowGrid = new Grid();
            rowGrid.Margin = new Thickness(0, 8, 0, 0);

            rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var textBox = new TextBox();
            textBox.Name = $"CaseFolderPathTextBox_{_evidenceIndex}";
            textBox.IsReadOnly = true;
            Grid.SetColumn(textBox, 0);

            var browseButton = new Button();
            browseButton.Content = "찾아보기...";
            browseButton.Margin = new Thickness(8, 0, 0, 0);
            browseButton.VerticalAlignment = VerticalAlignment.Center;
            browseButton.Click += BrowseFolderButton_Click;
            browseButton.Tag = textBox;
            Grid.SetColumn(browseButton, 1);

            // 삭제 버튼
            var deleteButton = new Button();
            deleteButton.Content = "삭제";
            deleteButton.Margin = new Thickness(8, 0, 0, 0);
            deleteButton.VerticalAlignment = VerticalAlignment.Center;
            deleteButton.Click += DeleteEvidenceButton_Click;
            deleteButton.Tag = rowGrid;
            Grid.SetColumn(deleteButton, 2);

            rowGrid.Children.Add(textBox);
            rowGrid.Children.Add(browseButton);
            rowGrid.Children.Add(deleteButton);

            EvidenceStackPanel.Children.Add(rowGrid);

            _evidenceIndex++;
        }


        // 이미지 파일 찾아보기 버튼
        private async void BrowseFolderButton_Click(object sender, RoutedEventArgs e)
        {
            var picker = new FileOpenPicker();
            var hwnd = WindowNative.GetWindowHandle(App.MainWindowInstance);
            InitializeWithWindow.Initialize(picker, hwnd);

            picker.FileTypeFilter.Add(".e01");
            picker.FileTypeFilter.Add(".001");
            picker.FileTypeFilter.Add(".dd");
            picker.FileTypeFilter.Add(".raw");
            picker.FileTypeFilter.Add(".img");

            var file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                // 수정: 어떤 TextBox에 넣을지 결정
                TextBox targetTextBox = CaseFolderPathTextBox;

                if (sender is Button button && button.Tag is TextBox taggedTextBox)
                {
                    // 동적으로 만든 칸 또는 XAML에서 Tag로 묶어둔 칸
                    targetTextBox = taggedTextBox;
                }

                targetTextBox.Text = file.Path;

                // 상태 저장은 기존처럼 첫 번째 칸만
                if (ReferenceEquals(targetTextBox, CaseFolderPathTextBox))
                {
                    SaveState();
                }
            }
        }

        // "아티팩트 분석" 버튼
        private async void EvidenceProcess_Button_Click(object sender, RoutedEventArgs e)
        {
            var imagePath = CaseFolderPathTextBox.Text?.Trim();

            if (string.IsNullOrEmpty(imagePath))
            {
                var dialog = new ContentDialog
                {
                    XamlRoot = this.XamlRoot,
                    Title = "이미지 파일 누락",
                    Content = "분석할 디스크 이미지 파일을 선택해주세요.",
                    CloseButtonText = "확인"
                };
                await dialog.ShowAsync();
                return;
            }

            if (App.MainWindowInstance is MainWindow window)
            {
                window.RootFrameControl.Navigate(typeof(WinUiApp.Pages.CaseAnalysisPage), "ArtifactsProcess");

            }
        }
    }
}
