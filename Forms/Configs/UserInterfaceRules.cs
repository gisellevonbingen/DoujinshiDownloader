using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.DoujinshiDownloader.Configs
{
    public class UserInterfaceRules
    {
        public bool ConfirmBeforeExitProgram { get; set; } = true;
        public bool ConfirmBeforeRemoveDownload { get; set; } = true;

        public UserInterfaceRules()
        {

        }

        public void Read(JToken jToken)
        {
            this.ConfirmBeforeExitProgram = jToken.Value<bool?>("ConfirmBeforeExitProgram") ?? this.ConfirmBeforeExitProgram;
            this.ConfirmBeforeRemoveDownload = jToken.Value<bool?>("ConfirmBeforeRemoveDownload") ?? this.ConfirmBeforeRemoveDownload;
        }

        public void Write(JToken jToken)
        {
            jToken["ConfirmBeforeExitProgram"] = this.ConfirmBeforeExitProgram;
            jToken["ConfirmBeforeRemoveDownload"] = this.ConfirmBeforeRemoveDownload;
        }

    }

}
