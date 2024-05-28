using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

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

using static PalGlobal.Pal_Global;
using static PalCommon.Pal_Common;
using System.Net;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Ink;
using System.Runtime.InteropServices;

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

        public enum MapTileCursorColorType
        {
            Normal,
            Selected,
            Obstacle,
            Event,
        }

        public static readonly BYTE[,] mc_byMapTileCursorColor =
        {
            // 光标
            { 0x00, 0xFF, 0x00 },
            // 选中
            { 0x00, 0x00, 0xFF },
            // 障碍
            { 0xFF, 0x00, 0x00 },
            // 事件
            { 0x00, 0xFF, 0xFF },
        };

        public static readonly DWORD[,] mc_dwMapTileCursor = {
            // #00FF00     #0000FF     #FF0000     #00FFFF
            // 列1：光标   列2：选中   列3：障碍   列4：事件
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

        public const  WORD m_MapWidth = 2064, m_MapHeight = 2055, m_MapTileWidth = 32, m_MapTileHeight = 16, wOffsetX_H = 16, wOffsetY_H = 8;

        public static readonly PAL_Rect m_MapRect       = new PAL_Rect(0, 0, m_MapWidth, m_MapHeight);
        public static readonly PAL_Rect m_MapTileRect   = new PAL_Rect(0, 0, m_MapTileWidth, m_MapTileHeight);

        public static INT               m_iMapNum   = -1, m_iSceneNum = -1, m_iStartEvent = -1, m_iEndEvent = -1;
        public static Pal_Map_Tile[,,]  Tiles       = new Pal_Map_Tile[128, 64, 2];
        public static BYTE[]            TileSprite  = (BYTE[])NULL;

        public static void
        DrawMapTileCursor(
            MapTileCursorColorType  mtccCursorType,
            Image                   dest,
            PAL_POS                 pos
        )
        {
            INT             iCursorType, x, y, iDrawEndX, iDrawEndY, iBitPerPixel, stride, pixelOffset, iThisPixel;
            IntPtr          lpBitmapPixel;
            WriteableBitmap wbRenderer;
            PAL_Rect        rect        = new PAL_Rect(PAL_X(pos), PAL_Y(pos), 32, 15);
            BYTE[]          pixel_RGB24 = new BYTE[3];

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

                case MapTileCursorColorType.Normal:
                default:
                    iCursorType = 0;
                    break;
            }

            //
            // 获取位图渲染器
            //
            wbRenderer = (WriteableBitmap)dest.Source;

            //
            // 锁定 WriteableBitmap 的像素数据
            //
            wbRenderer.Lock();
            lpBitmapPixel = wbRenderer.BackBuffer;

            //
            // 计算每行像素的字节长度
            //
            iBitPerPixel = (PixelFormats.Rgb24.BitsPerPixel + 7) / 8;
            stride = wbRenderer.PixelWidth * iBitPerPixel;

            //
            // 获取绘制终点
            //
            iDrawEndX = PAL_X(pos) + 32;
            iDrawEndY = PAL_Y(pos) + 15;

            //
            // 开始绘制
            //
            for (y = 0; y < 15; y++)
            {
                for (x = 0; x < 32; x++)
                {
                    //
                    // 计算当前像素的内存地址
                    //
                    pixelOffset = (PAL_Y(pos) + y) * stride + (PAL_X(pos) + x) * iBitPerPixel;

                    //
                    // 获取当前像素黑白值（位值）
                    // 若为 0 则为透明色，直接跳过
                    //
                    if ((mc_dwMapTileCursor[y, iCursorType] & (1 << x)) == 0) continue;

                    //
                    // 获取当前像素颜色（RGB）
                    //
                    pixel_RGB24[0] = mc_byMapTileCursorColor[iCursorType, 0];
                    pixel_RGB24[1] = mc_byMapTileCursorColor[iCursorType, 1];
                    pixel_RGB24[2] = mc_byMapTileCursorColor[iCursorType, 2];

                    //
                    // 设置当前像素颜色
                    //
                    Marshal.Copy(pixel_RGB24, 0, lpBitmapPixel + pixelOffset, pixel_RGB24.Length);
                }
            }

            //
            // 标记更新区域
            //
            wbRenderer.AddDirtyRect(rect);

            //
            // 解锁 WriteableBitmap 的像素数据
            //
            wbRenderer.Unlock();
        }
    }
}
