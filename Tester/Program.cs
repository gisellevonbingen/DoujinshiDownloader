using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Giselle.DoujinshiDownloader;
using Giselle.DoujinshiDownloader.Doujinshi;
using Giselle.DoujinshiDownloader.Schedulers;

namespace Tester
{
    public class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            new Thread(() =>
            {
                Thread.Sleep(100);

                var list = new List<string>();
                list.Add("968964");
                list.Add("1173593/f92e88a42e");
                list.Add("1169369/2d43df7acb");
                list.Add("845086/697c12ae00");

                for(int i =0; ;i++)
                {
                    Console.ReadLine();

                    var form = DoujinshiDownloader.Instance.MainForm;
                    form.Invoke(new Action(() =>
                    {
                        try
                        {
                            var request = new DownloadRequest();
                            request.DownloadInput = DownloadInput.Parse(list[i % list.Count]);
                            request.DownloadMethod = new DownloadMethodHitomi();
                            request.Title = request.DownloadInput.ToString().Replace("/", "_");
                            var task = form.Register(request);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                        
                    }));

                }


            }).Start();

            DoujinshiDownloader.Main(args);

        }

        private static void Task_Progressed(object sender, DownloadTaskProgressingEventArgs e)
        {
            var task = (DownloadTask)sender;

            lock (task)
            {
                Console.Clear();

                for (int i = 0; i < task.Count; i++)
                {
                    var r = task.Progress[i];

                    Console.WriteLine(i.ToString("D2") + " = " + r);
                }

                Console.WriteLine((task.Progress.Count(DownloadResult.Complete) / ((double)task.Count / 100.0)).ToString("000.00") + "%");
            }

        }

        private static void Task_StateChanged(object sender, EventArgs e)
        {
            var task = (DownloadTask)sender;

            lock (task)
            {
                Console.WriteLine(task.State);
                Console.WriteLine(task.Exception);
            }

            //if (task.State.HasFlag(TaskState.Completed) == true)
            //{
            //    DoujinshiDownloader.Instance.MainForm.Unreigster(task);
            //}

        }

        public static void TestDownloadInput()
        {
            TestDownloadInput("aa");
            TestDownloadInput("1173593");
            TestDownloadInput("https://hitomi.la/galleries/1173593.html");
            TestDownloadInput("https://hitoami.la/gallersies/1173593.htsml");
            TestDownloadInput("/ 1173593 / f92e88a42e");
            TestDownloadInput("1173593/f92e88a42e");
            TestDownloadInput("1173593/f92e88a42e /");
            TestDownloadInput("a/b /");
            TestDownloadInput("https://exhentai.org/g/1173593/f92e88a42e");
            TestDownloadInput("https://exhentai.org/g/1173593/f92e88a42e/");
            TestDownloadInput("https://e-hentai.org/g/1173593/f92e88a42e");
            TestDownloadInput("https://e-hentai.org/g/1173593/f92e88a42e/");
        }

        public static void TestDownloadInput(string s)
        {
            DownloadInput input = new DownloadInput();
            DownloadInput.TryParse(s, out input);

            Console.WriteLine("===========================");
            Console.WriteLine("Input :" + s);
            Console.WriteLine("Parsed:" + input);
        }

    }

}
