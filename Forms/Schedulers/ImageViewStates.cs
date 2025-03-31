using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.DoujinshiDownloader.Doujinshi;

namespace Giselle.DoujinshiDownloader.Schedulers
{
    public class ImageViewStates : IEnumerable<ImageViewState>
    {
        private readonly List<ImageViewState> Views;

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

        public int IndexOf(ImageViewState state) => this.Views.IndexOf(state);

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
