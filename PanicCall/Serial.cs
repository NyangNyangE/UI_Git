using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Timers;
using System.Windows;
using System.Runtime.InteropServices;

namespace PanicCall
{
    public class Serial
    {


        public delegate void _delegate();
        public delegate void _delegate_arg(object obj);
        public delegate void cognition_delegate(int a, int b);

        _delegate_arg trace;
        _delegate_arg WriteException;
        _delegate_arg SerialSend;
        _delegate_arg SerialRecive;
        _delegate_arg ConnectTrace;

        DateTime oldtime;
        MainWindow Parent;
        SerialPort serial;
        int packet_step;
        byte[] buff;
        List<string> PortList;
        Timer AutoConnectTimer;
        Timer ConnectRequstTimer;
        public bool isConnect;

        static int portnum;
        const int BuffSize = 128;

        public Serial(MainWindow parent)
        {
            try
            {
                this.Parent = parent;
                isConnect = false;
                packet_step = 0;
                buff = new byte[BuffSize];
                PortList = new List<string>();

                WriteException = new _delegate_arg(MainWindow.WriteException);
                SetSerial();
                oldtime = new DateTime();
                trace = new _delegate_arg(MainWindow.trace);
                SerialSend = new _delegate_arg(SerialView.Send);
                SerialRecive = new _delegate_arg(SerialView.Recive);
                ConnectTrace = new _delegate_arg(parent.ConnectTrace);
                AutoConnectTimer = new Timer(1000);
                ConnectRequstTimer = new Timer(500);

                portnum = 0;
            }
            catch (Exception ex)
            {
                Parent.Dispatcher.Invoke(WriteException, ex.ToString());
            }
        }

        void SetSerial()
        {
            try
            {
                serial = new SerialPort();
                serial.Encoding = Encoding.Default;
                serial.Parity = Parity.None;
                serial.DataBits = 8;
                serial.StopBits = StopBits.One;
                serial.DataReceived += new SerialDataReceivedEventHandler(serial_DataReceived);
            }
            catch (Exception ex)
            {
                Parent.Dispatcher.Invoke(WriteException, ex.ToString());
            }
        }

        void serial_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            byte _byte;
            string packet = "";

            try
            {
                packet_step = 0;
                while (serial.BytesToRead > 0)
                {
                    _byte = (byte)serial.ReadByte();
                    if (Properties.Settings.Default.IsReverse == true)
                    {
                        _byte = (byte)~_byte;
                    }
                    packet += ((byte)(_byte)).ToString("X2") + " ";

                    if (packet_step == 0 && (byte)(_byte) != 0x02)
                    {
                        packet_step = 0;
                        continue;
                    }

                    buff[packet_step++] = (byte)(_byte);

                    // 최소 3바이트 이상 STX, Command, CheckSum, ETX
                    if (packet_step != 0 && (byte)(_byte) == 0x03)
                    {
                        Parent.Dispatcher.Invoke(SerialRecive, packet);
                        packet = "";

                        command();
                    }

                    if (packet_step == 2)
                    {
                        if (!CheckCommand())
                        {
                            packet_step = 0;
                            continue;
                        }
                    }

                    if (packet_step > 1)
                    {
                        if (!CheckLength())
                        {
                            packet_step = 0;
                            continue;
                        }
                    }

                    if (packet_step == BuffSize)
                    {
                        packet_step = 0;
                    }

                    System.Threading.Thread.Sleep(10); // 하드웨어 처리시간이 늦어서 싱크를 맞추기 위해 Receive 대기 시간을 늘림 일단 100 //10으로 수정
                }
            }
            catch (Exception ex)
            {
                Parent.Dispatcher.Invoke(WriteException, ex.ToString());
            }

            if (packet != "")
                Parent.Dispatcher.Invoke(SerialRecive, packet);
        }

        void Write(byte[] _packet, bool reverse, bool makechecksum)
        {
            try
            {
                if (!serial.IsOpen)
                    return;

                if (makechecksum)
                    MakeCheckSum(_packet);

                // 기타 에서 데이터 암호화 통신에 체크하면 예전처럼 데이터 뒤집기
                if (Properties.Settings.Default.IsReverse == true)
                {
                    if (reverse)
                        ReversePacket(_packet, _packet.Length);
                }


                // 1바이트씩 끊어서 보내는 경우
                //for (int i = 0; i < 3; i++)
                //{
                //    for (int j = 0; j < _packet.Length; j++)
                //    {
                //        serial.Write(_packet, j, 1);
                //        Parent.Dispatcher.Invoke(SerialSend, _packet);

                //        System.Threading.Thread.Sleep(10);
                //    }
                //}

                // 모든 데이터를 통으로 보내는 경우
                for (int i = 0; i < 1; i++)
                {
                    serial.Write(_packet, 0, _packet.Length);
                    Parent.Dispatcher.Invoke(SerialSend, _packet);
                    System.Threading.Thread.Sleep(100);
                }
            }
            catch (Exception ex)
            {
                Parent.Dispatcher.Invoke(WriteException, ex.ToString());
            }
        }

        void Write(byte[] _packet, int length)
        {
            try
            {
                if (!serial.IsOpen)
                    return;

                if (Properties.Settings.Default.IsReverse == true)
                {
                    ReversePacket(_packet, length);
                }

                byte[] buff = new byte[length];

                for (int i = 0; i < length; i++)
                {
                    buff[i] = _packet[i];
                }

                for (int i = 0; i < 3; i++)
                {
                    serial.Write(_packet, 0, length);
                    Parent.Dispatcher.Invoke(SerialSend, buff);
                    System.Threading.Thread.Sleep(100);
                }
            }
            catch (Exception ex)
            {
                Parent.Dispatcher.Invoke(WriteException, ex.ToString());
            }
        }

