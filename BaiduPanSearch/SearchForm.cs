using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;
using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace BaidupanSearch
{
    public partial class SearchForm : Form
    {
        private static string browser = "";
        private static readonly int rn = 35;
        private static readonly int pagesize = 30;
        private static int total = 0;
        private static int page = 1;
        private static int totalpage = 0;
        private static string key = "";

        static SearchForm()
        {
            Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(@"http\shell\open\command\");
            browser = key.GetValue("").ToString().Split(new string[] { " -" }, StringSplitOptions.RemoveEmptyEntries)[0];
        }

        static void OpenLink(string url)
        {
            System.Diagnostics.Process.Start(browser, url);
        }

        static string HttpGet(string url)
        {
            string result = "";
            MemoryStream ms = null;

            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                req.Method = "GET";

                HttpWebResponse res = (HttpWebResponse)req.GetResponse();

                if (req.HaveResponse)
                {
                    ms = new MemoryStream();
                    res.GetResponseStream().CopyTo(ms);
                    result = Encoding.GetEncoding(res.ContentEncoding == "" ? "utf-8" : res.ContentEncoding).GetString(ms.ToArray());
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

        static void GetTotal(string total, out int count)
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

        public SearchForm()
        {
            InitializeComponent();

            //Set ListView Item Row Height
            ImageList imgList = new ImageList();
            imgList.ImageSize = new Size(1, 21);
            lvResult.SmallImageList = imgList;
        }

        private void SetTotal(int total, int page, int totalpage)
        {
            lbResult.Text = string.Format("共 {0} 条 , 当前第 {1} 页 , 共 {2} 页 ", total, page, totalpage);
        }

        private void Reset()
        {
            lbResult.Text = "";
            key = "";
            total = 0;
            page = 1;
            totalpage = 0;
        }

        private void Search()
        {
            lvResult.BeginUpdate();

            try
            {
                string url = string.Format("http://www.baidu.com/s?wd=site:pan.baidu.com%20{0}&pn={1}&rn={2}", key, (page - 1) * rn, rn);
                string result = HttpGet(url);

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

                            ListViewItem lvi = new ListViewItem();

                            lvi.Text = "  " + (i + 1).ToString() + "、  " + a.InnerText.Replace("|百度云 网盘-分享无限制", "");
                            lvi.ToolTipText = div.InnerText;
                            lvi.SubItems.Add(a.Attributes["href"].Value);

                            lvResult.Items.Add(lvi);
                        }

                        var nums = hd.DocumentNode.SelectSingleNode("//span[@class='nums']");
                        GetTotal(nums.InnerText, out total);

                        totalpage = total % pagesize == 0 ? total / pagesize : total / pagesize + 1;
                        cmdNext.Enabled = totalpage > page;
                        SetTotal(total, page, totalpage);
                    }
                    else
                    {
                        SetTotal(0, 0, 0);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            lvResult.EndUpdate();
        }

        private void cmdSearch_Click(object sender, EventArgs e)
        {
            if (tbKey.Text == "") return;

            Reset();
            lvResult.Items.Clear();
            key = tbKey.Text;
            Search();
        }

        private void lvResult_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var lv = sender as ListView;
            if (lv.SelectedItems.Count > 0)
            {
                var lvi = lv.SelectedItems[0];
                OpenLink(lvi.SubItems[1].Text);
            }
        }

        private void cmdPrevious_Click(object sender, EventArgs e)
        {
            page--;
            lvResult.Items.Clear();
            Search();

            cmdPrevious.Enabled = page != 1;
            cmdNext.Enabled = true;
        }

        private void cmdNext_Click(object sender, EventArgs e)
        {
            page++;
            lvResult.Items.Clear();
            Search();

            cmdNext.Enabled = page != pagesize;
            cmdPrevious.Enabled = true;
        }

    }
}
