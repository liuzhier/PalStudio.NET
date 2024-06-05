using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Net;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Ink;
using System.Runtime.InteropServices;

using PalVideo;

using BOOL      = System.Boolean;
using CHAR      = System.Char;
using BYTE      = System.Byte;
using SHORT     = System.Int16;
using WORD      = System.UInt16;
using INT       = System.Int32;
using UINT      = System.UInt32;
using SDWORD    = System.Int32;
using DWORD     = System.UInt32;
using SQWORD    = System.Int64;
using QWORD     = System.UInt64;

using PAL_POS   = System.UInt32;

using LPSTR     = System.String;
using FILE      = System.IO.File;
using PAL_Rect  = System.Windows.Int32Rect;

using PalGlobal;
using PalResources;

using static PalGlobal.Pal_Global;
using static PalGlobal.Pal_File;
using static PalCommon.Pal_Common;
using static PalCfg.Pal_Cfg;
using static PalConfig.Pal_Config;
using static PalResources.Pal_Resources;
using System.Windows.Input;
using System.Reflection;

namespace PalMap
{
    public class Pal_Map
    {
        public class Pal_Map_Tile
        {
            public BOOL fIsNoPassBlock  = FALSE;
            public WORD LowTile_Num     = 0;
            public BYTE LowTile_Layer   = 0;
            public WORD HighTile_Num    = 0;
            public BYTE HighTile_Layer  = 0;
        }

        public enum MapDrawingStep
        {
            LowTile     = 1 << 0,
            HighTile    = 1 << 1,
            EventSpirit = 1 << 2,
            MaskTile    = 1 << 3,
        }

        public enum MapTileCursorColorType
        {
            Active,
            Selected,
            Obstacle,
            Event,
        }

        public static readonly BYTE[,] mc_byMapTileCursorColor =
        {
            // A     R     G     B
            { 0xFF, 0x00, 0xFF, 0x00 }, // 活动
            { 0xFF, 0x00, 0x00, 0xFF }, // 选中
            { 0xFF, 0xFF, 0x00, 0x00 }, // 障碍
            { 0xFF, 0x00, 0xFF, 0xFF }, // 事件
        };

        public static readonly DWORD[,] mc_dwMapTileCursor = {
            // #00FF00     #0000FF     #FF0000     #00FFFF
            // 列1：活动   列2：选中   列3：障碍   列4：事件
            { 0x0003C000, 0x0003C000, 0x0003C000, 0x0003C000 },
            { 0x000C3000, 0x000DB000, 0x000C3000, 0x000C3000 },
            { 0x00300C00, 0x00318C00, 0x00300C00, 0x00300C00 },
            { 0x00C00300, 0x00C18300, 0x00C00300, 0x00C00300 },
            { 0x030000C0, 0x030180C0, 0x030000C0, 0x030000C0 },
            { 0x0C000030, 0x0C000030, 0x0C000030, 0x0C018030 },
            { 0x3000000C, 0x3000000C, 0x3000000C, 0x3001800C },
            { 0xC0000003, 0xC0000003, 0xC30000C3, 0xC003C003 },
            { 0x3000000C, 0x3000000C, 0x3000000C, 0x3001800C },
            { 0x0C000030, 0x0C000030, 0x0C000030, 0x0C018030 },
            { 0x030000C0, 0x030180C0, 0x030000C0, 0x030000C0 },
            { 0x00C00300, 0x00C18300, 0x00C00300, 0x00C00300 },
            { 0x00300C00, 0x00318C00, 0x00300C00, 0x00300C00 },
            { 0x000C3000, 0x000DB000, 0x000C3000, 0x000C3000 },
            { 0x0003C000, 0x0003C000, 0x0003C000, 0x0003C000 },
        };

        public const WORD   mc_wMapWidth = 2064, mc_wMapHeight = 2055, mc_wMapTileWidth = 32, mc_wMapTileHeight = 15, mc_wOffsetX_H = 16, mc_wOffsetY_H = 8, mc_wMinSceneIndex = 1;
        public const INT    mc_iMapViewportScaleMin = 100, mc_iMapViewportScaleMax = 999;


