using MiniBlinkPinvoke;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Panuon.UI.Silver;
using PlugBase;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms.Integration;
using System.Windows.Input;

namespace CloudDisk
{
    /// <summary>
    /// CloudDisk.xaml 的交互逻辑
    /// </summary>
    [AssemblyInfoAndCompanyInfo(AssemblyName = "CloudDisk", AssemblyVersion = "1.0.0.0", CompanyName = "WenYu", MenuName = "云盘")]
    public partial class CloudDisk : UserControl, IPlugBase
    {
        Grid plugsGrid;
        BlinkBrowser Browser = null;
        public static string Url = "https://pan.baidu.com/";
        private const string DefaultUrlForAddedTabsPAN = "https://pan.baidu.com/";
        //FileDataViewModel fileDataViewModel = new FileDataViewModel();
        ObservableCollection<FileData> fileDataLists = new ObservableCollection<FileData>();
        List<FileDataGridItem> fileDataGridItems = new List<FileDataGridItem>();

        public CloudDisk()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 初始化插件
        /// </summary>
        /// <param name="mainWindow"></param>
        public void Initialize(Grid plugsGrid)
        {
            this.plugsGrid = plugsGrid;
            Grid grid = Content as Grid;
            DependencyObject parent = grid.Parent;
            if (parent != null)
            {
                parent.SetValue(ContentPresenter.ContentProperty, null);
            }
            plugsGrid.Children.Add(grid);
            Browser = CreateNewTab();

            BrowserGrid.Visibility = Visibility.Visible;
            FileListGrid.Visibility = Visibility.Hidden;

            FileDataListGrid.DataContext = fileDataLists;
            // 初始化全局组件后启动浏览器
            InitializeChromium();
        }

        private void BrowserTab_SelectedIndexChanged(object sender, EventArgs e)
        {
            ChangeTab(0);
        }

        /// <summary>
        /// 切换页
        /// </summary>
        /// <param name="type">0 ，切换选项卡 1 跳转地址</param>
        private void ChangeTab(int type)
        {
            var tab = BrowserTab.SelectedTab;
            if (tab != null && tab.Controls.Count != 0)
            {
                var browser = tab.Controls[0] as BlinkBrowser;
                if (browser != null)
                {
                    if (type == 0)
                    {
                        Url = browser.Url;
                    }
                    else if (type == 1)
                    {
                        browser.Url = Url;
                    }
                }
            }
        }

        /// <summary>
        /// 菜单按钮被点击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Click(object sender, MouseButtonEventArgs e)
        {
            foreach (Grid g in plugsGrid.Children)
            {
                g.Visibility = Visibility.Hidden;
            }
            CloudDiskGrid.Visibility = Visibility.Visible;
            
        }

        /// <summary>
        /// 初始化浏览器
        /// </summary>
        public void InitializeChromium()
        {
            Browser.GlobalObjectJs = this;
            
            Url = DefaultUrlForAddedTabsPAN;
            Browser.Url = Url;
            Browser.Focus();
            
            /*new Task(() => {
                //加载网盘
                LoadHttpRequest();
            }).Start();*/

        }

        /// <summary>
        /// 文档加载完成
        /// </summary>
        private void Browser_DocumentReadyCallback()
        {
            // 获取cookies,并设置
            /*if (Browser.Url == "")
            {*/
                HttpUtil.cookieContainer = new CookieContainer();
                string[] cookies = Browser.Cookies.Split(';');

            foreach (string cookie in cookies)
            {
                if (Regex.IsMatch(cookie, @"BDUSS="))
                {
                    HttpUtil.cookieContainer.Add(new Cookie("BDUSS", cookie.Replace("BDUSS=", "").Trim(), "/", Util.Domain));
                    Util.BDUSS = cookie.Replace("BDUSS=", "").Trim();
                }
                if (Regex.IsMatch(cookie, @"STOKEN="))
                {
                    HttpUtil.cookieContainer.Add(new Cookie("STOKEN", cookie.Replace("STOKEN=", "").Trim(), "/", Util.Domain));
                    Util.BDUSS = cookie.Replace("STOKEN=", "").Trim();
                }

                if (Regex.IsMatch(cookie, @"BAIDUID="))
                {
                    Util.BaiDuID = cookie.Replace("BAIDUID=", "").Trim();
                }
            }

            // 加载网盘界面
            if (BrowserGrid.Visibility == Visibility.Visible)
                {
                    new Task(() => {
                        //加载网盘
                        LoadHttpRequest();
                    }).Start();
                }
            /*}*/

            
        }

