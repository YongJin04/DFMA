using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Windows.Storage.Pickers;
using WinRT.Interop;

using WinUiApp.Interop;

namespace WinUiApp.Pages.ArtifactsAnalysis
{
    public sealed partial class CaseImformation : Page
    {
        //  페이지 간 이동 후에도 유지할 static 상태
        private static string? _savedCaseRoot;
        private static string? _savedCaseName;
        private static string? _savedCaseCreateTime;
        private static string? _savedTimezone;
        private static string? _savedToolVersion;

        private bool _isLoading = false;

        // 현재 케이스 루트 폴더 (예: C:\...\...\Cases\DFMA-Case-001)
        private string? _currentCaseRoot;

        public CaseImformation()
        {
            this.InitializeComponent();

            // 이전에 보던 내용이 있으면 복원
            _isLoading = true;
            LoadSavedState();
            _isLoading = false;
        }

        // Navigation 시 전달되는 파라미터 처리
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var caseRoot = e.Parameter as string;

            if (!string.IsNullOrEmpty(caseRoot) && Directory.Exists(caseRoot))
            {
                // EvidenceProcess 등에서 케이스 절대 경로를 넘겨 받은 경우
                _currentCaseRoot = caseRoot;
                CaseFolderPathTextBox.Text = caseRoot;

                // EvidenceProcess 에서 온 경우에는 "찾아보기..." 비활성화
                BrowseCaseFolderButton.IsEnabled = false;

                _ = LoadCaseInfoFromCurrentRootAsync();
            }
        }

        //  상태 저장 / 불러오기 (EvidenceProcess 패턴 참고)
        private void SaveState()
        {
            if (_isLoading) return;

            _savedCaseRoot = CaseFolderPathTextBox.Text;
            _savedCaseName = CaseNameTextBox.Text;
            _savedCaseCreateTime = CaseCreateTimeTextBox.Text;
            _savedTimezone = TimezoneTextBox.Text;
            _savedToolVersion = ToolVersionTextBox.Text;
        }

        private void LoadSavedState()
        {
            CaseFolderPathTextBox.Text = _savedCaseRoot ?? string.Empty;
            CaseNameTextBox.Text = _savedCaseName ?? string.Empty;
            CaseCreateTimeTextBox.Text = _savedCaseCreateTime ?? string.Empty;
            TimezoneTextBox.Text = _savedTimezone ?? string.Empty;
            ToolVersionTextBox.Text = _savedToolVersion ?? string.Empty;
        }

        // UI 필드 초기화
        private void ClearCaseInfoFields()
        {
            if (string.IsNullOrEmpty(_currentCaseRoot))
                CaseFolderPathTextBox.Text = string.Empty;

            CaseNameTextBox.Text = string.Empty;
            CaseCreateTimeTextBox.Text = string.Empty;
            TimezoneTextBox.Text = string.Empty;
            ToolVersionTextBox.Text = string.Empty;

            SaveState();
        }

        private async Task ShowMessageAsync(string title, string content)
        {
            var dialog = new ContentDialog
            {
                XamlRoot = this.XamlRoot,
                Title = title,
                Content = content,
                CloseButtonText = "확인"
            };
            await dialog.ShowAsync();
        }

        //  "찾아보기..." 버튼 (시작 페이지에서 들어온 경우 사용)
        private async void BrowseCaseFolderButton_Click(object sender, RoutedEventArgs e)
        {
            var picker = new FileOpenPicker();
            var hwnd = WindowNative.GetWindowHandle(App.MainWindowInstance);
            InitializeWithWindow.Initialize(picker, hwnd);

            // 케이스 DB 선택
            picker.FileTypeFilter.Add(".dfmadb");

            var file = await picker.PickSingleFileAsync();
            if (file == null)
                return;

            var caseRoot = Path.GetDirectoryName(file.Path);
            if (string.IsNullOrEmpty(caseRoot))
            {
                await ShowMessageAsync("경로 오류", "선택한 케이스 파일의 폴더 경로를 확인할 수 없습니다.");
                return;
            }

            _currentCaseRoot = caseRoot;
            CaseFolderPathTextBox.Text = caseRoot;

            await LoadCaseInfoFromCurrentRootAsync();
        }

