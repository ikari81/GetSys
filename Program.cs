using System;
using System.Data.SqlClient;
using System.IO;
using System.Threading;

namespace GetSys
{
    class Program
    {
        public static string smtpIP, gsmIP, ConnStr;
        public static string[] ControlProcesses = null;
        public static bool[] ProcessesStatus = null;
        public static string[] emailList = null;
        public static string[] SMSList = null;
        public static int uptime = 0;
        public static int ReportHour = 0;
        public static int StartHour = 8;
        public static int StopHour = 17;

        public static bool useSMTP = false;
        public static bool useGSM = false;
        public static bool netControl = false;
        public static bool checkDB = false;
        public static bool SendDailyReport = false;
        public static bool firstStart = false;

        /*Флаги текущих проверок*/
        public static bool disks = false;
        public static bool processes = false;
        public static bool dbstat = false;

        public static bool[] diskFail;
        public static SqlConnection conn;
        public static SqlCommand cmd;
        public static SqlDataReader reader;
        public static DriveInfo[] allDrives;
        public static double Threshold = 110;         //Порог свободного места на дисках
        public static double DBThreshold = 10;         //Порог свободного места в файле базы
        public static string aliasip;
        public static int Pause = 0;



        static string dupe(char c, int count)
        {
            string temp = null;
            for (int i = 0; i < count; i++)
                temp += c;
            return temp;
        }

        static void Main(string[] args)
        {
            SysOper.L();
            if ((args.Length > 0) && (args[0].ToUpper().Contains("hide".ToUpper())))
            {
                IntPtr instance = SysOper.GetConsoleWindow();
                SysOper.ShowWindow(instance, SysOper.SW_HIDE);
            }

            //Не запускаемся если больше одной копии
            if (SysOper.InstanceExist())
                return;

            Console.Title = "Агент мониторинга и сигнализации";
            smtpIP = gsmIP = ConnStr = null;
            useSMTP = useGSM = netControl = false;
            Console.WriteLine(StartHour.ToString() + " " + StopHour.ToString());
            Console.WriteLine(((DateTime.Now.Hour > StartHour) && (DateTime.Now.Hour < StopHour)));
            Thread.Sleep(Pause);
            
            //Читаем настройки
            GetSys.Preferences Prefs = new GetSys.Preferences(ref args);
            Console.WriteLine(dupe('-', Console.WindowWidth - 1));
            Console.WriteLine("Текущие настройки:");
            Console.WriteLine("{0}{1}", (useGSM) ? "Использовать GSM-шлюз, " : "Не использовать GSM-шлюз", (useGSM) ? gsmIP : "");
            Console.WriteLine("{0}{1}", (useSMTP) ? "Использовать SMTP-шлюз, " : "Не использовать SMTP-шлюз", (useSMTP) ? smtpIP : "");
            Console.WriteLine("Строка соединения с базой данных: {0}", ConnStr);
            Console.WriteLine("Alias: {0}", aliasip);
            Console.WriteLine("Контролируемые процессы:");
            foreach (string s in ControlProcesses)
                Console.WriteLine(s);
            Console.WriteLine(dupe('-', Console.WindowWidth - 1));

            Thread chkUptime = new Thread(GetSys.Uptime.CheckUptime);
            chkUptime.Start();

            Thread chkProc = new Thread(GetSys.ProcessChecker.CheckProcesses);
            chkProc.Start();

            Thread chkDrive = new Thread(GetSys.Drives.CheckDrives);
            chkDrive.Start();

            if (checkDB)
            {
                Thread chkDb = new Thread(GetSys.Database.checkDb);
                chkDb.Start();
                conn = new SqlConnection();
                conn.ConnectionString = ConnStr;
            }
            Thread tcpSrv = new Thread(GetSys.TcpRemoteControl.tcpThread);
            tcpSrv.Start();
        }
    }
}