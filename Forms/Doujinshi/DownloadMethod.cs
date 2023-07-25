using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Commons.Collections;
using Giselle.DoujinshiDownloader.Utils;

namespace Giselle.DoujinshiDownloader.Doujinshi
{
    public interface IDownloadMethod : INamed
    {
        List<IDownloadOption> Options { get; }
        Site Site { get; }
        GalleryAgent CreateAgent(DownloadInput downloadInput, WebRequestProvider webRequestProvider);
        void ApplyOptions(GalleryAgent agent, Dictionary<IDownloadOption, object> options);
    }

    public static class DownloadMethod
    {
        public static List<IDownloadMethod> Knowns { get; } = new List<IDownloadMethod>();
        public static IDownloadMethod Hitomi { get; } = Knowns.AddAndGet(new DownloadMethodHitomi("Hitomi"));
        public static IDownloadMethod E_Hentai { get; } = Knowns.AddAndGet(new DownloadMethodE_Hentai("E-Hentai"));
        public static IDownloadMethod ExHentai { get; } = Knowns.AddAndGet(new DownloadMethodExHentai("ExHentai"));
    }

    public abstract class DownloadMethod<AGENT> : IEquatable<DownloadMethod<AGENT>>, IDownloadMethod where AGENT : GalleryAgent
    {
        public string Name { get; }

        public List<IDownloadOption> Options { get; } = new List<IDownloadOption>();

        public DownloadMethod(string name)
        {
            this.Name = name;
        }

        public abstract Site Site { get; }

        public bool Equals(DownloadMethod<AGENT> other)
        {
            return this == other;
        }

        public abstract AGENT CreateAgent(DownloadInput downloadInput, WebRequestProvider webRequestProvider);

        GalleryAgent IDownloadMethod.CreateAgent(DownloadInput downloadInput, WebRequestProvider webRequestProvider) => this.CreateAgent(downloadInput, webRequestProvider);

        public virtual void ApplyOptions(AGENT agent, Dictionary<IDownloadOption, object> options)
        {

        }

        void IDownloadMethod.ApplyOptions(GalleryAgent agent, Dictionary<IDownloadOption, object> options) => this.ApplyOptions((AGENT)agent, options);
    }

}