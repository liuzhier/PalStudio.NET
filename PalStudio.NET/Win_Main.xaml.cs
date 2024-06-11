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
using System.Windows.Interop;
using System.Reflection;

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
using PAL_POS   = System.UInt32;

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
using static PalCfg.Pal_Cfg;
using static PalConfig.Pal_Config;

namespace PalStudio.NET
{
    /// <summary>
    /// Window1.xaml 的交互逻辑
    /// </summary>
    public partial class Win_Main : Window
    {
        private enum MapEditMode
        {
            Default = Select,
            Min     = Select,
            Max     = Delete,

            Select  = 0,
            Edit    = 1,
            Delete  = 2,
        }

        private enum MapLayerMode
        {
            Default = LowTile,
            Min     = LowTile,
            Max     = Event,

            LowTile     = 0,
            HighTile    = 1,
            NoPass      = 2,
            Event       = 3,
        }

        private static BOOL             m_fCanClose = TRUE; 
        private static INT              m_iThisMapTile = -1, m_iMapViewportScale = 200;
        private static WORD             m_wActivePosX = 0, m_wActivePosY = 0, m_wSelectPosX = 0, m_wSelectPosY = 0;
        private static double           m_douViewportWidth = 0, m_douViewportHeight = 0;
        private static ScaleTransform   m_stMapViewport_ScaleTransform = new ScaleTransform();
        private static MapDrawingStep   m_EventTileBlockAndMaskTileBlockDisplayStatus = MapDrawingStep.EventSpirit | MapDrawingStep.MaskTile;
        private static MapEditMode      me_MapEditMode  = MapEditMode.Default;
        private static MapLayerMode     me_MapLayerMode = MapLayerMode.Default;

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

            //
            // 初始化 <Map Viewport Surface>
            //
            Pal_Map.InitMapViewportSurface();

            //
            // 备份 <Viewport Size>
            //
            m_douViewportWidth  = MapViewport_ScrollViewer.ViewportWidth;
            m_douViewportHeight = MapViewport_ScrollViewer.ViewportHeight;

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
            // 开始将 <Surface> 绘制到 <Image>
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

