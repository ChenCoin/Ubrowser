using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using System.ComponentModel;
using mshtml;
using System.Windows.Media;
using System.IO;
using System.Windows.Markup;
using System.Xml;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

namespace Ubrowser
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        
        public MainWindow()
        {
            InitializeComponent();
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        #region 标题栏事件：最小化、最大化和关闭

        /// <summary>
        /// 窗口移动事件
        /// </summary>
        private void TitleBar_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        /// <summary>
        /// 标题栏双击事件
        /// </summary>
        int mouseDown_i = 0;
        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            mouseDown_i += 1;
            System.Windows.Threading.DispatcherTimer timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 0, 0, 300);
            timer.Tick += (s, e1) => { timer.IsEnabled = false; mouseDown_i = 0; };
            timer.IsEnabled = true;

            if (mouseDown_i % 2 == 0)
            {
                timer.IsEnabled = false;
                mouseDown_i = 0;
                btn_max_Click(sender,e);
            }
        }
        
        /// <summary>
        /// 窗口最小化
        /// </summary>
        private void btn_min_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized; //设置窗口最小化
        }

        /// <summary>
        /// 窗口最大化与还原
        /// </summary>
        private Boolean is_Max = false;
        private double window_Height=0, window_Width=0 ,window_Top=0 ,window_Left=0;
        private void btn_max_Click(object sender, RoutedEventArgs e)
        {
            if (is_Max)
            {
                this.Width = window_Width;
                this.Height = window_Height;
                this.Left = window_Left;
                this.Top = window_Top;
                is_Max = false;
            }
            else
            {
                //先保存窗口大小和位置的信息
                window_Height = this.Height;
                window_Width  = this.Width;
                window_Top    = this.Top;
                window_Left   = this.Left;

                Rect rc = SystemParameters.WorkArea;//获取工作区大小  
                this.Left = 0;//设置位置  
                this.Top = 0;
                this.Width = rc.Width;
                this.Height = rc.Height;
                is_Max = true;

                //Image img = new Image();
                //img.Source = new BitmapImage(new Uri("/Image/maximize_in_normal.png"));
                //img.Source = new BitmapImage(new Uri(@"\Image\maximize_in_normal.png", UriKind.Relative));
                //img.Source = null;

                //btn_max.Style.Setters.Contains.ControlTemplate.btnbg = img;
            }
        }

        /// <summary>
        /// 窗口关闭
        /// </summary>
        private void btn_close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        #endregion 标题栏事件

        #region 标签新建和关闭：Item创建事件和关闭事件
        /// <summary>
        /// 主页的链接跳转按钮事件
        /// </summary>
        private void btn_new_page(object sender, RoutedEventArgs e)
        {
            uri_jump();
        }

        /// <summary>
        /// 主页的输入框回车事件
        /// </summary>
        private void InputUri_KeyDown(object sender, KeyEventArgs e)
        {
            if (Key.Enter == e.Key)
            {
                uri_jump();
            }
        }

        /// <summary>
        /// Uri跳转
        /// </summary>
        private void uri_jump()
        {
            Uri uri;
            try
            {
                uri = new Uri(InputUri.Text);
            }
            catch (Exception)
            {
                try
                {
                    uri = new Uri("http://" + InputUri.Text);
                }
                catch (Exception)
                {
                    uri = new Uri("http://www.baidu.com");
                }
            }
            CreateNewTab(uri);
            InputUri.Text = "";
        }
        
        /// <summary>
        /// TabItem关闭事件
        /// </summary>
        private void Button_clo_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            string header;
            try
            {
                header = btn.Tag.ToString();
            }
            catch (Exception)
            {
                //tab_control.Items.Remove(tab_control.Items.CurrentItem);
                return;
            }
            foreach (TabItem item in tab_control.Items)
            {
                if (item.Header.ToString() == header)
                {
                    WebBrowser wb = (WebBrowser)item.Content;
                    wb.Dispose();
                    tab_control.Items.Remove(item);
                    break;
                }
            }
        }
        #endregion 标签新建和关闭
        
        #region 在当前窗口打开网页：创建Item的实际实现、当前页面打开链接、标题关联

        /// <summary>
        /// 关闭程序时，关闭所有资源
        /// </summary>
        /// <param name="e"></param>
        //关闭窗口时，关闭所有浏览器页面（似乎没有明显效果）
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            foreach (var item in tab_control.Items)
            {
                var tabItem = item as TabItem;
                if (tabItem != null)
                {
                    var helperInstance = HelperRegistery.GetHelperInstance(tabItem);
                    if (helperInstance != null)
                    {
                        helperInstance.NewWindow -= WebBrowserOnNewWindow;
                        helperInstance.Disconnect();
                    }
                }
            }
        }

        /// <summary>
        /// 创建新的网页Item
        /// </summary>
        /// <param name="uri"></param>
        private void CreateNewTab(Uri uri)
        {
            var webBrowser = new WebBrowser { Source = uri };
            webBrowser.Navigated += new NavigatedEventHandler(WebBrowserOnNavigated);
            webBrowser.LoadCompleted += new LoadCompletedEventHandler(WebBrowserOnNavigated); 

            var tabItem = new TabItem { Content = webBrowser, Header = "正在加载..." };

            var webBrowserHelper = new WebBrowserHelper(webBrowser);
            HelperRegistery.SetHelperInstance(tabItem, webBrowserHelper);
            webBrowserHelper.NewWindow += WebBrowserOnNewWindow;
            //SuppressScriptErrors(webBrowser, true);

            tab_control.Items.Add(tabItem);
            tabItem.IsSelected = true;

        }

        /// <summary>
        /// 禁止js出错提示        
        /// /// </summary>
        /// <param name="webBrowser"></param>
        /// <param name="hide"></param>
        static void SuppressScriptErrors(WebBrowser webBrowser, bool hide)
        {
            webBrowser.Navigating += (s, e) =>
            {
                var fiComWebBrowser = typeof(WebBrowser).GetField("_axIWebBrowser2", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                if (fiComWebBrowser == null)
                    return;

                object objComWebBrowser = fiComWebBrowser.GetValue(webBrowser);
                if (objComWebBrowser == null)
                    return;

                objComWebBrowser.GetType().InvokeMember("Silent", System.Reflection.BindingFlags.SetProperty, null, objComWebBrowser, new object[] { hide });
            };
        }

        /// <summary>  
        /// 设置浏览器静默，不弹错误提示框  
        /// </summary>  
        /// <param name="webBrowser">要设置的WebBrowser控件浏览器</param>  
        /// <param name="silent">是否静默</param>  
        private void SetWebBrowserSilent(WebBrowser webBrowser, bool silent)
        {
            FieldInfo fi = typeof(WebBrowser).GetField("_axIWebBrowser2", BindingFlags.Instance | BindingFlags.NonPublic);
            if (fi != null)
            {
                object browser = fi.GetValue(webBrowser);
                if (browser != null)
                    browser.GetType().InvokeMember("Silent", BindingFlags.SetProperty, null, browser, new object[] { silent });
            }
        }

        /// <summary>
        /// 窗口页面标题栏 关联 浏览器的标题，事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WebBrowserOnNavigated(object sender, NavigationEventArgs e)
        {
            dynamic browser = sender;

            setTitle(browser);

            SetWebBrowserSilent(browser,true);
            print("new new");
            
        }
        
        /// <summary>
        /// 将浏览器的网页标题 写给 Item标题
        /// </summary>
        /// <param name="browser"></param>
        private void setTitle(WebBrowser browser)
        {
            try
            {
                TabItem item = (TabItem)browser.Parent;
                IHTMLDocument2 doc = (IHTMLDocument2)browser.Document;
                String str = doc.title;
                str = (str.Length < 10 ? str : str.Substring(0, 10));
                item.Header = str;

                if (str.Equals(""))
                {
                    item.Header = "正在加载...";
                }
                print(doc.title);

            }
            catch (Exception) { }
        }

        /// <summary>
        /// 使得网页跳转在当前程序进行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WebBrowserOnNewWindow(object sender, CancelEventArgs e)
        {
            dynamic browser = sender;
            dynamic activeElement = browser.Document.activeElement;
            // 这儿是在新窗口中打开，如果要在内部打开，改变当前browser的Source就行了
            try
            {
                var link = activeElement.ToString();
                CreateNewTab(new Uri(link));
                e.Cancel = true;
            }
            catch (Exception) { }
        }

        #endregion 在当前窗口打开网页

        #region 书签界面的显示和关闭
        /// <summary>
        /// 显示书签界面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bookmark_show(object sender, RoutedEventArgs e)
        {
            bookmark.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// 隐藏书签界面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bookmark_close(object sender, RoutedEventArgs e)
        {
            bookmark.Visibility = Visibility.Hidden;
        }

        #endregion

        #region 前进、后退和新增书签，事件

        private int select;//新增书签Item 和 原有网页Item 跳转，保存原有网页Item的位置

        /// <summary>
        /// 顶栏新增书签按钮点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void addBookmark_Click(object sender, RoutedEventArgs e)
        {
            if (isMainPage())
                return;

            select = tab_control.SelectedIndex;
            WebBrowser wb = getCurrentPage();
            IHTMLDocument2 doc = (IHTMLDocument2)wb.Document;
            String str1 = doc.title;
            String str2 = wb.Source.ToString();

            browser_title.Text = str1;
            browser_uri.Text = str2;

            addbokmarkview.Visibility = Visibility.Visible;
            addbokmarkview.IsSelected = true;
        }

        /// <summary>
        /// 新增书签界面，确定添加书签
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            addbokmarkview.Visibility = Visibility.Collapsed;
            ( (TabItem)tab_control.Items.GetItemAt(select) ).IsSelected = true;

            add_bm_toView(browser_title.Text, browser_uri.Text);
            writeBookmark(browser_title.Text, browser_uri.Text);
        }

        /// <summary>
        /// 新增书签界面，取消添加书签
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            addbokmarkview.Visibility = Visibility.Collapsed;
            ((TabItem)tab_control.Items.GetItemAt(select)).IsSelected = true;
        }

        /// <summary>
        /// 将书签添加到书签栏界面
        /// </summary>
        /// <param name="title"></param>
        /// <param name="uri"></param>
        private void add_bm_toView(String title,String uri)
        {
            title = (title.Length<10 ? title:title.Substring(0,10));
            Button btn = new Button
            {
                Content = title,
                Tag = uri,
                Height = 24,
                Width = 96,
                Margin = new Thickness(10),
                Padding = new Thickness(5, 0, 0, 0),
                VerticalContentAlignment = VerticalAlignment.Center,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Background = System.Windows.Media.Brushes.White,
            };
            btn.Click += new RoutedEventHandler(bookmark_click);
            thebookmark.Children.Add(btn);
        }

        /// <summary>
        /// 书签栏中书签点击事件，在新页面打开链接
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bookmark_click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            Uri uri;
            try
            {
                uri = new Uri(btn.Tag.ToString());
            }
            catch (Exception)
            {
                uri = new Uri("http://www.baidu.com");
            }
            CreateNewTab(uri);
        }

        /// <summary>
        /// 网页后退
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pageBack_Click(object sender, RoutedEventArgs e)
        {
            if (isMainPage())
                return;
            WebBrowser wb = getCurrentPage();
            if (wb.CanGoBack)
                wb.GoBack();
        }
        
        /// <summary>
        /// 网页前进
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pageForward_Click(object sender, RoutedEventArgs e)
        {
            if (isMainPage())
                return;
            WebBrowser wb = getCurrentPage();
            if(wb.CanGoForward)
                wb.GoForward();
        }
        
        /// <summary>
        /// 判断是不是主页
        /// </summary>
        /// <returns></returns>
        private bool isMainPage()
        {
            TabItem item = (TabItem)tab_control.SelectedItem;
            if (item.Header.Equals("主页"))
                return true;
            return false;
        }

        /// <summary>
        /// 获取当前正在显示的的Item
        /// </summary>
        /// <returns></returns>
        private WebBrowser getCurrentPage()
        {
            TabItem item = (TabItem)tab_control.SelectedItem;
            /*//获取子控件，但此处不需要
            DependencyObject child;
            for (int i = 0; i <= VisualTreeHelper.GetChildrenCount(tab_control) - 1; i++)
            {
                child = VisualTreeHelper.GetChild(tab_control, i);
                if (child is WebBrowser)
                {
                    print("hhh");
                }
            }
            */
            if (item.Content is WebBrowser)
            {
                return (WebBrowser)item.Content;
            }
            return new WebBrowser();
        }

        #endregion

        #region 书签的XML本地存储
        
        /// <summary>
        /// 写入一条书签到XML文件
        /// </summary>
        /// <param name="title">书签标题</param>
        /// <param name="uri">书签链接</param>
        private void writeBookmark(String title, String uri)
        {
            xml_is_exist();//判断XML文件是否存在，不存在则创建

            XmlDocument xmlDoc = new XmlDocument();//读取文件
            xmlDoc.Load("bookmark.xml");

            XmlNode root = xmlDoc.SelectSingleNode("bookmark");//查找<bookmark>

            XmlElement xe1 = xmlDoc.CreateElement("item");//创建一个<item>节点
            xe1.SetAttribute("title", title);//设置该节点title属性
            xe1.SetAttribute("uri", uri);//设置该节点uri属性

            /* //此处是写入子节点到XML文件，不需要
            XmlElement xesub1 = xmlDoc.CreateElement("title");
            xesub1.InnerText = "CS从入门到精通";//设置文本节点
            xe1.AppendChild(xesub1);//添加到<book>节点中
            XmlElement xesub2 = xmlDoc.CreateElement("author");
            xesub2.InnerText = "候捷";
            xe1.AppendChild(xesub2);
            XmlElement xesub3 = xmlDoc.CreateElement("price");
            xesub3.InnerText = "58.3";
            xe1.AppendChild(xesub3);
            */
            root.AppendChild(xe1);//添加到<bookmark>节点中
            xmlDoc.Save("bookmark.xml");
        }

        /// <summary>
        /// 读取全部书签
        /// </summary>
        /// <returns>XML中全部书签信息</returns>
        private List<String> readBookmark()
        {
            List<String> list = new List<string>();

            xml_is_exist();//判断XML文件是否存在，不存在则创建

            XmlDocument xmlDoc = new XmlDocument();//读取文件
            xmlDoc.Load("bookmark.xml");

            XmlNodeList nodeList = xmlDoc.SelectSingleNode("bookmark").ChildNodes;//获取bookstore节点的所有子节点
            foreach (XmlNode xn in nodeList)//遍历所有子节点
            {
                XmlElement xe = (XmlElement)xn;//将子节点类型转换为XmlElement类型
                String str1 = xe.GetAttribute("title");
                String str2 = xe.GetAttribute("uri");
                
                list.Add(str1);
                list.Add(str2);
            }

            return list;
        }

        /// <summary>
        /// 判断XML是否已存在，不存在则新建，并初始化
        /// </summary>
        private void xml_is_exist()
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load("bookmark.xml");
            }
            catch (Exception)
            {
                XmlTextWriter writer = new XmlTextWriter("bookmark.xml", null);
                writer.Formatting = Formatting.Indented;//使用自动缩进便于阅读
                writer.WriteStartElement("bookmark");//书写根元素

                /* //写入子节点
                writer.WriteStartElement("item");
                writer.WriteEndElement();
                */

                writer.WriteFullEndElement();// 关闭根元素
                writer.Close();//将XML写入文件并关闭writer
            }
        }

        #endregion

        #region 其它
        /// <summary>
        /// 用以输出log
        /// </summary>
        /// <param name="str"></param>
        private void print(String str) {
            Console.WriteLine("--- "+str+" ---");
        }

        /// <summary>
        /// 程序启动时，加载书签
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //tryWrite();
            //程序启动后，读取本地书签，并添加到界面
            List<String> list = readBookmark();

            for (int i = 0; i < list.Count; i += 2)
            {
                Console.WriteLine(list[i].ToString());
                add_bm_toView(list[i].ToString(), list[i+1].ToString());
            }

        }
        #endregion

    }
}