        //  케이스 정보 로드 (case_info 테이블만)
        private async Task LoadCaseInfoFromCurrentRootAsync()
        {
            if (string.IsNullOrEmpty(_currentCaseRoot))
            {
                ClearCaseInfoFields();
                return;
            }

            string dbPath = Path.Combine(_currentCaseRoot, "DFMA-Case.dfmadb");
            if (!File.Exists(dbPath))
            {
                ClearCaseInfoFields();
                await ShowMessageAsync("DB 없음", $"케이스 DB 파일을 찾을 수 없습니다.\n경로: {dbPath}");
                return;
            }

            try
            {
                // sqlite3.dll 로드 (이미 로드되어 있으면 내부에서 무시)
                try
                {
                    NativeDllManager.LoadNativeLibrary("sqlite3.dll", @"dll");
                }
                catch
                {
                    // 이미 로드된 경우 등은 무시
                }

                IntPtr db;
                int flags = NativeSqliteHelper.SQLITE_OPEN_READWRITE;
                int rc = NativeSqliteHelper.sqlite3_open_v2(dbPath, out db, flags, null);

                if (rc != NativeSqliteHelper.SQLITE_OK)
                {
                    ClearCaseInfoFields();
                    await ShowMessageAsync("DB 열기 실패",
                        $"데이터베이스를 열 수 없습니다.\n경로: {dbPath}\nrc={rc}");
                    return;
                }

                try
                {
                    // case_info 테이블 읽기
                    var info = SelectAllCaseInfo(db);

                    info.TryGetValue("CaseName", out var caseName);
                    info.TryGetValue("CaseCreateTime", out var caseCreateTime);
                    info.TryGetValue("Timezone", out var timezone);
                    info.TryGetValue("ToolVersion", out var toolVersion);

                    CaseNameTextBox.Text = caseName ?? string.Empty;
                    CaseCreateTimeTextBox.Text = caseCreateTime ?? string.Empty;
                    TimezoneTextBox.Text = timezone ?? string.Empty;
                    ToolVersionTextBox.Text = toolVersion ?? string.Empty;

                    // 현재 UI 상태를 static에 저장
                    SaveState();
                }
                finally
                {
                    NativeSqliteHelper.sqlite3_close(db);
                }
            }
            catch (Exception ex)
            {
                ClearCaseInfoFields();
                await ShowMessageAsync(
                    "케이스 정보 로드 오류",
                    $"케이스 정보를 읽는 중 오류가 발생했습니다.\n{ex.Message}");
            }
        }

        //  SQLite sqlite3_exec P/Invoke 및 콜백
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int ExecCallback(
            IntPtr arg,
            int columnCount,
            IntPtr columnValues,
            IntPtr columnNames);

        [DllImport("sqlite3", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern int sqlite3_exec(
            IntPtr db,
            string sql,
            ExecCallback callback,
            IntPtr arg,
            out IntPtr errMsg);

        // case_info 테이블: key, value (TEXT)
        private Dictionary<string, string> SelectAllCaseInfo(IntPtr db)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            ExecCallback callback = (arg, columnCount, columnValues, columnNames) =>
            {
                var namePtrs = new IntPtr[columnCount];
                var valuePtrs = new IntPtr[columnCount];

                Marshal.Copy(columnNames, namePtrs, 0, columnCount);
                Marshal.Copy(columnValues, valuePtrs, 0, columnCount);

                string? key = null;
                string? value = null;

                for (int i = 0; i < columnCount; i++)
                {
                    string colName = Marshal.PtrToStringAnsi(namePtrs[i]) ?? string.Empty;
                    string colVal = valuePtrs[i] == IntPtr.Zero
                        ? string.Empty
                        : (Marshal.PtrToStringAnsi(valuePtrs[i]) ?? string.Empty);

                    if (colName.Equals("key", StringComparison.OrdinalIgnoreCase))
                        key = colVal;
                    else if (colName.Equals("value", StringComparison.OrdinalIgnoreCase))
                        value = colVal;
                }

                if (!string.IsNullOrEmpty(key))
                    result[key!] = value ?? string.Empty;

                return 0;
            };

            IntPtr errPtr;
            int rc = sqlite3_exec(
                db,
                "SELECT key, value FROM case_info;",
                callback,
                IntPtr.Zero,
                out errPtr);

            if (rc != NativeSqliteHelper.SQLITE_OK)
            {
                string message = $"SQLite SELECT 오류 (case_info, rc={rc})";
                if (errPtr != IntPtr.Zero)
                {
                    message += ": " + Marshal.PtrToStringAnsi(errPtr);
                    NativeSqliteHelper.sqlite3_free(errPtr);
                }
                throw new InvalidOperationException(message);
            }

            return result;
        }
    }
}