        private void OpenScene_Button_Click(object sender, MouseButtonEventArgs e)
        {
            INT                         i, nMapTiles;
            LPSTR                       lpszMapName;
            UtilCtrl_MapTileList_Item   utilCtrl_MapTileList_Item;

            if (e.ChangedButton != MouseButton.Left)
            {
                //
                // 如果不是左键则 <Pass>
                //
                return;
            }

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
                // 开始将 <Surface> 绘制到 <Image>
                //
                VIDEO_DrawSurfaceToImage(Pal_Map.m_MapViewport_Low_Surface,                         MapViewport_Low_Image,                          Pal_Map.m_MapRect);
                VIDEO_DrawSurfaceToImage(Pal_Map.m_MapViewport_High_Surface,                        MapViewport_High_Image,                         Pal_Map.m_MapRect);
                VIDEO_DrawSurfaceToImage(Pal_Map.m_MapViewport_EventTileSpiritAndMaskTile_Surface,  MapViewport_EventTileSpiritAndMaskTile_Image,   Pal_Map.m_MapRect);
                VIDEO_DrawSurfaceToImage(Pal_Map.m_MapViewport_Obstacle_Surface,                    MapViewport_Obstacle_Image,                     Pal_Map.m_MapRect);
                VIDEO_DrawSurfaceToImage(Pal_Map.m_MapViewport_Event_Surface,                       MapViewport_Event_Image,                        Pal_Map.m_MapRect);

                //
                // 绘制 <障碍块>
                //
                Pal_Map.DrawObstacleBlock(MapViewport_Obstacle_Image);

                //
                // 绘制 <事件块>
                //
                Pal_Map.DrawEventBlock(MapViewport_Event_Image);

                //
                // 将 <场景名称> 显示到左下角状态栏并添加 <ToolTip> 鼠标滑上提示
                //
                lpszMapName                 = Pal_Cfg_GetCfgNodeItem(lpszSceneDesc, $"0x{Pal_Map.m_iSceneNum + 1:X4}").lpszTitle;
                SceneName_TextBlock.Text    = $"{Pal_Map.m_iMapNum}({Pal_Map.m_iMapNum:X}) ＝＞ {Pal_Map.m_iSceneNum + 1:D}({Pal_Map.m_iSceneNum + 1:X})：{lpszMapName}";
                SceneName_TextBlock.ToolTip = SceneName_TextBlock.Text;

                if (MapViewport_Border.Visibility != Visibility.Visible)
                {
                    //
                    // 初始化所有的控件
                    //
                    WordMapBox_DockPanel.IsEnabled                  = TRUE;
                    WordMapBox_DockPanel.Opacity                    = 1;
                    ScenecCtrl_Save_Button.IsEnabled                = TRUE;
                    ScenecCtrl_Save_Button.Opacity                  = 1;
                    MapEditMode_ToolsButtonList.IsEnabled           = TRUE;
                    MapEditMode_ToolsButtonList.Opacity             = 1;
                    MapBlockDisplayMode_ToolsButtonList.IsEnabled   = TRUE;
                    MapBlockDisplayMode_ToolsButtonList.Opacity     = 1;
                    MapLayerMode_ToolsButtonList.IsEnabled          = TRUE;
                    MapLayerMode_ToolsButtonList.Opacity            = 1;
                    MapViewportScale_TextBox.IsEnabled              = TRUE;
                    MapViewport_Selected_Image.IsEnabled            = TRUE;
                    MapViewport_Active_Image.IsEnabled              = TRUE;

                    //
                    // 初始化 <视图> 与 <编辑模式>
                    //
                    MapEditMode_Select_Button.RaiseEvent(Pal_Global.SimulateMouseDownEvent);
                    MapBlockDisplayMode_EventBlock_Button.RaiseEvent(Pal_Global.SimulateMouseDownEvent);
                    MapBlockDisplayMode_NoPassBlock_Button.RaiseEvent(Pal_Global.SimulateMouseDownEvent);
                    MapBlockDisplayMode_EventTileBlockAndMaskTileBlock_Button.RaiseEvent(Pal_Global.SimulateMouseDownEvent);
                    MapBlockDisplayMode_HighTileBlock_Button.RaiseEvent(Pal_Global.SimulateMouseDownEvent);
                    MapBlockDisplayMode_LowTileBlock_Button.RaiseEvent(Pal_Global.SimulateMouseDownEvent);
                    MapLayerMode_LowTile_Button.RaiseEvent(Pal_Global.SimulateMouseDownEvent);

                    //
                    // 删除粉背景提示控件
                    //
                    MapViewportBox_DockPanel.Children.Remove(Tip_NotSelected_DockPanel);
                    MapViewport_Border.Visibility = Visibility.Visible;

                    VIDEO_DrawSurfaceToImage(Pal_Map.m_MapViewport_Active_Surface, MapViewport_Active_Image, Pal_Map.m_MapTileRect);
                    VIDEO_DrawSurfaceToImage(Pal_Map.m_MapViewport_Selected_Surface, MapViewport_Selected_Image, Pal_Map.m_MapTileRect);
                    Pal_Map.DrawMapTileCursor(MapTileCursorColorType.Active, MapViewport_Active_Image, PAL_XY(0, 0));
                    Pal_Map.DrawMapTileCursor(MapTileCursorColorType.Selected, MapViewport_Selected_Image, PAL_XY(0, 0));
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
            if ((iThisMapTile = UTIL_TextBoxTextIsMatch(ThisMapTileIndex_TextBox, m_iThisMapTile)) == PALSN_ERROR) return;

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

        private void ThisMapTileIndex_TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            INT         iThisMapTile;
            TextBox     textBox = sender as TextBox;

            if (textBox != NULL)
            {
                //
                // 判断用户输入的数值是否合法
                //
                if ((iThisMapTile = UTIL_TextBoxTextIsMatch(textBox, m_iThisMapTile)) == PALSN_ERROR)
                {
                    //
                    // 用户输入了错误的百分值
                    //
                    goto tagEnd;
                }

                if (iThisMapTile >= 0)
                {
                    //
                    // 用户输入百分值值正确
                    // 直接退出函数
                    //
                    return;
                }

tagEnd:
                //
                // 撤回到上次输入的数值
                //
                textBox.Text = m_iThisMapTile.ToString();
            }
        }

        private void MapViewportScale_TextBox_TextChanged(object sender, TextChangedEventArgs e) => UTIL_MapViewportScale_TextBox_TextChanged(sender, ref m_iMapViewportScale, MapViewport_Canvas, m_stMapViewport_ScaleTransform);

        private void MapViewportScale_TextBox_LostFocus(object sender, RoutedEventArgs e) => UTIL_MapViewportScale_TextBox_LostFocus(sender, m_iMapViewportScale, MapViewport_Canvas, m_stMapViewport_ScaleTransform);

        private void MapViewport_Moving_Image_MouseMove(object sender, MouseEventArgs e)
        {
            Point           point, currentPoint;
            Thickness       margin;
            double          left, top, X, Y;
            WORD            x, y, h;
            PAL_POS         posActiveCursor;

            //
            // 获取元素的最终布局位置
            //
            point = MapViewport_ScrollViewer.TransformToAncestor(this).Transform(new Point(0, 0));

            //
            // 获取元素的margin
            //
            margin = MapViewport_ScrollViewer.Margin;

            //
            // 计算相对于窗口的位置
            //
            left = point.X + margin.Left;
            top  = point.Y + margin.Top;

            //
            // 获取当前鼠标位置
            //
            currentPoint    = e.GetPosition(null);

            //
            // 获取 <Scene> 缩放前的坐标
            //
            m_wActivePosX = (WORD)((MapViewport_ScrollViewer.HorizontalOffset + currentPoint.X - left) / (m_iMapViewportScale / 100.00));
            m_wActivePosY = (WORD)((MapViewport_ScrollViewer.VerticalOffset   + currentPoint.Y - top)  / (m_iMapViewportScale / 100.00));

            //
            // <点坐标> 转 <块坐标>
            //
            Pal_Map.PAL_POS_TO_XYH(PAL_XY(m_wActivePosX, m_wActivePosY), out x, out y, out h);
            posActiveCursor = Pal_Map.PAL_XYH_TO_POS(x, y, h);

            m_wActivePosX = PAL_X(posActiveCursor);
            m_wActivePosY = PAL_Y(posActiveCursor);

            //
            // 将 <块坐标> 显示在状态栏
            //
            ActiveCursorPosXB_TextBlock.Text    = $"{x:D}({x:X})";
            ActiveCursorPosYB_TextBlock.Text    = $"{y:D}({y:X})";
            ActiveCursorPosHB_TextBlock.Text    = h.ToString();
            ActiveCursorPosX_TextBlock.Text     = $"{m_wActivePosX:D}({m_wActivePosX:X})";
            ActiveCursorPosY_TextBlock.Text     = $"{m_wActivePosY:D}({m_wActivePosY:X})";

            //
            // 更新 <Active Cursor> 的位置
            //
            MapViewport_Active_Image.Margin = new Thickness(m_wActivePosX * (m_iMapViewportScale / 100.00), m_wActivePosY * (m_iMapViewportScale / 100.00), 0, 0);
        }

        private void MapViewport_Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //
            // 备份 <Select Cursor> 坐标
            //
            m_wSelectPosX = m_wActivePosX;
            m_wSelectPosY = m_wActivePosY;

            //
            // 更新 <Active Cursor> 的位置
            //
            MapViewport_Selected_Image.Margin = new Thickness(m_wSelectPosX * (m_iMapViewportScale / 100.00), m_wSelectPosY * (m_iMapViewportScale / 100.00), 0, 0);
        }

