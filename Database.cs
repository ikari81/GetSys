using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace GetSys
{
    class Database
    {
        /*Поток проверки места в базе*/
        public static void checkDb()
        {
            bool lowSpace = false;
            while (true)
            {
                Thread.Sleep(9000);
                if (GetSys.Program.checkDB)
                {
                    string[] size = null;
                    try
                    {
                        lock (GetSys.Program.conn) //На время блокируем доступ к объекту
                        {
                            GetSys.Program.conn.Open();
                            GetSys.Program.cmd = GetSys.Program.conn.CreateCommand();
                            GetSys.Program.cmd.CommandText = @"sys.sp_spaceused;";
                            GetSys.Program.reader = GetSys.Program.cmd.ExecuteReader();
                            while (GetSys.Program.reader.Read())
                                size = GetSys.Program.reader.GetString(2).Split(' ');
                            GetSys.Program.conn.Close();
                            //string t = size[0].Replace('.',',');
                            float FreeSpace = float.Parse(size[0].Replace('.', ','));
                            if ((FreeSpace < GetSys.Program.DBThreshold) && (!lowSpace))
                            {
                                lowSpace = true;
                                GetSys.Program.dbstat = lowSpace;
                                if (GetSys.Program.useSMTP)    //Отправляем уведомление по почте всем из списка рассылки
                                    foreach (string mail in GetSys.Program.emailList)
                                    {
                                        Communication.SendMail(mail, System.Environment.MachineName.ToLower() + "@omskenergo.ru", "Внимание! Авария на сервере " + System.Environment.MachineName + ((GetSys.Program.aliasip.Length != 0) ? " (" + GetSys.Program.aliasip.Trim() + ")" : ""), DateTime.Now.ToString() + " Мало места в файле базы данных, требуется увеличение!");
                                    }
                                if (GetSys.Program.useGSM) //Отправляем уведомление по почте всем из списка рассылки
                                    foreach (string sms in GetSys.Program.SMSList)
                                    {
                                        Communication.SendSMS(sms, DateTime.Now.ToString() + " Attention! " + System.Environment.MachineName + ((GetSys.Program.aliasip.Length != 0) ? " (" + GetSys.Program.aliasip.Trim() + ")" : "") + ", low space in DB file, need resize!");
                                    }
                            }
                            if ((FreeSpace > GetSys.Program.DBThreshold) && (lowSpace))
                            {

                                lowSpace = false;
                                GetSys.Program.dbstat = lowSpace;

                            }

                        }
                    }
                    catch
                    { }
                }
            }
        }
        /*Конец потока проверки места в базе*/

    }
}