        bool CheckLength()
        {
            int nCmdResult = 0;
            nCmdResult = buff[1];

            if (nCmdResult == 0x31)
            {
                if (packet_step > 6)
                    return false;
            }
            else if (nCmdResult == 0x32)
            {
                if (packet_step > 6)
                    return false;
            }
            else if (nCmdResult == 0x33)
            {
                if (packet_step > 6)
                    return false;
            }
            else if (nCmdResult == 0x34)
            {
                if (packet_step > 7)
                    return false;
            }
            else if (nCmdResult == 0x35)
            {
                if (packet_step > 7)
                    return false;
            }
            else if (nCmdResult == 0x37)
            {
                if (packet_step > 7)
                    return false;
            }
            else if (nCmdResult == 0x38)
            {
                if (packet_step > 7)
                    return false;
            }
            else if (nCmdResult == 0x39)
            {
                if (packet_step > 7)
                    return false;
            }
            else if (nCmdResult == 0x3C)
            {
                if (packet_step > 3)
                    return false;
            }
            else if (nCmdResult == 0x1E)
            {
                if (packet_step > 6)
                    return false;
            }
            else if (nCmdResult == 0x2E)
            {
                if (packet_step > 6)
                    return false;
            }

            return true;
        }

        bool CheckCommand()
        {
            int nCmdResult = 0;
            nCmdResult = buff[1];

            if (nCmdResult == 0x31)
            {
                return true;
            }
            else if (nCmdResult == 0x32)
            {
                return true;
            }
            else if (nCmdResult == 0x33)
            {
                return true;
            }
            else if (nCmdResult == 0x34)
            {
                return true;
            }
            else if (nCmdResult == 0x35)
            {
                return true;
            }
            else if (nCmdResult == 0x37)
            {
                return true;
            }
            else if (nCmdResult == 0x38)
            {
                return true;
            }
            else if (nCmdResult == 0x39)
            {
                return true;
            }
            else if (nCmdResult == 0x3C)
            {
                return true;
            }
            else if (nCmdResult == 0x1D)
            {
                return true;
            }
            else if (nCmdResult == 0x1F)
            {
                return true;
            }
            else if (nCmdResult == 0x2E)
            {
                return true;
            }
            else if (nCmdResult == 0x54) // 2012.2.27 ACU 상태 확인 응답
            {
                return true;
            }

            return false;
        }

        bool CheckSum()
        {
            if (packet_step <= 0)
            {
                return false;
            }

            try
            {
                byte subtotal = 0x00;
                for (int i = 0; i < packet_step - 2; i++)
                {
                    subtotal = (byte)(subtotal + (buff[i] % 256));
                }

                if (buff[packet_step - 2] == subtotal)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Parent.Dispatcher.Invoke(WriteException, ex.ToString());
                return false;
            }
        }

        void ReversePaket(byte[] _packet)
        {
            for (int i = 0; i < _packet.Length; i++)
            {
                _packet[i] = (byte)~_packet[i];
            }
        }

        void MakeCheckSum(byte[] _packet)
        {
            int length = _packet.Length;

            byte subtotal = 0x00;
            for (int i = 0; i < length - 2; i++)
            {
                subtotal = (byte)(subtotal + (_packet[i] % 256));
            }

            _packet[_packet.Length - 2] = subtotal;
            //   return subtotal;
        }

        void MakeCheckSum(byte[] _packet, int length)
        {
            byte subtotal = 0x00;
            for (int i = 0; i < length - 2; i++)
            {
                subtotal = (byte)(subtotal + (_packet[i] % 256));
            }

            _packet[length - 2] = subtotal;
            //   return subtotal;
        }

