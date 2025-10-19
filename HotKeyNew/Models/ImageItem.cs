using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace HotKeyNew.Models
{
    class ImageItem
    {
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public BitmapImage ImageSource { get; set; }
    }
}
