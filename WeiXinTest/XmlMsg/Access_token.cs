using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WeiXinTest.XmlMsg
{
    
	/// <summary>  
	///Access_token 的摘要说明  
	/// </summary> 
    public class Access_token
    {
        public Access_token()
        {
        }

        string _access_token = "";
        string _expries_in = "";

        /// <summary>
        /// 获取凭证
        /// </summary>
        public string access_token 
        {
            get {return _access_token; }
            set { _access_token = value; }
        }

        /// <summary>
        /// 凭证有效时间，单位：秒  
        /// </summary>
        public string expires_in
        {
            get { return _expries_in; }
            set { _expries_in = value; }
        }
    }
}