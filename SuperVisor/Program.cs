using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;

using System.Net;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SuperVisor
{
    class Program
    {
        static HttpWebRequest req;                // 요청 객체
        static Stream postDataStream;             // 요청 데이터 스트림
        static HttpWebResponse res;               // 응답 객체
        static Stream resPostStream;              // 응답 데이터 스트림
        static StreamReader readerPost;           // 응답 데이터 스트림 리더
        static JObject jo;                        // Json 데이터
        
        public static void Tick()
        {
            bool Alive = false;

            Process[] ProcessList = Process.GetProcesses();

            foreach (Process pro in ProcessList)
            {
                if (pro.ProcessName == "PanicCall")
                {
                    Alive = true;
                    break;
                }
            }

            if (Alive == false)
            {
                deviceInfoToJson();
                SoftwareRun();
            }
        }

        static void Main(string[] args)
        {
            System.Diagnostics.Process[] myProc = System.Diagnostics.Process.GetProcessesByName("SuperVisor");

            if (myProc.Length == 0)
            {
                TaskbarManager TaskbarMan = new TaskbarManager();
                TaskbarMan.SetTaskbar_AutoHide();

                CheckAlive();
            }
        }

        ~Program()
        {
            TaskbarManager TaskbarMan = new TaskbarManager();
            TaskbarMan.ResetTaskbar_AutoHide();
        }

        private static void CheckAlive()
        {
            while (true)
            {
                Tick();
                Thread.Sleep(10000);
            }
        }

        private static void SoftwareRun()
        {
            try
            {
                string detectorPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
                detectorPath = detectorPath + "\\PanicCall.exe";
                detectorPath = detectorPath.Remove(0, 6);

                System.Diagnostics.ProcessStartInfo Run = new System.Diagnostics.ProcessStartInfo();
                Run.FileName = detectorPath;
                Process mainApp = System.Diagnostics.Process.Start(Run);
            }
            catch (Exception ex)
            {
 
            }
        }

        static private void deviceInfoToJson()
        {
            jo = new JObject();
            jo.Add("id", "1");                                  // 프로그램 고유 id
            jo.Add("name", "parking management system");        // 프로그램 이름
            jo.Add("content", "problem occurred");              // 에러 내용
            Console.Write(jo.ToString());
            byte[] result = System.Text.Encoding.GetEncoding("utf-8").GetBytes(jo.ToString());
            httpSend(result);
        }

        static void httpSend(byte[] data)
        {
            //전송
            req = (HttpWebRequest)WebRequest.Create("http://121.130.241.141/result");
            req.Method = "POST";
            req.ContentType = "application/json";
            req.ContentLength = data.Length;

            //데이터 전송

            postDataStream = req.GetRequestStream();
            postDataStream.Write(data, 0, data.Length);
            postDataStream.Close();

            //응답
            res = (HttpWebResponse)req.GetResponse();
            resPostStream = res.GetResponseStream();
            readerPost = new StreamReader(resPostStream, Encoding.Default);
            string resMessage = readerPost.ReadToEnd();
            Console.Write("\n------------------\n response \n\n " + resMessage);
            Console.ReadKey();
        }
    }
}
