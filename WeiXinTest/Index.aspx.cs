using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using IIFoxLibrary;
using System.Web.Security;
using System.Text;
using System.IO;
using System.Xml;
using WeiXinTest.XmlMsg;

namespace WeiXinTest
{
    public partial class Index : System.Web.UI.Page
    {

        //appid
        private string appid = System.Configuration.ConfigurationManager.AppSettings["appid"];
        //secret
        private string secret = System.Configuration.ConfigurationManager.AppSettings["secret"];
        private string token_url = System.Configuration.ConfigurationManager.AppSettings["token_url"];
        //token
        private string token = System.Configuration.ConfigurationManager.AppSettings["token"];

        protected void Page_Load(object sender, EventArgs e)
        {
            IIFoxLibrary.LogHelper.WriteLog("--------------------Begin----------------------");
            if (!IsPostBack)
            {
                if (Request.HttpMethod.ToLower() == "post")
                {
                    string weixin = "";
                    weixin = PostInput();//获取xml数据
                    if (!string.IsNullOrEmpty(weixin))
                    {
                        ResponseMsg(weixin);//调用消息适配器
                    }
                }
                else
                {
                    CheckValid();
                }
            }
        }

        #region 获取post请求数据
        private string PostInput()
        {
            Stream s = System.Web.HttpContext.Current.Request.InputStream;
            byte[] b = new byte[s.Length];
            s.Read(b, 0, (int)s.Length);
            return Encoding.UTF8.GetString(b);
        }
        #endregion


        #region 消息类型适配器
        private void ResponseMsg(string weixin)// 服务器响应微信请求
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(weixin);//读取xml字符串
            XmlElement root = doc.DocumentElement;
            ExmlMsg xmlMsg = GetExmlMsg(root);
            //XmlNode MsgType = root.SelectSingleNode("MsgType");
            //string messageType = MsgType.InnerText;
            string messageType = xmlMsg.MsgType;//获取收到的消息类型。文本(text)，图片(image)，语音等。
            IIFoxLibrary.LogHelper.WriteLog("事件类型：" + messageType);
            try
            {
                switch (messageType)
                {
                    //当消息为文本时
                    case "text":
                        textCase(xmlMsg);
                        break;
                    case "event":
                        if (xmlMsg.EventName.Trim() == "subscribe")
                        {
                            //刚关注时的时间，用于欢迎词  
                            int nowtime = ConvertDateTimeInt(DateTime.Now);
                            string msg = "你要关注我，我有什么办法。随便发点什么试试吧~~~";
                            string resxml = "<xml><ToUserName><![CDATA[" + xmlMsg.FromUserName + "]]></ToUserName><FromUserName><![CDATA[" + xmlMsg.ToUserName + "]]></FromUserName><CreateTime>" + nowtime + "</CreateTime><MsgType><![CDATA[text]]></MsgType><Content><![CDATA[" + msg + "]]></Content><FuncFlag>0</FuncFlag></xml>";
                            IIFoxLibrary.LogHelper.WriteLog("关注的欢迎词：" + resxml);
                            Response.Write(resxml);
                        }
                        break;
                    case "image":
                        break;
                    case "voice":
                        break;
                    case "vedio":
                        break;
                    case "location":
                        break;
                    case "link":
                        break;
                    default:
                        break;
                }
                Response.End();
            }
            catch (Exception)
            {

            }
        }

        private string getText(ExmlMsg xmlMsg)
        {
            string con = xmlMsg.Content.Trim();

            System.Text.StringBuilder retsb = new StringBuilder(200);
            retsb.Append("这里放你的业务逻辑");
            retsb.Append("接收到的消息：" + xmlMsg.Content);
            retsb.Append("用户的OPEANID：" + xmlMsg.FromUserName);
            return retsb.ToString();
        }

        #region 操作文本消息 + void textCase(XmlElement root)
        private void textCase(ExmlMsg xmlMsg)
        {
            int nowtime = ConvertDateTimeInt(DateTime.Now);
            string msg = "";
            msg = getText(xmlMsg);
            string resxml = "<xml><ToUserName><![CDATA[" + xmlMsg.FromUserName + "]]></ToUserName><FromUserName><![CDATA[" + xmlMsg.ToUserName + "]]></FromUserName><CreateTime>" + nowtime + "</CreateTime><MsgType><![CDATA[text]]></MsgType><Content><![CDATA[" + msg + "]]></Content><FuncFlag>0</FuncFlag></xml>";
            IIFoxLibrary.LogHelper.WriteLog("对信息进行操作！" + resxml);
            Response.Write(resxml);

        }
        #endregion

