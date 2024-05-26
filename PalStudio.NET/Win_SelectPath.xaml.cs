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

namespace PalStudio.NET
{
    /// <summary>
    /// Win_SelectPath.xaml 的交互逻辑
    /// </summary>
    public partial class Win_SelectPath : Window
    {
        private BOOL            m_fIsMoving, m_fIsChildTriggerEvent;
        private MouseEventType  m_metWinToolButtonMouseEvent = MouseEventType.None, m_metProjectMouseEvent = MouseEventType.None;

        private static readonly SolidColorBrush m_scbEnter     = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3d3d3d"));
        private static readonly SolidColorBrush m_scbDown      = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#383838"));
        private static readonly SolidColorBrush m_scbOrigin    = (SolidColorBrush)NULL;
        private static          SolidColorBrush m_scbBukup     = m_scbOrigin;

        private static readonly SolidColorBrush m_scbBorder_Origin  = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#424242"));
        private static readonly SolidColorBrush m_scbButton_Origin  = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#383838"));

        private static readonly SolidColorBrush m_scbBorder_Enter   = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7160e8"));
        private static readonly SolidColorBrush m_scbButton_Enter   = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#221d46"));

        public Win_SelectPath()
        {
            InitializeComponent();
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            //
            // 避免传播子控件的事件
            //
            if (m_fIsChildTriggerEvent) return;

            if (e.LeftButton == MouseButtonState.Pressed && !m_fIsMoving)
            {
                m_fIsMoving = TRUE;
                this.DragMove();
            }
        }

        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {
            //
            // 避免传播子控件的事件
            //
            if (m_fIsChildTriggerEvent)
            {
                Border_MouseUp(NULL, (MouseButtonEventArgs)NULL);
                return;
            }

            if (m_fIsMoving)
            {
                m_fIsMoving = FALSE;
                e.Handled   = TRUE;
            }
        }

        private enum MouseEventType
        {
            None    = 0,
            Enter   = 1 << 0,
            Leave   = 1 << 1,
            Down    = 1 << 2,
            Up      = 1 << 3,
        }

        private void
        Border_MouseEventProcessing(
            object                  sender
        )
        {
            //
            // 鼠标给予右上角按钮焦点后设置对应背景色
            //
            Border border = sender as Border;
            
            if (border == null) return;

            m_fIsChildTriggerEvent      = TRUE;

            if ((m_metWinToolButtonMouseEvent & MouseEventType.None)        != 0)
            {
                //
                // 什么都不做
                //
                m_fIsChildTriggerEvent  = FALSE;
                return;
            }
            else if ((m_metWinToolButtonMouseEvent & MouseEventType.Up)     != 0)
            {
                //
                // 鼠标弹起
                // 还原背景色
                //
                m_metWinToolButtonMouseEvent ^= MouseEventType.Up;

                if ((m_metWinToolButtonMouseEvent & MouseEventType.Down)    != 0)
                {
                    m_metWinToolButtonMouseEvent   ^= MouseEventType.Down;
                    border.Background               = m_scbBukup;
                }

                m_fIsChildTriggerEvent  = FALSE;
            }
            else if ((m_metWinToolButtonMouseEvent & MouseEventType.Leave)  != 0)
            {
                //
                // 鼠标离开
                // 更改为初始背景色并备份
                //
                m_metWinToolButtonMouseEvent   ^= MouseEventType.Leave;
                m_metWinToolButtonMouseEvent   ^= MouseEventType.Enter;
                m_metWinToolButtonMouseEvent   ^= MouseEventType.Down;
                border.Background               = m_scbOrigin;
                m_scbBukup                      = (SolidColorBrush)border.Background;

                if ((m_metWinToolButtonMouseEvent & MouseEventType.Down) == 0)
                {
                    m_fIsChildTriggerEvent = FALSE;
                }
            }
            else if ((m_metWinToolButtonMouseEvent & MouseEventType.Down)   != 0)
            {
                //
                // 鼠标按下
                // 更改背景色
                //
                border.Background   = m_scbDown;
            }
            else if ((m_metWinToolButtonMouseEvent & MouseEventType.Enter)  != 0)
            {
                //
                // 鼠标滑上
                // 更改并备份背景色
                //
                border.Background   = m_scbEnter;
                m_scbBukup          = (SolidColorBrush)border.Background;
            }
        }

