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
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows.Media.Media3D;

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

using PalMap;
using PalVideo;
using PalGlobal;
using PalResources;

using static PalGlobal.Pal_Global;
using static PalMain.Pal_Main;
using static PalCommon.Pal_Common;
using static PalVideo.Pal_Video;
using static PalMap.Pal_Map;
using static PalUtil.Pal_Util;

namespace PalStudio.NET
{
    /// <summary>
    /// Window1.xaml 的交互逻辑
    /// </summary>
    public partial class Win_Main : Window
    {
        private static BOOL             m_fCanClose = TRUE; 
        private static INT              m_iThisMapTile = -1, m_iMapViewportScale = 200;
        private static ScaleTransform   m_stMapViewport_ScaleTransform = new ScaleTransform();

        private static Win_SelectScene  win_SelectScene = new Win_SelectScene();

        public Win_Main()
        {
            InitializeComponent();
        }

        private bool GetCanClose()
        {
            //
            // 判断是否允许关闭窗口
            // 返回 <TRUE> 表示可以关闭
            // 返回 <FALSE> 表示不能关闭
            //
            return m_fCanClose;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            // 在这里添加您的关闭逻辑
            if (!GetCanClose())
            {
                //
                // 如果不能关闭，则取消关闭操作
                //
                e.Cancel = TRUE;
            }
            else
            {
                //
                // 如果可以关闭，则继续关闭过程
                // 优先关闭子进程
                //
                win_SelectScene.SetCanClose(TRUE);
                win_SelectScene.Close();

                base.OnClosing(e);
                PAL_Shutdown();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //
            // 进入 Pal Studio 配置读取
            //
            main((string[])NULL);

            InitMapViewportSurface();

            //
            // 初始缩放 <Map Viewport>
            //
            UTIL_MapViewportScale_TextChanged(m_iMapViewportScale, MapViewport_Canvas, m_stMapViewport_ScaleTransform, MapViewportScale_TextBox);

            //
            // 初始化所有视图的 <Transform Scale> （缩放器）
            //
            foreach (Image image in MapViewport_Canvas.Children)
            {
                if (image != NULL)
                {
                    image.RenderTransform = m_stMapViewport_ScaleTransform;
                }
            }

            //
            // 开始将 <Surface> 转换为 <Image>
            //
            //VIDEO_DrawSurfaceToImage(mc_sfMapTileCursor[0], MapViewport_Active_Image,   Pal_Map.m_MapTileRect);
            //VIDEO_DrawSurfaceToImage(mc_sfMapTileCursor[1], MapViewport_Selected_Image, Pal_Map.m_MapTileRect);
        }

        private void Win_ToolsButton_Loaded(object sender, RoutedEventArgs e)
        {
            //
            // 为右上角自定义工具按钮绑定需要操作的窗口
            //
            Win_ToolsButton.thisWindow = this;
        }

        private void Table_Button_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //
            // 改变 Table Button 背景色
            //
            Border border = sender as Border;

            if (border != NULL)
            {
                foreach (Border brother in Table_Button_Group.Children)
                {
                    brother.Background      = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f3f3f3"));
                    brother.BorderThickness = new Thickness(0, 1, 1, 1);
                }

                border.Background       = Brushes.White;
                border.BorderThickness  = new Thickness(0, 1, 1, 0);
            }
        }