        private BlinkBrowser CreateNewTab()
        {
            
            var tabPage = new System.Windows.Forms.TabPage() { Text = "" };

            BlinkBrowser browser = new BlinkBrowser();
            tabPage.Controls.Add(browser);
            browser.OnCreateViewEvent += Browser_OnCreateViewEvent;
            browser.OnUrlChangeCall += Browser_OnUrlChangeCall;
            browser.DocumentReadyCallback += Browser_DocumentReadyCallback;
            browser.OnDownloadFile += Browser_OnDownloadFile;

            browser.OnTitleChangeCall += (title) =>
            {
                tabPage.Invoke((EventHandler)delegate
                {
                    tabPage.Text = title;
                });
            };
            browser.OnUrlChange2Call += (nowUrl) =>
            {
                tabPage.Invoke((EventHandler)delegate
                {
                    if (BrowserTab.SelectedTab == tabPage)
                    {
                        Url = nowUrl;
                        /*textBox1.Invoke((EventHandler)delegate
                        {
                            textBox1.Text = nowUrl;
                        });*/
                    }
                });
            };
            browser.Dock = System.Windows.Forms.DockStyle.Fill;
            BrowserTab.TabPages.Add(tabPage);
            BrowserTab.SelectTab(tabPage);

            return browser;
        }

        private IntPtr Browser_OnCreateViewEvent(IntPtr webView, IntPtr param, wkeNavigationType navigationType, string url)
        {
            return CreateNewTab().handle;
        }

        /// <summary>
        /// url更改
        /// </summary>
        /// <param name="url"></param>
        private void Browser_OnUrlChangeCall(string url)
        {
            Console.WriteLine("UrlChange：" + url);
        }

        /// <summary>
        /// 文件下载
        /// </summary>
        /// <param name="url"></param>
        private void Browser_OnDownloadFile(string url)
        {
            Console.WriteLine("下载网址：" + url);
            Aria2.Aria2.AddUri(url);
            MessageBoxX.Show("下载文件：" + HttpUtility.UrlDecode(Regex.Match(url, @"\&fin=\S+&fn=").Value.Replace("&fin=", "").Replace("&fn=", "")), "");
        }
        
        /// <summary>
        /// 加载Http请求设置
        /// </summary>
        public void LoadHttpRequest()
        {
            Util.PanHome = HttpUtil.HttpGet(string.Format("https://{0}/disk/home?", Util.Domain), HttpUtil.html);
            HttpUtil.HttpGet(string.Format("https://pcs.baidu.com/rest/2.0/pcs/file?method=plantcookie&type=ett;"), HttpUtil.gif);
            HttpUtil.HttpGet(string.Format("https://d.pcs.baidu.com/rest/2.0/pcs/manage?method=listhost&t={3}&callback=jQuery{2}_{0}&_={1}", Util.GetMillisecondsTimeStamp(), Util.GetMillisecondsTimeStamp(), Util.GetRandomString(20), Util.GetMillisecondsTimeStamp()), HttpUtil.html);
            Util.Bdstoken = Regex.Match(Util.PanHome, "\"bdstoken\"\\s*:\\s*\"[a-zA-Z0-9]+\"").Value.Replace("bdstoken", "").Replace(":", "").Replace("\"", "").Trim();
            Console.WriteLine(string.Format("bdstoken:{0}", Util.Bdstoken));
            Util.Sign1 = Regex.Match(Util.PanHome, "\"sign1\"\\s*:\\s*\"[a-zA-Z0-9]+\"").Value.Replace("sign1", "").Replace(":", "").Replace("\"", "").Trim();
            Util.Sign3 = Regex.Match(Util.PanHome, "\"sign3\"\\s*:\\s*\"[a-zA-Z0-9]+\"").Value.Replace("sign3", "").Replace(":", "").Replace("\"", "").Trim();
            Util.Timestamp = Regex.Match(Util.PanHome, "\"timestamp\"\\s*:\\s*[0-9]+,").Value.Replace("timestamp", "").Replace(":", "").Replace("\"", "").Replace(",", "").Trim();
            //
            if (string.IsNullOrWhiteSpace(Util.Bdstoken))
            {
                // 没有获取到 bdstoken ,则重新登录
                Dispatcher.BeginInvoke(new Action(() => {
                    BrowserGrid.Visibility = Visibility.Visible;
                    FileListGrid.Visibility = Visibility.Hidden;
                    if (Browser.Url == string.Format("https://{0}/", Util.Domain))
                    {
                        return;
                    }
                    
                    Browser.Url = string.Format("https://{0}/", Util.Domain);
                    Browser.Focus();
                }));
            }
            else
            {
                // 获取到 bdstoken ,则加载文件列表
                Dispatcher.BeginInvoke(new Action(() => {
                    BrowserGrid.Visibility = Visibility.Hidden;
                    FileListGrid.Visibility = Visibility.Visible;

                    Console.WriteLine("获取到 bdstoken ,则加载文件列表");
                    LoadEvaluateScript();
                }));
            }
        }

