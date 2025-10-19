using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using HotKeyNew.Models;
using Microsoft.Win32;
using HotKeyNew.Services;
using System.Windows.Interop;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace HotKeyNew
{
    public partial class MainWindow : Window
    {
        //전역변수에는 _를 붙이는것이 코드관리에 좋은듯 하다..? 검색필요
        //private 필드는 _로 시작하는것이 일반적
        //-> private로 변경
        private ObservableCollection<ImageItem> _images = new ObservableCollection<ImageItem>(); 
        private ObservableCollection<HotkeyImageBinding> _hotkeys = new ObservableCollection<HotkeyImageBinding>();
        private HotKeyManager _hotkeyManager;

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out MOUSE_POINT lpPoint);

        [StructLayout(LayoutKind.Sequential)]
        private struct MOUSE_POINT
        {
            public int X;
            public int Y;
        }

        private bool _isRealClose = false;
        public MainWindow()
        {
            InitializeComponent();
            ImagePreviewItemsControl.ItemsSource = _images;
            HotkeyAssignmentItemsControl.ItemsSource = _hotkeys;
            for (int i = 1; i <= 12; i++)
            {
                _hotkeys.Add(new HotkeyImageBinding { KeyName = "F" + i });
                
            }
            _hotkeyManager = new HotKeyManager();
            _hotkeyManager.HotKeyPressed += OnGlobalHotkeyPressed;
        }

        private void LoadImagesFromFolder(string folderPath)
        {
            //폴더에서 이미지 불러오는 로직 작성
            _images.Clear();
            var imageExtensions = new[] { ".png", ".jpg", ".jpeg", ".bmp", ".gif" };
            var files = Directory.GetFiles(folderPath)
                .Where(f => imageExtensions.Contains(Path.GetExtension(f).ToLower()));

            foreach (var filePath in files)
            {
                var thumbnail = new BitmapImage();
                thumbnail.BeginInit();
                thumbnail.UriSource = new System.Uri(filePath);
                thumbnail.DecodePixelWidth = 100;
                thumbnail.CacheOption = BitmapCacheOption.OnLoad;
                thumbnail.EndInit();
                thumbnail.Freeze(); //구글링 결과 다른 스레드에서 접근할수 있게 해주는듯
                _images.Add(new ImageItem
                {
                    FilePath = filePath,
                    FileName = Path.GetFileName(filePath),
                    ImageSource = thumbnail
                });
            }
            StatusText.Text = $"{_images.Count}개의 이미지를 불러옴.";
        }

        private void SelectFolderButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog();
            if (dialog.ShowDialog() == true)
            {
                string folderPath = dialog.FolderName;
                SelectedFolderText.Text = folderPath;
                LoadImagesFromFolder(folderPath);
            }

        }
        private void RemoveHotkeyButton_Click(object sender, RoutedEventArgs e)
        {
            // 버튼의 Tag에 HotkeyImageBinding이 바인딩되어 있음
            if (sender is Button btn && btn.Tag is HotkeyImageBinding binding)
            {
                string keyName = binding.KeyName;
                binding.AssignedImage = null; // 할당 해제
                HotkeyAssignmentItemsControl.Items.Refresh(); // UI 갱신
                StatusText.Text = $"{keyName} 할당이 제거되었습니다.";
            }
        }
        private void AssignHotkeyButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is ImageItem selectedImage)
            {
                // F키 선택 다이얼로그 띄우기
                var dialog = new HotkeySelectionDialog();
                dialog.Owner = this; // 부모 창 설정 (가운데에 띄우기 위한)

                if (dialog.ShowDialog() == true)
                {
                    int keyIndex = dialog.SelectedKeyIndex.Value;
                    var targetSlot = _hotkeys[keyIndex];

                    // 이미 사진이 할당됐을때의 조건
                    if (targetSlot.AssignedImage != null)
                    {
                        var result = MessageBox.Show(
                            $"{targetSlot.KeyName}에 이미 '{targetSlot.ImageName}'이(가) 할당되어 있습니다.\n덮어쓰시겠습니까?",
                            "확인",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question);

                        if (result != MessageBoxResult.Yes)
                            return;
                    }

                    targetSlot.AssignedImage = selectedImage;
                    HotkeyAssignmentItemsControl.Items.Refresh();
                    StatusText.Text = $"{selectedImage.FileName}이(가) {targetSlot.KeyName}에 할당되었습니다.";
                }
            }
        }
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            var helper = new WindowInteropHelper(this);
            if (!_hotkeyManager.Register(helper.Handle))
            {
                MessageBox.Show("단축키 등록에 실패했습니다. \n다른프로그램이 사용 중일 수 있습니다.", 
                    "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                StatusText.Text = "글로벌 단축키가 등록되었습니다. (Alt + Shift + I)";
            }
        }
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _hotkeyManager?.Unregister();
        }
        private void OnGlobalHotkeyPressed(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                ShowMiniWindow();
            });
        }

        private void ShowMiniWindow()
        {
            var miniWindow = new MiniWindow(_hotkeys);
            GetCursorPos(out MOUSE_POINT mousePoint);
            var screen = System.Windows.Forms.Screen.FromPoint
                (new System.Drawing.Point(mousePoint.X, mousePoint.Y));
            miniWindow.WindowStartupLocation = WindowStartupLocation.Manual;
            miniWindow.Left = (double)screen.WorkingArea.Left + (screen.WorkingArea.Width - miniWindow.Width) / 2;
            miniWindow.Top = (double)screen.WorkingArea.Top + (screen.WorkingArea.Height - miniWindow.Height) / 2;
            miniWindow.Owner = this;
            miniWindow.ShowDialog();
        }

        //트레이로 숨김
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (!_isRealClose)
            {
                e.Cancel = true;  // 닫기 취소
                this.Hide();
                TrayIcon.Visibility = Visibility.Visible;

                // 우측 하단 Toast? 알림
                TrayIcon.ShowBalloonTip("Image Hotkey",
                    "트레이로 최소화되었습니다.\nAlt+Shift+I는 계속 사용 가능합니다.",
                    Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Info);
            }
        }

        //창 복원
        private void TrayIcon_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            ShowMainWindow();
        }

        // 트레이메뉴 - 열기
        private void TrayOpen_Click(object sender, RoutedEventArgs e)
        {
            ShowMainWindow();
        }

        // 트레이메뉴 - 종료
        private void TrayExit_Click(object sender, RoutedEventArgs e)
        {
            _isRealClose = true;
            Application.Current.Shutdown();
        }

        // 트레이창 복원
        private void ShowMainWindow()
        {
            this.Show();
            this.WindowState = WindowState.Normal;
            this.Activate();
            TrayIcon.Visibility = Visibility.Collapsed;
        }
    }
}