        public static readonly PAL_Rect m_MapRect       = new PAL_Rect(0, 0, mc_wMapWidth,      mc_wMapHeight);
        public static readonly PAL_Rect m_MapTileRect   = new PAL_Rect(0, 0, mc_wMapTileWidth,  mc_wMapTileHeight);
        public static Surface           m_MapViewport_Low_Surface, m_MapViewport_High_Surface, m_MapViewport_EventSpiritAndMaskTile_Surface, m_MapViewport_Obstacle_Surface, m_MapViewport_Event_Surface;

        public static List<Surface>     mc_sfMapTileCursor = new List<Surface>();

        public static INT               m_iMapNum   = -1, m_iSceneNum = -1, m_iStartEvent = -1, m_iEndEvent = -1;
        public static Pal_Map_Tile[,,]  Tiles       = new Pal_Map_Tile[128, 64, 2];
        public static BYTE[]            TileSprite  = (BYTE[])NULL;

        public static dynamic[,]        m_AllSceneData;

        private static void
        InitMapTileCursor()
        {
            INT             iCursorType, x, y, pixelOffset;
            Surface         surface;
            BYTE[,]         byColor = mc_byMapTileCursorColor;

            //
            // 初始化所有的光标 <Surface>
            //
            for (iCursorType = 0; iCursorType < mc_byMapTileCursorColor.GetLength(1); iCursorType++)
            {
                surface = new Surface(Pal_Map.mc_wMapTileWidth, Pal_Map.mc_wMapTileHeight);

                List<Color> colors = surface.palette.Colors.ToList();

                colors.RemoveAt(0);
                colors.Insert(0, Color.FromArgb(byColor[iCursorType, 0], byColor[iCursorType, 1], byColor[iCursorType, 2], byColor[iCursorType, 3]));

                surface.palette = new BitmapPalette(colors);

                for (y = 0; y < Pal_Map.mc_wMapTileHeight; y++)
                {
                    for (x = 0; x < Pal_Map.mc_wMapTileWidth; x++)
                    {
                        //
                        // 计算当前像素的内存地址
                        //
                        pixelOffset = y * Pal_Map.mc_wMapTileWidth + x;

                        //
                        // 获取当前像素黑白值（位值）
                        // 若为 0 则为透明色，直接跳过
                        //
                        if ((mc_dwMapTileCursor[y, iCursorType] & (1 << x)) == 0) continue;

                        //
                        // 设置颜色为对应色号
                        //
                        surface.pixels[pixelOffset] = 0;
                    }
                }

                mc_sfMapTileCursor.Add(surface);
            }
        }

        public static void
        InitMapViewportSurface()
        {
            //
            // 初始化 <Map Viewport Surface>
            //
            m_MapViewport_Low_Surface                       = new Surface(Pal_Map.mc_wMapWidth, Pal_Map.mc_wMapHeight);
            m_MapViewport_High_Surface                      = new Surface(Pal_Map.mc_wMapWidth, Pal_Map.mc_wMapHeight);
            m_MapViewport_EventSpiritAndMaskTile_Surface    = new Surface(Pal_Map.mc_wMapWidth, Pal_Map.mc_wMapHeight);
            m_MapViewport_Obstacle_Surface                  = new Surface(Pal_Map.mc_wMapWidth, Pal_Map.mc_wMapHeight);
            m_MapViewport_Event_Surface                     = new Surface(Pal_Map.mc_wMapWidth, Pal_Map.mc_wMapHeight);

            //
            // 初始化 <Map Tile Cursor> （主要是为了初始化它们的调色板）
            //
            InitMapTileCursor();

            //
            // 重设 <额外信息视图> 调色板
            //
            //m_MapViewport_Active_Surface.palette    = mc_sfMapTileCursor[0].palette;
            //m_MapViewport_Selected_Surface.palette  = mc_sfMapTileCursor[1].palette;
            m_MapViewport_Obstacle_Surface.palette  = mc_sfMapTileCursor[2].palette;
            m_MapViewport_Event_Surface.palette     = mc_sfMapTileCursor[3].palette;
        }

