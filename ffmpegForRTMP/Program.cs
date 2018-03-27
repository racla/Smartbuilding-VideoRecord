using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Newtonsoft.Json;

namespace ffmpegForRTMP
{
    class Program
    {
        static void Main(string[] args)
        {
            List<string> old_sns = new List<string>();
            List<string> old_rtmps = new List<string>();
            while (true) {
                List<string> new_sns = new List<string>();
                List<string> new_rtmps = new List<string>();
                using (SmartBuildingEntities sme = new SmartBuildingEntities())
                {
                    foreach (var item in sme.CamRTMP)
                    {
                        new_sns.Add(item.SN);
                        new_rtmps.Add(item.RTMPhd);
                    }
                }
                string[] sns = new_sns.Except(old_sns).ToArray();
                string[] rtmps = new_rtmps.Except(old_rtmps).ToArray();
                if (sns.Length > 0) {
                    old_sns = new_sns;
                    old_rtmps = new_rtmps;
                    for (var i = 0; i < rtmps.Length; i++)
                    {
                        video temp = new video();
                        temp.SN = sns[i];
                        temp.RTMPhd = rtmps[i];
                        Task.Factory.StartNew(SetTimer, temp);
                        //SetTimer(temp);
                    };
                }
                System.Threading.Thread.Sleep(1000);
            }
            Console.Read();
        }
        private class video {
            public string SN;
            public string RTMPhd;
        }
        private static void SetTimer(object video)
        {
            video temp = video as video;
            string sn = temp.SN;
            string RTMPhd = temp.RTMPhd;
            bool isOnline = false;
            //判断设备是否在线，在线的话执行一次SaveVideo,执行完了再判断是否在线
            while(true){
                while (!isOnline)
                {
                    System.Threading.Thread.Sleep(2000);//每2秒判断一次设备是否在线
                    string res = HTMLHelper.GetHtml(@"https://www.skytraveler7.com/SmartSite/v1/CamInfo/isOnline?SN="+sn, null, null);
                    var result = JsonConvert.DeserializeObject<dynamic>(res);
                    if (result.status[0].ToString() == "1")
                        isOnline = true;
                    else
                        isOnline = false;
                }
                SaveVideo(sn, RTMPhd);
                isOnline = false;
            }
            /*
            SaveVideo(sn, RTMPhd);
            Timer timer = new Timer(20 * 60 * 1000); //10分钟存储一次
            timer.Enabled = true;
            timer.Elapsed += (sender, e) => SaveVideo(sn, RTMPhd);*/
        }
        //private static void SaveVideo(object sender, ElapsedEventArgs e)
        private static bool SaveVideo(string sn, string RTMPhd)
        {
            string baseDir = @"D:\ffmpeg\";
            //string baseDir = @"K:\1.1\ffmpeg-20170731-b664d1f-win64-static\bin\";
            if (!Directory.Exists(baseDir + sn))
            {
                Directory.CreateDirectory(baseDir + sn);
            }
            DateTime Now = DateTime.Now;
            string date = DateTime.Now.ToString("yyMMddHHmm");
            using (SmartBuildingEntities sme = new SmartBuildingEntities()) {
                VideoPath vp = new VideoPath()
                {
                    VideoName = date + ".mp4",
                    StartTime = Now,
                    SN = sn
                };
                sme.VideoPath.Add(vp);
                sme.SaveChanges();
            }

            string spath = baseDir;
            string topath = baseDir + sn + @"\";
            RTMPhd = RTMPhd.Replace("\r\n", "");
            string cmd = @"D: && cd " + spath + " &&  ffmpeg -y -i "+RTMPhd+" -t 1200 -vcodec copy -acodec copy -f mp4 " + topath + date + ".mp4";
            Console.WriteLine("****************S*****************");
            Console.WriteLine(cmd);
            Console.WriteLine("****************E*****************");
            //string cmd = @"-y -i rtmp://live.hkstv.hk.lxdns.com/live/hks -vcodec copy -acodec copy -f mp4 K:\Project\姚门项目\智慧工地\管理系统\demo1\Mag\video\"+date+".mp4";
            EXECMD(cmd);
            return true; //表示执行完了
        }
        private static void EXECMD(string cmd)//object ocmd)
        {
            //string cmd = ocmd.ToString();
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.UseShellExecute = false;    //是否使用操作系统shell启动
            p.StartInfo.RedirectStandardInput = false;//接受来自调用程序的输入信息
            p.StartInfo.RedirectStandardOutput = true;//由调用程序获取输出信息
            p.StartInfo.RedirectStandardError = true;//重定向标准错误输出
            p.StartInfo.CreateNoWindow = false;//不显示程序窗口
            p.Start();//启动程序
            //向cmd窗口发送输入信息
            p.StandardInput.WriteLine(cmd + "&exit");
            //p.StandardInput.WriteLine(cmd);
            p.StandardInput.AutoFlush = true;
            //p.StandardInput.WriteLine("exit");
            //向标准输入写入要执行的命令。这里使用&是批处理命令的符号，表示前面一个命令不管是否执行成功都执行后面(exit)命令，如果不执行exit命令，后面调用ReadToEnd()方法会假死
            //同类的符号还有&&和||前者表示必须前一个命令执行成功才会执行后面的命令，后者表示必须前一个命令执行失败才会执行后面的命令
            //获取cmd窗口的输出信息
            //string output = p.StandardOutput.ReadToEnd();
            //StreamReader reader = p.StandardOutput;
            //string line = reader.ReadLine();
            //while (!reader.EndOfStream)
            //{

            //    line = reader.ReadLine();
            //}             
            p.WaitForExit();//等待程序执行完退出进程
            p.Close();
            // return output;
        }
    }
}
