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


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        #region 标题栏事件

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

        #region 标签新建和关闭
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_new_page(object sender, RoutedEventArgs e)
        {
            uri_jump();
        }

        private void InputUri_KeyDown(object sender, KeyEventArgs e)
        {
            if (Key.Enter == e.Key)
            {
                uri_jump();
            }
        }

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

        private void new_page()
        {

        }

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
                    tab_control.Items.Remove(item);
                    break;
                }
            }
        }
        #endregion 标签新建和关闭
        
        #region 在当前窗口打开网页

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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void CreateNewTab(Uri uri)
        {
            var webBrowser = new WebBrowser { Source = uri };
            webBrowser.Navigated += new NavigatedEventHandler(WebBrowserOnNavigated);

            var tabItem = new TabItem { Content = webBrowser, Header = "正在加载..." };

            var webBrowserHelper = new WebBrowserHelper(webBrowser);
            HelperRegistery.SetHelperInstance(tabItem, webBrowserHelper);
            webBrowserHelper.NewWindow += WebBrowserOnNewWindow;
            SuppressScriptErrors(webBrowser, true);

            tab_control.Items.Add(tabItem);
            tabItem.IsSelected = true;

        }

        //禁止js出错提示
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


        //窗口页面标题栏 = 网页标题
        private void WebBrowserOnNavigated(object sender, NavigationEventArgs e)
        {
            dynamic browser = sender;
            try
            {
                TabItem item = browser.Parent;
                IHTMLDocument2 doc = (IHTMLDocument2)browser.Document;
                String str = doc.title;
                str = (str.Length < 10 ? str : str.Substring(0, 10));
                item.Header = str;

                if (str.Equals("")) {
                    item.Header = "无标题";
                }
                
                print(doc.title);
            }
            catch (Exception) { }
        }

        private void WebBrowserOnNewWindow(object sender, CancelEventArgs e)
        {
            dynamic browser = sender;
            dynamic activeElement = browser.Document.activeElement;
            // 这儿是在新窗口中打开，如果要在内部打开，改变当前browser的Source就行了
            try
            {
                Uri link = activeElement.ToString();
                CreateNewTab(link);
                e.Cancel = true;
            }
            catch (Exception) { }
        }

        #endregion 在当前窗口打开网页

        #region 书签
        private void bookmark_show(object sender, RoutedEventArgs e)
        {
            bookmark.Visibility = Visibility.Visible;
        }

        private void bookmark_close(object sender, RoutedEventArgs e)
        {
            bookmark.Visibility = Visibility.Hidden;
        }

        #endregion

        #region 前进、后退和书签
        //加入书签
        private int select;
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

        //添加书签
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            addbokmarkview.Visibility = Visibility.Collapsed;
            ( (TabItem)tab_control.Items.GetItemAt(select) ).IsSelected = true;

            Button btn = new Button
            {
                Content = browser_title.Text,
                Tag = browser_uri.Text,
                Height = 24,
                Width = 64,
                Margin = new Thickness(10),
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Left,
            };
            btn.Click += new RoutedEventHandler(bookmark_click);
            thebookmark.Children.Add(btn);
        }

        private void bookmark_click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            Uri uri = new Uri(btn.Tag.ToString());
            CreateNewTab(uri);
        }

            //取消添加书签
            private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            addbokmarkview.Visibility = Visibility.Collapsed;
            ((TabItem)tab_control.Items.GetItemAt(select)).IsSelected = true;
        }

        //网页后退
        private void pageBack_Click(object sender, RoutedEventArgs e)
        {
            if (isMainPage())
                return;
            WebBrowser wb = getCurrentPage();
            if (wb.CanGoBack)
                wb.GoBack();
        }

        //网页前进
        private void pageForward_Click(object sender, RoutedEventArgs e)
        {
            if (isMainPage())
                return;
            WebBrowser wb = getCurrentPage();
            if(wb.CanGoForward)
                wb.GoForward();
        }

        //判断是不是主页
        private bool isMainPage()
        {
            TabItem item = (TabItem)tab_control.SelectedItem;
            if (item.Header.Equals("主页"))
                return true;
            return false;
        }

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


        private void print(String str) {
            Console.WriteLine("--- "+str+" ---");
        }

    }
}
