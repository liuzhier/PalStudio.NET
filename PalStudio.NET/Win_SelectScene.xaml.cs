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
        private static  INT         m_nScene, m_iThisScene = -1, m_iLastScene = -1;
        private static  BOOL        m_fCanClose = FALSE, m_fIsLoadingCompleted = FALSE;
        private static  Surface     m_Surface;

        public  static  dynamic[,]  m_AllSceneData;

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
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            INT                     i;
            LPSTR                   lpszMapName;
            Pal_File                pfFile_Map;
            UtilCtrl_ItemList_Item  uciliMapNameItem;

            //
            // 初始化 <Surface>
            //
            m_Surface = new Surface(2064, 2055);

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
            for (i = 1; i < m_nScene; i++)
            {
                uciliMapNameItem = new UtilCtrl_ItemList_Item();
                lpszMapName = Pal_Cfg_GetCfgNodeItem(lpszSceneDesc, $"0x{i:X4}").lpszTitle;

                uciliMapNameItem.SetText($"[0x{i:X4}] {i:D5}: {lpszMapName}");
                uciliMapNameItem.SetParent(SceneNameList_DockPanel);
                uciliMapNameItem.Tag = i;

                SceneNameList_DockPanel.Children.Add(uciliMapNameItem);
                DockPanel.SetDock(uciliMapNameItem, Dock.Top);
            }
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
            ThisSceneNum_Label.Content      = $"[0x{iItemNum:X4}] {iItemNum:D5}";
        }

        private void SceneNameList_DockPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DockPanel       dockPanel = sender as DockPanel;
            INT             x, y, h, iPosX = 0, iPosY = 0, iThisScene, iNodeIndex, iOffset = 0;
            WORD            wSpriteNum, wDirection, wDirectionFrames, wCurrentFrameNum;
            SHORT           sSpriteLayer, sSpriteActualLayer;
            BYTE[]          Map_Tmp = null, Data_Buf = null, tmp_bySprite;
            Pal_Map_Tile    pmtThisTile;
            Pal_Object      poEvent;
            Pal_Resources   tmpResources;

            if (dockPanel       != NULL &&
                dockPanel.Tag   != NULL)
            {
                //
                // 获取当前选中场景的编号
                //
                iThisScene = (INT)dockPanel.Tag - 1;

                if (m_iThisScene == -1 || m_iThisScene != iThisScene)
                {
                    //
                    // 当前选中场景的编号发生变化
                    // 设置当前选中场景的编号
                    //
                    m_iThisScene        = iThisScene;

                    //
                    // 更新当前选择的场景名称
                    //
                    ReplaceThisSceneName(iThisScene + 1);
                    
                    //
                    // 获取 <事件对象起始编号> 节点的索引
                    //
                    iNodeIndex = Pal_Cfg_GetCfgNodeItemIndex(lpszScene, lpszEventObjectIndex);

                    //
                    // 获取 <地图编号>，当前选定场景的事件数
                    //
                    Pal_Map.m_iMapNum       = m_AllSceneData[m_iThisScene,      Pal_Cfg_GetCfgNodeItemIndex(lpszScene, lpszMapID)];
                    Pal_Map.m_iStartEvent   = m_AllSceneData[m_iThisScene,      iNodeIndex];
                    Pal_Map.m_iEndEvent     = m_AllSceneData[m_iThisScene + 1,  iNodeIndex];

                    //
                    // 初始化 <Map> 数据
                    //
                    {
                        Pal_Map_Tile   pmtTitle;

                        INT iTileThird      = Pal_Map.Tiles.GetLength(0);
                        INT iTileSsecond    = Pal_Map.Tiles.GetLength(1);
                        INT iTileFirst      = Pal_Map.Tiles.GetLength(2);

                        //
                        // 获取 <Map List> 文件
                        //
                        Map_Tmp = Pal_File_GetFile(lpszGameMap).bufFile;

                        //
                        // 获取当前场景的 <Map> 数据
                        //
                        PAL_MKFDecompressChunk(ref Data_Buf, Pal_Map.m_iMapNum, Map_Tmp);

                        //
                        // 获取每一块 <Map Tile> 的属性
                        //
                        for (y = 0; y < iTileThird; y++)
                        {
                            for (x = 0; x < iTileSsecond; x++)
                            {
                                for (h = 0; h < iTileFirst; h++)
                                {
                                    pmtTitle                = new Pal_Map_Tile();

                                    pmtTitle.fIsNoPassBlock = (Data_Buf[iOffset + 1] & 0x20) != 0;
                                    pmtTitle.LowTile_Num    = (WORD)(Data_Buf[iOffset++] | (((Data_Buf[iOffset] & 0x10) >> 4) << 8));
                                    pmtTitle.LowTile_Layer  = (BYTE)(Data_Buf[iOffset++] & 0xF);
                                    pmtTitle.HighTile_Num   = (WORD)((Data_Buf[iOffset++] | (((Data_Buf[iOffset] & 0x10) >> 4) << 8)) - 1);
                                    pmtTitle.HighTile_Layer = (BYTE)(Data_Buf[iOffset++] & 0xF);

                                    Pal_Map.Tiles[y, x, h] = pmtTitle;
                                }
                            }
                        }

                        Data_Buf = null;
                    }

                    //
                    // 初始化 <Map Tiles>
                    //
                    {
                        //
                        // 获取 <Map Tiles List> 文件
                        //
                        Map_Tmp = Pal_File_GetFile(lpszGameMapTile).bufFile;

                        //
                        // 获取当前场景的 <Map Titles> 数据
                        //
                        PAL_MKFReadChunk(ref Pal_Map.TileSprite, Pal_Map.m_iMapNum, Map_Tmp);
                    }

                    //
                    // 绘制 <Map Low Tile>
                    //
                    {
                        for (y = 0; y < Pal_Map.Tiles.GetLength(0); y++)
                        {
                            for (x = 0; x < Pal_Map.Tiles.GetLength(1); x++)
                            {
                                for (h = 0; h < Pal_Map.Tiles.GetLength(2); h++)
                                {
                                    //
                                    // 计算当前块的 <PosX>
                                    //
                                    iPosX = x * Pal_Map.m_MapTileWidth;
                                    iPosY = y * Pal_Map.m_MapTileHeight;

                                    if (((h + 1) % 2) == 0)
                                    {
                                        //
                                        // <Half> 半块
                                        //
                                        iPosX += Pal_Map.wOffsetX_H;
                                        iPosY += Pal_Map.wOffsetY_H;
                                    }

                                    pmtThisTile = Pal_Map.Tiles[y, x, h];
                                    PAL_RLEBlitToSurface(PAL_SpriteGetFrame(Pal_Map.TileSprite, pmtThisTile.LowTile_Num), m_Surface, PAL_XY(iPosX, iPosY));
                                }
                            }
                        }
                    }

                    //
                    // 清空资源列表
                    //
                    Pal_Global.m_prResources.Clear();

                    //
                    // 将所有 <Map Tile> 块放入资源列表
                    //
                    for (y = 0; y < Pal_Map.Tiles.GetLength(0); y++)
                    {
                        for (x = 0; x < Pal_Map.Tiles.GetLength(1); x++)
                        {
                            for (h = 0; h < Pal_Map.Tiles.GetLength(2); h++)
                            {
                                //
                                // 计算当前块的 <PosX>
                                //
                                iPosX = x * Pal_Map.m_MapTileWidth;
                                iPosY = y * Pal_Map.m_MapTileHeight;

                                if (((h + 1) % 2) == 0)
                                {
                                    //
                                    // <Half> 半块
                                    //
                                    iPosX += Pal_Map.wOffsetX_H;
                                    iPosY += Pal_Map.wOffsetY_H;
                                }

                                pmtThisTile = Pal_Map.Tiles[y, x, h];

                                //
                                // 将高层 <Map Tile> 放入资源列表
                                //
                                sSpriteActualLayer  = (SHORT)(iPosY + pmtThisTile.HighTile_Layer * 8 + 1);
                                wSpriteNum          = pmtThisTile.HighTile_Num;
                                tmpResources        = new Pal_Resources(PAL_SpriteGetFrame(Pal_Map.TileSprite, wSpriteNum), PAL_XY(iPosX, iPosY), sSpriteActualLayer);
                                Pal_Global.m_prResources.Add(tmpResources);
                            }
                        }
                    }

                    //
                    // 将所有 <Sprite> 块放入资源列表
                    //
                    {
                        //
                        // 获取全部 <Event> 数据
                        //
                        poEvent = Pal_Global.poMainData.Where(MainItem => MainItem.TableName.Equals(lpszEvent)).First();

                        for (y = Pal_Map.m_iStartEvent; y < Pal_Map.m_iEndEvent; y++)
                        {
                            iPosX               = poEvent.GetItem(y, lpszX);
                            iPosY               = poEvent.GetItem(y, lpszY);
                            sSpriteLayer        = poEvent.GetItem(y, lpszLayer);
                            wSpriteNum          = poEvent.GetItem(y, lpszSpriteNum);
                            wDirection          = poEvent.GetItem(y, lpszDirection);
                            wDirectionFrames    = poEvent.GetItem(y, lpszDirectionFrames);
                            wCurrentFrameNum    = poEvent.GetItem(y, lpszCurrentFrameNum);
                            tmp_bySprite        = PAL_GetEventObjectSprite(wSpriteNum);

                            if (tmp_bySprite == NULL) continue;

                            //
                            // <Pos Y> 绘制起点应该与 <Map Tile> 贴齐
                            //
                            tmp_bySprite        = PAL_SpriteGetFrame(tmp_bySprite, wDirection * wDirectionFrames + wCurrentFrameNum);
                            sSpriteActualLayer  = (SHORT)(iPosY + sSpriteLayer * 8 + 2);
                            iPosX              -= PAL_RLEGetWidth(tmp_bySprite) / 2;
                            iPosY              -= PAL_RLEGetHeight(tmp_bySprite) + sSpriteLayer - 9;

                            //
                            // 因为这里地图是完全显示的，没有截掉边缘处的多余三角块
                            // 所以要额外加上这些三角块的尺寸（其实就是偏移）
                            //
                            iPosX += 16;
                            iPosY += 8;

                            tmpResources    = new Pal_Resources(tmp_bySprite, PAL_XY(iPosX, iPosY), sSpriteActualLayer);
                            Pal_Global.m_prResources.Add(tmpResources);
                        }
                    }

                    //
                    // 通过 <图层> 对资源列表进行排序
                    //
                    Pal_Global.m_prResources = Pal_Global.m_prResources.OrderBy(res => res.m_sLayer).ToList();

                    //
                    // 绘制资源列表中所有的 <Sprite> 元素
                    //
                    foreach (Pal_Resources tmpRes in Pal_Global.m_prResources)
                    {
                        PAL_RLEBlitToSurface(tmpRes.m_bySpirit, m_Surface, tmpRes.m_pos);
                    }

                    //
                    // 开始将 <Surface> 转换为 <Image>
                    //
                    VIDEO_DrawSurfaceToImage(m_Surface, MapViewport_Image, Pal_Map.m_MapRect);

                    //
                    // 绘制 <障碍块>
                    //
                    if (FALSE)
                    {
                        for (y = 0; y < Pal_Map.Tiles.GetLength(0); y++)
                        {
                            for (x = 0; x < Pal_Map.Tiles.GetLength(1); x++)
                            {
                                for (h = 0; h < Pal_Map.Tiles.GetLength(2); h++)
                                {
                                    if (Pal_Map.Tiles[y, x, h].fIsNoPassBlock)
                                    {
                                        //
                                        // 计算当前块的 <PosX>
                                        //
                                        iPosX = x * Pal_Map.m_MapTileWidth;
                                        iPosY = y * Pal_Map.m_MapTileHeight;

                                        if (((h + 1) % 2) == 0)
                                        {
                                            //
                                            // <Half> 半块
                                            //
                                            iPosX += Pal_Map.wOffsetX_H;
                                            iPosY += Pal_Map.wOffsetY_H;
                                        }

                                        DrawMapTileCursor(MapTileCursorColorType.Obstacle, MapViewport_Image, PAL_XY(iPosX, iPosY));
                                    }
                                }
                            }
                        }
                    }

                    //
                    // 删除粉背景提示控件，允许用户点击 <确认> 按钮
                    //
                    if (MapViewport_Border.Visibility != Visibility.Visible)
                    {
                        MapViewportBox_DockPanel.Children.Remove(Tip_NotSelected_DockPanel);
                        MapViewport_Border.Visibility = Visibility.Visible;
                        EnterScene_Button.IsEnabled = TRUE;
                    }
                }
            }
        }

        private void EnterScene_Button_Click(object sender, RoutedEventArgs e)
        {
            //
            // 点击 <确认> 按钮后，设置当前场景
            //
            Pal_Map.m_iSceneNum = m_iThisScene;

            //
            // “关闭”（隐藏）当前窗口
            //
            this.Close();
        }

        public BOOL fIsEnterScene
        {
            get
            {
                BOOL fIsEnter   = m_iLastScene != Pal_Map.m_iSceneNum;

                if (Pal_Map.m_iSceneNum == m_iThisScene) m_iLastScene = m_iThisScene;

                return fIsEnter;
            }
        }
    }
}
