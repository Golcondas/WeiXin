using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.IO;
using System.Xml;
using System.Text;
using WeiXinTest.XmlMsg;
using System.Net;

namespace WeiXinTest.Handler
{
    /// <summary>
    /// WeiXinHandler 的摘要说明
    /// </summary>
    public class WeiXinHandler : IHttpHandler
    {
        public static string msg;
        static string token = System.Configuration.ConfigurationManager.AppSettings["token"];
        static string appid = System.Configuration.ConfigurationManager.AppSettings["appid"];
        static string secret = System.Configuration.ConfigurationManager.AppSettings["secret"];
        public void ProcessRequest(HttpContext context)
        {

            Menu();
            context.Response.ContentType = "text/plain";
            //context.Response.Write("Hello World");
            if (context.Request.HttpMethod.ToLower() == "get")
            {
                //验证
                ValidateUrl();

            }
            else
            {
                // post
                //菜单
                //Menu();
                var postContent = PostDataInfo();
                HandlerMsg(postContent);
            }


        }

        //自定义菜单
        public void Menu()
        {
            string strMenu = "";
            strMenu = @"{
                            ""button"": [{
                                            ""type"": ""view"", 
                                            ""name"": ""弹升理财"", 
                                            ""url"": ""http://service.tanshenglicai.com/""
                                        }, 
                                        {
                                            ""type"": ""click"", 
                                            ""name"": ""Rary"", 
                                            ""key"": ""V1002_TODAY_MUSIC""
                                        }
                        }";
//            strMenu = @"
//                          {
//                            ""button"":[
//                                {	
//                                ""type"":""click"",
//                                ""name"":""今日歌曲"",
//                                ""key"":""V1001_TODAY_MUSIC""
//                                },
//                               {
//                               ""type"": ""click"": 
//                               ""name"": ""Rary"", 
//                               ""key"": ""V1002_TODAY_MUSIC""
//                               },
//                                 {
//                                ""name"":""菜单"",
//                                ""sub_button"":[
//                                 {	
//                                    ""type"":""view"",
//                                    ""name"":""搜索"",
//                                    ""url"":""http://www.soso.com/""
//                                },
//                                {
//                                    ""type"":""view"",
//                                    ""name"":""视频"",
//                                    ""url"":""http://v.qq.com/""
//                                },
//                                {
//                                    ""type"":""click"",
//                                    ""name"":""赞一下我们"",
//                                    ""key"":""V1001_GOOD""
//                                }]
//                            }]
//                        }
//                      ";
            string access_token = IsExistAccess_Token();
            string i = GetPage("https://api.weixin.qq.com/cgi-bin/menu/create?access_token=" + access_token, strMenu);

            IIFoxLibrary.LogHelper.WriteLog("自定义菜单数据信息:" + i);

        }

        public string GetPage(string posturl, string postData)
        {
            Stream outstream = null;
            Stream instream = null;
            StreamReader sr = null;
            HttpWebResponse response = null;
            HttpWebRequest request = null;
            Encoding encoding = Encoding.UTF8;
            byte[] data = encoding.GetBytes(postData);
            // 准备请求...
            try
            {
                // 设置参数
                request = WebRequest.Create(posturl) as HttpWebRequest;
                CookieContainer cookieContainer = new CookieContainer();
                request.CookieContainer = cookieContainer;
                request.AllowAutoRedirect = true;
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = data.Length;
                outstream = request.GetRequestStream();
                outstream.Write(data, 0, data.Length);
                outstream.Close();
                //发送请求并获取相应回应数据
                response = request.GetResponse() as HttpWebResponse;
                //直到request.GetResponse()程序才开始向目标网页发送Post请求
                instream = response.GetResponseStream();
                sr = new StreamReader(instream, encoding);
                //返回结果网页（html）代码
                string content = sr.ReadToEnd();
                string err = string.Empty;
                HttpContext.Current.Response.Write(content);

                IIFoxLibrary.LogHelper.WriteLog("菜单结果：" + content);
                return content;
            }
            catch (Exception ex)
            {
                string err = ex.Message;
                return string.Empty;
            }
        }

        ///// <summary>
        ///// 获取Access_Token
        ///// </summary>
        ///// <returns></returns>
        //private string GetAccess_Token()
        //{

        //    string url = token_url + "&appid=" + appid + "&secret=" + secret;

        //    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
        //    HttpWebResponse response = (HttpWebResponse)request.GetResponse();

        //    using (Stream resStream = response.GetResponseStream())
        //    {
        //        StreamReader reader = new StreamReader(resStream, Encoding.Default);
        //        DataTable dt = JsonToDataTable(reader.ReadToEnd());
        //        access_token = dt.Rows[0]["access_token"].ToString();
        //        resStream.Close();
        //    }
        //    return access_token;

        //}

