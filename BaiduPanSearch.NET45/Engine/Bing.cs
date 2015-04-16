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
    /// 必应搜索引擎
    /// </summary>
    public class Bing : SearchBase
    {
        /// <summary>
        /// 搜索URL模板
        /// </summary>
        const string SearchUrlTemplate = "http://cn.bing.com/search?q=site%3apan.baidu.com+{0}&first={1}";

        public Bing()
        {
            base.PageSize = 10;
        }

        List<GridRowItem> ParseResult(HtmlNode hn)
        {
            if (hn != null)
            {
                var h2s = hn.SelectNodes("li/h2/a");

                if (h2s != null && h2s.Count > 0)
                {
                    var ls = new List<GridRowItem>();
                    foreach (var h2 in h2s)
                    {
                        string title = HttpUtility.HtmlDecode(h2.InnerText);
                        string url = base.ParseUrl(HttpUtility.HtmlDecode(h2.Attributes["href"].Value));

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
                            var b_results = hd.GetElementbyId("b_results");
                            if (b_results != null)
                            {
                                var sb_count = b_results.SelectSingleNode("li/span[@class='sb_count']");
                                if (sb_count != null)
                                {
                                    base.ResultStatus = "共" + sb_count.InnerText;
                                    base.TotalPage = ParseTotal(sb_count.InnerText) / PageSize;
                                }

                                ls = ParseResult(b_results);
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
                int first = (CurrentPage - 2) * PageSize + 1;
                string url = string.Format(SearchUrlTemplate, base.Keyword, first);

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
                            var b_results = hd.GetElementbyId("b_results");
                            if (b_results != null)
                            {
                                CurrentPage--;
                                ls = ParseResult(b_results);
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
                int first = CurrentPage * PageSize + 1;
                string url = string.Format(SearchUrlTemplate, base.Keyword, first);

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
                            var b_results = hd.GetElementbyId("b_results");
                            if (b_results != null)
                            {
                                CurrentPage++;
                                ls = ParseResult(b_results);
                            }
                        }
                    }
                }
            }

            return ls;
        }
    }
}
