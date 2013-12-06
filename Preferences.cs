using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace GetSys
{
    class Preferences
    {
        StreamReader f;
        public Preferences(ref string[] args)
        {
            if ((args.Length > 0) && !(args[0].ToUpper().Contains("hide".ToUpper())))
            {
                f = new StreamReader(args[0]);   //Связываем переменную с файлом конфигурации
            }
            else
            {
                string path = AppDomain.CurrentDomain.BaseDirectory.ToString();
                f = new StreamReader(path + @"\getsys.conf");   //Связываем переменную с файлом конфигурации
            }

            string[] commands = f.ReadToEnd().Split('\n');    //Читаем файл конфигурации в строковой массив
            f.Close();

            /*Получаем настройки из конфигурационного файла*/
            foreach (string s in commands)
            {
                if ((s.ToUpper().Contains("SMTP")) && (s[0] != '#'))
                {
                    GetSys.Program.smtpIP = s.Substring(5, s.Length - 5);
                    GetSys.Program.useSMTP = true;
                }
                if ((s.ToUpper().Contains("GSM")) && (s[0] != '#'))
                {
                    GetSys.Program.gsmIP = s.Substring(4, s.Length - 5);
                    GetSys.Program.useGSM = true;
                }
                if ((s.ToUpper().Contains("ALS")) && (s[0] != '#'))
                {
                    GetSys.Program.aliasip = new string(' ', s.Length - 4);
                    GetSys.Program.aliasip = s.Substring(4, s.Length - 4);
                }
                if ((s.ToUpper().Contains("CONNECTION")) && (s[0] != '#'))
                {
                    GetSys.Program.ConnStr = s.Substring(17, s.Length - 17);
                    GetSys.Program.checkDB = true;
                }
                if ((s.ToUpper().Contains("CONTROLPROC")) && (s[0] != '#'))
                {
                    string numProc = s.Substring(17, s.Length - 17);
                    GetSys.Program.ControlProcesses = new string[numProc.Length];
                    GetSys.Program.ProcessesStatus = new bool[numProc.Length];
                    for (int i = 0; i < GetSys.Program.ProcessesStatus.Length; i++)
                        GetSys.Program.ProcessesStatus[i] = true;
                    GetSys.Program.ControlProcesses = numProc.Trim().Split(',');
                }
                if ((s.ToUpper().Contains("SENDMAIL")) && (s[0] != '#'))
                {
                    string numProc = s.Substring(9, s.Length - 9);
                    GetSys.Program.emailList = new string[numProc.Length];
                    GetSys.Program.emailList = numProc.Trim().Split(',');
                }
                if ((s.ToUpper().Contains("SMS")) && (s[0] != '#'))
                {
                    string numProc = s.Substring(4, s.Length - 4);
                    GetSys.Program.SMSList = new string[numProc.Length];
                    GetSys.Program.SMSList = numProc.Trim().Split(',');
                }
                if ((s.ToUpper().Contains("Stime".ToUpper())) && (s[0] != '#'))
                {
                    string time = s.Substring(6, s.Length - 6);
                    string[] TimeList = new string[time.Length];
                    TimeList = time.Trim().Split(',');
                    GetSys.Program.StartHour = int.Parse(TimeList[0]);
                    GetSys.Program.StopHour = int.Parse(TimeList[1]);
                }
                if ((s.ToUpper().Contains("NETCONTROL")) && (s[0] != '#'))
                {
                    GetSys.Program.netControl = true;
                }
                if ((s.ToUpper().Contains("SENDDAILYREPORT")) && (s[0] != '#'))
                {
                    GetSys.Program.SendDailyReport = true;
                }
                if (s.ToUpper().Contains("SLEEPATSTART"))
                {
                    GetSys.Program.Pause = int.Parse(s.Substring(13, s.Length - 13));
                }
                if (s.ToUpper().Contains("DBTHRESHOLD"))
                {
                    GetSys.Program.DBThreshold = double.Parse(s.Substring(12, s.Length - 12));
                }
                if (s.ToUpper().Contains("REPORTHOUR"))
                {
                    GetSys.Program.ReportHour = int.Parse(s.Substring(11, s.Length - 11));
                }
                if (s.ToUpper().Contains("DRIVETHRESHOLD"))
                {
                    GetSys.Program.Threshold = double.Parse(s.Substring(15, s.Length - 15));
                }
            }
            /*Завершение получения настроек*/
        }
    }
}
