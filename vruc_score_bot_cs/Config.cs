using System;

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
            var ini = new IniFile(filename);

            if (!ini.KeyExists("student_id", SECTION))
            {
                // Init
                ini.Write("student_id", "1970114514", SECTION);
                ini.Write("vruc_password", "1919810", SECTION);
                ini.Write("vruc_mailbox", "1970114514@ruc.edu.cn", SECTION);
                ini.Write("vruc_mailbox_password", "1919810", SECTION);
                ini.Write("sendto_mailbox", "", SECTION);
                Console.WriteLine("配置文件已生成，请填充字段后再次启动该程序");
                Console.ReadKey();
                System.Environment.Exit(1);
            }

            student_id = ini.Read("student_id", SECTION);
            vruc_password = ini.Read("vruc_password", SECTION);
            vruc_mailbox = ini.Read("vruc_mailbox", SECTION);
            vruc_mailbox_password = ini.Read("vruc_mailbox_password", SECTION);

            sendto_mailbox = ini.Read("sendto_mailbox", SECTION);

            if (sendto_mailbox.Length == 0)
                sendto_mailbox = vruc_mailbox;
        }
    }
}
