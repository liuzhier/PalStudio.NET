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
            // 创建BitmapImage对象
            BitmapImage image = new BitmapImage();

            // 将Image控件的Source属性设置为BitmapImage对象
            ToolsButton_Image.Source = new BitmapImage(new Uri((string)this.Tag, UriKind.Relative));
        }
    }
}