        /// <summary>
        /// 在此WebBrowser的上下文中加载执行一些Javascript代码
        /// </summary>
        public void LoadEvaluateScript()
        {

            FileStream fsLogid = new FileStream(Environment.CurrentDirectory + "\\js\\" + "logid.js", FileMode.Open);
            StreamReader srLogid = new StreamReader(fsLogid, new UTF8Encoding(false));
            string functionLogid = srLogid.ReadToEnd();
            srLogid.Close();
            fsLogid.Close();
            functionLogid = functionLogid.Replace("BAIDUID", Util.BaiDuID);
            JsValue jsValueLogin = Browser.InvokeJSW(functionLogid);
            Util.Logid = jsValueLogin.ToString();
            Console.WriteLine(string.Format("login:{0}", Util.Logid));
            
            FileStream fsSign = new FileStream(Environment.CurrentDirectory + "\\js\\" + "sign.js", FileMode.Open);
            StreamReader srSign = new StreamReader(fsSign, new UTF8Encoding(false));
            string functionSign = srSign.ReadToEnd();
            srSign.Close();
            fsSign.Close();
            functionSign = functionSign.Replace("SIGN3", Util.Sign3);
            functionSign = functionSign.Replace("SIGN1", Util.Sign1);
            JsValue jsValueSign = Browser.InvokeJSW(functionSign);
            Util.Sign = jsValueSign.ToString();
            Console.WriteLine(string.Format("sign:{0}", Util.Sign));

            GetPanList();
            Dispatcher.BeginInvoke(new Action(() => {
                PanFileDir.Items.Clear();
                PanFileDir.Items.Add("全部文件");
                PanFileDir.SelectedIndex = PanFileDir.Items.Count - 1;
            }));
        }

        /// <summary>
        /// 获取百度盘文件列表
        /// </summary>
        /// <param name="dir"></param>
        /// /// <param name="page"></param>
        public void GetPanList(string dir = "/", string page = "1")
        {
            Util.PanList = HttpUtil.HttpGet(string.Format("https://{0}/api/list?order=time&desc=0&showempty=0&web=1&page={1}&num=100&dir={2}&t=0.{6}&channel=chunlei&web=1&app_id=250528&bdstoken={3}&logid={4}&clienttype=0&startLogTime={5}", Util.Domain, page, Uri.EscapeDataString(dir), Uri.EscapeDataString(Util.Bdstoken), Uri.EscapeDataString(Util.Logid), Util.GetMillisecondsTimeStamp(), Util.GetRandomString()), HttpUtil.json);
            JObject result = JsonConvert.DeserializeObject<JObject>(Util.PanList);
            List<JObject> panList = result["list"].ToObject<List<JObject>>();

            Dispatcher.BeginInvoke(new Action(() => {
                if (page == "1")
                {
                    fileDataLists.Clear();
                    fileDataGridItems.Clear();
                }
                
                foreach (JObject panFile in panList)
                {
                    FileData fileData = new FileData();
                    fileData.IsCheck = false;
                    fileData.Name = (string)panFile["server_filename"];
                    if ((string)panFile["isdir"] == "1")
                    {
                        fileData.Size = "-";
                    }
                    else
                    {
                        fileData.Size = Util.SizeToStorage((string)panFile["size"]);
                    }
                    fileData.Time = Util.ToDateTime((string)panFile["server_mtime"]).ToString("yyyy-MM-dd hh:mm:ss");

                    fileDataLists.Add(fileData);

                    FileDataGridItem fileDataGridItem = new FileDataGridItem();
                    fileDataGridItem.fileData = fileData;
                    fileDataGridItem.panFile = panFile;
                    fileDataGridItems.Add(fileDataGridItem);
                }
                //文件加载数量
                if (fileDataLists.Count % 100 == 0)
                {
                    PanFileLoadNum.Content = string.Format("已加载{0}个", fileDataLists.Count);
                }
                else
                {
                    PanFileLoadNum.Content = string.Format("已全部加载，共{0}个", fileDataLists.Count);
                }
                
            }));
            
        }


