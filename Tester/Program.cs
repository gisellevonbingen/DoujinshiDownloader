using Giselle.DoujinshiDownloader;
using Giselle.DoujinshiDownloader.Doujinshi;
using Giselle.DoujinshiDownloader.Schedulers;
using Giselle.DoujinshiDownloader.Utils;
using ImageMagick;
using Jint;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Tester
{
    public class Program
    {
        [STAThread]
        public static void Main()
        {
            //var engine = new Engine();
            //engine.SetValue("A", new Action<string>(Console.WriteLine));
            //engine.Execute("HTMLImageElement = function(){};");
            //engine.Execute("A('123')");

            using (var instance = new DoujinshiDownloader(new CommandLineOptions()))
            {
                TestDownloadInputParse();
                TestDownload();
            }

        }

        public static void TestDownload()
        {
            var tests = new List<DownloadTest>
            {
                new DownloadTest(new DownloadInput(1651862), DownloadMethod.Hitomi), //WEBP, AVIF
                new DownloadTest(new DownloadInput(1846070), DownloadMethod.Hitomi), //WEBP, AVIF
                new DownloadTest(new DownloadInput(1474824), DownloadMethod.Hitomi), //JPG
                new DownloadTest(new DownloadInput(1490664), DownloadMethod.Hitomi), //JPG
                new DownloadTest(new DownloadInput(1836951), DownloadMethod.Hitomi) //GIF
            };

            foreach (var test in tests)
            {
                var input = test.Input;
                var site = test.Method.Site;
                var agent = test.Method.CreateAgent(input, new WebRequestProvider());
                var info = agent.GetGalleryInfo();
                var thumbnail = info.GetFirstThumbnail(agent);
                var request = new DownloadRequest
                {
                    Validation = GalleryValidation.CreateByInfo(test.Method, agent, info, thumbnail),
                    FileName = PathUtils.FilterInvalids(info.Title)
                };

                var printed = false;

                using (var task = new DownloadTask(request))
                {
                    task.Start();

                    while (true)
                    {
                        if (task.Count > 0 && printed == false)
                        {
                            printed = true;
                            Console.WriteLine(string.Join(Environment.NewLine, task.ImageViewStates.Select(s => s.View.Url)));
                        }

                        Console.WriteLine($"{input} : {task.Index}/{task.Count}, {task.Index / (task.Count / 100.0D):F2}");

                        if (task.Running == false)
                        {
                            break;
                        }

                        Thread.Sleep(1000);
                    }

                }

            }

        }

        public static void TestDownloadInputParse()
        {
            var list = new List<string>
            {
                "1426111",
                "1426111/f31e10992f",
                "https://e-hentai.org/g/1426111/f31e10992f",
                "https://exhentai.org/g/1426111/f31e10992f",
                "https://e-hentai.org/g/1426111/f31e10992f/",
                "https://exhentai.org/g/1426111/f31e10992f/",
                "https://hitomi.la/galleries/1445130.html",
                "https://hitomi.la/reader/1262768.html",
                "https://hitomi.la/reader/1262768.html#1-"
            };

            foreach (var str in list)
            {
                var downloadInput = DownloadInput.Parse(str);
                Console.WriteLine("=================");
                Console.WriteLine(str);
                Console.WriteLine(downloadInput);
                Console.WriteLine();
            }

        }

        public class DownloadTest : Tuple<DownloadInput, DownloadMethod>
        {
            public DownloadInput Input => this.Item1;
            public DownloadMethod Method => this.Item2;

            public DownloadTest(DownloadInput input, DownloadMethod method) : base(input, method)
            {

            }

        }

    }

}
