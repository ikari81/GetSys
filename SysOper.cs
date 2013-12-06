using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;

namespace GetSys
{
    class SysOper
    {
        public static void L()
        {
            if (DateTime.Now.Year >= 2014)
            {
                Console.WriteLine("Лицензия кончилась, обратитесь к разработчику");
                Environment.Exit(0);
            }
        }

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        public const int SW_HIDE = 0;
        public const int SW_SHOW = 5;
        public const int SW_MINIMIZE = 6;

        //Для запуска только одной копии
        static Mutex mutex;
        public static bool InstanceExist()
        {
            bool cnew;
            mutex = new Mutex(false, "OneIn", out cnew);
            return (!cnew);
        }
    }
}
