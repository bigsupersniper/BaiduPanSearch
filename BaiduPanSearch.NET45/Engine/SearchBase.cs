using BaiduPanSearch.NET45.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace BaiduPanSearch.NET45.Engine
{
    public abstract class SearchBase : ISearch
    {
        const string ShareUrlTemplate = "http://pan.baidu.com/share/link?uk={0}&shareid={1}";

        public int PageSize { get; protected set; }

        public int TotalPage { get; protected set; }

        public int CurrentPage { get; protected set; }

        public string Keyword { get; protected set; }

        public string ResultStatus { get; protected set; }

        public Dictionary<int, List<GridRowItem>> Cached { get; protected set; }

        public abstract Task<List<GridRowItem>> Search(string keyword);

        public abstract Task<List<GridRowItem>> PageUp();

        public abstract Task<List<GridRowItem>> PageDown();

        public SearchBase()
        {
            this.CurrentPage = 1;
            this.Cached = new Dictionary<int, List<GridRowItem>>();
        }

        /// <summary>
        /// 分解参数字符串
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        protected IDictionary<string, string> SplitQuery(string url)
        {
            IDictionary<string, string> dict = null;

            if (!string.IsNullOrEmpty(url))
            {
                Uri uri = new Uri(url);
                string qs = uri.Query.Trim('?');
                string[] pms = qs.Split('&');
                if (pms.Length > 0)
                {
                    dict = new Dictionary<string, string>();
                    foreach (var p in pms)
                    {
                        var _ps = p.Split('=');
                        if (_ps.Length >= 2)
                        {
                            dict.Add(_ps[0], _ps[1]);
                        }
                    }
                }
            }

            return dict;
        }

        /// <summary>
        /// 转换为可以用链接
        /// </summary>
        /// <param name="originUrl"></param>
        /// <returns></returns>
        protected string ParseUrl(string originUrl)
        {
            string url = originUrl;

            var dict = SplitQuery(originUrl);
            if (dict != null)
            {
                if (dict.ContainsKey("uk") && dict.ContainsKey("shareid"))
                {
                    url = string.Format(ShareUrlTemplate, dict["uk"], dict["shareid"]);
                }
            }

            return url;
        }

        /// <summary>
        /// 转换含总条数的文本
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        protected int ParseTotal(string result)
        {
            int count = 0;

            if (!string.IsNullOrEmpty(result))
            {
                string _res = "";
                MatchCollection ms = Regex.Matches(result, "\\d+");
                if (ms != null)
                {
                    foreach (var m in ms)
                    {
                        _res += m.ToString();
                    }
                }

                int.TryParse(_res, out count);
            }

            return count;
        }
    }
}