        /*public List<string> GetPanDownload(string fidlist)
        {
            string panDownload = HttpUtil.HttpGet(string.Format("https://{0}/api/download?sign={1}&timestamp={2}&fidlist=[{3}]&type=dlink&vip=0&channel=chunlei&web=1&app_id={4}&bdstoken={5}&logid={6}&clienttype=0&startLogTime={7}", Util.Domain, Uri.EscapeDataString(Util.Sign), Util.Timestamp, fidlist, Util.App_id, Uri.EscapeDataString(Util.Bdstoken), Uri.EscapeDataString(Util.Logid), Util.GetMillisecondsTimeStamp()), HttpUtil.json);
            JObject result = JsonConvert.DeserializeObject<JObject>(panDownload);
            List<string> pdUrls = new List<string>();
            Console.WriteLine("Cookie: " + HttpUtil.cookieContainer.Count);
            if ((int)result["errno"] == 0)
            {
                List<JObject> pdList = result["dlink"].ToObject<List<JObject>>();
                foreach (var item in pdList)
                {
                    pdUrls.Add((string)item["dlink"]);
                }
                return pdUrls;
            }
            
            return null;
        }*/


        #region Menu

        // 加载行
        private void FileDataListGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            //右键单击
            e.Row.MouseRightButtonUp += (s, a) =>
            {
                a.Handled = true;
                (sender as DataGrid).SelectedIndex = (s as DataGridRow).GetIndex();
                (s as DataGridRow).Focus();

                FileDataGridItem fileDataGridItem = fileDataGridItems.Find(delegate (FileDataGridItem fdgi) {
                    return fdgi.fileData.Equals((s as DataGridRow).Item as FileData);
                });

                if (fileDataGridItem == null)
                {
                    return;
                }

                foreach (var item in fileDataGridItems)
                {
                    item.fileData.IsCheck = false;
                }
                fileDataGridItem.fileData.IsCheck = true;

                Console.WriteLine(fileDataGridItem.fileData.Name);

                FileDataContextMenu(fileDataGridItem);
            };
        }

        // 左键单击
        private void FileDataListGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            FileDataGridItem fileDataGridItem = fileDataGridItems.Find(delegate(FileDataGridItem fdgi) {
                return fdgi.fileData.Equals((sender as DataGrid).SelectedItem as FileData);
            });

            if (fileDataGridItem == null)
            {
                return;
            }

            foreach (var item in fileDataGridItems)
            {
                item.fileData.IsCheck = false;
            }
            fileDataGridItem.fileData.IsCheck = true;

