using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Media;
using System.Windows;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;
using System.Windows.Interop;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.InteropServices;
using System.Windows.Markup;
using System.Net.NetworkInformation;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.IO.Ports;


namespace PanicCall
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    /// 



    public enum isTcpSuccess                    //~추가: 만공등 색바꾸는데 사용
    {
        Failed,
        Success
    }
    public struct PisServers                       //각 서버  전체 주차여유공간수와 각 서버의 ip  :  각서버의 공간수를 합쳐서 입구 pis 로 패킷 보냄
    {
        public string dstIP;
        public string serverIP;
        public int cars1;
        public int areas1;
        public int cars2;
        public int areas2;
        public int cars3;
        public int areas3;
    }

    public partial class MainWindow : Window
    {

        public const uint WM_COPYDATA = 0x004A;

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr FindWindow(string lpClassName, String lpWindowName);



        // System.Windows.Threading.DispatcherTimer LprTimer = new System.Windows.Threading.DispatcherTimer(); //~수정: 차량번호 인식 타이머 : 번호 인식 주기를 설정 (현재 1분 마다 번호 검색)
        System.Windows.Threading.DispatcherTimer DetectStarter = new System.Windows.Threading.DispatcherTimer();
        System.Windows.Threading.DispatcherTimer DetectChecker = new System.Windows.Threading.DispatcherTimer();

        public static Maps maps = new Maps();
        public delegate void _delegate_arg(object obj);


        bool isdownload = false;
        bool istimerlock = false;
        string connectprotocol = "";
        WebClient webClient;
        Storyboard storyMapAdd;
        Storyboard storySupport;
        static ListBox LogList;
        public delegate void _delegate();
        bool isLoaded = false;
        bool isMinimized = false;
        bool isStart = false;
        string ProgramFilesPath = string.Empty;
        string WanIP = string.Empty;
        bool movelock = true;
        bool ExitBan = true;
        bool isPause = true;
        int pauseCount = 0;                                                          //Detector 1분마다 체크, 멈춤 상태일때 증가 
        

        IntPtr c_hwnd;

        //~추가: 변수
        List<string> messageList = new List<string>();                               // c++ 프로그램으로 부터 받은 데이터 저장
        bool msgReceive = false;
        NamedPipeServer PServer1 = new NamedPipeServer(@"\\.\\pipe\\c++_send", 0);   //읽기 
        NamedPipeServer PServer2 = new NamedPipeServer(@"\\.\\pipe\\c#_send", 1);    //쓰기 
        TcpListener listener;                                                        //APP 요청 수신 대기
        private object msgLock = new object();
        public static int pisIndex = 1;                                                   //pis 추가될때마다 증가
        bool isMainSrv = false;
        List<PisServers> srvList;
        double px, py;
        public static double scale = 0.95;
        int cars = 0, areas = 0;                                                     //전체 차량 수


        //~추가 : 시흥은계 pis
        public static int groupCount = 0;
        public static int selectedIndex = 0;
        public static Group selectedGroup;
        public static bool isSelectChecked = false;
        public List<Group> groupList;
        private bool isAllSelected = false;
        public static bool isOpenAddGroup = false;

        _delegate_arg _trace2; // 비UI 객체에서 UI객체로 접근하기위한 용도
        _delegate_arg SerialSend;



        private bool IsRequestingOptions
        {
            get
            {
                return (Keyboard.Modifiers == (ModifierKeys.Alt | ModifierKeys.Control) && Keyboard.IsKeyDown(Key.T));
            }
        }
        private bool IsAllSelectKey
        {
            get
            {
                return ((Keyboard.Modifiers == ModifierKeys.Control) && Keyboard.IsKeyDown(Key.A));
            }
        }
        private bool IsAllCancelKey
        {
            get 
            {
                return ((Keyboard.Modifiers == ModifierKeys.Control) && Keyboard.IsKeyDown(Key.D));
            }
        }

        public MainWindow()
        {
            InitializeComponent();


            System.Diagnostics.Process[] myProc = System.Diagnostics.Process.GetProcessesByName("PanicCall");

            if (myProc.Length > 1)
            {
                MessageBox.Show("이미 실행중입니다.", "Panic Moniroting System");
                Application.Current.Shutdown();
            }
            //ShowInTaskbar = false;
        }





        private void InitPath()
        {
            ProgramFilesPath = RegistryManager.GetCommonProgramfilesFolder();
            if (ProgramFilesPath == string.Empty || ProgramFilesPath == "")
            {
                MessageBox.Show("해당 시스템의 경로를 확인하지 못하였습니다.", "에러");
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                InitPath();
                InitLogListBox();

                InitNetwork();

                this.Topmost = false; // true 이면 최상위 윈도우


                storyMapAdd = new Storyboard();
                storySupport = new Storyboard();

                _trace2 = new _delegate_arg(trace2);
                SerialSend = new _delegate_arg(SerialView.Send);
                srvList = new List<PisServers>();
                groupList = new List<Group>();

                AppPropertyies();
                MpaSetting();
                Setting();
                InitHandler();

                Load();


                ProcessorKiller();


                ThreadPool.QueueUserWorkItem(new WaitCallback(StopProgress), null);


                
                //~추가: 새로 시작하는 스레드들 시작점
                TaskManager();


                PServer1.ReceiveEvent += new NamedPipeServer.ReceiveEventHandler(Receive_Data);
                //IntPtr hwnd = Win32APIStorage.FindWindow(null, "MFCApplication1");
                //Win32APIStorage.SendMessage(hwnd, Win32APIStorage.WM_COPYDATA, IntPtr.Zero, ref cds);

                Slider.Maximum = 1.9;
                Slider.Minimum = 0;
                Slider.Value = 0.95;

                //~추가: 파이프 통신
                PServer1.Start();
                PServer2.Start();


                DetectStarter.Tick += new EventHandler(DetectStart);
                DetectStarter.Interval = new TimeSpan(0, 0, 5);
                DetectStarter.Start();

                DetectChecker.Tick += new EventHandler(DetectPauseCheck);
                DetectChecker.Interval = new TimeSpan(0, 0, 20);
                DetectChecker.Start();

                Run_Restart();
            }
            catch (Exception ex) { WriteException(ex.ToString()); }
        }

        private void Run_Restart()
        {
            Process[] procSystecList = Process.GetProcessesByName("PGrestart");
            foreach (Process proc in procSystecList)
            {
                proc.Kill();
                proc.WaitForExit();
            }

            Thread.Sleep(100);

            Process[] procSystecList2 = Process.GetProcessesByName("restart");
            foreach (Process proc in procSystecList2)
            {
                proc.Kill();
                proc.WaitForExit();
            }

            Thread.Sleep(100);

            string FilePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            FilePath = FilePath.Remove(0, 6);

            string SupportFilePath = "C:\\Program Files (x86)\\Twowinscom\\Parking Guidance\\PGrestart.exe";
            if (System.IO.File.Exists(SupportFilePath))
            {
                try
                {
                    System.Diagnostics.ProcessStartInfo Run = new System.Diagnostics.ProcessStartInfo();
                    Run.FileName = SupportFilePath;
                    System.Diagnostics.Process.Start(Run);
                    Thread.Sleep(100);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }


        void FillString(ref byte[] st1, string st2)
        {
            string msg = st2.ToString();
            ASCIIEncoding ascii = new ASCIIEncoding();

            st1 = new byte[30];

            byte[] temp = System.Text.Encoding.Default.GetBytes(st2.ToCharArray());

            for (int i = 0; i < 30; i++)
            {
                st1[i] = 0;
            }

            for (int i = 0; i < temp.Length; i++)
            {
                st1[i] = temp[i];
            }

        }


        void webClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            isdownload = true;
            //Console.WriteLine(e.ProgressPercentage);
        }

        void webClient_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            isdownload = false;

            Console.WriteLine("다운완료");
        }

        void DetectStart(object sender, EventArgs e)
        {
            startDetection();
            DetectStarter.Stop();
        }

        void DetectPauseCheck(object sender, EventArgs e)
        {
            if (isStart == false)
                return;
            if (isPause)
                pauseCount++;
            else
                pauseCount = 0;

            if (pauseCount > 3)
            {
                int result = startDetection();  // 1일때 시작,  0일때 종료     0이라면 이미 실행중 ( 멈춰 있는 상태 ) -> 종료 -> 한번더 실행 -> 시작
                if (result == 0)
                    startDetection();
            }

            isPause = true;
        }

        //~추가: 파이프 통신 받은 데이터 메시지리스트에 추가 
        private void Receive_Data(object sender, ReceiveEventArgs e)
        {
            if (e.receiveData.Length == 0)
                return;

            string data = e.receiveData.Trim('\0');


            Dispatcher.BeginInvoke(new Action(delegate()
            {
                SerialView.Recive(data);
            }));


            if (messageList.Count > 300)
                lock(msgLock)
                    messageList.Clear();

            messageList.Add(data);

            isPause = false;
        }

        //~추가: roi 설정값 저장 c++ 에서 사용
        private void saveROI()
        {
            try
            {
                StreamWriter file = new StreamWriter("C:\\Program Files (x86)\\Twowinscom\\Parking Guidance\\roi.txt");
                string str = "";

                foreach (Map map in maps)
                {
                    foreach (PanicControl panic in map.PanicList)
                    {
                        foreach (Camera camera in panic.cameras)
                        {
                            if (camera != null && string.IsNullOrWhiteSpace(camera.IP) == false)
                            {
                                str += camera.IP + " ";
                                str += panic.Addr + " ";
                                str += Convert.ToInt32(camera.ROI_1_x) + " ";
                                str += Convert.ToInt32(camera.ROI_1_y) + " ";
                                str += Convert.ToInt32(camera.ROI_2_x) + " ";
                                str += Convert.ToInt32(camera.ROI_2_y) + " ";
                                str += Convert.ToInt32(camera.ROI_3_x) + " ";
                                str += Convert.ToInt32(camera.ROI_3_y) + " ";

                                file.WriteLine(str);
                                str = "";
                            }
                        }
                    }
                }
                file.Close();
            }
            catch (Exception ex)
            {

            }

        }

        //~추가: 스레드 시작점
        private void TaskManager()
        {
            Task.Factory.StartNew(messageProcessing);       //c++ 받은 데이터 처리 스레드 : pipe 받은 데이터는 list 에 저장되어있음 
            Task.Factory.StartNew(changeColor);             //panic 색변경 및 만공등 색변경 tcp 송신
            Task.Factory.StartNew(makeNSendPisPacket);      //pis 주차수 변경 및 pis 에 데이터 송신 485
            Task.Factory.StartNew(pisServer);                //pis서버 오픈 

            //                            : list 가 차있으면 데이터 처리
        }

        public string LocalIPAddress() //Local ipv4 얻기
        {
            IPHostEntry host;
            string localIP = "";
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                    break;
                }
            }
            return localIP;
        }


        //~추가: c++ 받은 데이터 처리
        // 1. 차량 인식 결과 처리 : 0.구분 parse[0] 가 numbers 가 아닐때는 5. 번까지 처리
        // 2. 번호 인식 결과 처리 : 0.구분 parse[0] 가 numbers 일때 8번까지 처리
        private void messageProcessing()
        {
            while (true)
            {
                try
                {
                    string msg;

                    if (messageList.Count == 0)
                        continue;

                    lock (msgLock)
                    {
                        msg = messageList[0];
                        messageList.RemoveAt(0);
                    }

                    if (msg.Length <= 0)
                        continue;

                    

                    msgReceive = true;                  //메시지 받는중 true


                    string[] parse = msg.Split('_');   // 0.구분 

                    string type = parse[0];                 //car or number 
                    int addr = Int32.Parse(parse[1]);                 //panic addr
                    string ip = parse[2];                   //ip
                    string color = parse[3];                //red or green
                    int cars = Int32.Parse(parse[4]);       //주차된 차량수     
                    int areas = Int32.Parse(parse[5]);      //주차면 수

                   
                    foreach (Map map in maps)
                    {                     
                        foreach (PanicControl panic in map.PanicList)
                        {
                            foreach (Camera camera in panic.cameras)
                            {
                                if (camera == null)
                                    continue;

                                if (camera.IP.Equals(ip))
                                {
                                    if (color.Equals("Red"))
                                        camera.isfull = true;
                                    else
                                        camera.isfull = false;

                                    camera.cars = cars;
                                    camera.areas = areas;
                                }
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                }
            }


        }

        //~추가: 색 변경 스레드 : 바라보고 있는 카메라 red 이면 해당 번호에 맞는 만공등 색 변경, pis값 추가, 입구 pis 변경
        void changeColor()
        {
            while (true)
            {
                if (! msgReceive)
                {
                    Thread.Sleep(1000);
                    continue;
                }
                try
                {
                    for(int i = 0; i < maps.Count; i++)
                    {
                        int map_cars = 0, map_areas = 0;

                        foreach (PanicControl panic in maps[i].PanicList)
                        {
                            int redCount = 0;
                            foreach (Camera camera in panic.cameras)                                    
                            {                              
                                if (camera == null)         
                                    continue;
                                if (camera.isfull)
                                    redCount++;

                                map_cars += camera.cars;
                                map_areas += camera.areas;
                            }

                            
                            if (redCount == panic.camerasCount())
                            {
                                Dispatcher.Invoke(new Action(delegate()  //빨강
                                {
                                    panic.Yes_parking();
                                    if (panic.isRed != 1)
                                        trace(panic.Addr + " 카메라 적색으로 변경 ");

                                    panic.isRed = 1;
                                }));
                                UdpSend(panic.Addr, "Red");
                            }
                            else
                            {
                                Dispatcher.Invoke(new Action(delegate()
                                {
                                    panic.No_parking();
                                    if(panic.isRed == 1)
                                       trace(panic.Addr + " 카메라 녹색으로 변경 ");

                                    panic.isRed = 0;
                                }));
                                UdpSend(panic.Addr, "Green");
                            }


                            //udp 너무 빠르게 보내서 만공등 led 변경 안되는 문제 sleep 로 속도 늦춤
                            Thread.Sleep(2);
                        }

                        maps[i].Areas = map_areas;
                        maps[i].Cars = map_cars;
                    }

                   // makeGatePisPacket(cars, areas);
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(new Action(delegate()
                    {
                        trace2("change color function error : " + ex.Message);
                    }));
                }
                msgReceive = false;   //메시지 안받는 상태로 변경 ( msgReceive 는 msgprocessing() 에서 계속 true 로 바꾸기 때문에 메시지 받고있다면 sleep 지나면 항상 true
                Thread.Sleep(1000);   //한사이클 색변경 빠르게 계속 할 필요없음 ( 차 인식 사이클이 늦기 때문 ) 
               
            }
        }


        
        public int UdpSend(int panic_addr, string color)//bool isRed, string wisnet)
        {

            //string x84CPU_IP = "172.16.24.1"; // 84 cpu ip 고정, 84 cpu 사용 안함

            int addr = panic_addr;
            bool isRed = color.Equals("Red") ? true : false;


            // color packet
            #region packet

            Byte[] packet = new Byte[8];

            packet[0] = 0x02;
            packet[1] = 0x60;

            if (color.Equals("Red"))
                packet[2] = 0x01;
            else 
                packet[2] = 0x02;
            

            packet[3] = (byte)((addr / 100) + 0x30);
            packet[4] = (byte)(((addr % 100) / 10) + 0x30);
            packet[5] = (byte)((addr % 10) + 0x30);



            packet[6] = (byte)(((packet[0] + packet[1] + packet[2] + packet[3] + packet[4] + packet[5])) & (0xff));
            if (packet[6] == 02 || packet[6] == 03)
                packet[6] = 255;

            packet[7] = 0x03;
            #endregion

            
            Dispatcher.BeginInvoke(new Action(delegate()
            {
                string data = "";
                foreach (var item in packet)
                {
                    data += ((byte)(item)).ToString("X2") + " ";
                }
                SerialView.Send(data);
            }));


            int sPort = 6931;

            //Endpoint.Broadcast는 255.255.255.255 자동으로 네트워크카드 선택, 172, 192 둘중 하나 선택하고 싶을땐 172.xxx.xxx.255 (끝에만 255일시 해당 네트워크 브로드캐스팅)
            IPEndPoint ip = new IPEndPoint(IPAddress.Parse("172.16.255.255"), sPort);
            Socket soc = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            soc.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);

            try
            {
                soc.SendTo(packet, ip);
                return (int)isTcpSuccess.Success;
            }
            catch (Exception ex)
            {
                Dispatcher.BeginInvoke(new Action(delegate()
                {
                    trace2("데이터 전송 실패 : " + ex.Message);
                }));
                return (int)isTcpSuccess.Failed;
            }
            finally
            {
                soc.Close();
            }

        }


        //주차 공간 가능수를 파악후 패킷 생성, 전송
        private void makeNSendPisPacket()
        {
            while (true)
            {
                try
                {
                    //전체 남은 주차 공간수 확인
                    areas = 0; cars = 0;
                    foreach (Map map in maps)
                    {
                        areas += map.Areas;
                        cars += map.Cars;
                    }

                    foreach (Map map in maps)
                    {
                        foreach (PisControl pis in map.PisList)
                        {

                            #region pis.group car area 
                            countGroupCars(pis.group1);
                            countGroupCars(pis.group2);
                            countGroupCars(pis.group3);

                            #endregion

                            if (pis.isGatePis == false) //일반 pis 일때
                            {
                                Dispatcher.BeginInvoke(new Action(delegate()
                                {
                                    pis.setUICount();         //pis UI 에 cars 표시                                  
                                }));

                                byte[] packet = makePisPacketType1(pis);
                                if (packet != null)
                                {
                                    pis485Send(packet);

                                    Dispatcher.BeginInvoke(new Action(delegate()
                                    {
                                        try
                                        {
                                            string data = "";
                                            foreach (var item in packet)
                                                data += ((byte)(item)).ToString("X2") + " ";
                                            SerialView.Send(data);
                                        }
                                        catch (Exception ex)
                                        {
                                        }
                                    }));
                                }
                            }
                            else                          //입구에 표시되는 pis 일때
                            {
                                if (string.IsNullOrWhiteSpace(pis.serverIp))   //main pc 인경우  데이터 합침
                                {

                                    foreach (PisServers server in srvList)      //PisServer : main pc 가 아닌 각 server 들의 입구 pis
                                    {
                                        if (server.dstIP.Equals(pis.Ip))        //보내야 하는 목적지 pis ip 가 같은경우 합산
                                        {
                                            pis.group1.cars += server.cars1;
                                            pis.group1.areas += server.areas1;
                                            pis.group2.cars += server.cars2;
                                            pis.group2.areas += server.areas2;
                                            pis.group3.cars += server.cars3;
                                            pis.group3.areas += server.areas3;
                                        }
                                    }

                                    Dispatcher.BeginInvoke(new Action(delegate()
                                    {
                                        pis.setUICount();         //pis UI 에 cars 표시
                                    }));

                                    byte[] packet = makePisPacketType2(pis);
                                    if (packet != null)   
                                    {
                                        pisTcpSend(packet, pis.Ip);

                                        Dispatcher.BeginInvoke(new Action(delegate()
                                        {
                                            try
                                            {
                                                string data = "";
                                                foreach (var item in packet)
                                                    data += ((byte)(item)).ToString("X2") + " ";
                                                SerialView.Send(data);
                                            }
                                            catch (Exception ex)
                                            {
                                            }
                                        }));
                                    }
                                }
                                else                       //main pc 가 아닌경우 데이터 main pc로 전송
                                {
                                    Dispatcher.BeginInvoke(new Action(delegate()
                                    {
                                        pis.setUICount();         //pis UI 에 cars 표시
                                    }));
                                    sendPisCountsToServer(pis);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Dispatcher.BeginInvoke(new Action(delegate()
                    {
                        trace2(ex.Message + "pismake error");         
                    }));
                }

                Thread.Sleep(3000);
            }
        }

        private void countGroupCars(Group group)
        {
            if (group.selectedIndex == 0)                           //전체맵 선택일 경우
            {
                group.areas = areas; group.cars = cars;
            }
            else if (group.selectedIndex < maps.Count)                                                   //특정맵 선택일 경우
            {
                foreach (Map _map in maps)
                {
                    if (_map.MapName.Equals(group.selectedGroupName) == true)
                    {
                        group.areas = _map.Areas; group.cars = _map.Cars;
                    }
                }
            }
            else                     //없음또는 그룹선택일경우
            {
                group.CountCars();
            }
        }
       

        //일반 pis : 양쪽 표시 하는 pis
        private byte[] makePisPacketType1(PisControl pis)
        {
            if (pis.addr < 0)
                return null;

            //  dle stx attitude 값들 고정 
            List<byte> packet = new List<byte>()        
                        {
                          0x10, 0x02, 0x00, 0x94, 0x00, 0x01, 0x63, 0x00, 0x00, 0x03, 0x01, 0x00, 0x00, 0x14, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00            
                        };

            //addr 추가
            int addr = pis.addr;
            packet.Insert(2, (byte)addr);


            //주차장 상태, 남은 주차면수 추가
            //글자,색 추가 : 그룹2부터 그룹1  역순으로 패킷에 붙음  : ex) stx addr atti 그룹2color  그룹1color 그룹2data 그룹1data etx 

            byte[] strColor = new byte[] { 0x07, 0x00, 0x07, 0x00 };  // 없음 일경우 흰색으로 알려줌
            string str2;            //역순이므로 2부터

            if (pis.group2.areas == 0)                   // 양쪽 표시 안하는 경우 
                str2 = "없음";
            else
            {
                double area = pis.group2.areas;
                double cars = pis.group2.cars;
                double per = (area - cars) / area;


                if (per < 0.1)        //혼잡
                {
                    str2 = "혼잡";
                    strColor = new byte[] { 0x01, 0x00, 0x01, 0x00 };   //red
                }
                else if (per < 0.3)    //보통
                {
                    str2 = "보통";
                    strColor = new byte[] { 0x03, 0x00, 0x03, 0x00 };   //yeloyellow
                }
                else                  //여유
                {
                    str2 = "여유";
                    strColor = new byte[] { 0x02, 0x00, 0x02, 0x00 };     //green
                }

            }
            foreach (byte temp in strColor)
                packet.Add(temp);


            strColor = new byte[] { 0x07, 0x00, 0x07, 0x00 };  // 없음 일경우 흰색으로 알려줌
            string str1;
            if (pis.group1.areas == 0)
                str1 = "없음";
            else
            {
                double area = pis.group1.areas;
                double cars = pis.group1.cars;
                double per =  (area - cars) / area;

                if (per < 0.1)        //혼잡
                {
                    str1 = "혼잡";
                    strColor = new byte[] { 0x01, 0x00, 0x01, 0x00 };   //red
                }
                else if (per < 0.3)    //보통
                {
                    str1 = "보통";
                    strColor = new byte[] { 0x03, 0x00, 0x03, 0x00 };   //yeloyellow
                }
                else                  //여유
                {
                    str1 = "여유";
                    strColor = new byte[] { 0x02, 0x00, 0x02, 0x00 };     //green
                }

            }
            foreach (byte temp in strColor)
                packet.Add(temp);


            //남은 주차면수 는 초록색 고정  , 그룹2 그룹1 두번 foreach 
            strColor = new byte[] { 0x02, 0x02, 0x02, 0x02 };
            foreach (byte temp in strColor)
                packet.Add(temp);
            foreach (byte temp in strColor)
                packet.Add(temp);


            //주차 상태 글자 추가
            Encoding ksc = Encoding.GetEncoding("ks_c_5601");
            byte[] buff = ksc.GetBytes(str2);
            foreach (byte temp in buff)
                packet.Add(temp);

            buff = ksc.GetBytes(str1);
            foreach (byte temp in buff)
                packet.Add(temp);

            //남은 주차면수 추가   : 천이상 넘어가면 표시 안됨 백의자리까지만 해나서
            int num = pis.group2.areas - pis.group2.cars;
            packet.Add(0x20);                                               //천의자리
            byte tmp = ((num / 100) == 0) ? (byte)0x20 : (byte)(num / 100 + 0x30);
            packet.Add(tmp);                                                //백의자리  :  0일경우 표시 안하기 위해 0x20 사용
            tmp = (((num % 100) / 10) == 0 && tmp == 0x20) ? (byte)0x20 : (byte)((num % 100) / 10 + 0x30);
            packet.Add(tmp);                                                //십의자리   
            packet.Add((byte)(num % 10 + 0x30));                            //일의자리

            num = pis.group1.areas - pis.group1.cars;
            packet.Add(0x20);                                               //천의자리
            tmp = ((num / 100) == 0) ? (byte)0x20 : (byte)(num / 100 + 0x30);
            packet.Add(tmp);                                                //백의자리  :  0일경우 표시 안하기 위해 0x20 사용
            tmp = (((num % 100) / 10) == 0 && tmp == 0x20) ? (byte)0x20 : (byte)((num % 100) / 10 + 0x30);
            packet.Add(tmp);                                                //십의자리  
            packet.Add((byte)(num % 10 + 0x30));                            //일의자리


            //dle etx 추가 : 고정값
            packet.Add(0x10);
            packet.Add(0x03);


            //데이터 길이 lens 추가 lens 위치는 index = 4,  lens 길이는 0x31 고정 
            packet.Insert(4, 0x31);

            //return byte[]
            return packet.ToArray();

        }

        private byte[] makePisPacketType2(PisControl pis)
        {

            if (string.IsNullOrWhiteSpace(pis.Ip))
                return null;

            //  dle stx attitude 값들 고정 
            List<byte> packet = new List<byte>()        // 0x94 까지가 cmd : 
                    {
                       0x10, 0x02, 0x00, 0x94, 0x00, 0x01, 0x63, 0x00, 0x00, 0x03, 0x01, 0x00, 0x00, 0x14, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00     
                    };


            //x축 y 축 시작점 추가  :  y축이 16에서 32 면 2번째줄    32 ~ 48 일시 3번째줄  :  마지막은 default 0

            //addr 추가 : 해당 인덱스 2
            if (pis.addr < 0)
                return null;
            packet.Insert(2, (byte)pis.addr);


            //주차장 위치, 남은 주차면수 추가
            //글자,색 추가 : 


            //2줄  남은 주차 여유 공간 색
            byte[] Color = new byte[] { 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02 };   //  8칸까지 문자 영역 지정 : 한줄에 4칸씩

            foreach (byte temp in Color)
                packet.Add(temp);

            //pis 3층 선택되있을때
            if (pis.group3.areas != 0)
            {
                for (int i = 0; i < 4; i++)
                    packet.Add(0x02);
            }

            //남은 주차면수 추가   : 천이상 넘어가면 표시 안됨 백의자리까지만 해나서
            int num;
            byte tmp;

            num = pis.group1.areas - pis.group1.cars;

            packet.Add(0x20);                                               //천의자리
            tmp = ((num / 100) == 0) ? (byte)0x20 : (byte)(num / 100 + 0x30);
            packet.Add(tmp);                                                //백의자리  :  0일경우 표시 안하기 위해 0x20 사용
            tmp = (((num % 100) / 10) == 0 && tmp == 0x20) ? (byte)0x20 : (byte)((num % 100) / 10 + 0x30);
            packet.Add(tmp);                                                //십의자리  
            packet.Add((byte)(num % 10 + 0x30));                            //일의자리



            num = pis.group2.areas - pis.group2.cars;

            packet.Add(0x20);                                               //천의자리
            tmp = ((num / 100) == 0) ? (byte)0x20 : (byte)(num / 100 + 0x30);
            packet.Add(tmp);                                                //백의자리  :  0일경우 표시 안하기 위해 0x20 사용
            tmp = (((num % 100) / 10) == 0 && tmp == 0x20) ? (byte)0x20 : (byte)((num % 100) / 10 + 0x30);
            packet.Add(tmp);                                                //십의자리   
            packet.Add((byte)(num % 10 + 0x30));                            //일의자리



            //pis가 3층이 선택되어있을때 
            if (pis.group3.areas != 0)
            {
                num = pis.group3.areas - pis.group3.cars;

                packet.Add(0x20);                                               //천의자리
                tmp = ((num / 100) == 0) ? (byte)0x20 : (byte)(num / 100 + 0x30);
                packet.Add(tmp);                                                //백의자리  :  0일경우 표시 안하기 위해 0x20 사용
                tmp = (((num % 100) / 10) == 0 && tmp == 0x20) ? (byte)0x20 : (byte)((num % 100) / 10 + 0x30);
                packet.Add(tmp);                                                //십의자리   
                packet.Add((byte)(num % 10 + 0x30));

                //데이터 길이 lens 추가 lens 위치는 index = 4,  lens 길이는 0x29 고정  : cmd(1) + attribute(16) + color(12) + text(12) = 41 = 0x29
                packet.Insert(4, 0x29);
            }
            else
            {
                //데이터 길이 lens 추가 lens 위치는 index = 4,  lens 길이는 0x29 고정  : cmd(1) + attribute(16) + color(8) + text(8) = 41 = 0x21
                packet.Insert(4, 0x21);
            }

         

            //dle etx 추가 : 고정값
            packet.Add(0x10);
            packet.Add(0x03);


            

            //Tcpsend
            return packet.ToArray();

        }

        //RS485 주소값으로 각 pis 구분
        private void pis485Send(byte[] packet)
        {
            SerialPort serial = new SerialPort();
            try
            {
                serial.PortName = "COM3";
                serial.BaudRate = 9600;
                serial.DataBits = 8;
                serial.StopBits = StopBits.One;
                serial.Parity = Parity.None;
                serial.Open();

                if (serial.IsOpen)
                {
                    serial.Write(packet, 0, packet.Length);
                }
            }
            catch (Exception ex)
            {
                Dispatcher.BeginInvoke(new Action(delegate()
                {
                    trace2("P.I.S RS485 연결 실패 : " + ex.Message);
                }));
            }
            finally
            {
                serial.Close();
            }

        }

        // IP 와 주소값으로 pis 구분 : 입구쪽 PIS 는 tcp 방식 전송
        private void pisTcpSend(byte[] packet, string ip)
        {
            int sPort = 5000;
            Socket soc = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                IAsyncResult result = soc.BeginConnect(IPAddress.Parse(ip), sPort, null, null);
                bool success = result.AsyncWaitHandle.WaitOne(300, false);

                if (soc.Connected)
                {
                    IAsyncResult result2 = soc.BeginSend(packet, 0, packet.Length, SocketFlags.None, null, soc);
                    result2.AsyncWaitHandle.WaitOne(300, true);
                }
            }
            catch (Exception ex)
            {
                Dispatcher.BeginInvoke(new Action(delegate()
                {
                    trace2("P.I.S 연결 실패 : " + ex.Message);
                }));
            }
            finally
            {
                soc.Close();
            }
        }

        //각 서버별 보내오는 데이터 서버의 ip를 기준으로 list 에 중복없이 저장 : 서버별 남은주차영역 데이터
        private void pisServer()
        {
            try
            {
                listener = new TcpListener(9093);
                listener.Start(20);
                while (true)
                {
                    Socket client = listener.AcceptSocket();
                    try
                    {
                        if (client.Connected == false)
                            continue;

                        byte[] buff = new byte[256];
                        client.Receive(buff, 0, 256, SocketFlags.None);
                        string data = System.Text.Encoding.GetEncoding("utf-8").GetString(buff);

                        Dispatcher.BeginInvoke(new Action(delegate()
                        {
                            SerialView.Recive(data);
                        }));

                        string[] splited = data.Split('_');

                        PisServers temp;
                        temp.dstIP = splited[0];
                        temp.serverIP = splited[1];
                        temp.areas1 = Int32.Parse(splited[2]);
                        temp.cars1 = Int32.Parse(splited[3]);
                        temp.areas2 = Int32.Parse(splited[4]);
                        temp.cars2 = Int32.Parse(splited[5]);
                        temp.areas3 = Int32.Parse(splited[6]);
                        temp.cars3 = Int32.Parse(splited[7]);

                        // 서버리스트에  각서버 ip 로 비교후 이미 같은 서버 데이터가 있을시 갱신,  없을시 새로 추가  
                        bool isAdd = false;
                        for (int i = 0; i < srvList.Count; i++)
                        {
                            if (srvList[i].serverIP.Equals(temp.serverIP) && srvList[i].dstIP.Equals(temp.dstIP))
                            {
                                srvList[i] = temp;
                                isAdd = true;
                            }
                        }
                        if (isAdd == false)
                            srvList.Add(temp);
                    }
                    catch (Exception ex)
                    {
                        Dispatcher.BeginInvoke(new Action(delegate()
                        {
                            trace2("Pis Server send error : " + ex.Message);
                        }));
                    }
                    finally 
                    {
                        client.Dispose();
                        client.Close();
                    }

                }
            }
            catch(Exception ex)
            {
                Dispatcher.BeginInvoke(new Action(delegate()
                {
                    trace2("Pis Server error : " + ex.Message);
                }));
            }
        }
        private void sendPisCountsToServer(PisControl pis)
        {
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                if (string.IsNullOrWhiteSpace(pis.Ip) || pis.addr == -1)
                    return;


                string str = pis.Ip + "_" + getMyIp() + "_" + pis.group1.areas + "_" + pis.group1.cars + "_" + pis.group2.areas + "_" + pis.group2.cars + "_" + pis.group3.areas + "_" + pis.group3.cars;

                client.Connect(pis.serverIp, 9093);

                if (client.Connected)
                {
                    byte[] data = Encoding.UTF8.GetBytes(str);
                    client.Send(data);

                    Dispatcher.BeginInvoke(new Action(delegate()
                    {
                        SerialView.Send(str);
                    }));
                }
            }
            catch (Exception ex)
            {
                //Dispatcher.BeginInvoke(new Action(delegate()
                //{
                //    trace2("메인 서버로 데이터 전송 실패  : " + ex.Message);
                //}));
            }
            finally
            {
                client.Dispose();
                client.Close();
            }
        }

        private string getMyIp()
        {
            IPHostEntry host;
            string localIP = "";
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                }
            }

            return localIP;
        } 


        private static bool InvokeRequired
        {
            get { return System.Windows.Threading.Dispatcher.CurrentDispatcher != Application.Current.Dispatcher; }
        }

        private delegate void dputchar(string name);

        public void putchar(string data)
        {
            if (InvokeRequired)
            {
                Dispatcher.BeginInvoke(new dputchar(putchar), data);
            }
            else
            {
                lock (this)
                {
                    string tmps = "";
                    DateTime now = DateTime.Now;
                    tmps += now.ToLocalTime();
                    tmps += ":";
                    tmps += now.Millisecond;
                    tmps += "\t";
                    tmps += data;

                    TextBlock tb = new TextBlock();
                    tb.Foreground = Brushes.Blue;
                    tb.Text = tmps;

                    LogList.Items.Add(tb);
                }
            }
        }


        public void restart_program()
        {
            try
            {
                string FilePath = "restart.exe";
                Process restart = new Process();
                restart.StartInfo.FileName = FilePath;
                restart.StartInfo.CreateNoWindow = true;
                restart.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                restart.Start();
            }
            catch
            {
            }
        }



        private void AutoBackupRun()
        {
            //System.Windows.Threading.DispatcherTimer AutoBackupTimer = new System.Windows.Threading.DispatcherTimer();
            //AutoBackupTimer.Tick += new EventHandler(AutoBackup);
            //AutoBackupTimer.Interval = new TimeSpan(0, 0, 10);
            //AutoBackupTimer.Start();
        }


        private void ProcessorKiller()
        {
            System.Windows.Threading.DispatcherTimer ProcessorKillerTimer = new System.Windows.Threading.DispatcherTimer();
            ProcessorKillerTimer.Tick += new EventHandler(ProcessorKillerTimerTick);
            ProcessorKillerTimer.Interval = new TimeSpan(0, 0, 11);//기본 1초 
            ProcessorKillerTimer.Start();
        }


        private void ProcessorKillerTimerTick(object sender, EventArgs e)
        {
            if (ExitBan)
            {
                ProcessorManager.Killer();
            }
        }





        public void Save()
        {
            string FilePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase) + "\\back";
            FilePath = FilePath.Remove(0, 6);
            DirectoryInfo dir = new DirectoryInfo(FilePath);

            if (!dir.Exists)
                dir.Create();

            try
            {
                if (maps.Count > 0)
                {
                    maps[maps.SelectIndex].Zoom = zoomBox.Zoom;
                    maps[maps.SelectIndex].PosX = zoomBox.PanX;
                    maps[maps.SelectIndex].PosY = zoomBox.PanY;
                }

                string FileName = "\\Maps.bak";

                Stream strm = new FileStream(FilePath + FileName, FileMode.Create, FileAccess.ReadWrite);
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(strm, maps);
                strm.Close();

            }
            catch
            {
            }

            try
            {
                string FileName = "\\Groups.bak";

                Stream strm = new FileStream(FilePath + FileName, FileMode.Create, FileAccess.ReadWrite);
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(strm, groupList);
                strm.Close();
            }
            catch
            {
            }

        }

        void Load(bool _AutoLoad = false)
        {
            string FilePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase) + "\\back";
            FilePath = FilePath.Remove(0, 6);
            DirectoryInfo dir = new DirectoryInfo(FilePath);

            if (!dir.Exists)
            {
                isLoaded = true;
                return;
            }

            try
            {
                string FileName = "\\Maps.bak";
                Stream strm = new FileStream(FilePath + FileName, FileMode.Open, FileAccess.ReadWrite);
                IFormatter formatter = new BinaryFormatter();

                Maps temp = (Maps)formatter.Deserialize(strm);

                string MapFilePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase) + "\\Maps\\";
                MapFilePath = MapFilePath.Remove(0, 6);

                foreach (Map map in temp)
                {
                    maps.Add(map);

                    foreach (PanicControl panic in map.PanicList)
                    {
                        panic.SetMove(!(Properties.Settings.Default.IsButtonMoveLock));

                        if (Properties.Settings.Default.IsAdminLock)
                            panic.UnSetContextMenu();
                        else
                            panic.SetContextMenu();

                    }
                    foreach (PisControl pis in map.PisList)
                    {
                        pis.SetMove(!(Properties.Settings.Default.IsButtonMoveLock));
                    }


                    map.ImagePath = MapFilePath + map.FileName;

                }

                maps.IconLock = temp.IconLock;
                maps.IconSize = temp.IconSize;
                maps.SelectIndex = temp.SelectIndex;
                maps.MapMoveLock = temp.MapMoveLock;
                maps.ViewMapSize = temp.ViewMapSize;

                strm.Close();

                if (maps.Count > 0)
                {
                    zoomBox.ZoomMode = ZoomBoxLibrary.ZoomBoxPanel.eZoomMode.CustomSize;

                    zoomBox.Zoom = maps[maps.SelectIndex].Zoom;
                    zoomBox.PanX = maps[maps.SelectIndex].PosX;
                    zoomBox.PanY = maps[maps.SelectIndex].PosY;

                    cbMapList.SelectedIndex = maps.SelectIndex;

                }
            }
            catch (Exception ex)
            {
                WriteException(ex.ToString());
                //자동 재시도 합니다...
                if (!_AutoLoad)
                {
                    trace2("불러오기에 실패하여 최근 백업된 데이터를 불러옵니다.");
                    AutoLoad();
                }
                else
                {
                    trace2("맵 불러오기 실패");
                }
            }
            finally
            {
                isLoaded = true;
            }


            try
            {
                string FileName = "\\Groups.bak";
                Stream strm = new FileStream(FilePath + FileName, FileMode.Open, FileAccess.ReadWrite);
                IFormatter formatter = new BinaryFormatter();

                List<Group> temp = (List<Group>)formatter.Deserialize(strm);

                foreach (Group g in temp)
                {
                    groupList.Add(g);
                }

                strm.Close();
            }
            catch (Exception ex)
            {
                WriteException(ex.ToString());

                trace2("그룹 불러오기 실패");
            }
        }

        private void Setting()
        {
            MapMoveLock.IsChecked = false;//시작부터체크
            Properties.Settings.Default.IsMapMoveLock = false;//시작부터 체크해제
            ButtonMoveLock.IsChecked = Properties.Settings.Default.IsButtonMoveLock;
            //MapMoveLock.IsChecked           = Properties.Settings.Default.IsMapMoveLock;
            MapScaleLock.IsChecked = Properties.Settings.Default.IsMapScaleLock;
            AutoSearchPanicButton.IsChecked = Properties.Settings.Default.IsAutoSearchPanicButton;
            Properties.Settings.Default.nDvrSelect = 0;
        }

        public void StopProgress(object obj)
        {

        }

        // 네트워크 변경 이벤트
        void NetworkChange_NetworkAvailabilityChanged(object sender, NetworkAvailabilityEventArgs e)
        {
            if (e.IsAvailable)
            {
                Properties.Settings.Default.IsNetwork = true;
                Dispatcher.Invoke(_trace2, "네트워크에 연결되었습니다.");
                //  네트워크 사용 가능
            }
            else
            {
                Properties.Settings.Default.IsNetwork = false;
                Dispatcher.Invoke(_trace2, "네트워크와의 연결이 해제되었습니다. 연결 상태를 점검해 주세요");
                // 네트워크 사용 불가
            }
        }

        void MainWindow_Closed(object sender, EventArgs e)
        {
            try//라이브뷰어 삭제
            {
                foreach (Window win in Application.Current.Windows)
                {
                    if (win.Title == "TechLiveViewer")
                        win.Close();
                }
                //Window win = Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.Title == "TechLiveViewer");
                //win.Close();
            }
            catch (Exception eee)
            {
                trace("종료시 라이브뷰어 삭제 애러" + eee.Message);
                //LogManager.NormalTrace(eee.Message);       
            }

            //Save();
            trace("프로그램이 종료 되었습니다");
            //Properties.Settings.Default.Save();

            SerialView.CloseInstance();
        }

        public static void trace(object arg)
        {
            string tmps = "";
            DateTime now = DateTime.Now;
            tmps += now.ToLocalTime();
            tmps += ":";
            tmps += now.Millisecond;
            tmps += "\t";
            tmps += arg;

            LogList.Items.Insert(0, tmps);

            string FilePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase) + "\\Log";
            FilePath = FilePath.Remove(0, 6);
            DirectoryInfo dir = new DirectoryInfo(FilePath);

            if (!dir.Exists)
                dir.Create();

            string filename = FilePath + "\\" + now.ToShortDateString() + ".txt";
            FileStream file = new FileStream(filename, FileMode.Append, FileAccess.Write);
            StreamWriter sw = new StreamWriter(file);
            sw.WriteLine(tmps);
            sw.Close();

        }

        public static void trace2(object arg)
        {
            string tmps = "";
            DateTime now = DateTime.Now;
            tmps += now.ToLocalTime();
            tmps += ":";
            tmps += now.Millisecond;
            tmps += "\t";
            tmps += arg;

            TextBlock tb = new TextBlock();

            tb.Text = tmps;
            tb.Foreground = Brushes.Red;
            LogList.Items.Insert(0, tb);

            string FilePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase) + "\\Log";
            FilePath = FilePath.Remove(0, 6);
            DirectoryInfo dir = new DirectoryInfo(FilePath);

            if (!dir.Exists)
                dir.Create();

            string filename = FilePath + "\\UIException.txt";
            FileStream file = new FileStream(filename, FileMode.Append, FileAccess.Write);
            StreamWriter sw = new StreamWriter(file);
            sw.WriteLine(tmps);
            sw.Close();
        }

        public static void CheckTrace(object arg)
        {
            string tmps = "";
            DateTime now = DateTime.Now;
            tmps += now.ToLocalTime();
            tmps += ":";
            tmps += now.Millisecond;
            tmps += "\t";
            tmps += arg;

            TextBlock tb = new TextBlock();

            tb.Text = tmps;
            tb.Foreground = Brushes.Orange;
            LogList.Items.Insert(0, tb);

            string FilePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase) + "\\Log";
            FilePath = FilePath.Remove(0, 6);
            DirectoryInfo dir = new DirectoryInfo(FilePath);

            if (!dir.Exists)
                dir.Create();

            string filename = FilePath + "\\" + now.ToShortDateString() + ".txt";
            FileStream file = new FileStream(filename, FileMode.Append, FileAccess.Write);
            StreamWriter sw = new StreamWriter(file);
            sw.WriteLine(tmps);
            sw.Close();
        }

        public static void WriteLog(object arg)
        {
            string tmps = "";
            DateTime now = DateTime.Now;
            tmps += now.ToLocalTime();
            tmps += ":";
            tmps += now.Millisecond;
            tmps += "\t";
            tmps += arg;

            string FilePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase) + "\\NormalLog";
            FilePath = FilePath.Remove(0, 6);
            DirectoryInfo dir = new DirectoryInfo(FilePath);

            if (!dir.Exists)
                dir.Create();

            string filename = FilePath + "\\" + now.ToShortDateString() + ".txt";
            FileStream file = new FileStream(filename, FileMode.Append, FileAccess.Write);
            StreamWriter sw = new StreamWriter(file);
            sw.WriteLine(tmps);
            sw.Close();
        }

        public static void WriteException(object arg)
        {
            string tmps = "";
            DateTime now = DateTime.Now;
            tmps += now.ToLocalTime();
            tmps += ":";
            tmps += now.Millisecond;
            tmps += "\t";
            tmps += arg;

            string FilePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase) + "\\Exception";
            FilePath = FilePath.Remove(0, 6);
            DirectoryInfo dir = new DirectoryInfo(FilePath);

            if (!dir.Exists)
                dir.Create();

            string filename = FilePath + "\\" + now.ToShortDateString() + ".txt";
            FileStream file = new FileStream(filename, FileMode.Append, FileAccess.Write);
            StreamWriter sw = new StreamWriter(file);
            sw.WriteLine(tmps);
            sw.Close();
        }

        public static void panicTrace(object arg)
        {
            string tmps = "";
            DateTime now = DateTime.Now;
            tmps += now.ToLocalTime();
            tmps += ":";
            tmps += now.Millisecond;
            tmps += "\t";
            tmps += arg;

            TextBlock tb = new TextBlock();

            tb.Text = tmps;
            tb.Foreground = Brushes.Red;
            LogList.Items.Insert(0, tb);

            string FilePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase) + "\\PanicLog";
            FilePath = FilePath.Remove(0, 6);
            DirectoryInfo dir = new DirectoryInfo(FilePath);

            if (!dir.Exists)
                dir.Create();

            string filename = FilePath + "\\" + now.ToShortDateString() + ".txt";
            FileStream file = new FileStream(filename, FileMode.Append, FileAccess.Write);
            StreamWriter sw = new StreamWriter(file);
            sw.WriteLine(tmps);
            sw.Close();
        }

        //---------------------------------------------------------------------------------------
        // ● Description   : 네트워크 활성화 여부를 판단하여 셋팅
        // ● Date          : 2011년 1월
        //---------------------------------------------------------------------------------------
        // ○ 이 함수는 SMS관련 함수가 호출되기 전에 호출되어야한다.
        //---------------------------------------------------------------------------------------
        private void InitNetwork()
        {
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                Properties.Settings.Default.IsNetwork = true;
            }
            else
            {
                trace("네트워크에 연결이 되어있지 않습니다.");
                Properties.Settings.Default.IsNetwork = false;
            }
        }

        private void InitLogListBox()
        {
            LogList = new ListBox();
            grid.Children.Add(LogList);
            LogList.Height = 150.0;
            LogList.SetValue(NameProperty, "LogBox");
            LogList.SetValue(Grid.RowProperty, 3);
            LogList.SetValue(Grid.ColumnSpanProperty, 2);
            LogList.BorderThickness = new Thickness(0);
            // LogList.Background = new SolidColorBrush(Color.FromArgb(255, 35, 35, 38)); 

            LogList.SelectionMode = SelectionMode.Single;
        }

        private void InitHandler()
        {
            maps.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(maps_CollectionChanged);
            cbMapList.SelectionChanged += new SelectionChangedEventHandler(cbMapList_SelectionChanged);
            this.Closed += new EventHandler(MainWindow_Closed);
            NetworkChange.NetworkAvailabilityChanged += new NetworkAvailabilityChangedEventHandler(NetworkChange_NetworkAvailabilityChanged);
        }

        private void InitZoomBox()
        {
            BitmapImage tmp = new BitmapImage(new Uri("Pack://application:,,,/Images/intro.bmp"));
            zoomBox.PanX = (zoomBox.RenderSize.Width / 2) - (tmp.Width / 2);
            zoomBox.PanY = (zoomBox.RenderSize.Height / 2) - (tmp.Height / 2);
            MapImage.Source = tmp;
            zoomBox.ApplyZoom(true);
        }

        private void AppPropertyies()
        {
            Application.Current.Properties["MainWindow"] = this;
            Application.Current.Properties["Root"] = Root;
            Application.Current.Properties["zoomBox"] = zoomBox;
            Application.Current.Properties["IconCanvas"] = IconCanvas;
            Application.Current.Properties["MapImage"] = MapImage;
            Application.Current.Properties["btIconAdd"] = btIconAdd;

            Application.Current.Properties["ControlCanvas"] = ControlCanvas;
            Application.Current.Properties["cbMapList"] = cbMapList;
            Application.Current.Properties["GroupList"] = groupList;
            //Application.Current.Properties["ElevatorList"] = ElevatorList;


            storyMapAdd = (Storyboard)AddMap.Resources["MapAddCtlStory"];
        }

        private void MpaSetting()
        {
            zoomBox.Width = grid.ActualWidth;
            //zoomBox.Height = grid.RowDefinitions[1].ActualHeight - grid.RowDefinitions[0].ActualHeight;
            zoomBox.Height = grid.RowDefinitions[1].ActualHeight;

            Size MapSize = new Size();
            MapSize.Height = grid.RowDefinitions[1].ActualHeight;
            MapSize.Width = grid.ActualWidth;

            cbMapList.ItemsSource = maps;
            maps.ViewMapSize = MapSize;

            System.Windows.Controls.ContextMenu menu = new System.Windows.Controls.ContextMenu();

            MenuItem item_1 = new MenuItem();
            MenuItem item_2 = new MenuItem();

            item_1.Header = "맵 수정";
            item_2.Header = "맵 삭제";

            menu.Items.Add(item_1);
            menu.Items.Add(item_2);

            MapImage.ContextMenu = menu;
            item_1.Click += new RoutedEventHandler(item_1_Click);
            item_2.Click += new RoutedEventHandler(item_2_Click);
        }

        void item_1_Click(object sender, RoutedEventArgs e)
        {
            MapInfo info = new MapInfo(this);

            info.Top = Root.ActualHeight / 2 - info.Height * 2;
            info.Left = Root.ActualWidth / 2 - info.Width / 2;
            info.ShowDialog();

            cbMapList.SelectedIndex = maps.SelectIndex;

        }

        void item_2_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBoxResult.Yes == MessageBox.Show("해당 맵의 모든 정보가 삭제됩니다 정말 지우시겠습니까?", "맵삭제", MessageBoxButton.YesNo))
            {
                //cbMapList.Items.Remove(maps[maps.SelectIndex]);

                try
                {
                    while (maps[maps.SelectIndex].PanicList.Count > 0)
                    {
                        maps[maps.SelectIndex].PanicList[0].RemoveIocn();
                    }

                   


                    maps.RemoveAt(maps.SelectIndex);

                    foreach (UIElement element in Root.Children)
                    {
                        if (element.GetType() == new AddIcon().GetType())
                        {
                            Root.Children.Remove(element as AddIcon);
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    trace2(ex.Message);
                }

            }
        }


        bool IsLock()
        {
            if (Properties.Settings.Default.IsAdminLock)
            {
                InputPassword pass = new InputPassword();
                pass.Top = Root.ActualHeight / 2 - pass.Height;
                pass.Left = Root.ActualWidth / 2 - pass.Width / 2;

                if (pass.ShowDialog() == false)
                {
                    MessageBox.Show("잘못된 비밀번호 입니다!!", "Message");
                    return true;
                }
            }

            return false;
        }

        public static bool IsNetwork()
        {
            if (Properties.Settings.Default.IsNetwork)
            {
                return false;
            }

            MessageBox.Show("네트워크에 연결이 되어있지않으면 이용이 불가능합니다.", "Message");
            return true;
        }


       

        TimeSpan LastAdd;
        public void Add(double X, double Y)
        {
            Random Rnadom = new Random(DateTime.Now.Millisecond);

            if ((DateTime.Now.TimeOfDay - LastAdd).Milliseconds < 50) return;
            LastAdd = DateTime.Now.TimeOfDay;

            Ellipse Ellipse = new Ellipse();
            Ellipse.Stroke = GetRandomColorBrush();

            int Size = (Byte)Rnadom.Next(5, 15);
            Ellipse.Width = Ellipse.Height = Size;
            Ellipse.Name = "_" + Ellipse.GetHashCode().ToString();

            Canvas.SetLeft(Ellipse, X - Size / 2);
            Canvas.SetTop(Ellipse, Y - Size / 2);

            Root.RegisterName(Ellipse.Name, Ellipse);
            Root.Children.Add(Ellipse);

            Storyboard EllipseStoryboard = (Resources["EllipseStoryBoard"] as Storyboard).Clone();
            Storyboard.SetTargetName(EllipseStoryboard, Ellipse.Name);

            EllipseStoryboard.Completed += new EventHandler(EllipseStoryboard_Completed);
            Ellipse.BeginStoryboard(EllipseStoryboard);

            //    Ellipse.ReleaseMouseCapture();

        }

        SolidColorBrush GetRandomColorBrush()
        {
            Random Rnadom = new Random(DateTime.Now.Millisecond);
            Color StrokeColor = Color.FromArgb(
            (Byte)Rnadom.Next(80, 255),
            (Byte)Rnadom.Next(0, 255),
            (Byte)Rnadom.Next(0, 255),
            (Byte)Rnadom.Next(0, 255)
            );

            return new SolidColorBrush(StrokeColor);
        }

        void EllipseStoryboard_Completed(object sender, EventArgs e)
        {

            Storyboard Storyboard = (sender as ClockGroup).Timeline as Storyboard;
            Ellipse Ellipse = FindName(Storyboard.GetTargetName(Storyboard)) as Ellipse;

            Root.UnregisterName(Ellipse.Name);
            Root.Children.Remove(Ellipse);
        }

        void cbMapList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbMapList.SelectedIndex > -1)
            {
                lock (this)
                {
                    zoomBox.ApplyZoom(true);
                    maps[maps.SelectIndex].Zoom = zoomBox.Zoom;
                    maps[maps.SelectIndex].PosX = zoomBox.PanX;
                    maps[maps.SelectIndex].PosY = zoomBox.PanY;

                    maps.SelectIndex = cbMapList.SelectedIndex;
                    try
                    {
                        MapImage.Source = new BitmapImage(new Uri(maps[maps.SelectIndex].ImagePath));
                    }
                    catch
                    {
                        MessageBox.Show(maps[maps.SelectIndex].ImagePath + "를 찾을수 없습니다", "파일오류");
                    }
                    SetViewitem();

                    zoomBox.Zoom = maps[maps.SelectIndex].Zoom;
                    zoomBox.PanX = maps[maps.SelectIndex].PosX;
                    zoomBox.PanY = maps[maps.SelectIndex].PosY;

                    zoomBox.ApplyZoom(true);


                }
            }
        }


        void maps_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                if (isLoaded)
                {
                    maps[maps.SelectIndex].Zoom = zoomBox.Zoom;
                    maps[maps.SelectIndex].PosX = zoomBox.PanX;
                    maps[maps.SelectIndex].PosY = zoomBox.PanY;

                    maps.SelectIndex = e.NewStartingIndex;

                    lock (this)
                    {
                        BitmapImage tmp = new BitmapImage(new Uri(maps[e.NewStartingIndex].ImagePath));
                        MapImage.Source = tmp;

                        maps[e.NewStartingIndex].PosY = (zoomBox.RenderSize.Height / 2) - (tmp.Height / 2);
                        maps[e.NewStartingIndex].PosX = (zoomBox.RenderSize.Width / 2) - (tmp.Width / 2);

                        zoomBox.PanX = maps[e.NewStartingIndex].PosX;
                        zoomBox.PanY = maps[e.NewStartingIndex].PosY;

                        zoomBox.Zoom = 100;

                        cbMapList.SelectedIndex = e.NewStartingIndex;
                    }
                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                if (maps.Count == 0)
                {
                    MapImage.Source = null;
                    IconCanvas.Children.Clear();
                }
                else
                {
                    MapImage.Source = new BitmapImage(new Uri(maps[0].ImagePath));
                    maps.SelectIndex = 0;
                    SetViewitem();

                    zoomBox.Zoom = maps[0].Zoom;
                    zoomBox.PanX = maps[0].PosX;
                    zoomBox.PanY = maps[0].PosY;

                    cbMapList.SelectedIndex = 0;
                }
            }
        }

        


        private void zoomBox_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (cbMapList.SelectedIndex > -1)
            {
               // maps[maps.SelectIndex].Zoom = zoomBox.Zoom;
               // SetIconScale(e);
            }
        }

        private void AddMap_MouseLeave(object sender, MouseEventArgs e)
        {
            if (AddMap.isview == true)
                return;

            storyMapAdd.Begin();
            storyMapAdd.Remove();

            AddMap.isview = false;
            AddMap.Visibility = Visibility.Hidden;

            cbMapList.Focus();
        }

        public void AnswerIcon()
        {
            foreach (PanicControl pc in maps.NotInsertPanic)
            {
                foreach (Map map in maps)
                {
                    foreach (PanicControl _pc in map.PanicList)
                    {
                        if (pc.Addr == _pc.Addr)
                        {
                            maps.NotInsertPanic.Remove(pc);
                            return;
                        }
                    }
                }
            }

            foreach (UIElement element in Root.Children)
            {
                if (element.GetType().Equals(new AddIcon().GetType()))
                {
                    ((AddIcon)element).SerchInsertPanic();
                    break;
                }
            }

            btIconAdd.PlayNewStory();
        }




        public void SetViewitem()
        {
            IconCanvas.Children.Clear();

            foreach (PanicControl panic in maps[maps.SelectIndex].PanicList)
            {
                IconCanvas.Children.Add(panic);
            }
            foreach (PisControl pis in maps[maps.SelectIndex].PisList)
            {
                IconCanvas.Children.Add(pis);
            }

        }

        public void SetIconScale(MouseWheelEventArgs e)
        {
            //if (zoomBox != null)
            //{
            //    if (e.Delta > 0)
            //    {
            //        NavigationCommands.IncreaseZoom.Execute(null, zoomBox);
            //        NavigationCommands.IncreaseZoom.Execute(null, maps[0].PanicList[0]);
            //    }
            //    if (e.Delta < 0)
            //    {
            //        NavigationCommands.DecreaseZoom.Execute(null, zoomBox);
            //    }

                
            //    maps[maps.SelectIndex].Zoom = zoomBox.Zoom;
            //}
            
        }


        private void MapMoveLock_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.IsMapMoveLock = true;
            zoomBox.MouseMode = ZoomBoxLibrary.ZoomBoxPanel.eMouseMode.None;
            maps.MapMoveLock = true;
            movelock = false;
        }

        private void MapMoveLock_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.IsMapMoveLock = false;
            zoomBox.MouseMode = ZoomBoxLibrary.ZoomBoxPanel.eMouseMode.Pan;
            maps.MapMoveLock = false;
            movelock = true;
        }

        private void MapScaleLock_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.IsMapScaleLock = true;
            zoomBox.WheelMode = ZoomBoxLibrary.ZoomBoxPanel.eWheelMode.None;
            zoomBoxSilder.Visibility = Visibility.Hidden;
        }

        private void MapScaleLock_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.IsMapScaleLock = false;
            zoomBox.WheelMode = ZoomBoxLibrary.ZoomBoxPanel.eWheelMode.Zoom;
            zoomBoxSilder.Visibility = Visibility.Visible;
        }

        private void ButtonMoveLock_Click(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.IsAdminLock)
            {
                maps.IconLock = true;
                ButtonMoveLock.IsChecked = true;
                MessageBox.Show("관리자 설정 해제후 사용해 주세요", "버튼이동");
            }
        }

        private void ButtonMoveLock_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.IsButtonMoveLock = false;

            if (maps.Count < 1)
                return;

            maps.IconLock = false;

            foreach (Map map in maps)
            {
                foreach (PanicControl panic in map.PanicList)
                    panic.SetMove(true);
                foreach (PisControl pis in map.PisList)
                    pis.SetMove(true);
            }
        }

        private void ButtonMoveLock_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.IsButtonMoveLock = true;

            if (maps.Count < 1)
                return;

            maps.IconLock = true;

            foreach (Map map in maps)
            {
                foreach (PanicControl panic in map.PanicList)
                    panic.SetMove(false);
                foreach (PisControl pis in map.PisList)
                    pis.SetMove(false);

            }
        }



        private void AutoSearchPanicButton_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.IsAutoSearchPanicButton = false;
        }

        private void AutoSearchPanicButton_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.IsAutoSearchPanicButton = true;
        }





        private void btIconAdd_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (IsLock())
                return;

            AddIcon addIcon = new AddIcon();

            foreach (UIElement element in Root.Children)
            {
                if (element.GetType() == addIcon.GetType())
                {
                    Root.Children.Remove(element as AddIcon);
                    break;
                }
            }

            Root.Children.Add(addIcon);
            addIcon.SetValue(Canvas.TopProperty, 330.0);
            addIcon.SetValue(Canvas.LeftProperty, 20.0);
            addIcon.SetValue(Panel.ZIndexProperty, 10);
        }

        private void btGroupControl_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            AddGroup addGroup = new AddGroup();

            foreach (UIElement element in Root.Children)
            {
                if (element.GetType() == addGroup.GetType())
                {
                    Root.Children.Remove(element as AddGroup);
                    break;
                }
            }

            isOpenAddGroup = true;
            Root.Children.Add(addGroup);
            addGroup.SetValue(Canvas.TopProperty, 120.0);
            addGroup.SetValue(Canvas.LeftProperty, 0.0);
            addGroup.SetValue(Panel.ZIndexProperty, 10);
        }

        private void btMapAddControl_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (IsLock())
                return;

            AddMap.Visibility = Visibility.Visible;
            storyMapAdd.Begin();

        }


        public void Exit()
        {
            Environment.Exit(0);
            System.Diagnostics.Process.GetCurrentProcess().Kill();
            this.Close();
        }



        //~추가: 차 인식 프로그램 시작 
        private void btStartControl_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            startDetection();
        }
        private int startDetection()
        {
            saveROI();

            string detectorPath = "C:\\Program Files (x86)\\Twowinscom\\Parking Guidance\\Detector\\Detector.exe";

            //이미 동작 중이면 정지, UI 버튼을 시작으로 변경
            if (isStart)
            {
                try
                {
                    Process[] processList = Process.GetProcessesByName("Detector");

                    if (processList.Length > 0)
                    {
                        processList[0].Kill();
                    }
                    isStart = false;

                    Dispatcher.BeginInvoke(new Action(delegate()
                    {
                        btStartControl.icon.Source = new BitmapImage(new Uri("C:\\Program Files (x86)\\Twowinscom\\Parking Guidance\\images\\start.png", UriKind.Absolute));
                        btStartControl.text.Text = "시작";
                    }));
                   
                }
                catch (Exception ex)
                {
                    trace2("차량 번호 인식 프로그램 종료 실패");
                }
                return 0;
            }

            //프로그램 시작,   UI  버튼을 중지로 변경
            if (System.IO.File.Exists(detectorPath))
            {
                try
                {
                    System.Diagnostics.ProcessStartInfo Run = new System.Diagnostics.ProcessStartInfo();
                    Run.FileName = detectorPath;
                    Process mfcApp = System.Diagnostics.Process.Start(Run);
                    isStart = true;

                    Dispatcher.BeginInvoke(new Action(delegate()
                    {
                        btStartControl.icon.Source = new BitmapImage(new Uri("C:\\Program Files (x86)\\Twowinscom\\Parking Guidance\\images\\pause.png", UriKind.Absolute));
                        btStartControl.text.Text = "중지";
                    }));

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
                MessageBox.Show("Detector is not exist");

            return 1;
        }


        private void btAdminControl_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (IsLock())
                return;

            Configuration.Admin admin = new Configuration.Admin();
            admin.Top = Root.ActualHeight / 2 - admin.Height / 2;
            admin.Left = Root.ActualWidth / 2 - admin.Width / 2;

            if (admin.ShowDialog() == true)
            {
                if (Properties.Settings.Default.IsAdminLock)
                {
                    maps.IconLock = true;
                    ButtonMoveLock.IsChecked = true;

                    foreach (Map map in maps)
                    {
                        foreach (PanicControl panic in map.PanicList)
                        {
                            panic.SetMove(false);
                            panic.UnSetContextMenu();
                        }

                    }
                }
                else
                {
                    foreach (Map map in maps)
                    {
                        foreach (PanicControl panic in map.PanicList)
                        {
                            panic.SetContextMenu();
                        }
                    }
                }
            }
        }

        public void ConnectTrace(object arg)
        {
            ConnectState.Text = arg.ToString();
        }

        private void btSerialView_Click(object sender, RoutedEventArgs e)
        {
            if (IsLock())
                return;

            //Configuration.ConnectionState sv = new Configuration.ConnectionState();
            SerialView sv = SerialView.getInstance();
            sv.Top = Root.ActualHeight / 2 - sv.Height / 2;
            sv.Left = Root.ActualWidth / 2 - sv.Width / 2;
            sv.Owner = this;//실행시 최상위 유지 
            sv.Show();
            //SerialView sv = SerialView.getInstance();

        }


        public void Minimize()
        {
            isMinimized = true;
            WindowState = WindowState.Minimized;
        }


        protected override void OnActivated(EventArgs e)
        {
            if (isMinimized == true)
            {
                isMinimized = false;
            }

            base.OnActivated(e);
        }

        protected override void OnDeactivated(EventArgs e)
        {
            base.OnDeactivated(e);
        }

        private void AutoLoad()
        {
            string FilePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase) + "\\back";
            FilePath = FilePath.Remove(0, 6);
            DirectoryInfo dir1 = new DirectoryInfo(FilePath);

            if (!dir1.Exists)
                dir1.Create();

            DirectoryInfo dir2 = new DirectoryInfo(dir1.FullName + "\\" + Properties.Settings.Default.LastBackupDate);

            if (dir2.Exists)
            {
                return;
            }

            //----------------- 백업파일 검사
            DirectoryInfo dir3 = new DirectoryInfo(dir2 + "\\back");
            DirectoryInfo dir4 = new DirectoryInfo(dir2 + "\\maps");
            DirectoryInfo dir5 = new DirectoryInfo(dir2 + "\\wav");

            if (!dir3.Exists || !dir4.Exists || !dir5.Exists)
            {
                return;
            }

            //----------------- 붙여넣기할 폴더 검사
            string BackPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase) + "\\back";
            string MapPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase) + "\\maps";
            string WavPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase) + "\\wav";

            BackPath = BackPath.Remove(0, 6);
            MapPath = MapPath.Remove(0, 6);
            WavPath = WavPath.Remove(0, 6);

            DirectoryInfo dirBack = new DirectoryInfo(BackPath);
            DirectoryInfo dirMap = new DirectoryInfo(MapPath);
            DirectoryInfo dirWav = new DirectoryInfo(WavPath);


            if (!dirBack.Exists)
                dirBack.Create();

            if (!dirMap.Exists)
                dirMap.Create();

            if (!dirWav.Exists)
                dirWav.Create();

            //----------------- 맵초기화
            while (maps.Count > 0)
            {
                maps.SelectIndex = 0;
                while (maps[0].PanicList.Count > 0)
                {
                    maps[0].PanicList[0].RemoveIocn();
                }


                maps.RemoveAt(0);
            }
            maps.Clear();

            foreach (UIElement element in Root.Children)
            {
                if (element.GetType() == new AddIcon().GetType())
                {
                    Root.Children.Remove(element as AddIcon);
                    break;
                }
            }


            //----------------- 백업파일 복사

            foreach (FileInfo f in dir1.GetFiles())
            {
                f.CopyTo(BackPath + "\\" + f.Name, true);
            }

            foreach (FileInfo f in dir2.GetFiles())
            {
                try
                {
                    f.CopyTo(MapPath + "\\" + f.Name, true);
                }
                catch
                {
                }
            }

            foreach (FileInfo f in dir3.GetFiles())
            {
                try
                {
                    f.CopyTo(WavPath + "\\" + f.Name, true);
                }
                catch
                {
                }
            }

            //----------------- 불러오기
            Load(true);

            maps.NotInsertPanic.Clear();
            btIconAdd.StopNewStory();
        }



        private void btExportControl_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (IsLock())
                return;

            System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog();
            dlg.Description = "현재 상태를 백업할 폴더를 선택해 주세요.";
            System.Windows.Forms.DialogResult result = dlg.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                Save();

                DirectoryInfo dir1 = new DirectoryInfo(dlg.SelectedPath + "\\DataBackup");

                if (!dir1.Exists)
                    dir1.Create();

                DirectoryInfo dir2 = new DirectoryInfo(dir1.FullName + "\\" + DateTime.Now.ToShortDateString());

                if (dir2.Exists)
                {
                    MessageBoxResult mr = MessageBox.Show("이미 저장된 파일이 있습니다. 파일을 덮어 쓰시겠습니까?", "데이터 백업", MessageBoxButton.OKCancel);

                    if (mr == MessageBoxResult.No)
                    {
                        return;
                    }
                }
                else
                {
                    dir2.Create();
                }

                DirectoryInfo dir_back = new DirectoryInfo(dir2.FullName + "\\back");
                DirectoryInfo dir_map = new DirectoryInfo(dir2.FullName + "\\Maps");
                DirectoryInfo dir_wav = new DirectoryInfo(dir2.FullName + "\\wav");

                if (!dir_back.Exists)
                    dir_back.Create();

                if (!dir_map.Exists)
                    dir_map.Create();

                if (!dir_wav.Exists)
                    dir_wav.Create();

                string FilePath1 = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase) + "\\back";
                string FilePath2 = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase) + "\\Maps";
                string FilePath3 = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase) + "\\wav";

                FilePath1 = FilePath1.Remove(0, 6);
                FilePath2 = FilePath2.Remove(0, 6);
                FilePath3 = FilePath3.Remove(0, 6);

                DirectoryInfo d1 = new DirectoryInfo(FilePath1);
                DirectoryInfo d2 = new DirectoryInfo(FilePath2);
                DirectoryInfo d3 = new DirectoryInfo(FilePath3);

                if (d1.Exists)
                {
                    FileInfo[] files = d1.GetFiles();

                    foreach (FileInfo file in files)
                    {
                        try
                        {
                            FileInfo s_file = new FileInfo(file.FullName);
                            FileInfo c_file = s_file.CopyTo(dir_back.FullName + "\\" + file.Name, true);
                        }
                        catch
                        {

                        }
                    }
                }

                if (d2.Exists)
                {
                    FileInfo[] files = d2.GetFiles();

                    foreach (FileInfo file in files)
                    {
                        try
                        {
                            FileInfo s_file = new FileInfo(file.FullName);
                            FileInfo c_file = s_file.CopyTo(dir_map.FullName + "\\" + file.Name, true);
                        }
                        catch
                        {

                        }
                    }
                }

                if (d3.Exists)
                {
                    FileInfo[] files = d3.GetFiles();

                    foreach (FileInfo file in files)
                    {
                        try
                        {
                            FileInfo s_file = new FileInfo(file.FullName);
                            FileInfo c_file = s_file.CopyTo(dir_wav.FullName + "\\" + file.Name, true);
                        }
                        catch
                        {

                        }
                    }
                }
            }
        }

        private void btSave_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Save();
            saveROI();
            Properties.Settings.Default.Save();
            MessageBox.Show("저장완료");
        }

        private void btLoadControl_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (IsLock())
                return;

            System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog();
            dlg.ShowNewFolderButton = false;
            dlg.Description = "저장된 파일이 들어있는 폴더를 선택해 주세요.";
            System.Windows.Forms.DialogResult result = dlg.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                //----------------- 백업파일 검사
                DirectoryInfo dir1 = new DirectoryInfo(dlg.SelectedPath + "\\back");
                DirectoryInfo dir2 = new DirectoryInfo(dlg.SelectedPath + "\\maps");
                DirectoryInfo dir3 = new DirectoryInfo(dlg.SelectedPath + "\\wav");

                if (!dir1.Exists || !dir3.Exists || !dir2.Exists)
                {
                    MessageBox.Show("백업된 데이터가 없습니다!! 정확한 경로를 선택해 주세요.", "불러오기 오류");
                    return;
                }

                //----------------- 붙여넣기할 폴더 검사
                string BackPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase) + "\\back";
                string MapPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase) + "\\maps";
                string WavPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase) + "\\wav";

                BackPath = BackPath.Remove(0, 6);
                MapPath = MapPath.Remove(0, 6);
                WavPath = WavPath.Remove(0, 6);

                DirectoryInfo dirBack = new DirectoryInfo(BackPath);
                DirectoryInfo dirMap = new DirectoryInfo(MapPath);
                DirectoryInfo dirWav = new DirectoryInfo(WavPath);


                if (!dirBack.Exists)
                    dirBack.Create();

                if (!dirMap.Exists)
                    dirMap.Create();

                if (!dirWav.Exists)
                    dirWav.Create();

                //----------------- 맵초기화
                while (maps.Count > 0)
                {
                    maps.SelectIndex = 0;
                    while (maps[0].PanicList.Count > 0)
                    {
                        maps[0].PanicList[0].RemoveIocn();
                    }

                    maps.RemoveAt(0);
                }
                maps.Clear();

                foreach (UIElement element in Root.Children)
                {
                    if (element.GetType() == new AddIcon().GetType())
                    {
                        Root.Children.Remove(element as AddIcon);
                        break;
                    }
                }


                //----------------- 백업파일 복사

                foreach (FileInfo f in dir1.GetFiles())
                {
                    f.CopyTo(BackPath + "\\" + f.Name, true);
                }

                foreach (FileInfo f in dir2.GetFiles())
                {
                    try
                    {
                        f.CopyTo(MapPath + "\\" + f.Name, true);
                    }
                    catch
                    {
                    }
                }

                foreach (FileInfo f in dir3.GetFiles())
                {
                    try
                    {
                        f.CopyTo(WavPath + "\\" + f.Name, true);
                    }
                    catch
                    {
                    }
                }

                //----------------- 불러오기
                Load();

                maps.NotInsertPanic.Clear();
                btIconAdd.StopNewStory();

            }

        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

            e.Cancel = ExitBan;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (IsRequestingOptions)
            {
                ExitBan = false;
                ShowSecretOption();
                ExitBan = true;
            }
            else if (IsAllSelectKey) //키비교 ctrl + A
            {
                if (isOpenAddGroup) //그룹 선택이 되있는지 확인
                {
                    foreach (PanicControl panic in maps[maps.SelectIndex].PanicList)
                    {
                        selectedGroup.AddPanic(panic);
                        panic.selectedPanic();
                    }
                }
            }
            else if (IsAllCancelKey) //키비교 ctrl + D
            {
                if (isOpenAddGroup)
                {
                    foreach (PanicControl panic in maps[maps.SelectIndex].PanicList)
                    {
                        selectedGroup.RemovePanic(panic);
                        panic.unSelectedPanic();
                    }
                }
            }
            else
            {
                ExitBan = true;
            }

            base.OnKeyDown(e);
        }

        private void ShowSecretOption()
        {
            SecretOption Option = new SecretOption(this);
            Option.Top = Root.ActualHeight / 2 - Option.Height / 2;
            Option.Left = Root.ActualWidth / 2 - Option.Width / 2;

            Option.ShowDialog();
        }

        private void btKeyboardControl_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            string KeyboardPath = @"C:\Windows\system32\osk.exe";
            if (System.IO.File.Exists(KeyboardPath))
            {
                System.Diagnostics.ProcessStartInfo Run = new System.Diagnostics.ProcessStartInfo();
                Run.FileName = KeyboardPath;
                System.Diagnostics.Process.Start(Run);
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {

        }

        private void changedValue(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            scale = e.NewValue;
            foreach (Map map in maps)
            {
                foreach (PanicControl panic in map.PanicList)
                {
                    panic.panicScaleTransform.ScaleX = e.NewValue;
                    panic.panicScaleTransform.ScaleY = e.NewValue;
                }

                foreach (PisControl pis in map.PisList)
                {
                    pis.PisScaleTransform.ScaleX = e.NewValue;
                    pis.PisScaleTransform.ScaleY = e.NewValue;
                    //pis.RenderTransform = new ScaleTransform(e.NewValue, e.NewValue);
                }
            }
        }


        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SelectButton_Checked(object sender, RoutedEventArgs e)
        {
            isSelectChecked = true;
            
        }

        private void SelectButton_Unchecked(object sender, RoutedEventArgs e)
        {
            isSelectChecked = false;

            foreach (Map map in MainWindow.maps)
                foreach (PanicControl panic in map.PanicList)
                    panic.unSelectedPanic();

            zoomBox.MouseMode = ZoomBoxLibrary.ZoomBoxPanel.eMouseMode.Pan;
        }

       

        //Rectangle rec = new Rectangle();
        //public static bool isMousePress;
        //double x, y;
        //Point firstMousePoint;
        //private void ControlCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        //{
        //    if (isSelectChecked == false)
        //        return;
        //    Point p = e.GetPosition(can);
        //    isMousePress = true;
        //    x = p.X;
        //    y = p.Y;

        //    rec = new Rectangle();
        //    rec.Fill = new SolidColorBrush(Color.FromArgb(30, 30, 30, 150));
        //    rec.Stroke = Brushes.Blue;
        //    rec.StrokeThickness = 2;

        //    can.Children.Add(rec);
        //}

        //private void ControlCanvas_MouseMove(object sender, MouseEventArgs e)
        //{
        //    Point p = e.GetPosition(can);
        //    if (isMousePress)
        //    {
        //        rec.Margin = new Thickness((p.X < x) ? p.X : x, (p.Y < y) ? p.Y : y, 0, 0);
        //        rec.Width = Math.Abs(p.X - x);
        //        rec.Height = Math.Abs(p.Y - y);
        //    }
        //}


        //private void ControlCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        //{
        //    isMousePress = false;
        //    can.Children.Remove(rec);
        //}

        //private void selectedIcon()
        //{
        //    foreach (Map map in maps)
        //    {
        //        foreach(PanicControl panic in map.PanicList)
        //        {
        //            if(panic.
        //        }
        //        foreach (PisControl pis in map.PisList)
        //        {
        //            if(pis.BoundsRelativeTo(
        //        }
        //    }
        //}
        //private void setSelectionRect()
        //{
        //    int x, y;
        //    int w, h;
              
        //    //point selectionStart,end
        //    x = selectionStart.X > selectionEnd.X ? selectionEnd.X : selectionStart.X;
        //    y = selectionStart.Y > selectionEnd.Y ? selectionEnd.Y : selectionEnd.Y;

        //    w = selectionStart.X > selectionEnd.X ? selectionStart.X - selectionEnd.X : selectionEnd.X - selectionStart.X;
        //    y = selectionStart.Y > selectionEnd.Y ? selectionStart.Y - selectionEnd.Y : selectionEnd.Y - selectionEnd.Y;

        //    //rectangle
        //    selection = new Rectangle(x, y, w, h);
        //}

    }

    public class ZoomModeListConverter : TWWPFUtilityLib.TWEnumListConverter<ZoomBoxLibrary.ZoomBoxPanel.eZoomMode> { }
    public class MouseModeListConverter : TWWPFUtilityLib.TWEnumListConverter<ZoomBoxLibrary.ZoomBoxPanel.eMouseMode> { }
    public class WheelModeListConverter : TWWPFUtilityLib.TWEnumListConverter<ZoomBoxLibrary.ZoomBoxPanel.eWheelMode> { }
}