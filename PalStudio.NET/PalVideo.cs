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
        public INT      w, h;
        public INT      pitch;
        public RGB[]    palette = new RGB[256];
        public BYTE[]   pixels  = null;
        public BYTE[]   colors  = null;

        private Surface() { }

        public
        Surface(
            INT     iWidth,
            INT     iHeight,
            INT     iPaletteNum = 0,
            BOOL    fNight      = FALSE
        )
        {
            ResetSurface(iWidth, iHeight, iPaletteNum, fNight);
        }

        public void
        ResetSurface(
            INT     iWidth,
            INT     iHeight,
            INT     iPaletteNum = 0,
            BOOL    fNight      = FALSE
        )
        {
            INT i, j;
            BYTE[] binPat;

            w       = iWidth;
            h       = iHeight;
            pitch   = w * 1;
            pixels  = new BYTE[pitch * h];

            for (i = 0; i < pixels.Length; i++)
            {
                pixels[i] = 0xFF;
            }

            //
            // Get palette data
            //
            binPat = Pal_File_GetFile(lpszPalette).bufFile;

            PAL_MKFReadChunk(ref binPat, iPaletteNum, binPat);

            //
            // Initialize color palette
            //
            for (i = 0; i < palette.Length; i++)
            {
                j = (fNight ? 256 * 3 : 0) + i * 3;

                palette[i].red  = (BYTE)(binPat[j] << 2);
                palette[i].gree = (BYTE)(binPat[j + 1] << 2);
                palette[i].blue = (BYTE)(binPat[j + 2] << 2);
            }
        }

        public Surface
        CleanSpirit(
            BYTE        iColorIndex = 0
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

        public BYTE[]
        GetPixelRGB()
        {
            INT     i, j;
            RGB     rgbPalette;

            colors = new BYTE[pixels.Length * 3];

            for (i = 0; i < pixels.Length; i++)
            {
                j = i * 3;

                rgbPalette = palette[pixels[i]];

                colors[j++] = rgbPalette.red;
                colors[j++] = rgbPalette.gree;
                colors[j++] = rgbPalette.blue;
            }

            return colors;
        }
    }

    public unsafe class
    Pal_Video
    {
        /*++
        public Surface m_Surface = null;

        public
        Pal_Video(
            INT     iWidth,
            INT     iHeight
        )
        {
            m_Surface = new Surface(iWidth, iHeight);
        }
        --*/

        public static void
        VIDEO_DrawSurfaceToImage(
            Surface     surface,
            Image       dest,
            PAL_Rect    rect,
            BOOL        fIsTransparent = TRUE
        )
        {
            INT             x, y, iBitPerPixel, stride, pixelOffset, iThisPixel;
            IntPtr          lpBitmapPixel;
            WriteableBitmap wbRenderer;
            RGB             palette_RGB;
            BYTE[]          pixel_RGB24 = new BYTE[3];

            if (dest.Source == NULL)
            {
                //
                // writeableBitmap 为 <NULL> 时将完全覆盖
                //
                dest.Source = new WriteableBitmap(surface.w, surface.h, 0, 0, PixelFormats.Indexed8, null);
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
            // 填充图像
            //
            for (y = rect.Y; y < rect.Height; y++)
            {
                for (x = rect.X; x < rect.Width; x++)
                {
                    //
                    // 计算当前像素的内存地址
                    //
                    pixelOffset = y * stride + x * iBitPerPixel;

                    //
                    // 获取当前像素索引
                    //
                    iThisPixel  = surface.pixels[y * surface.w + x];

                    //
                    // 跳过透明像素
                    //
                    if (iThisPixel == 0xFF && fIsTransparent) continue;

                    //
                    // 获取当前像素颜色（RGB）
                    //
                    palette_RGB     = surface.palette[iThisPixel];
                    pixel_RGB24[0]  = palette_RGB.red;
                    pixel_RGB24[1]  = palette_RGB.gree;
                    pixel_RGB24[2]  = palette_RGB.blue;

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

            //
            // 刷新Image控件以显示更新后的像素（废弃）
            //
            //dest.InvalidateVisual();
        }

        /*++
        public static void
        Video_DrawEnlargeBitmap(
            Surface     surface,
            Image       dest,
            INT         n_tupling
        )
        {
            INT     scaledWidth, scaledHeight, Width, Height, i, j, x, y, iAlpha = -1;
            Color   color;
            Bitmap  originalBitmap, scaledBitmap;
            BYTE[]  RGB_List;

            Width           = surface.w;
            Height          = surface.h;

            scaledWidth     = Width  * (n_tupling == -1 ? 1 : n_tupling);
            scaledHeight    = Height * (n_tupling == -1 ? 1 : n_tupling);

            originalBitmap  = new Bitmap(Width, Height);
            scaledBitmap    = (Bitmap)dest;

            RGB_List = surface.GetPixelRGB();

            for (i = 0, j = 0; i < RGB_List.Length; i += 3, j++)
            {
                x = j % Width;
                y = j / Width;

                if (RGB_List[i] == surface.palette[0xFF].red &&
                    RGB_List[i + 1] == surface.palette[0xFF].gree &&
                    RGB_List[i + 2] == surface.palette[0xFF].blue)
                    continue;

                color = Color.FromArgb(RGB_List[i], RGB_List[i + 1], RGB_List[i + 2]);
                originalBitmap.SetPixel(x, y, color);
            }

            if (n_tupling == -1)
            {
                Width           = scaledBitmap.Width;
                Height          = scaledBitmap.Height;

                scaledWidth     = Width;
                scaledHeight    = Height;

                originalBitmap = (Bitmap)Video_ChangeImageSize(originalBitmap, Width, Height);
            }

            using (Graphics graphics = Graphics.FromImage(scaledBitmap))
            {
                //
                // Set the drawing mode of the image to pixel alignment
                //
                graphics.InterpolationMode  = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                graphics.PixelOffsetMode    = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

                //
                // Clear the entire drawing surface and fill it with a transparent background color
                //
                graphics.Clear(Color.Transparent);

                //
                // Draw an enlarged image
                //
                graphics.DrawImage(originalBitmap, new Rectangle(0, 0, scaledWidth, scaledHeight), new Rectangle(0, 0, Width, Height), GraphicsUnit.Pixel);
            }
        }

        public static Image
        Video_ChangeImageSize(
            Image       Src,
            INT         Width,
            INT         Height
        )
        {
            Bitmap resizedImage;

            if (Src == null) return null;

            resizedImage = new Bitmap(Width, Height);

            using (Graphics graphics = Graphics.FromImage(resizedImage))
            {
                //
                // Set the drawing mode of the image to pixel alignment
                //
                graphics.InterpolationMode  = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                graphics.PixelOffsetMode    = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;


                //
                // Clear the entire drawing surface and fill it with a transparent background color
                //
                graphics.Clear(Color.Transparent);

                //
                // Draw an enlarged image
                //
                graphics.DrawImage(Src, new Rectangle(0, 0, Width, Height));
            }

            return resizedImage;
        }
        --*/

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
                    tmp = UTIL_SubBytes(surface.pixels, p + b, surface.pitch - b);

                    for (b = p; b < (p + surface.pitch); b++) surface.pixels[b] = 0;

                    Array.Copy(tmp, 0, surface.pixels, (b < surface.pitch) ? b : p, tmp.Length);
                    //memcpy(buf, p, b);
                    //memmove(p, &p[b], 320 - b);
                    //memcpy(&p[320 - b], buf, b);
                }

                a = (a + 1) % 32;
                p += surface.pitch;
            }

            index = (index + 1) % 32;
        }
    }
}