        void command()
        {
            bool bRet = false;
            bRet = CheckSum();

            if (bRet)
            {
                int nCmdResult = 0;
                nCmdResult = buff[1];

                if (nCmdResult == 0x31) // 비상알림~~
                {
                    int nAddr = ((buff[2] - 0x30) * 100) +
                                ((buff[3] - 0x30) * 10) +
                                ((buff[4] - 0x30) * 1);

                    if (nAddr > 0 && nAddr <= 999)
                    {
                        PalyAlram(nAddr);
                    }

                }

                else if (nCmdResult == 0x32) // 알람 중지!!
                {
                    StopAlram(); //시큐인포는 자동정지 안시킴
                }
                else if (nCmdResult == 0x33) // 인터폰 안내 요청
                {
                    int nAddr = ((buff[2] - 0x30) * 100) +
                                ((buff[3] - 0x30) * 10) +
                                ((buff[4] - 0x30) * 1);

                    if (nAddr > 0 && nAddr <= 999)
                    {
                        InterphoneAlarm(nAddr);
                    }
                }
                else if (nCmdResult == 0x34)
                {
                    int nAddr = ((buff[2] - 0x30) * 1000) +
                                ((buff[3] - 0x30) * 100) +
                                ((buff[4] - 0x30) * 10) +
                                ((buff[5] - 0x30) * 1);

                    CameraOn(nAddr);

                }

                else if (nCmdResult == 0x35)
                {
                    int nAddr = ((buff[2] - 0x30) * 1000) +
                                ((buff[3] - 0x30) * 100) +
                                ((buff[4] - 0x30) * 10) +
                                ((buff[5] - 0x30) * 1);

                    CameraOff(nAddr);

                }

                else if (nCmdResult == 0x3C) // COM 자동 연결 확인
                {
                    ConnectedSerialPort();
                }

                else if (nCmdResult == 0x37)
                {
                    int nAddr = ((buff[2] - 0x30) * 1000) +
                                ((buff[3] - 0x30) * 100) +
                                ((buff[4] - 0x30) * 10) +
                                ((buff[5] - 0x30) * 1);

                    PowerOn(nAddr);

                }

                else if (nCmdResult == 0x38)
                {
                    int nAddr = ((buff[2] - 0x30) * 1000) +
                                ((buff[3] - 0x30) * 100) +
                                ((buff[4] - 0x30) * 10) +
                                ((buff[5] - 0x30) * 1);
                    PowerOff(nAddr);
                }

                else if (nCmdResult == 0x39) // 비상버튼 인식 요청
                {
                    int nAddr = ((buff[3] - 0x30) * 100) +
                                ((buff[4] - 0x30) * 10) +
                                ((buff[5] - 0x30) * 1);

                    if (nAddr > 0 && nAddr <= 999)
                    {
                        CognitionRequestPanic(nAddr, buff[2]);
                    }
                }

                else if (nCmdResult == 0x1D) // E.V 비상 발생
                {
                    /*
                    int nAddr = ((buff[2] - 0x30) * 100000) +
                                ((buff[3] - 0x30) * 10000) +
                                ((buff[4] - 0x30) * 1000) +
                                ((buff[5] - 0x30) * 100) +
                                ((buff[6] - 0x30) * 10) +
                                ((buff[7] - 0x30) * 1);
                    */
                    byte[] bAddr = new byte[3];
                    bAddr[0] = buff[2];
                    bAddr[1] = buff[3];
                    bAddr[2] = buff[4];

                    ElevatorPanic(bAddr);
                }

                else if (nCmdResult == 0x1F) // E.V 비상 중지
                {
                    int nAddr = ((buff[2] - 0x30) * 100) +
                                ((buff[3] - 0x30) * 10) +
                                ((buff[4] - 0x30) * 1);

                }

                else if (nCmdResult == 0x2E) // E.V 부재중
                {
                    int nAddr = ((buff[2] - 0x30) * 100) +
                                ((buff[3] - 0x30) * 10) +
                                ((buff[4] - 0x30) * 1);
                }

                else if (nCmdResult == 0x54) // 2012.2.27 ACU 상태 확인 응답
                {
                    int nState = (buff[2] - 0x30);
                    int nAddr = ((buff[3] - 0x30) * 100) +
                                ((buff[4] - 0x30) * 10) +
                                ((buff[5] - 0x30) * 1);

                    // 일단 받아놓고.. 나중에 쓸일 생기면 그때 쓰는걸로! 현재는 필요없음
                    ACUState(nState);
                }

                packet_step = 0;
            }
        }

        private void ACUState(int _State)
        {
            object arg = _State;
            _delegate_arg ACUStateTesting = new _delegate_arg(Parent.SetAcuState);
            Parent.Dispatcher.Invoke(ACUStateTesting, arg);
        }

        private void ElevatorPanic(byte[] _Addr)
        {
            int Ho = 0;

            Ho = ((_Addr[0] - 0x30) * 100) +
                    ((_Addr[1] - 0x30) * 10) +
                    ((_Addr[2] - 0x30) * 1);

            SendAnserElevatorAlram(Ho);

            object arg = _Addr;
            _delegate_arg ElevatorPanic = new _delegate_arg(Parent.ElevatorPanicAlram);
            Parent.Dispatcher.Invoke(ElevatorPanic, arg);
        }

        public void ConnectionTesting(int _AcuNumber)
        {
            byte[] packet = { 0x02, 0x53, 0x00, 0x30, 0x30, 0x30, 0x00, 0x03 };

            packet[3] = (byte)((_AcuNumber / 100) + 0x30);
            packet[4] = (byte)(((_AcuNumber % 100) / 10) + 0x30);
            packet[5] = (byte)(((_AcuNumber % 100) % 10) + 0x30);

            Write(packet, true, true);
        }

       
        void PowerOn(int addr)
        {
            object arg = addr;
            _delegate_arg on = new _delegate_arg(Parent.PowerOn);
            Parent.Dispatcher.Invoke(on, arg);
        }

        void PowerOff(int addr)
        {
            object arg = addr;
            _delegate_arg off = new _delegate_arg(Parent.PowerOff);
            Parent.Dispatcher.Invoke(off, arg);
        }

        void CameraOn(int addr)
        {
            object arg = addr;
            _delegate_arg on = new _delegate_arg(Parent.CameraOn);
            Parent.Dispatcher.Invoke(on, arg);
        }

        void CameraOff(int addr)
        {
            object arg = addr;
            _delegate_arg off = new _delegate_arg(Parent.CameraOff);
            Parent.Dispatcher.Invoke(off, arg);
        }

        public void SendDeviceScan(int addr)
        {
            byte[] packet = { 0x02, 0x36, 0x30, 0x30, 0x30, 0x30, 0x00, 0x03 };

            if (addr > 0)
            {
                packet[3] = (byte)(addr / 100 + 0x30);
                packet[4] = (byte)((addr % 100) / 10 + 0x30);
                packet[5] = (byte)((addr % 100) % 10 + 0x30);
            }

            Write(packet, true, true);
        }

        // 프로토콜 폐지
        //public void SendCameraScan(int addr)
        //{
        //    byte[] packet = { 0x02, 0x33, 0x30, 0x30, 0x30, 0x30, 0x00, 0x03 };

        //    if (addr > 0)
        //    {
        //        packet[3] = (byte)(addr / 100 + 0x30);
        //        packet[4] = (byte)((addr % 100) / 10 + 0x30);
        //        packet[5] = (byte)((addr % 100) % 10 + 0x30);
        //    }

        //    Write(packet, true, true);
        //}

