using Giselle.DoujinshiDownloader.Doujinshi;
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
    public class ImageViewStates : IEnumerable<ImageViewState>
    {
        private List<ImageViewState> Views;

        public ImageViewStates(IEnumerable<GalleryImageView> imageViewUrls)
        {
            this.Views = imageViewUrls.Select(v => new ImageViewState() { View = v }).ToList();
        }

        public ImageViewState this[int index]
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
                if (flag.HasFlag(view.State) == true)
                {
                    count++;
                }

            }

            return count;
        }

        public IEnumerator<ImageViewState> GetEnumerator()
        {
            return this.Views.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

    }

}
