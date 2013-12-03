using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace GetSys
{
    class Reporting
    {
        /*Формирование отчета*/
        public static void Report()
        {
            string TextOfReport;
            string userNameWin, compName;
            userNameWin = System.Environment.UserName;
            compName = System.Environment.MachineName;
            string min = "минут";
            string[] compIP = new string[System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName()).AddressList.Length];
            TextOfReport = @DateTime.Now.ToString() + " Сервер " + compName + ".\nАптайм сервера: " + GetSys.Program.uptime.ToString() + " " + min + ".\n"
                + "IP адреса сервера:\n";

            for (int i = 0; i < System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName()).AddressList.Length; i++)
            {
                compIP[i] = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName()).AddressList[i].ToString();
                TextOfReport += compIP[i] + "\n";
            }
            if (GetSys.Program.checkDB)
            {
                try
                {
                    lock (GetSys.Program.conn) //На время блокируем доступ к объекту
                    {
                        GetSys.Program.conn.Open();
                        GetSys.Program.cmd = GetSys.Program.conn.CreateCommand();
                        GetSys.Program.cmd.CommandText = @"sys.sp_spaceused;";
                        GetSys.Program.reader = GetSys.Program.cmd.ExecuteReader();
                        while (GetSys.Program.reader.Read())
                            TextOfReport += "База данных: " + GetSys.Program.reader.GetString(0) + ". Размер: " + GetSys.Program.reader.GetString(1) + ". Свободно в базе: " + GetSys.Program.reader.GetString(2) + ".\n";
                        GetSys.Program.conn.Close();
                        GetSys.Program.conn.Open();
                        GetSys.Program.cmd.CommandText = @"select physical_name, size from sys.database_files where file_id=1;";
                        GetSys.Program.reader = GetSys.Program.cmd.ExecuteReader();
                        while (GetSys.Program.reader.Read())
                            TextOfReport += "Расположение файлов базы данных: " + GetSys.Program.reader.GetString(0) + "\n";
                        GetSys.Program.conn.Close();
                    }
                }
                catch
                { }
            }
            TextOfReport += "Свободно места на дисках:\n";
            {
                GetSys.Program.allDrives = DriveInfo.GetDrives();
                foreach (DriveInfo MyDriveInfo in GetSys.Program.allDrives)
                {
                    if (MyDriveInfo.IsReady == true)
                    {
                        double free = MyDriveInfo.AvailableFreeSpace;
                        double a = (free / 1024) / 1024;
                        string Vol = MyDriveInfo.Name + ": " + a.ToString("#.##") + " Мб";
                        TextOfReport += Vol + "\n";
                    }
                }
            }
            if (GetSys.Program.useSMTP)    //Отправляем уведомление по почте всем из списка рассылки
                foreach (string mail in GetSys.Program.emailList)
                {
                    Communication.SendMail(mail, System.Environment.MachineName.ToLower() + "@omskenergo.ru", "Отчет с сервера " + System.Environment.MachineName + ((GetSys.Program.aliasip.Length != 0) ? " (" + GetSys.Program.aliasip.Trim() + ")" : "") + ", " + DateTime.Now.ToString(), TextOfReport);
                }
        }
        /*Конец формирования отчета*/
    }
}
