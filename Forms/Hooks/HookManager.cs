using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Giselle.DoujinshiDownloader.Schedulers;

namespace Giselle.DoujinshiDownloader.Hooks
{
    public class HookManager
    {
        public static string DownloadResultCancelled { get; } = "Cancelled";
        public static string DownloadResultExcepted { get; } = "Excepted";
        public static string DownloadResultSuccess { get; } = "Success";

        public HookManager()
        {

        }

        public string GetResult(TaskState state)
        {
            if (state.HasFlag(TaskState.Completed) == true)
            {
                if (state.HasFlag(TaskState.Cancelled) == true)
                {
                    return DownloadResultCancelled;
                }
                else if (state.HasFlag(TaskState.Excepted) == true)
                {
                    return DownloadResultExcepted;
                }
                else
                {
                    return DownloadResultSuccess;
                }

            }
            else
            {
                return "None";
            }

        }

        public Hook GetHookDownload(TaskState state)
        {
            if (state.HasFlag(TaskState.Running) == true)
            {
                return Hook.DownloadStart;
            }
            else if (state.HasFlag(TaskState.Completed) == true)
            {
                if (state.HasFlag(TaskState.Cancelled) == true)
                {
                    return Hook.DownloadCancelled;
                }
                else if (state.HasFlag(TaskState.Excepted) == true)
                {
                    return Hook.DownloadExcepted;
                }
                else
                {
                    return Hook.DownloadSuccess;
                }

            }

            return null;
        }

        public Dictionary<Macro, string> GetHookDownloadArguments(DownloadTask task) => new Dictionary<Macro, string>()
        {
            [Macro.ProgramDirectory] = Application.StartupPath,
            [Macro.DownloadDirectory] = task.DownloadDirectory,
            [Macro.DownloadPath] = task.DownloadFile?.FilePath,
            [Macro.DownloadArchiveName] = Path.GetFileName(task.DownloadFile?.FilePath),
            [Macro.DownloadArchiveName2] = Path.GetFileNameWithoutExtension(task.DownloadFile?.FilePath),
            [Macro.DownloadResult] = this.GetResult(task.State),
        };

        public void FireHookDownload(DownloadTask task)
        {
            var macro = this.GetHookDownload(task.State);

            if (macro != null)
            {
                this.FireHookDownload(task, macro);
            }

        }

        public void FireHookDownload(DownloadTask task, Hook hook)
        {
            var commandline = DoujinshiDownloader.Instance.Config.Values.Hook.Commandlines[hook];
            this.Fire(commandline, this.GetHookDownloadArguments(task));
        }

        public static string ReplaceMacro(string text, Macro macro, string value)
        {
            var placeholder = macro.ToPlaceHolder();
            var index = text.IndexOf(placeholder, StringComparison.OrdinalIgnoreCase);

            if (index > -1)
            {
                return $"{text.Substring(0, index)}{value}{text.Substring(index + placeholder.Length)}";
            }
            else
            {
                return text;
            }

        }

        public void Fire(string commandline, Dictionary<Macro, string> arguments)
        {
            if (string.IsNullOrEmpty(commandline) == true)
            {
                return;
            }

            commandline = arguments.Aggregate(commandline, (prev, pair) => ReplaceMacro(prev, pair.Key, pair.Value));

            try
            {
                Console.WriteLine(commandline);

                using (var process = Process.Start(new ProcessStartInfo("cmd.exe", $"/c \"{commandline}\"")
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = Application.StartupPath,
                }))
                {
                    if (DoujinshiDownloader.Instance.Config.Values.Hook.WaitForExit == true)
                    {
                        process.WaitForExit();
                    }

                }

            }
            catch (Exception ex)
            {
                DoujinshiDownloader.Instance.ShowCrashMessageBox(new HookExecuteException($"Executing \"{commandline}\"", ex));
            }

        }

    }

}