        public static void
        InitMap(
            INT         iThisScene
        )
        {
            INT             x, y, h, iNodeIndex, iOffset = 0;
            BYTE[]          Map_Tmp, Data_Buf = null;

            //
            // 获取 <事件对象起始编号> 节点的索引
            //
            iNodeIndex = Pal_Cfg_GetCfgNodeItemIndex(lpszScene, lpszEventObjectIndex);

            //
            // 获取 <地图编号>，当前选定场景的事件数
            //
            Pal_Map.m_iMapNum       = m_AllSceneData[iThisScene,        Pal_Cfg_GetCfgNodeItemIndex(lpszScene, lpszMapID)];
            Pal_Map.m_iStartEvent   = m_AllSceneData[iThisScene,        iNodeIndex];
            Pal_Map.m_iEndEvent     = m_AllSceneData[iThisScene + 1,    iNodeIndex];

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
        }

        public static BYTE[]
        PAL_MapGetTileBitmap(
            BYTE            x,
            BYTE            y,
            BYTE            h,
            BYTE            ucLayer
        )
        /*++
          Purpose:

            Get the tile bitmap on the specified layer at the location (x, y, h).

          Parameters:

            [IN]  x - Column number of the tile.

            [IN]  y - Line number in the map.

            [IN]  h - Each line in the map has two lines of tiles, 0 and 1.
                      (See map.h for details.)

            [IN]  ucLayer - The layer. 0 for bottom, 1 for top.

            [IN]  lpMap - Pointer to the loaded map.

          Return value:

            Pointer to the bitmap. NULL if failed.

        --*/
        {
            Pal_Map_Tile    Tile;

            //
            // Check for invalid parameters.
            //
            if (x >= 64 || y >= 128 || h > 1) return null;

            //
            // Get the tile data of the specified location.
            //
            Tile = Tiles[y, x, h];

            if (ucLayer == 0)
            {
                //
                // Bottom layer
                //
                return PAL_SpriteGetFrame(TileSprite, Tile.LowTile_Num);
            }
            else
            {
                //
                // Top layer
                //
                return PAL_SpriteGetFrame(TileSprite, Tile.HighTile_Num);
            }
        }

        public static BYTE
        PAL_MapGetTileHeight(
            BYTE       x,
            BYTE       y,
            BYTE       h,
            BYTE       ucLayer
        )
        /*++
          Purpose:

            Get the logical height value of the specified tile. This value is used
            to judge whether the tile bitmap should cover the sprites or not.

          Parameters:

            [IN]  x - Column number of the tile.

            [IN]  y - Line number in the map.

            [IN]  h - Each line in the map has two lines of tiles, 0 and 1.
                      (See map.h for details.)

            [IN]  ucLayer - The layer. 0 for bottom, 1 for top.

            [IN]  lpMap - Pointer to the loaded map.

          Return value:

            The logical height value of the specified tile.

        --*/
        {
            Pal_Map_Tile    Tile;

            //
            // Check for invalid parameters.
            //
            if (y >= 128 || x >= 64 || h > 1) return 0;

            Tile = Tiles[y, x, h];

            if (ucLayer == 0)
            {
                //
                // Bottom layer
                //
                return Tile.LowTile_Layer;
            }
            else
            {
                //
                // Top layer
                //
                return Tile.HighTile_Layer;
            }
        }

