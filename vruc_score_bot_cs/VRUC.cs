using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace vruc_score_bot_cs
{
    class VRUC
    {
        private static readonly string IFRAME_PATTERN = "<iframe id=\"login-iframe\" src=\"";
        private static readonly string CSRF_PATTERN = "name=\"csrf_token\" value=\"";

        HttpClient client;

        public VRUC(HttpClient client)
        {
            this.client = client;
        }

        public async Task<bool> CheckLogin()
        {
            using (HttpResponseMessage response = await client.GetAsync("http://v.ruc.edu.cn"))
                return !response.RequestMessage.RequestUri.ToString().Contains("login");
        }

        public async Task<bool> VRUCLogin(string username, string password, int captcha_retry_limit = 3)
        {
            string text;

            using (var vruc_resp = await client.GetAsync("http://v.ruc.edu.cn"))
                text = await vruc_resp.Content.ReadAsStringAsync();

            var iframe_url_begin = text.IndexOf(IFRAME_PATTERN) + IFRAME_PATTERN.Length;
            var iframe_url_end = text.IndexOf('"', iframe_url_begin);
            var iframe_url = "https://v.ruc.edu.cn" + text.Substring(iframe_url_begin, iframe_url_end - iframe_url_begin);

            using (var vruc_login_resp = await client.GetAsync(iframe_url))
                text = await vruc_login_resp.Content.ReadAsStringAsync();

            var csrf_begin = text.IndexOf(CSRF_PATTERN) + CSRF_PATTERN.Length;
            var csrf_end = text.IndexOf('"', csrf_begin);
            var csrf_token = text.Substring(csrf_begin, csrf_end - csrf_begin);

            for (var ind = 1; ind <= captcha_retry_limit; ind++)
            {
                using (var captcha_resp = await client.GetAsync("https://v.ruc.edu.cn/auth/captcha"))
                    text = await captcha_resp.Content.ReadAsStringAsync();

                CaptchaResponse captcha_response = JsonConvert.DeserializeObject<CaptchaResponse>(text);
                var b64image = captcha_response.b64s.Replace("data:image/png;base64,", "");

                var result = CaptchaSolver.Solve(b64image);

                using (var content = new StringContent(
                    JsonConvert.SerializeObject(new
                    {
                        username = $@"ruc:{username}",
                        password = password,
                        code = result,
                        remember_me = true,
                        redirect_uri = "/1145141919810",
                        twofactor_password = "",
                        twofactor_recovery = "",
                        token = csrf_token,
                        captcha_id = captcha_response.id
                    }),
                    System.Text.Encoding.UTF8,
                    "application/json"))
                using (var post_request = new HttpRequestMessage(
                        HttpMethod.Post,
                        "https://v.ruc.edu.cn/auth/login"))
                {
                    post_request.Headers.Add("accept", "application/json");
                    post_request.Content = content;
                    using (var post_result = await client.SendAsync(post_request))
                    {
                        if (post_result.StatusCode == System.Net.HttpStatusCode.BadRequest)
                        {
                            var post_result_text = await post_result.Content.ReadAsStringAsync();
                            dynamic resp_object = JsonConvert.DeserializeObject(post_result_text);

                            if (((string)resp_object.error_description).Contains("captcha"))
                            {
                                Console.WriteLine($@"Wrong captcha, try again ({ind}/{captcha_retry_limit})");
                                continue;
                            }
                            else
                            {
                                throw new ArgumentException($@"Wrong password: {post_result_text}");
                            }
                        }

                        if ((await post_result.Content.ReadAsStringAsync()).Contains("1145141919810"))
                            return true;
                    }
                }
            }

            throw new ApplicationException("无法登录");
        }
    }
}
