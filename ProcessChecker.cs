using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;

namespace GetSys
{
    class ProcessChecker
    {
        /*Проверка процессов*/
        public static void CheckProcesses()
        {
            bool[] prc = new bool[GetSys.Program.ControlProcesses.Length];     //Предыдущее состояние проверяемого процесса
            for (int i = 0; i < GetSys.Program.ControlProcesses.Length; i++)
            {
                prc[i] = true;
            }
            while (true)
            {
                int h = 0;
                Thread.Sleep(1000);
                Process[] procList = Process.GetProcesses();
                for (int i = 0; i < GetSys.Program.ControlProcesses.Length; i++)
                {
                    //Console.WriteLine(ControlProcesses[i]);
                    bool pr = false; //текущее состояние проверяемого процесса
                    foreach (Process s in procList)
                    {
                        //Console.WriteLine(s.ProcessName);
                        if (s.ProcessName.ToUpper().Contains(GetSys.Program.ControlProcesses[i].ToUpper()))
                        {
                            pr = true;
                            break;
                        }
                        else
                        {
                            pr = false;
                        }
                    }

                    if (pr)
                    {
                        //Устанавливаем флаг о доступности процесса
                        //prc[i] = true;
                    }


                    if ((!pr) && (prc[i])) //Первый раз обнаруживаем что процесс не запущен
                    {
                        prc[i] = false; //Устанавливаем флаг о том, что данный процесс не запущен
                        //Console.WriteLine("Процесс {0} не запущен", ControlProcesses[i]);
                        if (GetSys.Program.useSMTP)    //Отправляем уведомление по почте всем из списка рассылки
                            foreach (string mail in GetSys.Program.emailList)
                            {
                                Communication.SendMail(mail, System.Environment.MachineName.ToLower() + "@om.mrsks.ru", "Внимание! Авария на сервере " +
                                    System.Environment.MachineName + ((GetSys.Program.aliasip.Length != 0) ? " (" + GetSys.Program.aliasip.Trim() + ")" : ""), DateTime.Now.ToString() +
                                    " процесс " + GetSys.Program.ControlProcesses[i] + " не запущен");
                            }



                        if (GetSys.Program.useGSM) //Отправляем уведомление по смс всем из списка рассылки
                            foreach (string sms in GetSys.Program.SMSList)
                            {
                                Communication.SendSMS(sms, DateTime.Now.ToString() + " Attention! " + System.Environment.MachineName + ((GetSys.Program.aliasip.Length != 0) ? " (" + GetSys.Program.aliasip.Trim() + ")" : "") + ", process >" + GetSys.Program.ControlProcesses[i] + "< not running!");
                            }
                    }

                    if ((pr) && (!prc[i])) //Обнаруживаем восстановление процесса после вылета
                    {
                        prc[i] = true; //Устанавливаем флаг о том, что данный процесс не запущен
                        //Console.WriteLine("Процесс {0} не запущен", ControlProcesses[i]);
                        if (GetSys.Program.useSMTP)    //Отправляем уведомление по почте всем из списка рассылки
                            foreach (string mail in GetSys.Program.emailList)
                            {
                                Communication.SendMail(mail, System.Environment.MachineName.ToLower() + "@om.mrsks.ru", "Сервер " + System.Environment.MachineName + ((GetSys.Program.aliasip.Length != 0) ? " (" + GetSys.Program.aliasip.Trim() + ")" : ""), DateTime.Now.ToString() + " процесс " + GetSys.Program.ControlProcesses[i] + " вновь запущен");
                            }
                        if (GetSys.Program.useGSM) //Отправляем уведомление по смс всем из списка рассылки
                            foreach (string sms in GetSys.Program.SMSList)
                            {
                                Communication.SendSMS(sms, DateTime.Now.ToString() + " Attention! " + System.Environment.MachineName + ((GetSys.Program.aliasip.Length != 0) ? " (" + GetSys.Program.aliasip.Trim() + ")" : "") + ", process >" + GetSys.Program.ControlProcesses[i] + "<  run again!");
                            }
                    }

                    h = i;
                }
                for (int j = 0; j < h + 1; j++)
                {
                    if (prc[j] == false)
                    {
                        GetSys.Program.processes = true;
                        break;
                    }
                    else
                        GetSys.Program.processes = false;
                }
            }
        }
        /*Окончание проверки процессов*/
    }
}
