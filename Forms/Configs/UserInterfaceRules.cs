using Giselle.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.DoujinshiDownloader.Configs
{
    public class UserInterfaceRules : IJsonObject
    {
        public bool ConfirmBeforeExitProgram { get; set; } = true;
        public bool ConfirmBeforeRemoveDownload { get; set; } = true;

        public UserInterfaceRules()
        {

        }

        public void Read(JToken json)
        {
            this.ConfirmBeforeExitProgram = json.Value<bool?>("ConfirmBeforeExitProgram") ?? this.ConfirmBeforeExitProgram;
            this.ConfirmBeforeRemoveDownload = json.Value<bool?>("ConfirmBeforeRemoveDownload") ?? this.ConfirmBeforeRemoveDownload;
        }

        public void Write(JToken json)
        {
            json["ConfirmBeforeExitProgram"] = this.ConfirmBeforeExitProgram;
            json["ConfirmBeforeRemoveDownload"] = this.ConfirmBeforeRemoveDownload;
        }

    }

}
