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

using static PalGlobal.Pal_Global;

namespace PalStudio.NET
{
    /// <summary>
    /// UtilCtrl_ItemList_Item.xaml 的交互逻辑
    /// </summary>
    public partial class UtilCtrl_ItemList_Item : UserControl
    {
        private UIElement   m_Parent = (UIElement)NULL;

        public UtilCtrl_ItemList_Item()
        {
            InitializeComponent();
        }

        public void
        SetText(
            LPSTR           lpszText
        )
        {
            //
            // 设置文本内容
            //
            Item_Text.Content = lpszText;
        }

        public void
        SetParent(
            UIElement       uieParent
        )
        {
            //
            // 设置父级元素
            //
            m_Parent = uieParent;
        }

        private void
        SetBackground(
            SolidColorBrush scbColor
        )
        {
            Background_Border.Background = scbColor;
        }

        private void DockPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DockPanel Parent = m_Parent as DockPanel;

            if (sender  != NULL &&
                Parent  != NULL)
            {
                Parent.Tag = this.Tag;

                foreach (UtilCtrl_ItemList_Item brother in Parent.Children)
                {
                    brother.SetBackground(Brushes.Pink);
                }

                //
                // 设置当前 <Item> 背景色高亮
                //
                this.SetBackground(Brushes.LightSkyBlue);
            }
        }
    }
}