        public static void
        PAL_CalcCoverTiles(
           Pal_Resources        SpriteToDraw
        )
        /*++
           Purpose:

             Calculate all the tiles which may cover the specified sprite. Add the tiles
             into our list as well.

           Parameters:

             [IN]  SpriteToDraw - Pal_Resources struct.

           Return value:

             None.

        --*/
        {
            INT             x, y, i, l, dx = 0, dy = 0, dh = 0, iPosX, iPosY, iTileHeight;
            SHORT           sSpriteActualLayer;
            BYTE[]          byTile;
            Pal_Resources   tmpResources;

            INT sx      = PAL_X(SpriteToDraw.m_pos) - SpriteToDraw.m_sLayer / 2;
            INT sy      = PAL_Y(SpriteToDraw.m_pos) - SpriteToDraw.m_sLayer;
            INT sh      = (sx % 32 != 0) ? 1 : 0;

            INT width   = PAL_RLEGetWidth(SpriteToDraw.m_bySpirit);
            INT height  = PAL_RLEGetHeight(SpriteToDraw.m_bySpirit);

            //
            // Loop through all the tiles in the area of the sprite.
            //
            for (y = (sy - height - 15) / 16; y <= sy / 16; y++)
            {
                for (x = (sx - width / 2) / 32; x <= (sx + width / 2) / 32; x++)
                {
                    for (i = ((x == (sx - width / 2) / 32) ? 0 : 3); i < 5; i++)
                    {
                        //
                        // Scan tiles in the following form (* = to scan):
                        //
                        // . . . * * * . . .
                        //  . . . * * . . . .
                        //
                        switch (i)
                        {
                            case 0:
                                dx = x;
                                dy = y;
                                dh = sh;
                                break;

                            case 1:
                                dx = x - 1;
                                break;

                            case 2:
                                dx = sh != 0 ? x : (x - 1);
                                dy = sh != 0 ? (y + 1) : y;
                                dh = 1 - sh;
                                break;

                            case 3:
                                dx = x + 1;
                                dy = y;
                                dh = sh;
                                break;

                            case 4:
                                dx = sh != 0 ? (x + 1) : x;
                                dy = sh != 0 ? (y + 1) : y;
                                dh = 1 - sh;
                                break;
                        }

                        for (l = 0; l < 2; l++)
                        {
                            byTile      = PAL_MapGetTileBitmap((BYTE)dx, (BYTE)dy, (BYTE)dh, (BYTE)l);
                            iTileHeight = (CHAR)PAL_MapGetTileHeight((BYTE)dx, (BYTE)dy, (BYTE)dh, (BYTE)l);

                            //
                            // Check if this tile may cover the sprites
                            //
                            if (byTile != NULL && iTileHeight > 0 && (dy + iTileHeight) * 16 + dh * 8 >= sy)
                            {
                                //
                                // This tile may cover the sprite
                                //
                                iPosX               = dx * 32 + dh * 16 - 16;
                                iPosY               = dy * 16 + dh * 8 + 7 + l + iTileHeight * 8;
                                sSpriteActualLayer  = (SHORT)(iTileHeight * 8 + l);

                                tmpResources        = new Pal_Resources(byTile, PAL_XY(iPosX, iPosY), sSpriteActualLayer, 1);
                                Pal_Global.m_prResources.Add(tmpResources);
                            }
                        }
                    }
                }
            }
        }

        public static void
        InitMapResources()
        {
            INT             i, iPosX = 0, iPosY = 0;
            WORD            wSpriteNum, wDirection, wDirectionFrames, wCurrentFrameNum;
            SHORT           sSpriteLayer;
            BYTE[]          tmp_bySprite;
            Pal_Object      poEvent;
            Pal_Resources   tmpResources;

            //
            // 获取全部 <Event> 数据
            //
            poEvent = Pal_Global.poMainData.Where(MainItem => MainItem.TableName.Equals(lpszEvent)).First();

            //
            // 清空资源列表
            //
            Pal_Global.m_prResources.Clear();

            //
            // 将所有 <Sprite> 块放入资源列表
            //
            for (i = Pal_Map.m_iStartEvent; i < Pal_Map.m_iEndEvent; i++)
            {
                iPosX               = poEvent.GetItem(i, lpszX);
                iPosY               = poEvent.GetItem(i, lpszY);
                sSpriteLayer        = poEvent.GetItem(i, lpszLayer);
                wSpriteNum          = poEvent.GetItem(i, lpszSpriteNum);
                wDirection          = poEvent.GetItem(i, lpszDirection);
                wDirectionFrames    = poEvent.GetItem(i, lpszDirectionFrames);
                wCurrentFrameNum    = poEvent.GetItem(i, lpszCurrentFrameNum);
                tmp_bySprite        = PAL_GetEventObjectSprite(wSpriteNum);

                if (tmp_bySprite == NULL) continue;

                //
                // 计算实际 <Position> 和 <Layer>
                //
                tmp_bySprite    = PAL_SpriteGetFrame(tmp_bySprite, wDirection * wDirectionFrames + wCurrentFrameNum);
                sSpriteLayer    = (SHORT)(sSpriteLayer * 8 + 2);
                iPosX          -= PAL_RLEGetWidth(tmp_bySprite) / 2;
                iPosY          += sSpriteLayer + 9;

                tmpResources    = new Pal_Resources(tmp_bySprite, PAL_XY(iPosX, iPosY), sSpriteLayer, 0);
                Pal_Global.m_prResources.Add(tmpResources);

                //
                // 将所有可能盖住当前 <Event> 的 <Map Tile> 放入资源列表
                //
                PAL_CalcCoverTiles(Pal_Global.m_prResources.Last());
            }
        }