            Console.WriteLine(fileDataGridItem.fileData.Name);
        }

        //双击
        private void FileDataListGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            FileDataGridItem fileDataGridItem = fileDataGridItems.Find(delegate (FileDataGridItem fdgi) {
                return fdgi.fileData.Equals((sender as DataGrid).SelectedItem as FileData);
            });

            if (fileDataGridItem == null)
            {
                return;
            }

            if ((string)fileDataGridItem.panFile["isdir"] == "1")
            {
                CMOpen_Click(fileDataGridItem);
            }
            
        }

        //滚动条
        private void FileDataListGrid_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var scrollViewer = e.OriginalSource as ScrollViewer;
            if (e.VerticalOffset != 0 && e.VerticalOffset == scrollViewer.ScrollableHeight)
            {
                double sh = scrollViewer.ScrollableHeight;
                if (fileDataLists.Count % 100 == 0)
                {
                    scrollViewer.ScrollToVerticalOffset(sh - 10);

                    GetPanList(Util.Dir, ((fileDataLists.Count / 100) + 1).ToString());

                }
            }
        }

        //
        private void FileStatus_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine(sender);
        }

        /// <summary>
        /// 文件列表上下文菜单
        /// </summary>
        /// <param name="fileDataGridItem"></param>
        void FileDataContextMenu(FileDataGridItem fileDataGridItem)
        {
            ContextMenu contextMenu = new ContextMenu();
            CompositeCollection contextMenuBase = new CompositeCollection();
            CompositeCollection compositeCollection = new CompositeCollection();
            CollectionContainer collectionContainer = new CollectionContainer();

            MenuItem cmOpen = new MenuItem();
            cmOpen.Header = "打开";
            cmOpen.Click += (sender, e) => CMOpen_Click(fileDataGridItem);
            if ((string)fileDataGridItem.panFile["isdir"] != "1")
            {
                cmOpen.IsEnabled = false;
            }

            MenuItem cmDownload = new MenuItem();
            cmDownload.Header = "下载";
            cmDownload.Click += (misender, mie) => CMDownload_Click(fileDataGridItem);

            contextMenuBase.Add(cmOpen);
            contextMenuBase.Add(cmDownload);
            //contextMenuBase.Add(new Separator());
            collectionContainer.Collection = contextMenuBase;
            compositeCollection.Add(collectionContainer);
            contextMenu.ItemsSource = compositeCollection;
            contextMenu.IsOpen = true;
        }

        /// <summary>
        /// 上下文菜单，打开文件夹
        /// </summary>
        private void CMOpen_Click(FileDataGridItem fileDataGridItem)
        {
            Util.Dir = (string)fileDataGridItem.panFile["path"];
            GetPanList(Util.Dir);
            string[] str = Util.Dir.Split('/');
            PanFileDir.Items.Add(str[str.Length - 1]);
            PanFileDir.SelectedIndex = PanFileDir.Items.Count - 1;
        }

        /// <summary>
        /// 上下文菜单，下载
        /// </summary>
        private void CMDownload_Click(FileDataGridItem fileDataGridItem)
        {
            Console.WriteLine(string.Format("https://pcs.baidu.com/rest/2.0/pcs/file?method=download&path={0}&app_id={1}", (string)fileDataGridItem.panFile["path"], Util.App_id));
            Console.WriteLine(Aria2.Aria2.AddUri(string.Format("https://pcs.baidu.com/rest/2.0/pcs/file?method=download&path={0}&app_id={1}", (string)fileDataGridItem.panFile["path"], Util.App_id), 
                string.Format("BDUSS={0};STOKEN={1}", Util.BDUSS, Util.STOKEN)));
        }

        // 盘文件按下
        private void PanFileDir_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            //禁用右键选中项
            if (e.RightButton == MouseButtonState.Pressed)
                e.Handled = true;
        }

        // 盘文件目录单击
        private void PanFileDir_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ListBox listBox = sender as ListBox;
            if (listBox.SelectedItem == null || listBox.SelectedIndex == PanFileDir.Items.Count - 1)
            {
                return;
            }
            string dir = "";
            for (int i = 1; i <= listBox.SelectedIndex; i++)
            {
                dir += "/" + PanFileDir.Items[i];
            }

            if (listBox.SelectedItem.ToString() == "全部文件")
            {
                dir = "/";
            }

            Util.Dir = dir;
            GetPanList(Util.Dir);

            for (int i = PanFileDir.Items.Count - 1; i > listBox.SelectedIndex; i--)
            {
                PanFileDir.Items.RemoveAt(i);
            }
            Console.WriteLine(listBox.SelectedItem);
        }













        #endregion

        
    }



    /// <summary>
    /// GridItem和云盘文件信息
    /// </summary>
    public class FileDataGridItem
    {
        public FileData fileData { get; set; }
        public JObject panFile { get; set; }
    }
}
