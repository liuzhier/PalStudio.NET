using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Xml.Linq;

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
using PAL_Rect  = System.Windows.Int32Rect;
using PAL_POS   = System.UInt32;

using PalGlobal;
using PalCfg;
using PalVideo;
using PalMap;
using PalResources;

using static PalGlobal.Pal_Global;
using static PalGlobal.Pal_File;
using static PalCommon.Pal_Common;
using static PalUtil.Pal_Util;
using static PalConfig.Pal_Config;
using static PalCfg.Pal_Cfg;
using static PalVideo.Pal_Video;
using static PalMap.Pal_Map;
using static PalResources.Pal_Resources;

namespace PalStudio.NET
{
    /// <summary>
    /// Win_SelectScene.xaml 的交互逻辑
    /// </summary>
    public partial class Win_SelectScene : Window
    {
        private enum EnterSceneStatus
        {
            Init,
            Enter,
            Close,
        }

        private static INT              m_nScene, m_iThisScene = -1, m_iMapViewportScale = 100;
        private static WORD             m_wActivePosX = 0, m_wActivePosY = 0, m_wSelectPosX = 0, m_wSelectPosY = 0;
        private static double           m_douViewportWidth = 0, m_douViewportHeight = 0;
        private static BOOL             m_fCanClose = FALSE, fIsEventHighlighting = FALSE;
        private static EnterSceneStatus me_essEnterSceneStatus;
        private static ScaleTransform   m_stMapViewport_ScaleTransform = new ScaleTransform();


        public Win_SelectScene()
        {
            InitializeComponent();
        }

        public void
        SetCanClose(
            BOOL    fCanClose
        )
        {
            //
            // 设置是否允许关闭窗口
            // 设为 <TRUE> 表示可以关闭
            // 设为 <FALSE> 表示不能关闭
            //
            m_fCanClose = fCanClose;
        }

        private bool
        GetCanClose()
        {
            //
            // 判断是否允许关闭窗口
            // 返回 <TRUE> 表示可以关闭
            // 返回 <FALSE> 表示不能关闭
            //
            return m_fCanClose;
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);

            //
            // 开启窗口时初始化 <Scene> 的选定状态
            //
            me_essEnterSceneStatus = EnterSceneStatus.Init;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (!GetCanClose())
            {
                //
                // 这里隐藏窗口而不是真正的关闭
                //
                e.Cancel = TRUE;

                this.Hide();
            }
            else
            {
                //
                // 如果可以关闭，则继续关闭过程
                //
                base.OnClosing(e);
            }

            //
            // 关闭窗口即代表用户取消了选择 <Scene> 
            //
            if (me_essEnterSceneStatus != EnterSceneStatus.Enter) me_essEnterSceneStatus = EnterSceneStatus.Close;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            INT                     i;
            LPSTR                   lpszMapName;
            Pal_File                pfFile_Map;
            UtilCtrl_ItemList_Item  uciliMapNameItem;

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
            // 获取地图文件节点
            //
            pfFile_Map = Pal_File_GetFile(lpszGameMap);

            //
            // 获取场景的区块索引
            //
            i = Pal_Cfg_GetCfgNodeItemIndex(lpszMainData, lpszScene);

            //
            // 获取全部场景数据
            //
            m_AllSceneData = Pal_Global.poMainData[i].Data;

            //
            // 获取场景数量
            //
            m_nScene = m_AllSceneData.GetLength(0);

            //
            // 将所有场景名称添加至场景列表
            //
            for (i = Pal_Map.mc_wMinSceneIndex; i < m_nScene; i++)
            {
                uciliMapNameItem = new UtilCtrl_ItemList_Item();
                lpszMapName = Pal_Cfg_GetCfgNodeItem(lpszSceneDesc, $"0x{i:X4}").lpszTitle;

                uciliMapNameItem.SetText($"[0x{i:X4}] {i:D5}: {lpszMapName}");
                uciliMapNameItem.SetParent(SceneNameList_DockPanel);
                uciliMapNameItem.Tag = i;

                SceneNameList_DockPanel.Children.Add(uciliMapNameItem);
                DockPanel.SetDock(uciliMapNameItem, Dock.Top);
            }

            //
            // 允许用户通过输入框选择 <Scene>
            //
            ThisSceneIndex_TextBox.IsEnabled = TRUE;
        }

