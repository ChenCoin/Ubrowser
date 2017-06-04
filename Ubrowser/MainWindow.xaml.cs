using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
        
        int mouseDown_i = 0;
        /// <summary>
        /// 标题栏双击事件
        /// </summary>
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
                this.WindowState = this.WindowState == WindowState.Maximized ?
                              WindowState.Normal : WindowState.Maximized;
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
        private void btn_max_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal; //设置窗口还原
            }
            else
            {
                this.WindowState = WindowState.Maximized; //设置窗口最大化
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

        /*
private void Window_Loaded(object sender, RoutedEventArgs e)
{
       // 添加一个消息过滤器
       IntPtr hwnd = new WindowInteropHelper(this).Handle;
   HwndSource.FromHwnd(hwnd).AddHook(new HwndSourceHook(WndProc));
}
/*
private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam,
   ref bool handled)
{
   if (msg == WM_NCHITTEST)
   {
       // 获取屏幕坐标
       Point p = new Point();
       int pInt = lParam.ToInt32();
       p.X = (pInt << 16) >> 16;
       p.Y = pInt >> 16;
       if (isOnTitleBar(PointFromScreen(p)))
       {
           // 欺骗系统鼠标在标题栏上
           handled = true;
           return new IntPtr(2);
       }
   }

   return IntPtr.Zero;
}

private bool isOnTitleBar(Point p)
{
   // 假设标题栏在0和100之间
   if (p.Y >= 0 && p.Y < 100)
       return true;
   else
       return false;
}

private const int WM_NCHITTEST = 0x0084;
*/
    }
}