        //验证微信
        private void ValidateUrl()
        {
            HttpContext context = HttpContext.Current;
            string signature = context.Request["signature"];
            string timestamp = context.Request["timestamp"];
            string nonce = context.Request["nonce"];
            string echostr = context.Request["echostr"];


            //string token = "weixin";

            string[] ArrTemp = { token, timestamp, nonce };
            Array.Sort(ArrTemp);

            string tempStr = string.Join("", ArrTemp);
            //SHA1 加密
            string strSHA1 = FormsAuthentication.HashPasswordForStoringInConfigFile(tempStr, "SHA1");

            if (strSHA1.ToLower() == signature)
            {
                IIFoxLibrary.LogHelper.WriteLog("验证成功！");
                context.Response.Write(echostr);
            }
        }


        /// <summary>
        /// Post信息
        /// </summary>
        /// <returns></returns>
        private HttpContext PostDataInfo()
        {
            HttpContext content = System.Web.HttpContext.Current;
            return content;
        }

        /// <summary>
        /// 文本信息
        /// </summary>
        /// <param name="xmlDoc"></param>
        private string textCase(ExmlMsg xmlMsg)
        {
            string msg = "<xml>" +
                            "<ToUserName><![CDATA[" + xmlMsg.FromUserName + "]]></ToUserName>" +
                            "<FromUserName><![CDATA[" + xmlMsg.ToUserName + "]]></FromUserName>" +
                            "<CreateTime>" + GetCreateTime() + "</CreateTime>" +
                            "<MsgType><![CDATA[text]]></MsgType>" +
                            "<Content><![CDATA[" + xmlMsg.Content + "]]></Content>" +
                            "</xml>";
            return msg;
        }

        //获取事件信息和类型
        public ExmlMsg MsgType(XmlDocument xmlDoc)
        {
            ExmlMsg msg = new ExmlMsg();
            XmlElement xmlElePost = xmlDoc.DocumentElement;
            //IIFoxLibrary.LogHelper.WriteLog("接收到信息XML：" + xmlDoc.InnerText);

            msg.FromUserName = xmlElePost.SelectSingleNode("FromUserName").InnerText;
            msg.ToUserName = xmlElePost.SelectSingleNode("ToUserName").InnerText;
            msg.MsgType = xmlElePost.SelectSingleNode("MsgType").InnerText;
            msg.CreateTime = xmlElePost.SelectSingleNode("CreateTime").InnerText;
            IIFoxLibrary.LogHelper.WriteLog("---------------类型：" + msg.MsgType + "---------------");
            if (msg.MsgType.ToLower() == "text")
            {
                msg.Content = xmlElePost.SelectSingleNode("Content").InnerText;
                IIFoxLibrary.LogHelper.WriteLog("接收消息:" + msg.Content);
            }
            if (msg.MsgType.ToLower() == "event")
            {
                msg.EventName = xmlElePost.SelectSingleNode("Event").InnerText;
            }
            return msg;
        }

        //TimeSpan
        private int GetCreateTime()
        {
            DateTime startTime = new DateTime(1970, 1, 1, 8, 0, 0);
            return (int)(DateTime.Now - startTime).TotalSeconds;
        }

        /// <summary>
        /// 处理事件
        /// </summary>
        /// <param name="xmlPost"></param>
        private void HandlerMsg(HttpContext postContent)
        {
            Stream xmlPost = postContent.Request.InputStream;

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlPost);

            //XmlTextWriter writer = null;
            //IIFoxLibrary.LogHelper.WriteLog("----------------接收的当前信息日志：----------------\n");
            //byte[] b = new byte[xmlPost.Length];
            //xmlPost.Read(b, 0, (int)b.Length);
            //IIFoxLibrary.LogHelper.WriteLog(b.Length + ":" + xmlPost  + xmlDoc.DocumentElement.GetEnumerator().ToString());


            ExmlMsg xmlMsg = MsgType(xmlDoc);
            string xmlType = xmlMsg.MsgType.ToLower();
            IIFoxLibrary.LogHelper.WriteLog("----------------开始处理请求------------------------");
            try
            {
                switch (xmlType)
                {
                    case "text":
                        postContent.Response.Write(reply(xmlMsg));
                        break;
                    case "event":
                        //订阅时回复信息
                        if (xmlMsg.EventName.ToLower() == "subscribe")
                        {
                            IIFoxLibrary.LogHelper.WriteLog("回复信息：" + GetSubscribe(xmlMsg));
                            //回复订阅消息
                            postContent.Response.Write(GetSubscribe(xmlMsg));
                        }
                        //菜单按钮    

                        break;
                    case "image":
                        IIFoxLibrary.LogHelper.WriteLog("----------image：-----------------");
                        IIFoxLibrary.LogHelper.WriteLog(xmlMsg.ToString());
                        IIFoxLibrary.LogHelper.WriteLog("----------image end-----------------");
                        //图片 回复信息
                        xmlMsg.Content = "1";
                        postContent.Response.Write(reply(xmlMsg));
                        break;
                    default:
                        break;
                }
            }
            catch (Exception e)
            {
                IIFoxLibrary.LogHelper.WriteLog("错误：" + e.ToString());
                throw new Exception(e.ToString());
            }
        }

