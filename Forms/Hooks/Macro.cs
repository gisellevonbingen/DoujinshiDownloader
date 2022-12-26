using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Commons.Tags;

namespace Giselle.DoujinshiDownloader.Hooks
{
    public class Macro : INameTag
    {
        public static SimpleNameTags<Macro> Registry { get; } = new SimpleNameTags<Macro>();
        public static string PlaceholderPrefix = "${";
        public static string PlaceholderSuffix = "}";

        public static Macro ProgramDirectory { get; } = Registry.Register(new Macro(HookCategory.Download, "Program.Directory"));
        public static Macro DownloadDirectory { get; } = Registry.Register(new Macro(HookCategory.Download, "Download.Directory"));
        public static Macro DownloadPath { get; } = Registry.Register(new Macro(HookCategory.Download, "Download.Path"));
        public static Macro DownloadArchiveName { get; } = Registry.Register(new Macro(HookCategory.Download, "Download.ArchiveName"));
        public static Macro DownloadArchiveName2 { get; } = Registry.Register(new Macro(HookCategory.Download, "Download.ArchiveName2"));
        public static Macro DownloadResult { get; } = Registry.Register(new Macro(HookCategory.Download, "Download.Result"));

        public HookCategory Category { get; }
        public string Name { get; }

        public Macro(HookCategory category, string name)
        {
            this.Category = category;
            this.Name = name;
        }

        public override string ToString()
        {
            return this.Name;
        }

        public string ToPlaceHolder() => $"{PlaceholderPrefix}{this.Name}{PlaceholderSuffix}";

    }

}