        public static void
        Init(
            INT         iThisScene
        )
        {
            //
            // 初始化 <Scene> 对应的 <Map>
            //
            InitMap(iThisScene);

            //
            // 初始化 <Scene> 对应的 <Map> 资源
            //
            InitMapResources();
        }

        public static void
        DrawMapTileAndSprite(
            List<Pal_Resources>     list_prResources,
            Surface                 Low_Surface,
            Surface                 High_Surface,
            Surface                 MapViewport_EventSpiritAndMaskTile_Surface,
            MapDrawingStep          mdsMapDrawingStep
        )
        {
            INT             x, y, h, iPosX, iPosY;
            Pal_Map_Tile    pmtThisTile;

            //
            // 先绘制一遍完整的地图
            //
            if ((mdsMapDrawingStep & MapDrawingStep.LowTile)  != 0 ||
                (mdsMapDrawingStep & MapDrawingStep.HighTile) != 0)
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
                            iPosX = x * Pal_Map.mc_wMapTileWidth;
                            iPosY = y * (Pal_Map.mc_wMapTileHeight + 1);

                            if (((h + 1) % 2) == 0)
                            {
                                //
                                // <Half> 半块
                                //
                                iPosX += Pal_Map.mc_wOffsetX_H;
                                iPosY += Pal_Map.mc_wOffsetY_H;
                            }

                            pmtThisTile = Pal_Map.Tiles[y, x, h];

                            //
                            // 绘制低层 <Map Tile>
                            //
                            if ((mdsMapDrawingStep & MapDrawingStep.LowTile) != 0)
                            {
                                PAL_RLEBlitToSurface(PAL_SpriteGetFrame(Pal_Map.TileSprite, pmtThisTile.LowTile_Num), Low_Surface, PAL_XY(iPosX, iPosY));
                            }

                            //
                            // 绘制高层 <Map Tile> （跳过空图像）
                            //
                            if ((mdsMapDrawingStep & MapDrawingStep.HighTile) != 0)
                            {
                                if ((SHORT)pmtThisTile.HighTile_Num == -1) continue;

                                PAL_RLEBlitToSurface(PAL_SpriteGetFrame(Pal_Map.TileSprite, pmtThisTile.HighTile_Num), High_Surface, PAL_XY(iPosX, iPosY));
                            }
                        }
                    }
                }
            }

            //
            // 绘制资源列表中所有的 <Spirit> 元素
            //
            if ((mdsMapDrawingStep & MapDrawingStep.EventSpirit) != 0 ||
                (mdsMapDrawingStep & MapDrawingStep.MaskTile)    != 0)
            {
                foreach (Pal_Resources tmpRes in list_prResources)
                {
                    x = PAL_X(tmpRes.m_pos);
                    y = PAL_Y(tmpRes.m_pos) - PAL_RLEGetHeight(tmpRes.m_bySpirit) - tmpRes.m_sLayer;

                    //
                    // 因为这里地图是完全显示的，没有截掉边缘处的多余三角块
                    // 所以要额外加上这些三角块的尺寸（其实就是偏移）
                    //
                    x += 16;
                    y += 8;

                    if (tmpRes.m_byTag == 0)
                    {
                        //
                        // 绘制 <Event Spirit>
                        //
                        if ((mdsMapDrawingStep & MapDrawingStep.EventSpirit) != 0)
                        {
                            PAL_RLEBlitToSurface(tmpRes.m_bySpirit, MapViewport_EventSpiritAndMaskTile_Surface, PAL_XY(x, y));
                        }
                    }
                    else
                    {
                        //
                        // 绘制 <Make Tile> （遮挡块）
                        //
                        if ((mdsMapDrawingStep & MapDrawingStep.MaskTile) != 0)
                        {
                            PAL_RLEBlitToSurface(tmpRes.m_bySpirit, MapViewport_EventSpiritAndMaskTile_Surface, PAL_XY(x, y));
                        }
                    }
                }
            }
        }

        public static void
        DrawMapTileCursor(
            MapTileCursorColorType  mtccCursorType,
            Image                   dest,
            PAL_POS                 pos
        )
        {
            INT             iCursorType, x, y, pixelOffset;
            WriteableBitmap wbRenderer;
            PAL_Rect        rect;

            //
            // 检查需要绘制什么类型的光标
            //
            switch (mtccCursorType)
            {
                case MapTileCursorColorType.Event:
                    iCursorType = 3;
                    break;

                case MapTileCursorColorType.Obstacle:
                    iCursorType = 2;
                    break;

                case MapTileCursorColorType.Selected:
                    iCursorType = 1;
                    break;

                case MapTileCursorColorType.Active:
                default:
                    iCursorType = 0;
                    break;
            }

            rect = new PAL_Rect(PAL_X(pos), PAL_Y(pos), mc_sfMapTileCursor[iCursorType].w, mc_sfMapTileCursor[iCursorType].h);

            if (dest.Source == NULL)
            {
                //
                // writeableBitmap 为 <NULL> 时将完全覆盖
                //
                dest.Source = new WriteableBitmap(rect.Width, rect.Height, 0, 0, PixelFormats.Indexed8, mc_sfMapTileCursor[iCursorType].palette);
            }

            //
            // 获取位图渲染器
            //
            wbRenderer = (WriteableBitmap)dest.Source;

            //
            // 开始绘制指定的光标 <Surface> 到指定的 <Image>
            //
            for (y = 0; y < Pal_Map.mc_wMapTileHeight; y++)
            {
                for (x = 0; x < Pal_Map.mc_wMapTileWidth; x++)
                {
                    //
                    // 计算当前像素的内存地址
                    //
                    pixelOffset = (rect.Y * Pal_Map.mc_wMapWidth) + rect.X +  y * Pal_Map.mc_wMapWidth + x;

                    //
                    // 获取当前像素黑白值（位值）
                    // 若为 0 则为透明色，直接跳过
                    //
                    if ((mc_dwMapTileCursor[y, iCursorType] & (1 << x)) == 0) continue;

                    //
                    // 设置颜色为对应色号
                    //
                    unsafe
                    {
                        ((BYTE*)wbRenderer.BackBuffer)[pixelOffset] = 0;
                    }
                }
            }
        }

        public static void
        DrawObstacleBlock(
            Image       dest_Image
        )
        {
            INT         x, y, h, iPosX, iPosY;

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
                            iPosX = x * Pal_Map.mc_wMapTileWidth;
                            iPosY = y * (Pal_Map.mc_wMapTileHeight + 1);

                            if (((h + 1) % 2) == 0)
                            {
                                //
                                // <Half> 半块
                                //
                                iPosX += Pal_Map.mc_wOffsetX_H;
                                iPosY += Pal_Map.mc_wOffsetY_H;
                            }

                            Pal_Map.DrawMapTileCursor(MapTileCursorColorType.Obstacle, dest_Image, PAL_XY(iPosX, iPosY));
                        }
                    }
                }
            }
        }

        public static void
        DrawEventBlock(
            Image       dest_Image
        )
        {
            INT             i, iPosX, iPosY;
            Pal_Object      poEvent;

            //
            // 获取全部 <Event> 数据
            //
            poEvent = Pal_Global.poMainData.Where(MainItem => MainItem.TableName.Equals(lpszEvent)).First();

            //
            // 绘制所有 <Sprite> 的映射光标块
            //
            for (i = Pal_Map.m_iStartEvent; i < Pal_Map.m_iEndEvent; i++)
            {
                iPosX   = poEvent.GetItem(i, lpszX);
                iPosY   = poEvent.GetItem(i, lpszY);

                Pal_Map.DrawMapTileCursor(MapTileCursorColorType.Event, dest_Image, PAL_XY(iPosX, iPosY));
            }
        }
    }
}