        /// <summary>
        /// 关注时回复消息
        /// </summary>
        private string GetSubscribe(ExmlMsg xmlMsg)
        {
            string context = "欢迎你关注倪宝山\n";
            context += "  个人订阅号权限：\n";
            context += "0:回复文本消息(百度链接)\n";
            context += "1:回复图片消息（未获得）\n";
            context += "2:回复语音消息\n";
            context += "3:回复视频消息\n";
            context += "4:回复音乐消息\n";
            context += "5:回复图文消息\n";
            context += "6:接收事件推送\n";
            context += "7:获取地理位置接口\n";
            context += "8:调起微信扫一扫接口";
            xmlMsg.Content = context;
            return textCase(xmlMsg);
        }

        //收到信息回复
        private string reply(ExmlMsg xmlMsg)
        {
            string context = "欢迎你关注倪宝山\n";
            context += "  个人订阅号权限：\n";
            context += "0:回复文本消息(百度链接)\n";
            context += "1:回复图片消息（未获得）\n";
            context += "2:回复语音消息\n";
            context += "3:回复视频消息\n";
            context += "4:回复音乐消息\n";
            context += "5:回复图文消息\n";
            context += "6:接收事件推送\n";
            context += "7:获取地理位置接口\n";
            context += "8:调起微信扫一扫接口";
            try
            {
                switch (xmlMsg.Content)
                {
                    case "0":
                        //回复文本消息
                        StringBuilder str0 = new StringBuilder();
                        str0.Append("http://www.baidu.com 百度一下");
                        xmlMsg.Content = str0.ToString();
                        break;
                    case "1":
                        //回复图片消息
                        StringBuilder str1 = new StringBuilder();
                        str1.Append("收到图片了");
                        xmlMsg.Content = str1.ToString();
                        break;
                    case "2":
                        //回复语音消息
                        break;
                    case "3":
                        //回复视频消息
                        break;
                    case "4":
                        //回复音乐消息
                        break;
                    case "5":
                        //回复图文消息
                        break;
                    default:
                        xmlMsg.Content = context;
                        break;
                }
            }
            catch
            {

            }
            IIFoxLibrary.LogHelper.WriteLog("回复信息：" + xmlMsg.Content);
            return textCase(xmlMsg);
        }

        public static Access_token GetAccess_token()
        {
            string strUrl = "";
            strUrl = "https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid=" + appid + "&secret=" + secret;

            Access_token a = new Access_token();
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(strUrl);
            req.Method = "GET";
            using (WebResponse wr = req.GetResponse())
            {
                HttpWebResponse myResponse = (HttpWebResponse)req.GetResponse();
                StreamReader sr = new StreamReader(myResponse.GetResponseStream(), Encoding.UTF8);

                string content = sr.ReadToEnd();

                Access_token token = new Access_token();
                token = JsonHelper.ParseFromJson<Access_token>(content);
                a.access_token = token.access_token;
                a.expires_in = token.expires_in;
            }
            return a;
        }

        /// <summary>  
        /// 根据当前日期 判断Access_Token 是否超期  如果超期返回新的Access_Token   否则返回之前的Access_Token  
        /// </summary>  
        /// <param name="datetime"></param>  
        /// <returns></returns>
        public static string IsExistAccess_Token()
        {
            string Token = string.Empty;
            // 读取XML文件中的数据，并显示出来 ，注意文件路径  
            string filepath = System.Web.HttpContext.Current.Server.MapPath("../XmlMsg/XMLFile.xml");
            StreamReader str = new StreamReader(filepath, System.Text.Encoding.UTF8);
            XmlDocument xml = new XmlDocument();
            xml.Load(str);
            str.Close();
            str.Dispose();
            Token = xml.SelectSingleNode("xml").SelectSingleNode("Access_Token").InnerText;
            DateTime YouXRQ = Convert.ToDateTime(xml.SelectSingleNode("xml").SelectSingleNode("Access_Expries").InnerText);

            if (DateTime.Now > YouXRQ)
            {
                DateTime _youxrq = DateTime.Now;
                Access_token mode = GetAccess_token();
                xml.SelectSingleNode("xml").SelectSingleNode("Access_Token").InnerText = mode.access_token;
                _youxrq = _youxrq.AddSeconds(int.Parse(mode.expires_in));
                xml.SelectSingleNode("xml").SelectSingleNode("Access_Expries").InnerText = _youxrq.ToString();
                xml.Save(filepath);
                Token = mode.access_token;
            }
            return Token;
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}