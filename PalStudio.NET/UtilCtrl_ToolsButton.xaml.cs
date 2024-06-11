using System;
using System.Collections.Generic;
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

using static PalGlobal.Pal_Global;

namespace PalStudio.NET
{
    /// <summary>
    /// UserControl1.xaml 的交互逻辑
    /// </summary>
    public partial class UtilCtrl_ToolsButton : UserControl
    {
        private UIElement m_Parent = (UIElement)NULL;

        public UtilCtrl_ToolsButton()
        {
            InitializeComponent();
        }

        private void UtilCtrl_ToolsButton_Loaded(object sender, RoutedEventArgs e)
        {
            //
            // 绘制 <高祖控件> 的 <Tag> 中指定的图像
            //
            ToolsButton_Image.Source = new BitmapImage(new Uri((string)this.Tag, UriKind.Relative));

            //
            // 若 <高祖控件> 的 <ZIndex> 为 <-1> ，则将图像尺寸设置为 <自适应>
            //
            if (Panel.GetZIndex(this) < 0) ToolsButton_Image.Margin = new Thickness(0);

            //
            // 绑定父级组件
            //
            SetParent((UIElement)this.Parent);
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

        private Brush
        GetBackground() => ToolsButton_Button.Background;

        private void
        SetBackground(
            SolidColorBrush scbColor
        )
        {
            ToolsButton_Button.Background = scbColor;
        }

        private void UtilCtrl_ToolsButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DockPanel Parent = m_Parent as DockPanel;

            if (sender      != NULL &&
                Parent      != NULL &&
                Parent.Tag  != NULL)
            {
                if (Parent.Tag.Equals("ToolsButtonGroupType_Radio"))
                {
                    foreach (UtilCtrl_ToolsButton brother in Parent.Children)
                    {
                        brother.SetBackground(Brushes.Pink);
                    }

                    //
                    // 设置当前 <Item> 背景色高亮
                    //
                    this.SetBackground(Brushes.LightSkyBlue);
                }
                else if (Parent.Tag.Equals("ToolsButtonGroupType_CheckBox"))
                {
                    if (this.GetBackground().Equals(Brushes.Pink))
                    {
                        //
                        // 设置当前 <Item> 背景色高亮
                        //
                        this.SetBackground(Brushes.LightSkyBlue);
                    }
                    else if (this.GetBackground().Equals(Brushes.LightSkyBlue) && (Panel.GetZIndex(this) == -2))
                    {
                        //
                        // 此开关有两层模式
                        // 设置当前 <Item> 背景色更加高亮
                        //
                        this.SetBackground(Brushes.Blue);
                    }
                    else
                    {
                        //
                        // 取消当前 <Item> 背景色高亮
                        //
                        this.SetBackground(Brushes.Pink);
                    }
                }
            }
        }
    }
}
