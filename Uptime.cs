using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace GetSys
{
    class Uptime
    {
        /*Проверка аптайма, недавней перезагрузки и отправка утреннего отчета*/
        public static void CheckUptime()
        {

            GetSys.Program.uptime = 0;
            //Console.WriteLine("Uptime {0} минут", System.Environment.TickCount / (Int64)(1000 * 60));
            GetSys.Program.uptime = (Environment.TickCount & Int32.MaxValue) / (1000 * 60);
            Console.WriteLine("{0} минут", GetSys.Program.uptime);
            if (Math.Abs((uint)GetSys.Program.uptime) < 5)
            {
                if (GetSys.Program.useSMTP) //Отправляем уведомление по почте всем из списка рассылки
                    foreach (string mail in GetSys.Program.emailList)
                    {
                        Communication.SendMail(mail, System.Environment.MachineName.ToLower() + "@omskenergo.ru", DateTime.Now.ToString() + " Server restarted!", DateTime.Now.ToString() + " Внимание! Система " + System.Environment.MachineName + ((GetSys.Program.aliasip.Length != 0) ? " (" + GetSys.Program.aliasip.Trim() + ")" : "") + "была перезагружена!");
                    }
                if (GetSys.Program.useGSM) //Отправляем уведомление по почте всем из списка рассылки
                    foreach (string sms in GetSys.Program.SMSList)
                    {
                        Communication.SendSMS(sms, DateTime.Now.ToString() + " Attention! " + System.Environment.MachineName + ((GetSys.Program.aliasip.Length != 0) ? " (" + GetSys.Program.aliasip.Trim() + ")" : "") + " was rebooted!");
                    }
            }
            //Время первого запуска программы
            string[] LastDate = DateTime.Now.ToString().Split(' ');
            bool sendReport = false;
            while (true)
            {
                GetSys.Program.uptime = (Environment.TickCount & Int32.MaxValue) / (1000 * 60);
                Thread.Sleep(1000);
                if ((!sendReport) && (int.Parse(DateTime.Now.Hour.ToString()) == GetSys.Program.ReportHour) && (GetSys.Program.SendDailyReport))
                {
                    GetSys.Reporting.Report();   //Оправляем отчет
                    sendReport = true;
                    GetSys.Program.firstStart = false;
                }
                if (GetSys.Program.firstStart)
                {
                    GetSys.Program.firstStart = false;
                    GetSys.Reporting.Report();   //Оправляем отчет
                }
                if (((sendReport) && (int.Parse(DateTime.Now.Hour.ToString()) != GetSys.Program.ReportHour)) && (GetSys.Program.SendDailyReport))
                    sendReport = false;
            }
        }
        /*Конец проверки аптайма*/
    }
}
