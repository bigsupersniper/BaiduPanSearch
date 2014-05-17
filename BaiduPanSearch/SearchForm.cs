using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Threading;
using System.Runtime.Remoting.Messaging;
using Microsoft.Win32;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Cache;

namespace BaidupanSearch
{
    public partial class SearchForm : Form
    {
        private static BackgroundWorker statusCheckWorker = null;
        private static BackgroundWorker searchWorker = null;
        private static List<ListViewItem> searchResult = null;
        private static AutoCompleteStringCollection searchKeysHistory = null;
        private static string browser = "";
        private static readonly int rn = 35;
        private static readonly int pagesize = 30;
        private static int totalcount = 0;
        private static int page = 1;
        private static int totalpage = 0;
        private static string searchKey = "";

        private string QueryString(string queryString, string key)
        {
            if (!string.IsNullOrEmpty(queryString))
            {
                queryString = queryString.Trim('?');
                string keypair = queryString.Split(new string[] { "&" }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault(i => i.IndexOf(key + "=") == 0);
                if (keypair != null)
                {
                    return keypair.Substring(keypair.IndexOf("=") + 1);
                }
            }

            return null;
        }

        private string AsyncHttpGet(string url, out Uri responseUri, BackgroundWorker worker = null)
        {
            string result = "";
            MemoryStream ms = null;

            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                req.Method = "GET";
                req.Timeout = 30000;
                //req.CachePolicy = new HttpRequestCachePolicy(HttpCacheAgeControl.MaxAge, TimeSpan.FromDays(1));

                int reqTime = 0;
                System.Timers.Timer timer = null;
                if (worker != null)
                {
                    timer = new System.Timers.Timer(1000);
                    timer.Elapsed += (sender, e) =>
                    {
                        worker.ReportProgress(++reqTime * 15);
                    };
                    timer.Start();
                }

                HttpWebResponse res = (HttpWebResponse)req.GetResponse();

                if (req.HaveResponse)
                {
                    responseUri = res.ResponseUri;

                    ms = new MemoryStream();

                    if (worker != null)
                    {
                        int per = reqTime * 15;
                        timer.Stop();
                        Stream stream = res.GetResponseStream();
                        byte[] buffer = new byte[1024];
                        int count = stream.Read(buffer, 0, 1024);

                        while (count > 0)
                        {
                            ms.Write(buffer, 0, count);
                            per += 5;
                            if (per < 95)
                            {
                                worker.ReportProgress(per);
                            }
                            else
                            {
                                worker.ReportProgress(95);
                            }

                            count = stream.Read(buffer, 0, 1024);
                        }
                    }
                    else
                    {
                        res.GetResponseStream().CopyTo(ms);
                    }

                    ms.Position = 0;
                    result = Encoding.GetEncoding(res.ContentEncoding == "" ? "utf-8" : res.ContentEncoding).GetString(ms.ToArray());
                }
                else
                {
                    responseUri = null;
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                if (ms != null)
                {
                    ms.Close();
                }
            }

            return result;
        }

        static void ParseTotal(string total, out int count)
        {
            string result = "";

            MatchCollection ms = Regex.Matches(total, "\\d+");
            if (ms != null)
            {
                foreach (var m in ms)
                {
                    result += m.ToString();
                }
            }

            int.TryParse(result, out count);
        }

        private void StatusCheckWorkerConfig()
        {
            statusCheckWorker = new BackgroundWorker();
            statusCheckWorker.WorkerReportsProgress = true;

            statusCheckWorker.DoWork += (sender, e) =>
            {
                int count = searchResult.Count;
                var ls = new List<dynamic>();

                for (int i = 0; i < count; i++)
                {
                    var item = searchResult[i];
                    bool active = false;

                    string link = item.SubItems[2].Text;
                    Uri resUri = null;

                    try
                    {
                        string html = AsyncHttpGet(link, out resUri);
                        HtmlAgilityPack.HtmlDocument hd = new HtmlAgilityPack.HtmlDocument();
                        hd.LoadHtml(html);
                        active = hd.GetElementbyId("share_nofound_des") == null;
                    }
                    catch { }

                    ls.Add(new { Status = active, Item = item, Uri = resUri });

                    //每检测5条记录报告下进度
                    if (count <= 5)
                    {
                        if (i == count - 1)
                        {
                            statusCheckWorker.ReportProgress(100, ls);
                        }
                    }
                    else
                    {
                        if ((i - 4) % 5 == 0)
                        {
                            int per = (i + 1) * 100 / 30;
                            statusCheckWorker.ReportProgress(per, ls);
                        }
                        else
                        {
                            if ((i + 1) % 5 == count % 5)
                            {
                                statusCheckWorker.ReportProgress(100, ls);
                            }
                        }
                    }
                }
            };

            statusCheckWorker.ProgressChanged += (sender, e) =>
            {
                progressBar1.Value = e.ProgressPercentage;

                var userState = e.UserState as List<dynamic>;

                if (userState != null)
                {
                    userState.ForEach(i =>
                    {
                        ListViewItem item = i.Item;
                        item.UseItemStyleForSubItems = false;

                        if (!i.Status)
                        {
                            item.SubItems[1].Text = "×";
                            //链接无效则设置到分享着的主目录
                            item.SubItems[2].Text = "http://pan.baidu.com/share/home?uk=" + QueryString(i.Uri.Query, "uk");
                            item.SubItems[1].ForeColor = Color.Red;
                        }
                        else
                        {
                            item.SubItems[1].ForeColor = Color.Green;
                            item.SubItems[1].Text = "√";
                            //设置真实分享链接
                            item.SubItems[2].Text = i.Uri.ToString();
                        }
                    });
                }
            };

            statusCheckWorker.RunWorkerCompleted += (sender, e) =>
            {
                cmdSearch.Enabled = true;
                cmdPrevious.Enabled = page > 1;
                cmdNext.Enabled = page < totalpage;
            };

        }

        private void SearchWorkerConfig()
        {
            searchWorker = new BackgroundWorker();
            searchWorker.WorkerReportsProgress = true;

            searchWorker.DoWork += (sender, e) =>
            {
                string url = string.Format("http://www.baidu.com/s?wd=site:pan.baidu.com%20{0}&pn={1}&rn={2}", searchKey, (page - 1) * rn, rn);
                try
                {
                    Uri resUri = null;
                    string result = AsyncHttpGet(url, out resUri, searchWorker);

                    if (!string.IsNullOrEmpty(result))
                    {
                        HtmlAgilityPack.HtmlDocument hd = new HtmlAgilityPack.HtmlDocument();
                        hd.LoadHtml(result);

                        var aTag = hd.DocumentNode.SelectNodes("//a[@data-click]");
                        var divTag = hd.DocumentNode.SelectNodes("//div[@class='c-abstract']");

                        if (aTag != null && divTag != null)
                        {
                            var aTags = aTag.ToArray();
                            var divTags = divTag.ToArray();

                            for (int i = 0; i < aTags.Length; i++)
                            {
                                var a = aTags[i];
                                var div = divTags[i];

                                string href = a.Attributes["href"].Value;
                                ListViewItem lvi = new ListViewItem();

                                lvi.Text = "  " + (i + 1).ToString() + "、  " + a.InnerText.Replace("|百度云 网盘-分享无限制", "");
                                lvi.ToolTipText = div.InnerText;
                                lvi.SubItems.Add("");
                                lvi.SubItems.Add(href);

                                searchResult.Add(lvi);
                            }

                            var nums = hd.DocumentNode.SelectSingleNode("//span[@class='nums']");
                            ParseTotal(nums.InnerText, out totalcount);
                        }
                    }

                    searchWorker.ReportProgress(100);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            };

            searchWorker.ProgressChanged += (sender, e) =>
            {
                progressBar1.Value = e.ProgressPercentage;
            };

            searchWorker.RunWorkerCompleted += (sender, e) =>
            {
                if (searchResult.Count > 0)
                {
                    lvResult.Items.AddRange(searchResult.ToArray());
                    totalpage = totalcount % pagesize == 0 ? totalcount / pagesize : totalcount / pagesize + 1;
                    SetResultLabel(totalcount, page, totalpage);
                }
                else
                {
                    SetResultLabel(0, 0, 0);
                }

                statusCheckWorker.RunWorkerAsync();
            };
        }

        private void Search()
        {
            cmdSearch.Enabled = false;
            cmdPrevious.Enabled = false;
            cmdNext.Enabled = false;
            lvResult.Items.Clear();
            searchResult.Clear();

            searchWorker.RunWorkerAsync();
        }

        private void SetResultLabel(int total, int page, int totalpage)
        {
            lbResult.Text = string.Format("共 {0} 条 , 当前第 {1} 页 , 共 {2} 页 ", total, page, totalpage);
        }

        public SearchForm()
        {
            InitializeComponent();

            //设置默认浏览器地址
            RegistryKey key = Registry.ClassesRoot.OpenSubKey(@"http\shell\open\command\");
            browser = key.GetValue("").ToString().Split(new string[] { " -" }, StringSplitOptions.RemoveEmptyEntries)[0];

            //设置搜索关键字历史
            searchKeysHistory = new AutoCompleteStringCollection();
            tbKey.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            tbKey.AutoCompleteSource = AutoCompleteSource.CustomSource;
            tbKey.AutoCompleteCustomSource = searchKeysHistory;

            //设置ListViewItem行高
            ImageList imgList = new ImageList();
            imgList.ImageSize = new Size(1, 21);
            lvResult.SmallImageList = imgList;

            //设置搜索结果集合
            searchResult = new List<ListViewItem>();

            //设置搜索与状态检测任务
            StatusCheckWorkerConfig();
            SearchWorkerConfig();
        }

        private void cmdSearch_Click(object sender, EventArgs e)
        {
            if (tbKey.Text == "") return;

            totalcount = 0;
            page = 1;
            totalpage = 0;
            searchKey = tbKey.Text;
            searchKeysHistory.Add(tbKey.Text);
            Search();
        }

        private void lvResult_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var lv = sender as ListView;
            if (lv.SelectedItems.Count > 0)
            {
                var lvi = lv.SelectedItems[0];
                Process.Start(browser, lvi.SubItems[2].Text);
            }
        }

        private void cmdPrevious_Click(object sender, EventArgs e)
        {
            page--;
            Search();
        }

        private void cmdNext_Click(object sender, EventArgs e)
        {
            page++;
            Search();
        }

    }
}