        void CognitionRequestPanic(int _addr, int _voltage)
        {
            lock (this)
            {
                bool ishave = false;

                foreach (Map map in MainWindow.maps)
                {
                    foreach (PanicControl panic in map.PanicList)
                    {
                        if (panic.Addr == _addr)
                        {
                            _delegate_arg Setvoltage = new _delegate_arg(panic.SetVoltage);
                            Parent.Dispatcher.Invoke(Setvoltage, _voltage);
                            ishave = true;
                        }
                    }
                }

                foreach (PanicControl panic in MainWindow.maps.NotInsertPanic)
                {
                    if (panic.Addr == _addr)
                    {
                        _delegate_arg Setvoltage = new _delegate_arg(panic.SetVoltage);
                        Parent.Dispatcher.Invoke(Setvoltage, _voltage);
                        ishave = true;
                    }
                }

                if (!ishave && (Properties.Settings.Default.IsAutoSearchPanicButton))
                {
                    cognition_delegate AddPanic = new cognition_delegate(AddNotHavePanic);
                    object[] temp = { _addr, _voltage };
                    Parent.Dispatcher.Invoke(AddPanic, temp);
                }

                SendAnswerCognitionRequestPanic(_addr, _voltage);
            }
        }

        void SendAnswerCognitionRequestPanic(int _addr, int _voltage)
        {
            byte[] packet = { 0x02, 0x59, 0x30, 0x30, 0x30, 0x00, 0x03 };

            packet[2] = (byte)(_addr / 100 + 0x30);
            packet[3] = (byte)((_addr % 100) / 10 + 0x30);
            packet[4] = (byte)((_addr % 100) % 10 + 0x30);

            if (PanicCall.Configuration.DVR.global_variable._monitoring == true)
            {
                Write(packet, true, true);
            }
        }

        public void SendStopAlram(int addr)
        {
            byte[] packet = { 0x02, 0x3d, 0x30, 0x30, 0x30, 0x00, 0x03 };

            packet[2] = (byte)(addr / 100 + 0x30);
            packet[3] = (byte)((addr % 100) / 10 + 0x30);
            packet[4] = (byte)((addr % 100) % 10 + 0x30);

            if (PanicCall.Configuration.DVR.global_variable._monitoring == true)
            {
                Write(packet, false, true);
            }
        }


        public void SendPcToStopAlram(int addr)
        {
            byte[] packet = { 0x02, 0x32, 0x30, 0x30, 0x30, 0x00, 0x03 };

            packet[2] = (byte)(addr / 100 + 0x30);
            packet[3] = (byte)((addr % 100) / 10 + 0x30);
            packet[4] = (byte)((addr % 100) % 10 + 0x30);

            // if (PanicCall.Configuration.DVR.global_variable._monitoring == true)
            // {
            Write(packet, true, true);
            // }
        }

        public void SendPlateONOFF(bool is_on)
        {
            if (is_on == true)
            {
                byte[] packet = { 0x02, 0x20, 0x33, 0x32, 0x31, 0x22, 0x03 };
                Write(packet, false, true);
            }
            else
            {
                byte[] packet = { 0x02, 0x21, 0x33, 0x32, 0x31, 0x23, 0x03 };
                Write(packet, false, true);
            }
        }

        public void Panic_ONOFF(bool is_on)
        {
            if (is_on == true)
            {
                byte[] packet = { 0x02, 0x22, 0x33, 0x32, 0x31, 0x22, 0x03 };
                Write(packet, false, true);
            }
            else
            {
                byte[] packet = { 0x02, 0x23, 0x33, 0x32, 0x31, 0x23, 0x03 };
                Write(packet, false, true);
            }
        }

        public void SendPcToStopAlramReset(int addr)
        {
            byte[] packet = { 0x02, 0x3E, 0x30, 0x30, 0x30, 0x00, 0x03 };

            packet[2] = (byte)(addr / 100 + 0x30);
            packet[3] = (byte)((addr % 100) / 10 + 0x30);
            packet[4] = (byte)((addr % 100) % 10 + 0x30);

            //  if (PanicCall.Configuration.DVR.global_variable._monitoring == true)
            //  {
            Write(packet, false, true);
            //  }
        }


        public void SendButtonSacn()
        {
            byte[] packet = { 0x02, 0x41, 0x00, 0x03 };
            Write(packet, true, true);
        }

        void CognitionRequestSensor(int _addr, int _voltage)
        {
            bool ishave = false;

            foreach (Map map in MainWindow.maps)
            {
                foreach (SensorControl sensor in map.SensorList)
                {
                    if (sensor.Addr == _addr)
                        ishave = true;
                }
            }

            foreach (SensorControl sensor in MainWindow.maps.NotInsertSensor)
            {
                if (sensor.Addr == _addr)
                    ishave = true;
            }

            if (!ishave)
            {
                cognition_delegate AddSensor = new cognition_delegate(AddNotHaveSensor);
                object[] temp = { _addr, _voltage };
                Parent.Dispatcher.Invoke(AddSensor, temp);
            }
        }

        void AddNotHavePanic(int addr, int vol)
        {
            foreach (Map map in MainWindow.maps)
            {
                foreach (PanicControl _panic in map.PanicList)
                {
                    if (_panic.Addr == addr)
                        return;
                }
            }

            foreach (PanicControl _panic in MainWindow.maps.NotInsertPanic)
            {
                if (_panic.Addr == addr)
                    return;
            }

            Parent.Dispatcher.Invoke(trace, "추가되지 않은 비상버튼이 발견 되었습니다" + " [주소 : " + addr.ToString() + "]");
            PanicControl panic = new PanicControl();
            panic.UnSetTimer();
            panic.Addr = addr;
            panic.Voltage = vol;
            panic.Connect = 1;
            panic.PanicName = addr.ToString();

            MainWindow.maps.NotInsertPanic.Add(panic);

            Parent.AnswerIcon();
        }

        void AddNotHaveSensor(int addr, int vol)
        {
            Parent.Dispatcher.Invoke(trace, "추가되지 않은 센서가 발견 되었습니다");
            SensorControl sersor = new SensorControl();
            sersor.Addr = addr;
            sersor.Voltage = vol;

            MainWindow.maps.NotInsertSensor.Add(sersor);

            Parent.AnswerIcon();
        }

