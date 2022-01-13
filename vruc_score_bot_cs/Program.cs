using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace vruc_score_bot_cs
{
    class Program
    {
        static readonly HttpClient client = new HttpClient(new LoggingHandler(new HttpClientHandler()));
        static readonly string prolog = @"
<html lang=""en"">
<head>
<meta http-equiv=""Content-Type"" content=""text/html; charset=utf-8""/>
</head>
<body>
<style>
table, th, td {
  border:1px solid black;
}
</style>
<table>
<tr>
<th>课程</th>
<th>学分</th>
<th>绩点</th>
<th>平时成绩</th>
<th>期末成绩</th>
<th>总成绩</th>
</tr>
";
        static readonly string epilog = "</table>\r\n</body>\r\n</html>\r\n";

        private static readonly string CONFIG_NAME = "Settings.ini";

        static async Task AsyncMain(Config config)
        {
            var vruc = new VRUC(client);
            var vrucjw = new VRUCJW(client);

            var mail = new MailClient(config);

            await Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        if (!await vruc.CheckLogin())
                        {
                            Console.WriteLine("Token expired, try to login");
                            await vruc.VRUCLogin(config.student_id, config.vruc_password);
                        }

                        Console.WriteLine("Login succeed");

                        var scores = await vrucjw.UpdateScores();

                        if (scores.Count() != 0)
                        {
                            var content = prolog
                                            + scores.Select(
                                                s => $"<tr>\r\n<td>{s.name}</td>\r\n<td>{s.xf}</td>\r\n<td>{s.jd}</td>\r\n<td>{s.score1}</td>\r\n<td>{s.score2}</td>\r\n<td>{s.score}</td>\r\n</tr>\r\n"
                                            ).Aggregate("", (x, y) => x + y)
                                            + epilog;
                            mail.SendMail("成绩更新", content, "html");
                            Console.WriteLine("Grades updated, mail send");
                        }
                        else
                            Console.WriteLine("Pending...");

                        await Task.Delay(295 * 1000);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("\nException Caught!");
                        Console.WriteLine("Message :{0} ", e.Message);

                        mail.SendMail("发生异常", e.Message, "plain");
                    }
                    finally
                    {
                        await Task.Delay(5 * 1000);
                    }
                }
            });
        }

        static void Main(string[] args)
        {
            var config = new Config(CONFIG_NAME);
            AsyncMain(config).Wait();

            Console.WriteLine("按任意键退出...");
            Console.ReadKey();
        }
    }
}
