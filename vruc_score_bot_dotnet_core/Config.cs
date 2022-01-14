using System;
using System.IO;
using IniParser;
using IniParser.Model;

namespace vruc_score_bot_cs
{
    class Config
    {
        private static readonly string SECTION = "Global";

        public string student_id { get; set; }

        public string vruc_password { get; set; }

        public string vruc_mailbox { get; set; }

        public string vruc_mailbox_password { get; set; }

        public string sendto_mailbox { get; set; }

        public Config(string filename)
        {
            var parser = new FileIniDataParser();
            var ini = new IniData();

            if (File.Exists(filename))
            {
                ini = parser.ReadFile(filename);
            }
            else
            {
                // Init1
                ini[SECTION].AddKey(new KeyData(nameof(student_id)) { Value = "1970114514", Comments = { "你的学号"  } });
                ini[SECTION].AddKey(new KeyData(nameof(vruc_password)) { Value = "1919810", Comments = { "你的微人大密码" } });
                ini[SECTION].AddKey(new KeyData(nameof(vruc_mailbox)) { Value = "1970114514@ruc.edu.cn", Comments = { "你的人大邮箱" } });
                ini[SECTION].AddKey(new KeyData(nameof(vruc_mailbox_password)) { Value = "1919810", Comments = { "你的人大邮箱密码" } });
                ini[SECTION].AddKey(new KeyData(nameof(sendto_mailbox)) { Value = "", Comments = { "邮件目的邮箱", "留空代表发给人大邮箱" } });
                parser.WriteFile(filename, ini);
                Console.WriteLine("配置文件已生成，请填充字段后再次启动该程序\r\n按任意键退出...\r\n");
                Console.ReadKey();
                System.Environment.Exit(0);
            }

            student_id = ini[SECTION][nameof(student_id)];
            vruc_password = ini[SECTION][nameof(vruc_password)];
            vruc_mailbox = ini[SECTION][nameof(vruc_mailbox)];
            vruc_mailbox_password = ini[SECTION][nameof(vruc_mailbox_password)];

            sendto_mailbox = ini[SECTION][nameof(sendto_mailbox)];

            if (sendto_mailbox.Length == 0)
                sendto_mailbox = vruc_mailbox;
        }
    }
}
