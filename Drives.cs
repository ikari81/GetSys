using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;

namespace GetSys
{
    class Drives
    {
        /*Поток проверки места на диске*/
        public static void CheckDrives()
        {
            try
            {
                GetSys.Program.allDrives = DriveInfo.GetDrives();
                GetSys.Program.diskFail = new bool[10];//allDrives.Length
                int i = 0;
                foreach (DriveInfo MyDriveInfo in GetSys.Program.allDrives)
                {
                    if (MyDriveInfo.IsReady == true)
                    {
                        GetSys.Program.diskFail[i] = false;
                        i++;
                    }
                }
                //string TextOfReport = "";
                while (true)
                {
                    i = 0;
                    Thread.Sleep(1500);
                    DriveInfo[] Drives = DriveInfo.GetDrives();
                    foreach (DriveInfo MyDriveInfo in Drives)
                    {
                        if (MyDriveInfo.IsReady == true)
                        {
                            if (((double)(MyDriveInfo.AvailableFreeSpace) / (1024 * 1024) < GetSys.Program.Threshold) && (!GetSys.Program.diskFail[i])) //мало места и до этого флаг недостаточного места был сброшен
                            {

                                //устанавливаем флаг недостаточного места
                                GetSys.Program.diskFail[i] = true;
                                if (GetSys.Program.useSMTP)    //Отправляем уведомление по почте всем из списка рассылки
                                    foreach (string mail in GetSys.Program.emailList)
                                    {
                                        Communication.SendMail(mail, System.Environment.MachineName.ToLower() + "@om.mrsks.ru", "Внимание! " + System.Environment.MachineName + ((GetSys.Program.aliasip.Length != 0) ? " (" + GetSys.Program.aliasip.Trim() + ")" : ""), DateTime.Now.ToString() + " Недостаточно места на диске " + MyDriveInfo.Name + ", " +
                                          ((double)(MyDriveInfo.AvailableFreeSpace) / (1024 * 1024)).ToString("#.##") + " Мб.");
                                    }
                                if (GetSys.Program.useGSM) //Отправляем уведомление по почте всем из списка рассылки
                                    foreach (string sms in GetSys.Program.SMSList)
                                    {
                                        Communication.SendSMS(sms, DateTime.Now.ToString() + " Attention! " + System.Environment.MachineName + ((GetSys.Program.aliasip.Length != 0) ? " (" + GetSys.Program.aliasip.Trim() + ")" : "") + ", low space at " + MyDriveInfo.Name.Remove(2) + ", "
                                            + ((double)(MyDriveInfo.AvailableFreeSpace) / (1024 * 1024)).ToString("#.##") + " Mb.");
                                    }
                            }
                            //если количество свободного места стало выше порога - сбрасываем флаг
                            if (((double)(MyDriveInfo.AvailableFreeSpace) / (1024 * 1024) > GetSys.Program.Threshold) && (GetSys.Program.diskFail[i]))
                            {
                                GetSys.Program.diskFail[i] = false;

                            }
                            i++;
                        }
                    }
                    for (int j = 0; j < i; j++)
                    {
                        if (GetSys.Program.diskFail[j] == true)
                        {
                            GetSys.Program.disks = false;
                            break;
                        }
                        else
                            GetSys.Program.disks = true;
                    }
                }
            }
            catch { }
        }

        /*Конец потока проверки места на диске*/
    }
}
