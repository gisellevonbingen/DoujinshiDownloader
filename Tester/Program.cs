using Giselle.DoujinshiDownloader;
using Giselle.DoujinshiDownloader.Doujinshi;
using Giselle.DoujinshiDownloader.Schedulers;
using Giselle.DoujinshiDownloader.Utils;
using System;
using System.Collections.Generic;
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
            using (var instance = new DoujinshiDownloader(new CommandLineOptions()))
            {
                TestDownloadInputParse();
                TestDownload();
            }

        }

        public static void TestDownload()
        {
            var tests = new List<DownloadTest>();
            tests.Add(new DownloadTest(new DownloadInput(1651862), DownloadMethod.Hitomi)); //WEBP, AVIF
            tests.Add(new DownloadTest(new DownloadInput(1846070), DownloadMethod.Hitomi)); //WEBP, AVIF
            tests.Add(new DownloadTest(new DownloadInput(1474824), DownloadMethod.Hitomi)); //JPG
            tests.Add(new DownloadTest(new DownloadInput(1490664), DownloadMethod.Hitomi)); //JPG
            tests.Add(new DownloadTest(new DownloadInput(1836951), DownloadMethod.Hitomi)); //GIF

            foreach (var test in tests)
            {
                var input = test.Input;
                var site = test.Method.Site;
                var agent = test.Method.CreateAgent();
                var info = agent.GetGalleryInfo(site, input);
                var thumbnail = agent.GetGalleryThumbnail(info.ThumbnailUrl);

                var request = new DownloadRequest();
                request.Validation = GalleryValidation.CreateByInfo(site, input, agent, info, thumbnail);
                request.FileName = PathUtils.FilterInvalids(info.Title);

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
            var list = new List<string>();
            list.Add("1426111");
            list.Add("1426111/f31e10992f");
            list.Add("https://e-hentai.org/g/1426111/f31e10992f");
            list.Add("https://exhentai.org/g/1426111/f31e10992f");
            list.Add("https://e-hentai.org/g/1426111/f31e10992f/");
            list.Add("https://exhentai.org/g/1426111/f31e10992f/");
            list.Add("https://hitomi.la/galleries/1445130.html");
            list.Add("https://hitomi.la/reader/1262768.html");
            list.Add("https://hitomi.la/reader/1262768.html#1-");

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
