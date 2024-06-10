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
            if (Panel.GetZIndex(this) == -1) ToolsButton_Image.Margin = new Thickness(0);
        }

        public void
        SimulateMouseDown()
        {
            this.OnPreviewMouseDown((MouseButtonEventArgs)NULL);
        }

        private void ToolsButton_Button_Click(object sender, RoutedEventArgs e)
        {
            SimulateMouseDown();
        }
    }
}
