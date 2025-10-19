using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using HotKeyNew.Models;

namespace HotKeyNew
{
    public partial class MiniWindow : Window
    {
        // MainWindow에서 전달받은 F키 바인딩 목록
        private ObservableCollection<HotkeyImageBinding> _hotkeys;

        //일관성 없는 엑세스 
        public MiniWindow(ObservableCollection<HotkeyImageBinding> hotkeys)
        {
            InitializeComponent();
            _hotkeys = hotkeys;

            // UI에 F1~F12 목록 바인딩
            HotkeyItemsControl.ItemsSource = _hotkeys;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Activate();
            this.Focus();  // 창에 포커스 = 키 입력 가능
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Close();
                return;
            }

            if (e.Key >= Key.F1 && e.Key <= Key.F12)
            {
                // F1=0, F2=1, ..., F12=11 인덱스 계산 (-90)
                int keyIndex = e.Key - Key.F1;
                var binding = _hotkeys[keyIndex];

                if (binding.AssignedImage != null)
                {
                    CopyImageToClipboard(binding.AssignedImage.FilePath);
                    this.Close();
                }
                else
                {
                    MessageBox.Show(
                        $"{binding.KeyName}에 할당된 이미지가 없습니다.",
                        "알림",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }

                e.Handled = true;
            }
        }

        private void CopyImageToClipboard(string imagePath)
        {
            try
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.UriSource = new Uri(imagePath, UriKind.Absolute);
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.EndInit();
                image.Freeze();

                Clipboard.SetImage(image);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"클립보드 복사 실패: {ex.Message}",
                    "오류",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}