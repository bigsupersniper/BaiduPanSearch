using BaiduPanSearch.NET45.Models;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace BaiduPanSearch.NET45.Engine
{
    /// <summary>
    /// 谷歌搜索引擎
    /// </summary>
    public class Google : SearchBase
    {
        /// <summary>
        /// 搜索URL模板
        /// </summary>
        const string SearchUrlTemplate = @"https://www.google.com.hk/search?q=site:pan.baidu.com+{0}&newwindow=1&safe=strict&es_sm=93&biw=1025&bih=225&ei=pnAvVZK8CM6xaarEgJgG&start={1}&sa=N&hl=zh-cn";

        public Google()
        {
            base.PageSize = 10;
        }

        List<GridRowItem> ParseResult(HtmlNode hn)
        {
            if (hn != null)
            {
                var lis = hn.SelectNodes("//li[@class='g']/h3/a");

                if (lis != null && lis.Count > 0)
                {
                    var ls = new List<GridRowItem>();
                    foreach (var h2 in lis)
                    {
                        string title = HttpUtility.HtmlDecode(h2.InnerText);
                        string href = h2.Attributes["href"].Value;
                        int start = href.IndexOf("/url?q=");
                        string url = "";
                        if (start > -1)
                        {
                            url = base.ParseUrl(HttpUtility.HtmlDecode(href.Substring(start + 7)));
                        }
                        ls.Add(new GridRowItem
                        {
                            Title = title,
                            Url = url
                        });
                    }

                    if (!base.Cached.ContainsKey(base.CurrentPage))
                    {
                        base.Cached.Add(base.CurrentPage, ls);
                    }

                    return ls;
                }
            }

            return null;
        }

        public override async Task<List<GridRowItem>> Search(string keyword)
        {
            List<GridRowItem> ls = null;

            if (!string.IsNullOrEmpty(keyword))
            {
                string url = string.Format(SearchUrlTemplate, keyword, 1);
                using (var client = new HttpClient())
                {
                    var res = await client.GetAsync(url);
                    if (res.IsSuccessStatusCode)
                    {
                        string html = await res.Content.ReadAsStringAsync();
                        if (!string.IsNullOrEmpty(html))
                        {
                            var hd = new HtmlDocument();
                            hd.LoadHtml(html);
                            var ires = hd.GetElementbyId("ires");
                            if (ires != null)
                            {
                                var resultStats = hd.GetElementbyId("resultStats");
                                if (resultStats != null)
                                {
                                    int start = resultStats.InnerText.IndexOf("结果");
                                    if (start > -1)
                                    {
                                        base.ResultStatus = resultStats.InnerText.Substring(0, start) + "结果";
                                    }
                                    else
                                    {
                                        base.ResultStatus = resultStats.InnerText;
                                    }

                                    base.TotalPage = ParseTotal(base.ResultStatus) / PageSize;
                                }

                                ls = ParseResult(hd.DocumentNode);
                            }
                        }

                        base.Keyword = keyword;
                    }
                }
            }

            return ls;
        }

        public override async Task<List<GridRowItem>> PageUp()
        {
            List<GridRowItem> ls = null;

            if (base.Cached.ContainsKey(CurrentPage - 1))
            {
                CurrentPage--;
                ls = base.Cached[CurrentPage];
            }
            else if (CurrentPage <= TotalPage)
            {
                int start = (CurrentPage - 2) * PageSize;
                string url = string.Format(SearchUrlTemplate, base.Keyword, start);

                using (var client = new HttpClient())
                {
                    var res = await client.GetAsync(url);
                    if (res.IsSuccessStatusCode)
                    {
                        string html = await res.Content.ReadAsStringAsync();
                        if (!string.IsNullOrEmpty(html))
                        {
                            var hd = new HtmlDocument();
                            hd.LoadHtml(html);
                            var ires = hd.GetElementbyId("ires");
                            if (ires != null)
                            {
                                CurrentPage--;
                                ls = ParseResult(hd.DocumentNode);
                            }
                        }
                    }
                }
            }

            return ls;
        }

        public override async Task<List<GridRowItem>> PageDown()
        {
            List<GridRowItem> ls = null;

            if (base.Cached.ContainsKey(CurrentPage + 1))
            {
                CurrentPage++;
                ls = base.Cached[CurrentPage];
            }
            else if (CurrentPage >= 1)
            {
                int start = (CurrentPage - 1) * PageSize;
                string url = string.Format(SearchUrlTemplate, base.Keyword, start);

                using (var client = new HttpClient())
                {
                    var res = await client.GetAsync(url);
                    if (res.IsSuccessStatusCode)
                    {
                        string html = await res.Content.ReadAsStringAsync();
                        if (!string.IsNullOrEmpty(html))
                        {
                            var hd = new HtmlDocument();
                            hd.LoadHtml(html);
                            var ires = hd.GetElementbyId("ires");
                            if (ires != null)
                            {
                                CurrentPage++;
                                ls = ParseResult(hd.DocumentNode);
                            }
                        }
                    }
                }
            }

            return ls;
        }
    }
}
