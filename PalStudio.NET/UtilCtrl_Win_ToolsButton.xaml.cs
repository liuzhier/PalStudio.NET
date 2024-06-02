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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using BOOL	    = System.Boolean;
using CHAR	    = System.Char;
using BYTE	    = System.Byte;
using SHORT	    = System.Int16;
using WORD	    = System.UInt16;
using INT	    = System.Int32;
using UINT      = System.UInt32;
using SDWORD    = System.Int32;
using DWORD     = System.UInt32;
using LPSTR	    = System.String;

using static PalGlobal.Pal_Global;
using System.Reflection;

namespace PalStudio.NET
{
    /// <summary>
    /// UserControl1.xaml 的交互逻辑
    /// </summary>
    public partial class UtilCtrl_Win_ToolsButton : UserControl
    {
        public  Window  thisWindow;

        private static readonly SolidColorBrush m_scbOrigin    = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#242424"));
        private static readonly SolidColorBrush m_scbEnter     = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3d3d3d"));
        private static readonly SolidColorBrush m_scbDown      = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#383838"));

        public UtilCtrl_Win_ToolsButton()
        {
            InitializeComponent();
        }

        private void Windows_Exit(object sender, RoutedEventArgs e)
        {
            if (thisWindow != NULL) thisWindow.Close();
        }

        private void Windows_Max(object sender, RoutedEventArgs e)
        {
            DockPanel button = sender as DockPanel;

            if (thisWindow != NULL && button != NULL)
            {
                thisWindow.WindowState = WindowState.Maximized;

                button.Visibility = Visibility.Collapsed;
                WinRevert_Button.Visibility = Visibility.Visible;
            }
        }

        private void Windows_Revert(object sender, RoutedEventArgs e)
        {
            DockPanel button = sender as DockPanel;

            if (thisWindow != NULL && button != NULL)
            {
                thisWindow.WindowState = WindowState.Normal;

                button.Visibility = Visibility.Collapsed;
                WinMax_Button.Visibility = Visibility.Visible;
            }
        }

        private void Windows_Min(object sender, RoutedEventArgs e)
        {
            if (thisWindow != NULL) thisWindow.WindowState = WindowState.Minimized;
        }

        private void Win_Moving_Element_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) thisWindow.DragMove();
        }

        private void ToolsButton_MouseEnter(object sender, MouseEventArgs e)
        {
            //
            // 鼠标进入时的颜色
            //
            DockPanel button = sender as DockPanel;

            if (button != NULL)
            {
                button.Background = m_scbEnter;
            }
        }

        private void ToolsButton_MouseLeave(object sender, MouseEventArgs e)
        {
            //
            // 鼠标离开时的颜色
            //
            DockPanel button = sender as DockPanel;

            if (button != NULL)
            {
                button.Background   = m_scbOrigin;
            }
        }

        public void SetIconVisibility(
            BOOL        fIsVisible
        )
        {
            Win_Icon.Visibility = fIsVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        public void SetTitleText(
            LPSTR       text
        )
        {
            Win_Title.Content = text;
        }
    }
}