        private void Win_ToolsButton_Loaded(object sender, RoutedEventArgs e)
        {
            //
            // 为右上角自定义工具按钮绑定需要操作的窗口
            //
            Win_ToolsButton.thisWindow = this;

            //
            // 隐藏图标，更改标题
            //
            Win_ToolsButton.SetIconVisibility(FALSE);
            Win_ToolsButton.SetTitleText("请选择欲编辑的场景");
        }

        private void
        ReplaceThisSceneName(
            INT         iItemNum
        )
        {
            ThisSceneName_TextBlock.Text    = Pal_Cfg_GetCfgNodeItem(lpszSceneDesc, $"0x{iItemNum:X4}").lpszTitle;
            ThisSceneNum_Label.Content      = $"[0x{iItemNum:X4}]";
            ThisSceneIndex_TextBox.Text     = iItemNum.ToString();
        }

        private void SceneNameList_DockPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            INT             iThisScene;
            LPSTR           lpszMapName;

            //
            // 获取当前选中场景的编号
            //
            iThisScene = (INT)SceneNameList_DockPanel.Tag - 1;

            if (m_iThisScene == -1 || m_iThisScene != iThisScene)
            {
                //
                // 当前选中场景的编号发生变化
                // 设置当前选中场景的编号
                //
                m_iThisScene        = iThisScene;

                //
                // 初始化当前选中的 <Scene> 对应的 <Map> 数据
                //
                Pal_Map.Init(m_iThisScene);

                //
                // 更新当前选择的场景名称
                //
                ReplaceThisSceneName(iThisScene + 1);

                //
                // 通过 <图层> 对资源列表进行排序
                //
                Pal_Global.m_prResources = Pal_Resources.ResourcesOrderByPosY(Pal_Global.m_prResources);

                //
                // 绘制资源列表中所有的 <Sprite> 元素
                //
                Pal_Map.DrawMapTileAndSprite(Pal_Global.m_prResources,
                    Pal_Map.m_MapViewport_Low_Surface.CleanSpirit(0xFF), Pal_Map.m_MapViewport_High_Surface.CleanSpirit(0xFF),
                    Pal_Map.m_MapViewport_EventSpiritAndMaskTile_Surface.CleanSpirit(0xFF),
                    MapDrawingStep.LowTile | MapDrawingStep.HighTile | MapDrawingStep.EventSpirit | MapDrawingStep.MaskTile);

                //
                // 开始将 <Surface> 绘制到 <Image>
                //
                VIDEO_DrawSurfaceToImage(Pal_Map.m_MapViewport_Low_Surface,                     MapViewport_Low_Image,                      Pal_Map.m_MapRect);
                VIDEO_DrawSurfaceToImage(Pal_Map.m_MapViewport_High_Surface,                    MapViewport_High_Image,                     Pal_Map.m_MapRect);
                VIDEO_DrawSurfaceToImage(Pal_Map.m_MapViewport_EventSpiritAndMaskTile_Surface,  MapViewport_EventSpiritAndMaskTile_Image,   Pal_Map.m_MapRect);
                VIDEO_DrawSurfaceToImage(Pal_Map.m_MapViewport_Obstacle_Surface,                MapViewport_Obstacle_Image,                 Pal_Map.m_MapRect);
                VIDEO_DrawSurfaceToImage(Pal_Map.m_MapViewport_Event_Surface,                   MapViewport_Event_Image,                    Pal_Map.m_MapRect);

                //
                // 绘制 <障碍块>
                //
                Pal_Map.DrawObstacleBlock(MapViewport_Obstacle_Image);

                //
                // 绘制 <事件块>
                //
                Pal_Map.DrawEventBlock(MapViewport_Event_Image);

                //
                // 将窗口标题设置为 <场景名称>
                //
                lpszMapName = Pal_Cfg_GetCfgNodeItem(lpszSceneDesc, $"0x{iThisScene + 1:X4}").lpszTitle;
                Win_ToolsButton.SetTitleText(lpszMapName);

                //
                // 将 <场景名称> 显示到左下角状态栏并添加 <ToolTip> 鼠标滑上提示
                //
                SceneName_TextBlock.Text    = $"{Pal_Map.m_iMapNum}({Pal_Map.m_iMapNum:X}) ＝＞ {iThisScene + 1:D}({iThisScene + 1:X})：{lpszMapName}";
                SceneName_TextBlock.ToolTip = SceneName_TextBlock.Text;

                //
                // 删除粉背景提示控件
                // 允许用户点击 <确认> 按钮
                // 允许调整 <Map Viewport> 缩放
                // 显示光标 <Select> 和 <Active>
                //
                if (MapViewport_Border.Visibility != Visibility.Visible)
                {
                    MapViewportBox_DockPanel.Children.Remove(Tip_NotSelected_DockPanel);
                    MapViewport_Border.Visibility           = Visibility.Visible;
                    EnterScene_Button.IsEnabled             = TRUE;
                    MapViewportScale_TextBox.IsEnabled      = TRUE;
                    MapViewport_Selected_Image.IsEnabled    = TRUE;
                    MapViewport_Active_Image.IsEnabled      = TRUE;

                    VIDEO_DrawSurfaceToImage(Pal_Map.m_MapViewport_Active_Surface,      MapViewport_Active_Image,   Pal_Map.m_MapTileRect);
                    VIDEO_DrawSurfaceToImage(Pal_Map.m_MapViewport_Selected_Surface,    MapViewport_Selected_Image, Pal_Map.m_MapTileRect);
                    Pal_Map.DrawMapTileCursor(MapTileCursorColorType.Active,    MapViewport_Active_Image,   PAL_XY(0, 0));
                    Pal_Map.DrawMapTileCursor(MapTileCursorColorType.Selected,  MapViewport_Selected_Image, PAL_XY(0, 0));
                }
            }
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e) => UTIL_TextBox_Num_PreviewTextInput(e);

        private void ThisSceneIndex_TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            INT iSceneNum;

            //
            // 判断用户输入的数值是否合法
            //
            if ((iSceneNum = UTIL_TextBoxTextIsMatch(ThisSceneIndex_TextBox, Pal_Map.m_iSceneNum)) == 0x7FFFFFFF) return;

            //
            // 数值未变动，退出函数
            //
            if (iSceneNum == Pal_Map.m_iSceneNum) return;

            //
            // 最大输入值不得超过 <Maximum scene index>
            //
            iSceneNum                   = Math.Max(iSceneNum, Pal_Map.mc_wMinSceneIndex);
            ThisSceneIndex_TextBox.Text = (Pal_Map.m_iSceneNum = Math.Min(iSceneNum, m_nScene - 1)).ToString();

            //
            // 模拟 <UtilCtrl_MapTileList_Item> 点击
            //
            ((UtilCtrl_ItemList_Item)SceneNameList_DockPanel.Children[Pal_Map.m_iSceneNum - Pal_Map.mc_wMinSceneIndex]).SimulateMouseDown();

            //
            // 更新当前 <Map Tile> 编号
            //
            SceneNameList_DockPanel_MouseDown(NULL, (MouseButtonEventArgs)NULL);
        }

        private void ThisSceneIndex_TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            INT         iThisScene;
            TextBox     textBox = sender as TextBox;

            if (textBox != NULL)
            {
                //
                // 判断用户输入的数值是否合法
                //
                if ((iThisScene = UTIL_TextBoxTextIsMatch(textBox, m_iThisScene)) == 0x7FFFFFFF)
                {
                    //
                    // 用户输入了错误的百分值
                    //
                    goto tagEnd;
                }

                if (iThisScene >= 0)
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
                textBox.Text = (m_iThisScene + Pal_Map.mc_wMinSceneIndex).ToString();
            }
        }

        private void MapViewportScale_TextBox_TextChanged(object sender, TextChangedEventArgs e) => UTIL_MapViewportScale_TextBox_TextChanged(sender, ref m_iMapViewportScale, MapViewport_Canvas, m_stMapViewport_ScaleTransform);

        private void MapViewportScale_TextBox_LostFocus(object sender, RoutedEventArgs e) => UTIL_MapViewportScale_TextBox_LostFocus(sender, m_iMapViewportScale, MapViewport_Canvas, m_stMapViewport_ScaleTransform);

        private void EnterScene_Button_Click(object sender, RoutedEventArgs e)
        {
            //
            // 点击 <确认> 按钮后，设置当前场景
            //
            Pal_Map.m_iSceneNum = m_iThisScene;

            //
            // 设置 <Scene> 的选定状态
            //
            me_essEnterSceneStatus = EnterSceneStatus.Enter;

            //
            // “关闭”（隐藏）当前窗口
            //
            this.Close();
        }

        public BOOL GetIsEnterScene
        {
            get
            {
                return me_essEnterSceneStatus == EnterSceneStatus.Enter;
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (e.Key == Key.D1 || e.Key == Key.NumPad1)
                {
                    //
                    // <数字键 1> （主键盘上或小键盘）
                    // 隐藏或显示 <Event Cursor>
                    //
                    MapViewport_Event_Image.Visibility = (MapViewport_Event_Image.Visibility == Visibility.Visible) ? Visibility.Collapsed : Visibility.Visible;
                }
                else if (e.Key == Key.D2 || e.Key == Key.NumPad2)
                {
                    //
                    // <数字键 2>
                    // 隐藏或显示 <Obstacle Cursor>
                    //
                    MapViewport_Obstacle_Image.Visibility = (MapViewport_Obstacle_Image.Visibility == Visibility.Visible) ? Visibility.Collapsed : Visibility.Visible;
                }
                else if (e.Key == Key.D3 || e.Key == Key.NumPad3)
                {
                    //
                    // <数字键 3> 
                    // 取消绘制 <Mask Tile> / 将事件突出显示
                    //
                    if (MapViewport_EventSpiritAndMaskTile_Image.Visibility == Visibility.Visible)
                    {
                        //
                        // 绘制资源列表中所有的 <Sprite> 元素
                        //
                        if (fIsEventHighlighting = !fIsEventHighlighting)
                        {
                            //
                            // 仅绘制 <Event Spirit> 到 <Surface>
                            //
                            Pal_Map.DrawMapTileAndSprite(Pal_Global.m_prResources, (Surface)NULL, (Surface)NULL,
                                Pal_Map.m_MapViewport_EventSpiritAndMaskTile_Surface.CleanSpirit(0xFF), MapDrawingStep.EventSpirit);
                        }
                        else
                        {
                            //
                            // 仅绘制 <Event Spirit> 和 <> 到 <Surface>
                            //
                            Pal_Map.DrawMapTileAndSprite(Pal_Global.m_prResources, (Surface)NULL, (Surface)NULL,
                                Pal_Map.m_MapViewport_EventSpiritAndMaskTile_Surface.CleanSpirit(0xFF), MapDrawingStep.EventSpirit | MapDrawingStep.MaskTile);
                        }

                        //
                        // 开始将 <Surface> 绘制到 <Image>
                        //
                        VIDEO_DrawSurfaceToImage(Pal_Map.m_MapViewport_EventSpiritAndMaskTile_Surface, MapViewport_EventSpiritAndMaskTile_Image, Pal_Map.m_MapRect);
                    }
                }
                else if (e.Key == Key.D4 || e.Key == Key.NumPad4)
                {
                    //
                    // <数字键 4>
                    // 隐藏或显示 <Mask Tile> 和 <Event Spirit>
                    //
                    MapViewport_EventSpiritAndMaskTile_Image.Visibility = (MapViewport_EventSpiritAndMaskTile_Image.Visibility == Visibility.Visible) ? Visibility.Collapsed : Visibility.Visible;
                }
                else if (e.Key == Key.D5 || e.Key == Key.NumPad5)
                {
                    //
                    // <数字键 5>
                    // 隐藏或显示 <High Tile>
                    //
                    MapViewport_High_Image.Visibility = (MapViewport_High_Image.Visibility == Visibility.Visible) ? Visibility.Collapsed : Visibility.Visible;
                }
                else if (e.Key == Key.D6 || e.Key == Key.NumPad6)
                {
                    //
                    // <数字键 6>
                    // 隐藏或显示 <Low Tile>
                    //
                    MapViewport_Low_Image.Visibility = (MapViewport_Low_Image.Visibility == Visibility.Visible) ? Visibility.Collapsed : Visibility.Visible;
                }
            }
        }

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
            m_wSelectPosX = m_wActivePosX;
            m_wSelectPosY = m_wActivePosY;

            //
            // 更新 <Active Cursor> 的位置
            //
            MapViewport_Selected_Image.Margin = new Thickness(m_wSelectPosX * (m_iMapViewportScale / 100.00), m_wSelectPosY * (m_iMapViewportScale / 100.00), 0, 0);
        }
    }
}
