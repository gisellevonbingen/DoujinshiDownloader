using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.DoujinshiDownloader.Configs;
using Giselle.Forms;

namespace Giselle.DoujinshiDownloader.Forms
{
    public class ImageConvertTypeItem : ComboBoxItemWrapper<ImageConvertType>
    {
        public static string GetText(ImageConvertType value)
        {
            return SR.Get($"ImageConvertType.{value.Name}") ?? value.Name;
        }

        public ImageConvertTypeItem(ImageConvertType value) : base(value, GetText(value))
        {

        }

    }

}