        // 연결 상태 확인 요청
        void SendConnectSerialPort()
        {
            byte[] packet = { 0x02, 0x3C, 0xC1, 0x03 };
            Write(packet, true, true);
        }

        /* param : bool call
         * description : true   : 통화 요청인 경우, Protocol (0x72)
         *               false  : 비상경보인 경우, Protocol (0x70)
         */
        public void SendOfficePhone(string map, PanicControl panic, bool call)
        {
            try
            {
                if (panic.IsPhoneSleep && !call)
                    return;

                byte[] packet = new byte[128];
                ushort temp = 0x0000;
                char[] chars = new char[128];

                int n = 0;
                int PhoneNum = 1;

                if (panic.IsInterphone2)
                    PhoneNum += 2;

                if (panic.IsInterphone3)
                    PhoneNum += 4;

                if (panic.IsInterphone4)
                    PhoneNum += 5;

                //wpacket.
                packet[n++] = 0x02;
                if (call)
                {
                    packet[n++] = 0x72;
                }
                else
                {
                    packet[n++] = 0x70;
                }
                packet[n++] = Convert.ToByte(PhoneNum);
                packet[n++] = 0x01;
                packet[n++] = 0x00;


                // 조합형 문자열을 완성형 문자로 그냥 전송
                byte maplength = 0;

                if (Properties.Settings.Default.IsInterphoneCharComplete == true)
                {
                    byte[] MapName = new byte[128];
                    MapName = Encoding.Default.GetBytes(map);

                    for (int i = 0; i < MapName.Length; i++)
                    {
                        packet[n++] = MapName[i];
                        maplength++;
                    }
                }
                else
                {
                    chars = map.ToCharArray(0, map.Length);

                    for (int j = 0; j < map.Length; j++)
                    {
                        ushort ch = (ushort)Jamo.DivideJaso(chars[j]);

                        temp = Convert.ToUInt16(chars[j]);

                        if (temp == ch)  //영문
                        {
                            packet[n++] = Convert.ToByte(ch);
                            maplength++;
                        }
                        else //한글
                        {
                            // 한글이기 때문에 상, 하위 바이트로 분리를 해줘야 함
                            // unicode --> uchar --> char --> 상,하위바이트 분리 --> byte

                            byte _byte1 = 0x00;
                            byte _byte2 = 0x00;

                            _byte1 = (byte)((ch >> 8) & 0xFF); //상위 바이트 
                            _byte2 = (byte)(ch & 0x00FF); //하위 바이트

                            packet[n++] = _byte1;
                            packet[n++] = _byte2;

                            maplength++;
                            maplength++;
                        }
                    }
                }

                packet[4] = maplength;


                packet[n++] = 0x02;
                packet[n++] = 0x03;

                packet[n++] = (byte)(panic.Addr / 100 + 0x30);
                packet[n++] = (byte)((panic.Addr % 100) / 10 + 0x30);
                packet[n++] = (byte)((panic.Addr % 100) % 10 + 0x30);

                packet[n++] = 0x03;
                packet[n] = 0x00;

                byte buttonlength = 0;
                int nPos = n++;

                byte[] ButtonName = new byte[128];
                ButtonName = Encoding.Default.GetBytes(panic.PanicName);

                if (Properties.Settings.Default.IsInterphoneCharComplete == true)
                {
                    for (int i = 0; i < ButtonName.Length; i++)
                    {
                        packet[n++] = ButtonName[i];
                        buttonlength++;
                    }
                }
                else
                {
                    chars = panic.PanicName.ToCharArray(0, panic.PanicName.Length);

                    for (int k = 0; k < panic.PanicName.Length; k++)
                    {
                        ushort ch = (ushort)Jamo.DivideJaso(chars[k]);

                        temp = Convert.ToUInt16(chars[k]);

                        if (temp == ch)
                        {
                            packet[n++] = Convert.ToByte(ch);
                            buttonlength++;
                        }
                        else
                        {
                            byte _byte1 = 0x00;
                            byte _byte2 = 0x00;

                            _byte1 = (byte)((ch >> 8) & 0xFF);
                            _byte2 = (byte)(ch & 0x00FF);

                            packet[n++] = _byte1;
                            packet[n++] = _byte2;
                            buttonlength++;
                            buttonlength++;
                        }
                    }
                }

                packet[nPos] = buttonlength;

                packet[n++] = 0x00;
                packet[n++] = 0x03;

                MakeCheckSum(packet, n);

                if (PanicCall.Configuration.DVR.global_variable._monitoring == true) //경비실 샌드패킷 
                {
                    Write(packet, n);
                }

            }
            catch (Exception ex)
            {
                Parent.Dispatcher.Invoke(WriteException, ex.ToString());
            }

        }

        public void SendPresetSet(PanicControl panic)
        {
            if (!Properties.Settings.Default.IsPresetMove)
                return;

            foreach (IntToInt preset in panic.Preset)
            {
                if ((int)Configuration.Preset.PresetSelection.PelcoD == preset.Int3)
                {
                    SendPelcoD(preset.Int1, preset.Int2);
                }
                else if ((int)Configuration.Preset.PresetSelection.PelcoP == preset.Int3)
                {
                    SendPelcoP(preset.Int1, preset.Int2);
                }
                else if ((int)Configuration.Preset.PresetSelection.DongYang == preset.Int3)
                {
                    SendDongYang(preset.Int1, preset.Int2);
                }
                else if ((int)Configuration.Preset.PresetSelection.SungJin == preset.Int3)
                {
                    SendSungJin(preset.Int1, preset.Int2);
                }
                else if ((int)Configuration.Preset.PresetSelection.SCC641 == preset.Int3)
                {
                    SendSCC641(preset.Int1, preset.Int2);
                }
            }
        }

