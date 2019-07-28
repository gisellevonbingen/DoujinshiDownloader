using Newtonsoft.Json.Linq;
using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.DoujinshiDownloader.Schedulers
{
    public class ImageViews : IEnumerable<ImageView>
    {
        private List<ImageView> Views;

        public ImageViews(IEnumerable<string> imageViewUrls)
        {
            this.Views = imageViewUrls.Select(v => new ImageView() { Url = v }).ToList();
        }

        public ImageView this[int index]
        {
            get
            {
                return this.Views[index];
            }

        }

        public int Count => this.Views.Count;

        public int CountState(ViewState flag)
        {
            int count = 0;

            foreach (var view in this.Views)
            {
                if (view.State.HasFlag(flag) == true)
                {
                    count++;
                }

            }

            return count;
        }

        public IEnumerator<ImageView> GetEnumerator()
        {
            return this.Views.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

    }

}