        private void OpenScene_Button_Click(object sender, RoutedEventArgs e)
        {
            INT                         i, nMapTiles;
            UtilCtrl_MapTileList_Item   utilCtrl_MapTileList_Item;

            //
            // 子窗口“关闭”前，禁止用户与主窗口互动
            //
            win_SelectScene.ShowDialog();

            //
            // 如果用户点击了 <确认> 按钮则继续
            //
            if (win_SelectScene.GetIsEnterScene)
            {
                //
                // 初始化 <Map Tiles> 视图列表
                //
                {
                    //
                    // 获取当前场景的 <Map Tiles> 总数
                    //
                    nMapTiles = PAL_SpriteGetNumFrames(Pal_Map.TileSprite);

                    //
                    // 清空 <Map Tiles> 视图列表
                    //
                    MapTilesList_DockPanel.Children.Clear();

                    for (i = 0; i < nMapTiles; i++)
                    {
                        utilCtrl_MapTileList_Item = new UtilCtrl_MapTileList_Item();

                        //
                        // 设置文本为 <Map Tile> 编号
                        //
                        utilCtrl_MapTileList_Item.SetText($"[0x{i:X4}] {i:D5}");
                        utilCtrl_MapTileList_Item.SetParent(MapTilesList_DockPanel);

                        //
                        // 开始初始化 <UtilCtrl_MapTileList_Item>
                        //
                        utilCtrl_MapTileList_Item.Init(i);

                        MapTilesList_DockPanel.Children.Add(utilCtrl_MapTileList_Item);
                        DockPanel.SetDock(utilCtrl_MapTileList_Item, Dock.Top);
                    }

                    //
                    // 将首个 <UtilCtrl_MapTileList_Item> 设为默认选中
                    //
                    ((UtilCtrl_MapTileList_Item)MapTilesList_DockPanel.Children[0]).SimulateMouseDown();

                    //
                    // 更新当前 <Map Tile> 编号
                    //
                    MapTilesList_DockPanel_MouseDown(MapTilesList_DockPanel, (MouseButtonEventArgs)NULL);
                }

                //
                // 开始将 <Surface> 转换为 <Image>
                //
                VIDEO_DrawSurfaceToImage(Pal_Map.m_MapViewport_Surface,             MapViewport_Image,          Pal_Map.m_MapRect);
                VIDEO_DrawSurfaceToImage(Pal_Map.m_MapViewport_Obstacle_Surface,    MapViewport_Obstacle_Image, Pal_Map.m_MapRect);
                VIDEO_DrawSurfaceToImage(Pal_Map.m_MapViewport_Event_Surface,       MapViewport_Event_Image,    Pal_Map.m_MapRect);

                //
                // 绘制 <障碍块>
                //
                Pal_Map.DrawObstacleBlock(MapViewport_Obstacle_Image);

                //
                // 绘制 <事件块>
                //
                Pal_Map.DrawEventBlock(MapViewport_Event_Image);

                if (MapViewport_Border.Visibility != Visibility.Visible)
                {
                    //
                    // 初始化所有的控件
                    //
                    WordMapBox_DockPanel.IsEnabled                  = TRUE;
                    WordMapBox_DockPanel.Opacity                    = 1;
                    SaveMap_Button.IsEnabled                        = TRUE;
                    SaveMap_Button.Opacity                          = 1;
                    MapEditMode_ToolsButtonList.IsEnabled           = TRUE;
                    MapEditMode_ToolsButtonList.Opacity             = 1;
                    MapBlockDisplayMode_ToolsButtonList.IsEnabled   = TRUE;
                    MapBlockDisplayMode_ToolsButtonList.Opacity     = 1;
                    MapLayerMode_ToolsButtonList.IsEnabled          = TRUE;
                    MapLayerMode_ToolsButtonList.Opacity            = 1;
                    MapViewportScale_TextBox.IsEnabled              = TRUE;

                    //
                    // 删除粉背景提示控件
                    //
                    MapViewportBox_DockPanel.Children.Remove(Tip_NotSelected_DockPanel);
                    MapViewport_Border.Visibility = Visibility.Visible;
                }
            }
        }

        private void MapTilesList_DockPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //
            // 获取当前选中 <Map Tile> 的编号
            //
            m_iThisMapTile = (INT)MapTilesList_DockPanel.Tag;

            //
            // 更新当前 <Map Tile> 编号
            //
            ThisMapTileIndex_Label.Content  = $"[0x{m_iThisMapTile:X4}]";
            ThisMapTileIndex_TextBox.Text   = m_iThisMapTile.ToString();

            //
            // 更新当前 <Map Tile> 图像
            //
            ThisMapTile_Image.Source = ((UtilCtrl_MapTileList_Item)MapTilesList_DockPanel.Children[m_iThisMapTile]).GetMapTileImage().Source;
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e) => UTIL_TextBox_Num_PreviewTextInput(e);

        private void ThisMapTileIndex_TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            INT     iThisMapTile;

            //
            // 判断用户输入的数值是否合法
            //
            if ((iThisMapTile = UTIL_TextBoxTextIsMatch(ThisMapTileIndex_TextBox, m_iThisMapTile)) == 0x7FFFFFFF) return;

            //
            // 数值未变动，退出函数
            //
            if (iThisMapTile == m_iThisMapTile) return;

            //
            // 最大输入值不得超过 <Maximum Map Tiles Index>
            //
            ThisMapTileIndex_TextBox.Text = (m_iThisMapTile = Math.Min(iThisMapTile, MapTilesList_DockPanel.Children.Count - 1)).ToString();

            //
            // 模拟 <UtilCtrl_MapTileList_Item> 点击
            //
            ((UtilCtrl_MapTileList_Item)MapTilesList_DockPanel.Children[m_iThisMapTile]).SimulateMouseDown();

            //
            // 更新当前 <Map Tile> 编号
            //
            MapTilesList_DockPanel_MouseDown(NULL, (MouseButtonEventArgs)NULL);
        }

        private void MapViewportScale_TextBox_TextChanged(object sender, TextChangedEventArgs e) => UTIL_MapViewportScale_TextBox_TextChanged(sender, ref m_iMapViewportScale, MapViewport_Canvas, m_stMapViewport_ScaleTransform);

        private void MapViewportScale_TextBox_LostFocus(object sender, RoutedEventArgs e) => UTIL_MapViewportScale_TextBox_LostFocus(sender, m_iMapViewportScale, MapViewport_Canvas, m_stMapViewport_ScaleTransform);
    }
}
