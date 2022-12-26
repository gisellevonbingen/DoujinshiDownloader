using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Commons.Collections;
using Giselle.Commons.Tags;

namespace Giselle.DoujinshiDownloader.Hooks
{
    public class Hook : INameTag
    {
        public static SimpleNameTags<Hook> Registry { get; } = new SimpleNameTags<Hook>();
        public static Hook DownloadStart { get; } = Registry.Register(new Hook(HookCategory.Download, "Download.Start"));
        public static Hook DownloadCancelled { get; } = Registry.Register(new Hook(HookCategory.Download, "Download.Cancelled"));
        public static Hook DownloadExcepted { get; } = Registry.Register(new Hook(HookCategory.Download, "Download.Excepted"));
        public static Hook DownloadSuccess { get; } = Registry.Register(new Hook(HookCategory.Download, "Download.Success"));
        public static Hook DownloadComplete { get; } = Registry.Register(new Hook(HookCategory.Download, "Download.Complete"));

        public HookCategory Category { get; }
        public string Name { get; }

        public Hook(HookCategory category, string name)
        {
            this.Category = category;
            this.Name = name;
        }

    }

}