        void SendPelcoD(int nCam, int nPset)
        {
            int nPreset = 0;
            int nCamera = 0;

            nPreset = nPset;
            nCamera = nCam;

            if (nPreset < 0 || nPreset > 80)
            {
                return;
            }

            if (nCamera < 0 || nCamera > 256)
            {
                return;
            }

            byte[] packetPreset = { 0xFF, 0x00, 0x00, 0x07, 0x00, 0x00, 0x00 };

            packetPreset[1] = Convert.ToByte(nCamera);
            packetPreset[5] = Convert.ToByte(nPreset);

            packetPreset[6] = Convert.ToByte(packetPreset[1] + packetPreset[2]);
            packetPreset[6] = Convert.ToByte(packetPreset[6] & 0xFF);
            packetPreset[6] = Convert.ToByte(packetPreset[6] + packetPreset[3]);
            packetPreset[6] = Convert.ToByte(packetPreset[6] & 0xFF);
            packetPreset[6] = Convert.ToByte(packetPreset[6] + packetPreset[4]);
            packetPreset[6] = Convert.ToByte(packetPreset[6] & 0xFF);
            packetPreset[6] = Convert.ToByte(packetPreset[6] + packetPreset[5]);
            packetPreset[6] = Convert.ToByte(packetPreset[6] & 0xFF);

            Write(packetPreset, true, false);

        }

        void SendPelcoP(int nCam, int nPset)
        {
            int nPreset = 0;
            int nCamera = 0;

            nPreset = nPset;
            nCamera = nCam;

            if (nPreset < 1 || nPreset > 80)
            {
                return;
            }

            if (nCamera < 1 || nCamera > 256)
            {
                return;
            }

            byte[] packetPreset = { 0xA0, 0x00, 0x00, 0x07, 0x00, 0x00, 0xAF, 0x00 };

            packetPreset[1] = Convert.ToByte(nCamera);
            packetPreset[5] = Convert.ToByte(nPreset);

            packetPreset[7] = Convert.ToByte(packetPreset[1] ^ packetPreset[2]);
            packetPreset[7] = Convert.ToByte(packetPreset[7] ^ packetPreset[3]);
            packetPreset[7] = Convert.ToByte(packetPreset[7] ^ packetPreset[4]);
            packetPreset[7] = Convert.ToByte(packetPreset[7] ^ packetPreset[5]);
            packetPreset[7] = Convert.ToByte(packetPreset[7] ^ packetPreset[6]);

            Write(packetPreset, true, false);
        }


        //---------------------------------------------------------------------------------------
        // ● Description   : Sensing Converter Protocol 추가
        // ● Date          : 2011년 2월
        // ● Name          : 김지우
        //---------------------------------------------------------------------------------------
        //      명령어,   ADDR,     Chanel  DelayTime, CheckSum, ETX
        //      0x53    0x09/0x09    0x10     0xFF       SUM    0x03
        //---------------------------------------------------------------------------------------
        public void SendScSet(PanicControl panic)
        {
            foreach (IntToInt sc in panic.Sc)
            {
                SendSc(sc.Int1, sc.Int2, sc.Int3);
            }
        }
        public void SendSc(int nAddr, int nChanel, int nDelayTime)
        {
            if (nAddr < 00 || nAddr > 99)
                return;

            if (nChanel < 0x01 || nChanel > 0x10)
                return;

            if (nDelayTime < 0x00 || nDelayTime > 0xFF)
                return;

            byte[] packet = { 0x02, 0x53, 0x00, 0x00, 0x01, 0x00, 0x00, 0x03 };

            packet[2] = (byte)((nAddr % 100) / 10 + 0x30);
            packet[3] = (byte)((nAddr % 100) % 10 + 0x30);
            packet[4] = (byte)((nChanel % 100) + 0x00);
            packet[5] = (byte)((nDelayTime) + 0x00);

            Write(packet, true, true);
        }



        void SendDongYang(int nCam, int nPset)
        {
            int nPreset = 0;
            int nCamera = 0;

            nPreset = nPset;
            nCamera = nCam;

            if (nPreset < 0 || nPreset > 80)
            {
                return;
            }

            if (nCamera < 0 || nCamera > 256)
            {
                return;
            }

            byte[] packetPreset = { 0x55, 0x00, 0x00, 0x00, 0x07, 0x00, 0x00, 0x80, 0x00, 0xAA, 0x00 };

            packetPreset[2] = Convert.ToByte(nCamera);
            packetPreset[7] = Convert.ToByte(0x80 + nPreset - 0x01);
            int sum = packetPreset[0] + packetPreset[1] + packetPreset[2] + packetPreset[3] + packetPreset[4] + packetPreset[5] + packetPreset[6] + packetPreset[7] + packetPreset[8] + packetPreset[9];
            sum = (0x2020 - sum) & 0xff;
            packetPreset[10] = Convert.ToByte(sum);

            Write(packetPreset, true, false);
        }

        void SendSungJin(int nCam, int nPset)
        {
            int nPreset = 0;
            int nCamera = 0;

            nPreset = nPset - 1;
            nCamera = nCam;

            if (nPreset < 0 || nPreset > 63)
            {
                return;
            }

            if (nCamera < 0 || nCamera > 256)
            {
                return;
            }

            byte[] packet = { 0xA0, 0x0C, 0x00, 0x01, 0x00, 0xFF, 0xFF, 0x00 };

            packet[2] = Convert.ToByte(nCamera);
            packet[4] = Convert.ToByte(nPreset);

            packet[7] = Convert.ToByte(packet[1] ^ packet[2]);
            packet[7] = Convert.ToByte(packet[7] ^ packet[3]);
            packet[7] = Convert.ToByte(packet[7] ^ packet[4]);
            packet[7] = Convert.ToByte(packet[7] ^ packet[5]);
            packet[7] = Convert.ToByte(packet[7] ^ packet[6]);

            Write(packet, true, false);
        }