        private void MapEditMode_Select_Button_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //
            // 将模式设置为 <Select Element Mode>
            //
            me_MapEditMode = MapEditMode.Select;
        }

        private void MapEditMode_Edit_Button_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //
            // 将模式设置为 <Edit Element Mode>
            //
            me_MapEditMode = MapEditMode.Edit;
        }

        private void MapEditMode_Delete_Button_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //
            // 将模式设置为 <Delete Element Mode>
            //
            me_MapEditMode = MapEditMode.Delete;
        }

        private void MapBlockDisplayMode_EventBlock_Button_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //
            // 隐藏或显示 <Event Cursor>
            //
            MapViewport_Event_Image.Visibility = (MapViewport_Event_Image.Visibility == Visibility.Visible) ? Visibility.Collapsed : Visibility.Visible;
        }

        private void MapBlockDisplayMode_NoPassBlock_Button_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //
            // 隐藏或显示 <Obstacle Cursor>
            //
            MapViewport_Obstacle_Image.Visibility = (MapViewport_Obstacle_Image.Visibility == Visibility.Visible) ? Visibility.Collapsed : Visibility.Visible;
        }

        private void MapBlockDisplayMode_EventTileBlockAndMaskTileBlock_Button_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //
            // 将 <Event Spirit> 突出显示 / 隐藏或显示 <Mask Tile> 和 <Event Spirit>
            //
            if (((m_EventTileBlockAndMaskTileBlockDisplayStatus & MapDrawingStep.EventSpirit) != 0) &&
                ((m_EventTileBlockAndMaskTileBlockDisplayStatus & MapDrawingStep.MaskTile)    != 0) &&
                MapViewport_EventTileSpiritAndMaskTile_Image.Visibility == Visibility.Visible)
            {
                //
                // 仅绘制 <Event Spirit> 到 <Surface>
                //
                m_EventTileBlockAndMaskTileBlockDisplayStatus ^= MapDrawingStep.MaskTile;
            }
            else if (((m_EventTileBlockAndMaskTileBlockDisplayStatus & MapDrawingStep.EventSpirit) != 0) &&
                ((m_EventTileBlockAndMaskTileBlockDisplayStatus & MapDrawingStep.MaskTile) == 0) &&
                MapViewport_EventTileSpiritAndMaskTile_Image.Visibility == Visibility.Visible)
            {
                //
                // 隐藏 <Event Spirit> 和 <MaskTile Spirit>
                //
                MapViewport_EventTileSpiritAndMaskTile_Image.Visibility = Visibility.Collapsed;

                //
                // 后续执行无意义，直接跳出
                //
                return;
            }
            else
            {
                //
                // 显示并绘制 <Event Spirit> 和 <MaskTile Spirit> 到 <Surface>
                //
                m_EventTileBlockAndMaskTileBlockDisplayStatus              |= MapDrawingStep.MaskTile;
                MapViewport_EventTileSpiritAndMaskTile_Image.Visibility     = Visibility.Visible;
            }

            //
            // 绘制 <Event Spirit> 或 <MaskTile Spirit> 到 <Surface>
            //
            Pal_Map.DrawMapTileAndSprite(Pal_Global.m_prResources, (Surface)NULL, (Surface)NULL,
                Pal_Map.m_MapViewport_EventTileSpiritAndMaskTile_Surface.CleanSpirit(0xFF), m_EventTileBlockAndMaskTileBlockDisplayStatus);

            //
            // 开始将 <Surface> 绘制到 <Image>
            //
            VIDEO_DrawSurfaceToImage(Pal_Map.m_MapViewport_EventTileSpiritAndMaskTile_Surface, MapViewport_EventTileSpiritAndMaskTile_Image, Pal_Map.m_MapRect);
        }

        private void MapBlockDisplayMode_HighTileBlock_Button_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //
            // 隐藏或显示 <High Tile>
            //
            MapViewport_High_Image.Visibility = (MapViewport_High_Image.Visibility == Visibility.Visible) ? Visibility.Collapsed : Visibility.Visible;
        }

        private void MapBlockDisplayMode_LowTileBlock_Button_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //
            // 隐藏或显示 <Low Tile>
            //
            MapViewport_Low_Image.Visibility = (MapViewport_Low_Image.Visibility == Visibility.Visible) ? Visibility.Collapsed : Visibility.Visible;
        }

        private void MapLayerMode_LowTile_Button_MouseDown(object sender, MouseButtonEventArgs e)
        {
            me_MapLayerMode = MapLayerMode.LowTile;
        }

        private void MapLayerMode_HighTile_Button_MouseDown(object sender, MouseButtonEventArgs e)
        {
            me_MapLayerMode = MapLayerMode.HighTile;
        }

        private void MapLayerMode_NoPass_Button_MouseDown(object sender, MouseButtonEventArgs e)
        {
            me_MapLayerMode = MapLayerMode.NoPass;
        }

        private void MapLayerMode_Event_Button_MouseDown(object sender, MouseButtonEventArgs e)
        {
            me_MapLayerMode = MapLayerMode.Event;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (e.Key == Key.M)
                {
                    //
                    // <字母键 T>
                    // 模式切换 <Select/Edit/Delect Element Mode>
                    //
                    if (me_MapEditMode < MapEditMode.Max)
                    {
                        me_MapEditMode++;
                    }
                    else
                    {
                        me_MapEditMode = MapEditMode.Min;
                    }

                    ((UtilCtrl_ToolsButton)((DockPanel)MapEditMode_ToolsButtonList.Child).Children[(INT)me_MapEditMode]).RaiseEvent(Pal_Global.SimulateMouseDownEvent);
                }
                else if(e.Key == Key.D1 || e.Key == Key.NumPad1)
                {
                    //
                    // <数字键 1> （主键盘上或小键盘）
                    // 隐藏或显示 <Event Cursor>
                    //
                    MapBlockDisplayMode_EventBlock_Button.RaiseEvent(Pal_Global.SimulateMouseDownEvent);
                }
                else if (e.Key == Key.D2 || e.Key == Key.NumPad2)
                {
                    //
                    // <数字键 2>
                    // 隐藏或显示 <Obstacle Cursor>
                    //
                    MapBlockDisplayMode_NoPassBlock_Button.RaiseEvent(Pal_Global.SimulateMouseDownEvent);
                }
                else if (e.Key == Key.D3 || e.Key == Key.NumPad3)
                {
                    //
                    // <数字键 3> 
                    // 将 <Event Spirit> 突出显示 / 隐藏或显示 <Mask Tile> 和 <Event Spirit>
                    //
                    MapBlockDisplayMode_EventTileBlockAndMaskTileBlock_Button.RaiseEvent(Pal_Global.SimulateMouseDownEvent);
                }
                else if (e.Key == Key.D4 || e.Key == Key.NumPad4)
                {
                    //
                    // <数字键 4>
                    // 隐藏或显示 <High Tile>
                    //
                    MapBlockDisplayMode_HighTileBlock_Button.RaiseEvent(Pal_Global.SimulateMouseDownEvent);
                }
                else if (e.Key == Key.D5 || e.Key == Key.NumPad5)
                {
                    //
                    // <数字键 5>
                    // 隐藏或显示 <Low Tile>
                    //
                    MapBlockDisplayMode_LowTileBlock_Button.RaiseEvent(Pal_Global.SimulateMouseDownEvent);
                }
                else if(e.Key == Key.Q)
                {
                    //
                    // <字母键 Q>
                    // 图层模式 <Low Tile>
                    //
                    MapLayerMode_LowTile_Button.RaiseEvent(Pal_Global.SimulateMouseDownEvent);
                }
                else if (e.Key == Key.W)
                {
                    //
                    // <字母键 W>
                    // 图层模式 <High Tile>
                    //
                    MapLayerMode_HighTile_Button.RaiseEvent(Pal_Global.SimulateMouseDownEvent);
                }
                else if (e.Key == Key.E)
                {
                    //
                    // <字母键 E>
                    // 图层模式 <Obstacle Tile>
                    //
                    MapLayerMode_NoPass_Button.RaiseEvent(Pal_Global.SimulateMouseDownEvent);
                }
                else if (e.Key == Key.R)
                {
                    //
                    // <字母键 R>
                    // 图层模式 <Event Tile>
                    //
                    MapLayerMode_Event_Button.RaiseEvent(Pal_Global.SimulateMouseDownEvent);
                }
            }
        }
    }
}
