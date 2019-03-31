using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using Xzy.EmbeddedApp.Utils;

namespace Xzy.EmbeddedApp.WinForm.Utils
{
    public class HttpClientHelp
    {
        public bool PostFunction(string url,string strContent,string macaddrss,string key, string ukey)
        {
            bool auth = false;
            try
            {
                string serviceAddress = url;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(serviceAddress);

                request.Method = "POST";
                request.ContentType = "application/json";
                using (StreamWriter dataStream = new StreamWriter(request.GetRequestStream()))
                {
                    dataStream.Write(strContent);
                    dataStream.Close();
                }
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                string encoding = response.ContentEncoding;
                if (encoding == null || encoding.Length < 1)
                {
                    encoding = "UTF-8";
                }
                StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding(encoding));
                string retString = reader.ReadToEnd();
                //解析josn
                JObject jo = JObject.Parse(retString);

                //验证来源
                int authflag = Int32.Parse(jo["code"].ToString());
                string token = jo["token"].ToString();
                int stamp = Int32.Parse(jo["stamp"].ToString());

                MD5 md5 = new MD5CryptoServiceProvider();
                byte[] result = Encoding.Default.GetBytes(ukey + macaddrss + stamp + authflag + key); //用户编码、网卡地址、时间、请求状态、私钥
                byte[] bytekey = md5.ComputeHash(result);
                string clienttoken = BitConverter.ToString(bytekey).Replace("-", "");
                clienttoken = clienttoken.ToLower();                

                if (token == clienttoken)
                {
                    if (authflag == 200)
                    {
                        auth = true;
                    }
                }
                else
                {
                    auth = false;
                }                
            }
            catch(Exception ex)
            {
                LogUtils.Error(string.Format("远程验证失败，错误信息:{0}",ex.Message.ToString()));
            }        

            return auth;
        }

            //private static readonly HttpClient client = new HttpClient();
            /// <summary>
            /// post请求
            /// </summary>
            /// <param name="url"></param>
            /// <param name="postData">post数据</param>
            /// <returns></returns>
            public async void PostResponse(string url)
        {
            HttpClient httpClient = new HttpClient();

            var data = new Dictionary<string, string>();
            data["ukey"] = "A054865422";
            data["macadd"] = "cs_45-65-fr-68";
            data["time"] = "1530264506";

            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] result = Encoding.Default.GetBytes(data["ukey"] + data["macadd"] + data["time"].ToString() + "094a0af9bcca4fcb3371");
            byte[] bytekey = md5.ComputeHash(result);
            string token = BitConverter.ToString(bytekey).Replace("-", "");
            data["token"] = token;

            var content = new FormUrlEncodedContent(data);
            
            HttpResponseMessage response = await httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();
            string statusCode = await response.Content.ReadAsStringAsync();
            Console.WriteLine(statusCode);
            //return statusCode;
        }
    }
}