        void SendSCC641(int nCam, int nPset)
        {
            int nPreset = -1;
            int nCamera = -1;

            uint sum = 0x0000;
            uint ffff = 0xFFFF;

            nPreset = nPset;
            nCamera = nCam;

            if (nPreset < 0 || nPreset > 127)
            {
                return;
            }

            if (nCamera < 1 || nCamera > 256)
            {
                return;
            }

            //packetPreset[1] : TARGET NUMBER
            //packetPreset[2] : CAMERA NUMBER
            //packetPreset[5] : PRESET NUMBER (0 ~ 127)
            byte[] packetPreset = { 0xA0, 0xFE, 0x00, 0x03, 0x19, 0x00, 0xFF, 0xFF, 0x00 };

            packetPreset[2] = Convert.ToByte(nCamera);
            packetPreset[5] = Convert.ToByte(nPreset);

            sum = Convert.ToUInt32(packetPreset[1] + packetPreset[2]);
            sum = sum + packetPreset[3];
            sum = sum + packetPreset[4];
            sum = sum + packetPreset[5];
            sum = sum + packetPreset[6];
            sum = sum + packetPreset[7];

            packetPreset[8] = Convert.ToByte(0xff & (ffff - sum));

            Write(packetPreset, true, false);
        }

        public void SendMatrixSet(PanicControl panic)
        {
            if (!Properties.Settings.Default.IsMatrixView)
                return;

            if ((int)Configuration.Matrix.MatrixSelection.DongYang == Properties.Settings.Default.nMatrixSelect)
            {
                foreach (IntToInt matrix in panic.Matrix)
                {
                    SendMatrixDongYang(matrix.Int1, matrix.Int2);
                }

            }
            else if ((int)Configuration.Matrix.MatrixSelection.SungJin
                == Properties.Settings.Default.nMatrixSelect)
            {
                foreach (IntToInt matrix in panic.Matrix)
                {
                    SendMatrixSungJin(matrix.Int1, matrix.Int2);
                }
            }
        }

        void SendMatrixDongYang(int nCam, int nMon)
        {
            int nMonitor = 0;
            int nCamera = 0;

            nMonitor = nMon;
            nCamera = nCam;

            if (nMonitor < 1 || nMonitor > 65)
            {
                return;
            }

            if (nCamera < 1 || nCamera > 256)
            {
                return;
            }

            //0x55 0x00 Camera 0x10 0x00 Monitor Key(01monitor 02 camera) 0x00 0x00 0xAA S UM
            byte[] packetMonitor = { 0x55, 0x00, 0x00, 0x10, 0x00, 0x00, 0x00, 0x00, 0x00, 0xAA, 0x00 };
            byte[] packetCamera = { 0x55, 0x00, 0x00, 0x10, 0x00, 0x00, 0x00, 0x00, 0x00, 0xAA, 0x00 };

            int sum = 0;

            packetMonitor[2] = Convert.ToByte(nCamera);
            packetMonitor[5] = Convert.ToByte(nMonitor);
            packetMonitor[6] = 0x01;
            sum = packetMonitor[0] + packetMonitor[1] + packetMonitor[2] + packetMonitor[3] + packetMonitor[4] + packetMonitor[5] + packetMonitor[6] + packetMonitor[7] + packetMonitor[8] + packetMonitor[9];
            sum = (0x2020 - sum) & 0xff;
            packetMonitor[10] = Convert.ToByte(sum);

            Write(packetMonitor, true, false);

            packetCamera[2] = Convert.ToByte(nCamera);
            packetCamera[5] = Convert.ToByte(nMonitor);
            packetCamera[6] = 0x02;
            sum = packetCamera[0] + packetCamera[1] + packetCamera[2] + packetCamera[3] + packetCamera[4] + packetCamera[5] + packetCamera[6] + packetCamera[7] + packetCamera[8] + packetCamera[9];
            sum = (0x2020 - sum) & 0xff;
            packetCamera[10] = Convert.ToByte(sum);
            Write(packetCamera, true, false);
        }

        void SendMatrixSungJin(int nCam, int nMon)
        {
            uint low = Convert.ToUInt32((nCam - 1) & 0xff);
            uint high = Convert.ToUInt32((nCam - 1) & 0xff00);

            high = high >> 8;

            byte[] packet = { 0xA0, 0x20, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00 };

            packet[3] = Convert.ToByte(nMon - 1);
            packet[4] = Convert.ToByte(low);
            packet[5] = Convert.ToByte(high);

            packet[7] = Convert.ToByte(packet[1] ^ packet[2]);
            packet[7] = Convert.ToByte(packet[7] ^ packet[3]);
            packet[7] = Convert.ToByte(packet[7] ^ packet[4]);
            packet[7] = Convert.ToByte(packet[7] ^ packet[5]);
            packet[7] = Convert.ToByte(packet[7] ^ packet[6]);

            Write(packet, true, false);
        }

        void ReversePacket(byte[] _packet, int size)
        {
            for (int i = 0; i < size; i++)
            {
                _packet[i] = (byte)~_packet[i];
            }
        }

        public void AutoConnectSerial()
        {
            /*
            serial.PortName = "COM3";
            serial.Open();
            ConnectedSerialPort();
           */

            try
            {
                portnum = 0;
                PortList.Clear();
                serial.BaudRate = Properties.Settings.Default.nBaudRate;

                Parent.Dispatcher.Invoke(ConnectTrace, "연결 중");
                Parent.Dispatcher.Invoke(trace, "자동 연결 시도 중 입니다");

                foreach (string com in SerialPort.GetPortNames())
                {
                    PortList.Add(com);
                }

                isConnect = false;
                oldtime = DateTime.Now;
                Connect();
            }
            catch (Exception ex)
            {
                Parent.Dispatcher.Invoke(WriteException, ex.ToString());
            }
        }

