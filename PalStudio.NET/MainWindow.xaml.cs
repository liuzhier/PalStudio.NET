using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

using BOOL      = System.Boolean;
using CHAR      = System.Char;
using BYTE      = System.Byte;
using SHORT     = System.Int16;
using WORD      = System.UInt16;
using INT       = System.Int32;
using UINT      = System.UInt32;
using SDWORD    = System.Int32;
using DWORD     = System.UInt32;
using LPSTR     = System.String;
using System.Windows.Media.Animation;
using System.Threading;

using static PalGlobal.Pal_Global;
using static PalMain.Pal_Main;

namespace PalStudio.NET
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        DispatcherTimer Close_timer;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
#if DEBUG
            this.Close();
            //win_SelectPath.Show();
            win_Main.Show();
            return;
#else
            //
            // 计时 5 秒后关闭窗口
            //
            this.MouseDoubleClick += new MouseButtonEventHandler(Window_MouseDoubleClick);

            do
            {
                // 创建DispatcherTimer实例
                Close_timer = new DispatcherTimer();

                // 设置DispatcherTimer的间隔时间为5秒
                Close_timer.Interval = TimeSpan.FromSeconds(2);

                // 注册Tick事件处理程序
                Close_timer.Tick += Timer_Tick;

                // 启动定时器
                Close_timer.Start();
            } while (FALSE);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            // 关闭定时器
            if (Close_timer != NULL) Close_timer.Stop();

            // 定时器触发时执行的操作，这里关闭窗口
            Close();

            win_SelectPath.Show();
        }

        private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (FALSE) Timer_Tick(NULL, (EventArgs)NULL);
#endif
        }
    }
}
