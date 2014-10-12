using System;
using System.Collections.Generic;
using System.Text;

using System.Net.NetworkInformation;
using System.Net;
using System.Net.Mail;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;

namespace AutoSendIP
{
    class Program
    {
        public const string SMTP_SERVER = "smtp.gmail.com";
        public const int PORT = 25;
        public const string USER_ID = "";
        public const string USER_PASSWORD = "";
        public const string MAIL_ADDRESS = "";
        public const string TO_ADDRESS = "";
        public const string SUBJECT = "IP ADDRESS - ";
        private const string GET_IP_URL = "http://whatismyipaddress.com/";


        static void Main(string[] args)
        {
            string strRealIP = string.Empty;
            while (true)
            {
                try
                {
                    SmtpClient mailClient = new SmtpClient(SMTP_SERVER, PORT);
                    mailClient.EnableSsl = true;

                    NetworkCredential crendetial = new NetworkCredential(USER_ID, USER_PASSWORD);
                    mailClient.Credentials = crendetial;

                    string strHostName = Dns.GetHostName();
                    IPHostEntry ipEntry = Dns.GetHostEntry(strHostName);
                    IPAddress[] addr = ipEntry.AddressList;

                    StringBuilder sbAddresses = new StringBuilder();

                    for (int i = 0; i < addr.Length; i++)
                    {
                        sbAddresses.AppendFormat("IP Address {0}: {1} ", i, addr[i].ToString());
                        sbAddresses.AppendLine();
                    }




                    WebRequest request = WebRequest.Create(GET_IP_URL);
                    WebResponse response = request.GetResponse();

                    System.IO.Stream stream = response.GetResponseStream();
                    StreamReader sr = new StreamReader(stream);
                    var content = sr.ReadToEnd();

                    Regex rx = new Regex(@"(?<First>2[0-4]\d|25[0-5]|[01]?\d\d?)\.(?<Second>2[0-4]\d|25[0-5]|[01]?\d\d?)\.(?<Third>2[0-4]\d|25[0-5]|[01]?\d\d?)\.(?<Fourth>2[0-4]\d|25[0-5]|[01]?\d\d?)");
                    Match match = rx.Match(content);

                    sbAddresses.AppendLine();
                    sbAddresses.AppendLine("----------------------------------------");


                    sbAddresses.AppendLine("真实的外网IP：" + match.Value);

                    System.Diagnostics.Process p = new System.Diagnostics.Process();
                    p.StartInfo.CreateNoWindow = true;
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.RedirectStandardOutput = true;
                    p.StartInfo.FileName = "ipconfig";
                    p.StartInfo.Arguments = "/all";
                    p.Start();
                    p.WaitForExit();
                    string s = p.StandardOutput.ReadToEnd();
                    sbAddresses.AppendLine("----------------------------------------");
                    sbAddresses.AppendLine();
                    sbAddresses.AppendLine("内网IP详细信息：" + s);

                    MailMessage message = new MailMessage(MAIL_ADDRESS, TO_ADDRESS, SUBJECT + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString(), sbAddresses.ToString());

                    if (string.Compare(strRealIP, match.Value, true) != 0)
                    {
                        strRealIP = match.Value;

                        mailClient.Send(message);

                        Console.WriteLine(DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString(), sbAddresses.ToString());
                        Console.WriteLine(match.Value);
                        Console.WriteLine("----------------------------------------");
                    }

                    Thread.Sleep(new TimeSpan(0, 3, 0));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }
}
