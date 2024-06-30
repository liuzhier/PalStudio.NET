using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Collections;
using System.Security.Cryptography;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Controls;
using System.IO;

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

using PAL_POS   = System.UInt64;
using PAL_Rect  = System.Windows.Int32Rect;

using PalGlobal;

using static PalGlobal.Pal_Global;
using static PalGlobal.Pal_File;
using static PalConfig.Pal_Config;
using static PalCommon.Pal_Common;
using static PalUtil.Pal_Util;
using PalMap;

namespace PalVideo
{
    public struct RGB
    {
        public BYTE     red;
        public BYTE     gree;
        public BYTE     blue;
    }

    public class Surface
    {
        public INT              w, h;
        public BitmapPalette    palette;
        public BYTE[]           pixels  = null;

        private Surface() { }

        public
        Surface(
            INT             iWidth,
            INT             iHeight,
            INT             iPaletteNum = 0,
            BOOL            fNight      = FALSE
        )
        {
            ResetSurface(iWidth, iHeight, iPaletteNum, fNight);
        }

        public void
        ResetSurface(
            INT             iWidth,
            INT             iHeight,
            INT             iPaletteNum = 0,
            BOOL            fNight      = FALSE
        )
        {
            INT         i, j;
            BYTE[]      binPat;
            List<Color> colors;

            w       = iWidth;
            h       = iHeight;
            pixels  = new BYTE[w * h];

            //
            // Get palette list data
            //
            binPat = Pal_File_GetFile(lpszPalette).bufFile;

            //
            // Get specified color palette data
            //
            PAL_MKFReadChunk(ref binPat, iPaletteNum, binPat);

            //
            // Create a list of 256 colors
            //
            colors = new List<Color>();

            //
            // Initialize color palette
            //
            for (i = 0; i < 256; i++)
            {
                //
                //If it is currently night, perform palette offset
                //
                j = (fNight ? 256 * 3 : 0) + i * 3;

                //
                // Set all transparency to 100%
                //
                colors.Add(Color.FromArgb(0xFF, (BYTE)(binPat[j] << 2), (BYTE)(binPat[j + 1] << 2), (BYTE)(binPat[j + 2] << 2)));
            }

            //
            // Replace the last color with a transparent one
            //
            colors.Remove(colors.Last());
            colors.Add(Color.FromArgb(0x00, 0x00, 0x00, 0x00));

            palette = new BitmapPalette(colors);

            //
            // Initialize background color to transparent color
            //
            this.CleanSpirit(0xFF);
        }

        public INT
        GetStride()
        {
            //
            // 返回每行像素的字节长度
            //
            return (this.w + 3) / 4 * 4;
        }

        public Surface
        CleanSpirit(
            BYTE            iColorIndex = 0
        )
        {
            if (iColorIndex == 0)
            {
                Array.Clear(pixels, 0, pixels.Length);
            }
            else
            {
                for (INT i = 0; i < pixels.Length; i++)
                {
                    pixels[i] = iColorIndex;
                }
            }

            return this;
        }
    }

    public unsafe class
    Pal_Video
    {
        /*++
        public Surface m_Surface = null;

        public
        Pal_Video(
            INT             iWidth,
            INT             iHeight
        )
        {
            m_Surface = new Surface(iWidth, iHeight);
        }
        --*/

        public static void
        VIDEO_DrawSurfaceToImage(
            Surface         surface,
            Image           dest,
            PAL_Rect        destRect,
            BOOL            fIsSameSrcRect = FALSE
        )
        {
            INT                 x, y, iPixelIndex;
            WriteableBitmap     wbRenderer;

            if (dest.Source == NULL)
            {
                //
                // writeableBitmap 为 <NULL> 时将完全覆盖
                //
                dest.Source = new WriteableBitmap(surface.w, surface.h, 0, 0, PixelFormats.Indexed8, surface.palette);
            }

            //
            // 获取位图渲染器
            //
            wbRenderer = (WriteableBitmap)dest.Source;

            //
            // 填充像素颜色
            //
            if (fIsSameSrcRect)
            {
                //
                // 锁定 WriteableBitmap 的 BackBuffer
                //
                wbRenderer.Lock();

                for (y = 0; y < destRect.Height; y++)
                {
                    for (x = 0; x < destRect.Width; x++)
                    {
                        iPixelIndex = (destRect.Y + y) * Pal_Map.mc_wMapWidth + x + destRect.X;

                        if (Pal_Map.SegmentTable[y, x] == 0) ((BYTE*)wbRenderer.BackBuffer)[iPixelIndex] = surface.pixels[iPixelIndex];
                   }
                }

                //
                // 标记被修改的区域
                //
                wbRenderer.AddDirtyRect(destRect);

                //
                // 解锁 WriteableBitmap 的 BackBuffer
                //
                wbRenderer.Unlock();

                //
                // 更新Image控件的Source属性
                // 由于 Update 方法可能不是在UI线程上调用的
                // 所以需要使用 Dispatcher 来确保UI线程更新
                //
                Application.Current.Dispatcher.Invoke(() =>
                {
                    dest.Source = wbRenderer;
                });
            }
            else
            {
                wbRenderer.WritePixels(new PAL_Rect(destRect.X, destRect.Y, destRect.Width, destRect.Height), surface.pixels, surface.GetStride(), 0);
            }

            //
            // 刷新 Image 控件以显示更新后的像素（废弃）
            //
            dest.InvalidateVisual();
        }

        public static void
        Video_ApplyWave(
           Surface                  surface
        )
        /*++
           Purpose:

             Apply screen waving effect when needed.

           Parameters:

             [OUT] lpSurface - the surface to be proceed.

           Return value:

             None.

        --*/
        {
            INT[]           wave        = new INT[32];
            INT             i, a, b, p;
            BYTE[]          tmp;
            INT             index = 0;

            Pal_Global.wScreenWave = (WORD)(Pal_Global.wScreenWave + Pal_Global.sWaveProgression);

            if (Pal_Global.wScreenWave == 0 || Pal_Global.wScreenWave >= 256)
            {
                //
                // No need to wave the screen
                //
                Pal_Global.wScreenWave      = 0;
                Pal_Global.sWaveProgression = 0;
                return;
            }

            //
            // Calculate the waving offsets.
            //
            a = 0;
            b = 60 + 8;

            for (i = 0; i < 16; i++)
            {
                b -= 8;
                a += b;

                wave[i]         = a * Pal_Global.wScreenWave / 256;
                wave[i + 16]    = surface.w - wave[i];

                if (wave[i + 16] > surface.w / 2) wave[i + 16] = surface.w - wave[i + 16];
            }

            //
            // Apply the effect.
            //
            a = index;
            p = 0;

            //
            // Loop through all lines in the screen buffer.
            //
            for (i = 0; i < surface.h; i++)
            {
                b = wave[a];

                if (b > 0)
                {
                    //
                    // Do a shift on the current line with the calculated offset.
                    //
                    tmp = UTIL_SubBytes(surface.pixels, p + b, surface.w - b);

                    for (b = p; b < (p + surface.w); b++) surface.pixels[b] = 0;

                    Array.Copy(tmp, 0, surface.pixels, (b < surface.w) ? b : p, tmp.Length);
                    //memcpy(buf, p, b);
                    //memmove(p, &p[b], 320 - b);
                    //memcpy(&p[320 - b], buf, b);
                }

                a = (a + 1) % 32;
                p += surface.w;
            }

            index = (index + 1) % 32;
        }
    }
}
