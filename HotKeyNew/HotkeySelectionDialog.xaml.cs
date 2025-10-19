using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace HotKeyNew
{
    /// <summary>
    /// HotkeySelectionDialog.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class HotkeySelectionDialog : Window
    {
        // 선택된 F키 인덱스 (0=F1, 1=F2, ..., 11=F12)
        public int? SelectedKeyIndex { get; private set; }

        public HotkeySelectionDialog()
        {
            InitializeComponent();
        }

        private void FKeyButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string tagStr)
            {
                SelectedKeyIndex = int.Parse(tagStr);
                DialogResult = true; // 성공적으로 선택됨
                Close();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedKeyIndex = null; // 취소
            DialogResult = false;
            Close();
        }
    }
}
