using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotKeyNew.Models
{
    class HotkeyImageBinding
    {
        public string KeyName { get; set; } //F1 F2 F3 ...
        public ImageItem AssignedImage { get; set; }
        public string ImageName => AssignedImage?.FileName;

    }
}