        #region 将datetime.now 转换为 int类型的秒
        /// <summary>
        /// datetime转换为unixtime
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        private int ConvertDateTimeInt(System.DateTime time)
        {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
            return (int)(time - startTime).TotalSeconds;
        }
        private int converDateTimeInt(System.DateTime time)
        {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
            return (int)(time - startTime).TotalSeconds;
        }

        /// <summary>
        /// unix时间转换为datetime
        /// </summary>
        /// <param name="timeStamp"></param>
        /// <returns></returns>
        private DateTime UnixTimeToTime(string timeStamp)
        {
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            long lTime = long.Parse(timeStamp + "0000000");
            TimeSpan toNow = new TimeSpan(lTime);
            return dtStart.Add(toNow);
        }
        #endregion


        #region 验证微信签名 保持默认即可
        /// <summary>
        /// 验证微信签名
        /// </summary>
        /// * 将token、timestamp、nonce三个参数进行字典序排序
        /// * 将三个参数字符串拼接成一个字符串进行sha1加密
        /// * 开发者获得加密后的字符串可与signature对比，标识该请求来源于微信。
        /// <returns></returns>
        private bool CheckSignature()
        {
            string signature = Request.QueryString["signature"].ToString();
            string timestamp = Request.QueryString["timestamp"].ToString();
            string nonce = Request.QueryString["nonce"].ToString();
            string[] ArrTmp = { token, timestamp, nonce };
            Array.Sort(ArrTmp);     //字典排序
            string tmpStr = string.Join("", ArrTmp);
            tmpStr = FormsAuthentication.HashPasswordForStoringInConfigFile(tmpStr, "SHA1");
            tmpStr = tmpStr.ToLower();
            if (tmpStr == signature)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void Valid()
        {
            string echoStr = Request.QueryString["echoStr"].ToString();
            if (CheckSignature())
            {
                if (!string.IsNullOrEmpty(echoStr))
                {
                    Response.Write(echoStr);
                    Response.End();
                }
            }
        }
        #endregion

        #region
        private ExmlMsg GetExmlMsg(XmlElement root)
        {

            ExmlMsg xmlMsg = new ExmlMsg()
            {
                FromUserName = root.SelectSingleNode("FromUserName").InnerText,
                ToUserName = root.SelectSingleNode("ToUserName").InnerText,
                CreateTime = root.SelectSingleNode("CreateTime").InnerText,
                MsgType = root.SelectSingleNode("MsgType").InnerText,
            };
            if (xmlMsg.MsgType.Trim().ToLower() == "text")
            {
                xmlMsg.Content = root.SelectSingleNode("Content").InnerText;
            }
            else if (xmlMsg.MsgType.Trim().ToLower() == "event")
            {
                xmlMsg.EventName = root.SelectSingleNode("Event").InnerText;
            }
            return xmlMsg;


        }
        #endregion




        //检查验证
        private void CheckValid()
        {
            string echostr = Request.QueryString["echostr"];
            if (validData())
            {
                if (!string.IsNullOrEmpty(echostr))
                {
                    IIFoxLibrary.LogHelper.WriteLog("-----------------CheckValid OK----------------------------");
                    Response.Write(echostr);
                    //Response.End();
                }
            }
        }



        //微信验证
        /*
         *  signature	微信加密签名，signature结合了开发者填写的token参数和请求中的timestamp参数、nonce参数。
         *  timestamp	时间戳
         *  nonce	随机数
         *  echostr	随机字符串
         */
        private bool validData()
        {
            string signature = Request.QueryString["signature"];
            string timestamp = Request.QueryString["timestamp"];
            string nonce = Request.QueryString["nonce"];
            string echostr = Request.QueryString["echostr"];

            string[] temp = { token, timestamp, nonce };
            Array.Sort(temp);//字典排序

            string strTemp = string.Join("", temp);
            //SHA1 加密
            string strSha1 = FormsAuthentication.HashPasswordForStoringInConfigFile(strTemp, "SHA1");
            if (strSha1.ToLower() == signature)
            {
                IIFoxLibrary.LogHelper.WriteLog("-----------------valid OK----------------------------");
                return true;
            }
            else
            {
                IIFoxLibrary.LogHelper.WriteLog("-----------------valid Failed------------------------");
                return false;
            }
        }
        #endregion
    }
}