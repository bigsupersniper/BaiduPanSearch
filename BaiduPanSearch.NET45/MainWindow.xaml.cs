using BaiduPanSearch.NET45.Engine;
using BaiduPanSearch.NET45.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BaiduPanSearch.NET45
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        string browser;
        ISearch engine;

        public MainWindow()
        {
            InitializeComponent();

            //设置默认浏览器地址
            RegistryKey key = Registry.ClassesRoot.OpenSubKey(@"http\shell\open\command\");
            browser = key.GetValue("").ToString().Split(new string[] { " -" }, StringSplitOptions.RemoveEmptyEntries)[0];

            //绑定搜索引擎
            var ls = new List<DropdownItem>
            {
                new DropdownItem{ Name = typeof(Bing).Name , EngineType = typeof(Bing) },
                new DropdownItem{ Name = typeof(Google).Name , EngineType = typeof(Google) }
            };
            cbbEngine.ItemsSource = ls;
            cbbEngine.SelectedIndex = 0;
        }

        /// <summary>
        /// 刷新按钮状态
        /// </summary>
        /// <param name="search"></param>
        void RefreshButton(ISearch search)
        {
            if (search.TotalPage <= 1)
            {
                btnPageUp.IsEnabled = false;
                btnPageDown.IsEnabled = false;
            }
            else
            {
                if (search.CurrentPage > 1)
                {
                    if (search.CurrentPage < search.TotalPage)
                    {
                        btnPageUp.IsEnabled = true;
                        btnPageDown.IsEnabled = true;
                    }
                    else
                    {
                        btnPageUp.IsEnabled = true;
                        btnPageDown.IsEnabled = false;
                    }
                }
                else
                {
                    btnPageUp.IsEnabled = false;
                    btnPageDown.IsEnabled = true;
                }
            }
        }

        /// <summary>
        /// 设置结果状态描述
        /// </summary>
        /// <param name="status"></param>
        /// <param name="page"></param>
        void SetResultStatus(string status, int page)
        {
            lbResult.Content = status + string.Format("   当前第 {0} 页", page);
        }

        /// <summary>
        /// 显示错误
        /// </summary>
        /// <param name="e"></param>
        void ShowError(Exception e)
        {
            MessageBox.Show(e.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void lvResult_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lvResult.SelectedIndex > -1)
            {
                var item = lvResult.SelectedItem as GridRowItem;
                Process.Start(browser, item.Url);
            }
        }

        private async void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(tbKeyword.Text) || cbbEngine.SelectedIndex < 0) return;

            pbProgress.IsIndeterminate = true;

            try
            {
                var engineItem = cbbEngine.SelectedItem as DropdownItem;
                engine = (ISearch)engineItem.EngineType.Assembly.CreateInstance(engineItem.EngineType.FullName);
                var ls = await engine.Search(tbKeyword.Text);
                if (ls != null)
                {
                    lvResult.ItemsSource = ls;
                    SetResultStatus(engine.ResultStatus, engine.CurrentPage);
                }

                RefreshButton(engine);
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
            finally
            {
                pbProgress.IsIndeterminate = false;
            }
        }

        private async void btnPageUp_Click(object sender, RoutedEventArgs e)
        {
            if (engine != null)
            {
                pbProgress.IsIndeterminate = true;

                try
                {
                    var ls = await engine.PageUp();
                    if (ls != null)
                    {
                        lvResult.ItemsSource = ls;
                        SetResultStatus(engine.ResultStatus, engine.CurrentPage);
                    }
                    RefreshButton(engine);
                }
                catch (Exception ex)
                {
                    ShowError(ex);
                }
                finally
                {
                    pbProgress.IsIndeterminate = false;
                }
            }
        }

        private async void btnPageDown_Click(object sender, RoutedEventArgs e)
        {
            if (engine != null)
            {
                pbProgress.IsIndeterminate = true;

                try
                {
                    var ls = await engine.PageDown();
                    if (ls != null)
                    {
                        lvResult.ItemsSource = ls;
                        SetResultStatus(engine.ResultStatus, engine.CurrentPage);
                    }
                    RefreshButton(engine);
                }
                catch (Exception ex)
                {
                    ShowError(ex);
                }
                finally
                {
                    pbProgress.IsIndeterminate = false;
                }
            }
        }
    }
}
