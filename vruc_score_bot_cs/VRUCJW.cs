using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace vruc_score_bot_cs
{
    class VRUCJW
    {
        HttpClient client;

        private Dictionary<string, string> class_set;

        public VRUCJW(HttpClient client)
        {
            this.client = client;
            this.class_set = new Dictionary<string, string>();
        }

        private static (string name, string value, string path) CreateCookie(string cookieString)
        {
            var properties = cookieString.Split(';').Select(s => s.Trim()).ToArray();
            var name = properties[0].Split('=')[0];
            var value = properties[0].Split('=')[1];
            var path = properties[2].Replace("path=", "");

            return (name, value, path);
        }

        public async Task<List<(string name, string xf, string jd, string score, string score1, string score2)>> UpdateScores()
        {
            var ret = new List<(string name, string xf, string jd, string score, string score1, string score2)>();

            using (var resp = await client.GetAsync(
                "https://v.ruc.edu.cn/oauth2/authorize?" +
                "response_type=code&" +
                "scope=all&" +
                "state=yourstate&" +
                "client_id=5d25ae5b90f4d14aa601ede8.ruc&" +
                "redirect_uri=http://jw.ruc.edu.cn/secService/oauthlogin"
                ))
            {
                if (!resp.RequestMessage.RequestUri.ToString().Contains("jw.ruc.edu.cn/Njw2017/index.html"))
                    throw new ApplicationException("API changed");
            }

            string token;

            {
                Uri uri = new Uri("http://jw.ruc.edu.cn");
                IEnumerable<Cookie> responseCookies = LoggingHandler.cookies.GetCookies(uri).Cast<Cookie>();
                var cookie = responseCookies.Where(cookie_ => cookie_.Name == "token").ToArray()[0];
                token = cookie.Value;
                cookie.Expired = true;
            }

            using (var requestMessage = new HttpRequestMessage(
                HttpMethod.Post,
                "https://jw.ruc.edu.cn/resService/jwxtpt/v1/xsd/cjgl_xsxdsq/findKccjList?" +
                "resourceCode=XSMH0507&" +
                "apiCode=jw.xsd.xsdInfo.controller.CjglKccjckController.findKccjList"))
            using (var content = new StringContent(
                JsonConvert.SerializeObject(new
                {
                    pyfa007id = "1",
                    jczy013id = new int[0],
                    fxjczy005id = "",
                    cjckflag = "xsdcjck",
                    page = new
                    {
                        pageIndex = 1,
                        pageSize = 500,
                        orderBy = "[{\"field\":\"jczy013id\",\"sortType\":\"asc\"}]",
                        conditions = "QZDATASOFTJddJJVIJY29uZGl0aW9uR3JvdXAlMjIlM0ElNUIlN0IlMjJsaW5rJTIyJTNBJTIyYW5kJTIyJTJDJTIyY29uZGl0aW9uJTIyJTNBJTVCJTVEJTdEyTTECTTE"
                    }
                }),
                System.Text.Encoding.UTF8,
                "application/json"))
            {
                requestMessage.Headers.Add("token", token);
                requestMessage.Headers.Add("userrolecode", "student");
                requestMessage.Headers.Add("accept", "application/json, text/plain, */*");
                requestMessage.Content = content;

                using (var resp = await client.SendAsync(requestMessage))
                {
                    var text = await resp.Content.ReadAsStringAsync();

                    var obj = JsonConvert.DeserializeAnonymousType(
                        text,
                        new
                        {
                            data = new List<GradeResult>()
                        });

                    foreach (var item in obj.data)
                    {
                        if (item.kcname != null && item.kcname.Length > 0)
                        {
                            if (item.jd == null)
                                item.jd = item.xf;

                            var key = $@"{item.xnxq}-{item.kcname}";
                            var value = $@"{item.xf} {item.jd} {item.zcj} {item.cjxm1} {item.cjxm3}";

                            if (!class_set.ContainsKey(key) || class_set[key] != value)
                            {
                                class_set[key] = value;
                                ret.Add((key, item.xf.ToString(), item.jd.Value.ToString(), item.zcj.ToString(), item.cjxm1, item.cjxm3));
                            }
                        }
                    }
                }
            }

            return ret;
        }
    }
}
