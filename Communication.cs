using System;
using System.Text;
using System.Net.Mail;
using System.Net.Sockets;
using System.Net;

namespace GetSys
{
    class Communication
    {
        public static void SendMail(string email, string from, string subj, string message)
        {
            try
            {
                SmtpClient Smtp = new SmtpClient(GetSys.Program.smtpIP, 25);
                MailMessage Message = new MailMessage();
                Message.From = new MailAddress(from);
                Message.To.Add(new MailAddress(email));
                Message.Subject = subj;
                Message.Body = message;
                Smtp.Send(Message);
            }
            catch (Exception)
            {
            }
        }

        public static void SendSMS(string num, string message)
        {
            if (((DateTime.Now.Hour >= GetSys.Program.StartHour) && (DateTime.Now.Hour < GetSys.Program.StopHour)) && ((DateTime.Now.DayOfWeek != DayOfWeek.Saturday) && ((DateTime.Now.DayOfWeek != DayOfWeek.Sunday))))
            {
                Console.WriteLine("Настройки временного диапазона запрещают отправку смс!");
            }
            else
            {
                Socket connection = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint ipEndpoint = new IPEndPoint(IPAddress.Parse(GetSys.Program.gsmIP), 2200);
                try
                {
                    connection.Connect(ipEndpoint);
                    connection.Send(Encoding.Unicode.GetBytes("SMS" + "@" + num + "@" + message + "@"));
                }
                catch
                {
                }
                connection.Close();
            }
        }
    }
}
