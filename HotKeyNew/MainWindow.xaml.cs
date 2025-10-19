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

namespace HotKeyNew
{
    public partial class MainWindow : Window
    {
        //전역변수에는 _를 붙이는것이 코드관리에 좋은듯 하다..? 검색필요
        //private 필드는 _로 시작하는것이 일반적
        //-> private로 변경
        private ObservableCollection<ImageItem> _images = new ObservableCollection<ImageItem>(); 
        private ObservableCollection<HotkeyImageBinding> _hotkeys = new ObservableCollection<HotkeyImageBinding>();
        public MainWindow()
        {
            InitializeComponent();
            ImagePreviewItemsControl.ItemsSource = _images;
            HotkeyAssignmentItemsControl.ItemsSource = _hotkeys;
            for (int i = 1; i <= 12; i++)
            {
                _hotkeys.Add(new HotkeyImageBinding { KeyName = "F" + i });
                
            }
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
    }
}