        private void Border_MouseEnter(object sender, MouseEventArgs e)
        {
            //
            // 鼠标滑上 Button
            //
            m_metWinToolButtonMouseEvent |= MouseEventType.Enter;
            Border_MouseEventProcessing(sender);
        }

        private void Border_MouseLeave(object sender, MouseEventArgs e)
        {
            //
            // 鼠标离开 Button
            // 取消鼠标滑上的状态
            //
            m_metWinToolButtonMouseEvent ^= MouseEventType.Enter;
            m_metWinToolButtonMouseEvent |= MouseEventType.Leave;
            Border_MouseEventProcessing(sender);
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //
            // 鼠标按下 Button
            //
            m_metWinToolButtonMouseEvent |= MouseEventType.Down;
            Border_MouseEventProcessing(sender);
        }

        private void Border_MouseUp(object sender, MouseButtonEventArgs e)
        {
            //
            // 鼠标弹起 Button
            //
            m_metWinToolButtonMouseEvent |= MouseEventType.Up;
            Border_MouseEventProcessing(sender);
        }

        private void
        Project_Button_MouseEventProcessing(
            object                  sender
        )
        {
            //
            // 鼠标给予右上角按钮焦点后设置对应背景色
            //
            Border border = sender as Border;

            if (border == null) return;

            m_fIsChildTriggerEvent = TRUE;
            

            if ((m_metWinToolButtonMouseEvent & MouseEventType.None)        != 0)
            {
                m_fIsChildTriggerEvent  = FALSE;
                return;
            }
            else if ((m_metWinToolButtonMouseEvent & MouseEventType.Up)     != 0)
            {
                m_metWinToolButtonMouseEvent ^= MouseEventType.Up;

                if ((m_metWinToolButtonMouseEvent & MouseEventType.Down)    != 0)
                {
                    m_metWinToolButtonMouseEvent   ^= MouseEventType.Down;
                    m_fIsChildTriggerEvent          = TRUE;
                }

                m_fIsChildTriggerEvent  = FALSE;
            }
            else if ((m_metWinToolButtonMouseEvent & MouseEventType.Leave)  != 0)
            {
                m_metWinToolButtonMouseEvent   ^= MouseEventType.Leave;
                m_metWinToolButtonMouseEvent   ^= MouseEventType.Enter;
                m_metWinToolButtonMouseEvent   ^= MouseEventType.Down;
                border.BorderBrush = m_scbBorder_Origin;
                (border.Child as DockPanel).Background = m_scbButton_Origin;
            }
            else if ((m_metWinToolButtonMouseEvent & MouseEventType.Enter)  != 0)
            {
                border.BorderBrush  = m_scbBorder_Enter;
                (border.Child as DockPanel).Background = m_scbButton_Enter;
            }
            else if ((m_metWinToolButtonMouseEvent & MouseEventType.Down)   != 0)
            {
                border.Background   = m_scbDown;
            }
        }

        private void Project_Button_MouseEnter(object sender, MouseEventArgs e)
        {
            m_metWinToolButtonMouseEvent |= MouseEventType.Enter;
            //Project_Button_MouseEventProcessing(sender);
        }

        private void Project_Button_MouseLeave(object sender, MouseEventArgs e)
        {
            m_metWinToolButtonMouseEvent ^= MouseEventType.Enter;
            m_metWinToolButtonMouseEvent |= MouseEventType.Leave;
            //Project_Button_MouseEventProcessing(sender);
        }

        private void Project_Button_MouseDown(object sender, MouseEventArgs e)
        {
            m_metWinToolButtonMouseEvent |= MouseEventType.Down;
            //Project_Button_MouseEventProcessing(sender);
        }

        private void Project_Button_MouseUp(object sender, MouseEventArgs e)
        {
            m_metWinToolButtonMouseEvent |= MouseEventType.Up;
            //Project_Button_MouseEventProcessing(sender);
        }
    }
}