        void Connect()
        {
            if (isConnect)
                return;

            if (portnum >= PortList.Count)
            {
                Parent.Dispatcher.Invoke(ConnectTrace, "연결 실패");
                //Parent.Dispatcher.Invoke(trace, "연결 실패. 연결 상태를 점검해 주세요");
                return;
            }
            oldtime = DateTime.Now;
            try
            {
                serial.PortName = PortList[portnum];
                Parent.Dispatcher.Invoke(trace, "연결 중 입니다 - " + PortList[portnum]);
            }
            catch (Exception ex)
            {
                Parent.Dispatcher.Invoke(WriteException, ex.ToString());
            }

            try
            {
                serial.Open();
            }
            catch (Exception ex)
            {
                Parent.Dispatcher.Invoke(WriteException, ex.ToString());

                lock (this)
                {
                    serial.Close();
                    portnum++;
                    Connect();
                }
                return;
            }

            if (serial.IsOpen)
            {
                ConnectRequstTimer.Elapsed += new ElapsedEventHandler(ConnectRequstTimer_Elapsed);
                ConnectRequstTimer.Start();
            }
            else
            {
                lock (this)
                {
                    portnum++;
                    Connect();
                }
            }
        }

        void ConnectRequstTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (isConnect)
                return;

            SendConnectSerialPort();    // 송출요청(PC -> LOCK)

            if (Math.Abs(oldtime.Second - DateTime.Now.Second) > 5)
            {
                lock (this)
                {
                    serial.Close();
                    portnum++;
                }
                ConnectRequstTimer.Stop();
                ConnectRequstTimer.Elapsed -= ConnectRequstTimer_Elapsed;
                Connect();
            }
        }

        void ConnectedSerialPort()
        {
            //   SendAnswerSerialPort();

            if (isConnect)
                return;

            isConnect = true;
            ConnectRequstTimer.Stop();
            ConnectRequstTimer.Elapsed -= ConnectRequstTimer_Elapsed;
            Parent.Dispatcher.Invoke(ConnectTrace, "연결 됨 [" + serial.PortName + "]");
            Parent.Dispatcher.Invoke(trace, "연결 성공 - " + serial.PortName);

            //SendDeviceScan(0);
        }

        public void Connect(string PortName, int BaudRate)
        {
            if (SerialPort.GetPortNames() != null)
            {
                Parent.Dispatcher.Invoke(ConnectTrace, "연결 중");

                if (serial.IsOpen)
                {
                    //    if (PortName == "AUTO" && serial.BaudRate == BaudRate)
                    //        return;

                    if (serial.PortName == PortName && serial.BaudRate == BaudRate)
                        return;

                    serial.Close();

                }

                if (PortName == "AUTO")
                {
                    AutoConnectSerial();
                }
                else
                {
                    serial.PortName = PortName;
                    serial.BaudRate = BaudRate;
                    Parent.Dispatcher.Invoke(trace, "연결 중 입니다 - " + serial.PortName);

                    try
                    {
                        serial.Open();

                        if (serial.IsOpen)
                        {
                            isConnect = true;
                            Parent.Dispatcher.Invoke(ConnectTrace, "연결 됨 [" + serial.PortName + "]");
                            Parent.Dispatcher.Invoke(trace, "연결 성공 - " + serial.PortName);

                            //SendDeviceScan(0);
                        }
                        else
                        {
                            Parent.Dispatcher.Invoke(ConnectTrace, "연결 실패");
                            Parent.Dispatcher.Invoke(trace, "연결 실패 - " + serial.PortName);
                            AutoConnectSerial();
                        }
                    }
                    catch (Exception ex)
                    {
                        serial.Close();
                        Parent.Dispatcher.Invoke(ConnectTrace, "연결 실패");
                        Parent.Dispatcher.Invoke(trace, "연결 실패 - " + serial.PortName);
                        AutoConnectSerial();

                        Parent.Dispatcher.Invoke(WriteException, ex.ToString());

                    }
                }
            }
        }

        public void ConnectPort()
        {
            AutoConnectSerial();
            //if (SerialPort.GetPortNames() != null)
            //{
            //    Parent.Dispatcher.Invoke(ConnectTrace, "연결 중");

            //    if (Properties.Settings.Default.PortSelect == "AUTO")
            //    {
            //        AutoConnectSerial();
            //    }
            //    else
            //    {
            //        serial.PortName = Properties.Settings.Default.PortSelect;
            //        serial.BaudRate = Properties.Settings.Default.nBaudRate;
            //        Parent.Dispatcher.Invoke(trace, "연결중 입니다 - " + serial.PortName);

            //        try
            //        {
            //            serial.Open();
            //        }
            //        catch(Exception ex)
            //        {
            //            serial.Close();
            //            Parent.Dispatcher.Invoke(WriteException, ex.ToString());
            //        }

            //        try
            //        {
            //            if (serial.IsOpen)
            //            {
            //                isConnect = true;
            //                Parent.Dispatcher.Invoke(ConnectTrace, "연결 됨 [" + serial.PortName + "]");
            //                Parent.Dispatcher.Invoke(trace, "연결 성공 - " + serial.PortName);

            //                //SendDeviceScan(0);
            //            }
            //            else
            //            {
            //                Parent.Dispatcher.Invoke(ConnectTrace, "연결 실패");
            //                Parent.Dispatcher.Invoke(trace, "연결 실패 - " + serial.PortName);
            //                AutoConnectSerial();
            //            }
            //        }
            //        catch (Exception ex)
            //        {
            //            Parent.Dispatcher.Invoke(WriteException, ex.ToString());
            //        }
            //    }
            //}
        }
    }
}
