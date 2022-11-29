using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WXWorkGroupSendMessage
{
    public static class StringExtension
    {
        /// <summary>
        /// 图片字符串展位符
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string PicStringPlaceholder(this string path)
        {
            return $@"[请勿删除-{{{path}}}-请勿删除]";
        }

        public const string PicPathRegexStr = @"\[请勿删除-{?|}-请勿删除\]?";
        static Regex regex = new Regex(PicPathRegexStr, RegexOptions.IgnoreCase);
        public static string[] RegexSplitPicPath(this string content) 
        {
            return Regex.Split(content, PicPathRegexStr);
        }

        public static string RegexPicPath(string content)
        {
            return regex.Replace(content, "");
        }

    }
}
