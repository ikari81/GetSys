using System;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace GetSys
{
    /*Используем для перезагрузки компьютера*/
    class reboot
    {
        //импортируем API функцию InitiateSystemShutdown
        [DllImport("advapi32.dll", EntryPoint = "InitiateSystemShutdownEx")]
        static extern int InitiateSystemShutdown(string lpMachineName, string lpMessage, int dwTimeout, bool bForceAppsClosed, bool bRebootAfterShutdown);
        //импортируем API функцию AdjustTokenPrivileges
        [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
        internal static extern bool AdjustTokenPrivileges(IntPtr htok, bool disall,
        ref TokPriv1Luid newst, int len, IntPtr prev, IntPtr relen);
        //импортируем API функцию GetCurrentProcess
        [DllImport("kernel32.dll", ExactSpelling = true)]
        internal static extern IntPtr GetCurrentProcess();
        //импортируем API функцию OpenProcessToken
        [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
        internal static extern bool OpenProcessToken(IntPtr h, int acc, ref IntPtr phtok);
        //импортируем API функцию LookupPrivilegeValue
        [DllImport("advapi32.dll", SetLastError = true)]
        internal static extern bool LookupPrivilegeValue(string host, string name, ref long pluid);
        //импортируем API функцию LockWorkStation
        [DllImport("user32.dll", EntryPoint = "LockWorkStation")]
        static extern bool LockWorkStation();
        //объявляем структуру TokPriv1Luid для работы с привилегиями
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        internal struct TokPriv1Luid
        {
            public int Count;
            public long Luid;
            public int Attr;
        }

        //объявляем необходимые, для API функций, константые значения, согласно MSDN
        internal const int SE_PRIVILEGE_ENABLED = 0x00000002;
        internal const int TOKEN_QUERY = 0x00000008;
        internal const int TOKEN_ADJUST_PRIVILEGES = 0x00000020;
        internal const string SE_SHUTDOWN_NAME = "SeShutdownPrivilege";

        //функция SetPriv для повышения привилегий процесса
        private void SetPriv()
        {
            TokPriv1Luid tkp; //экземпляр структуры TokPriv1Luid 
            IntPtr htok = IntPtr.Zero;
            //открываем "интерфейс" доступа для своего процесса
            if (OpenProcessToken(GetCurrentProcess(), TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, ref htok))
            {
                //заполняем поля структуры
                tkp.Count = 1;
                tkp.Attr = SE_PRIVILEGE_ENABLED;
                tkp.Luid = 0;
                //получаем системный идентификатор необходимой нам привилегии
                LookupPrivilegeValue(null, SE_SHUTDOWN_NAME, ref tkp.Luid);
                //повышаем привилегию своему процессу
                AdjustTokenPrivileges(htok, false, ref tkp, 0, IntPtr.Zero, IntPtr.Zero);
            }
        }

        //публичный метод для перезагрузки/выключения машины
        public int halt(bool RSh, bool Force)
        {
            SetPriv(); //получаем привилегии
            //вызываем функцию InitiateSystemShutdown, передавая ей необходимые параметры
            return InitiateSystemShutdown(null, null, 0, Force, RSh);
        }

        //публичный метод для блокировки операционной системы
        public int Lock()
        {
            if (LockWorkStation())
                return 1;
            else
                return 0;
        }
    }

    class TcpRemoteControl
    {
        public static void tcpThread()
        {
            /*Слушаем все интерфейсы*/
            IPAddress localAddress = IPAddress.Any;

            /*Создаем сокет протокола tcp*/
            Socket lsSckt = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            /*Создаем точку подключения*/
            IPEndPoint ipEndpoint = new IPEndPoint(localAddress, 9999);

            /*Связываем сокет с точкой подключения*/
            lsSckt.Bind(ipEndpoint);

            /*Слушаем сокет с ограничением в одно подключение*/
            lsSckt.Listen(1);

            /*Принимаем подключения асинхронно, обработка в callback-функции*/
            lsSckt.BeginAccept(new AsyncCallback(CallbackTCP), lsSckt);

            while (true)
                Thread.Sleep(10);

        }
        static void CallbackTCP(IAsyncResult AsyncCall)
        {
            System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
            Byte[] message = new byte[256];
            for (int i = 0; i < 256; i++)
                message[i] = 32;
            Socket listener = (Socket)AsyncCall.AsyncState;
            Socket client = listener.EndAccept(AsyncCall);
            try
            {
                client.Receive(message);
            }
            catch { }

            string str = Encoding.ASCII.GetString(message);

            if (str.ToUpper().Contains("reboot".ToUpper()) && (GetSys.Program.netControl))
            {
                Console.WriteLine("получена команда перезагрузки, выполняю");
                reboot var = new reboot();
                var.halt(true, false);
            }
            if (str.ToUpper().Contains("freboot".ToUpper()) && (GetSys.Program.netControl))
            {
                Console.WriteLine("получена команда перезагрузки, выполняю");
                reboot var = new reboot();
                var.halt(true, true);
            }
            if (str.ToUpper().Contains("lock".ToUpper()) && (GetSys.Program.netControl))
            {
                Console.WriteLine("получена команда блокировки, выполняю");
                reboot var = new reboot();
                var.Lock();
            }
            if (str.ToUpper().Contains("status".ToUpper()) && (GetSys.Program.netControl))
            {
                string[] messages = str.Split(':');
                Console.WriteLine("получена команда запроса статуса, выполняю");
                string messg = "";
                if (GetSys.Program.disks)
                    messg += "disks: OK, ";
                else messg += "disks: FAIL, ";
                if (!GetSys.Program.processes)
                    messg += "processes: OK, ";
                else messg += "processes: FAIL, ";
                if (GetSys.Program.dbstat)
                    messg += "database: FAIL, ";
                else messg += "database: OK";

                Communication.SendSMS(messages[0], messg);
            }
            client.Close();
            listener.BeginAccept(new AsyncCallback(CallbackTCP), listener);
        }
    }
}
