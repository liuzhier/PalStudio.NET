using PalGlobal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

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

using static PalGlobal.Pal_Global;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using PalMap;

namespace PalUtil
{
    public enum Pal_INT_SIZE
    {
        BOOL    = (1 << 0),
        CHAR    = (1 << 0),
        BYTE    = (1 << 0),

        SHORT   = (1 << 1),
        WORD    = (1 << 1),

        INT     = (1 << 2),
        UINT    = (1 << 2),

        SDWORD  = (1 << 2),
        DWORD   = (1 << 2),

        SQWORD  = (1 << 3),
        QWORD   = (1 << 3),
    }

    public class Pal_Util
    {
        public static void
        UTIL_ProcessParameters(
            string[]    args
        )
        {

        }

        public static void
        UTIL_RegEncode()
        {
            if (!Pal_Global.fIsRegEncode)
            {
                Pal_Global.fIsRegEncode = TRUE;
                //Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            }
        }

        public static BYTE[]
        UTIL_SubBytes(
            BYTE[]  array,
            INT     iOffset,
            INT     iLength         = -1,
            INT     iAlignmentNum   = -1
        )
        {
            //ArraySegment<BYTE>  asList;
            BYTE[]      bytes;
            INT         i, iRealLength = iLength;

            if (iRealLength == -1)
            {
                iRealLength = array.Length - iOffset;
            }

            if (iAlignmentNum != -1 && iRealLength % iAlignmentNum != 0)
            {
                iRealLength += iAlignmentNum - (iRealLength + iAlignmentNum) % iAlignmentNum;
            }

            bytes = new BYTE[iRealLength];

            if (iLength == -1) iLength =  iRealLength;
            for (i = 0; i < iLength; i++) bytes[i] = array[iOffset + i];


            //asList = new ArraySegment<BYTE>(array, iOffset, iLength);
                /*
                if (iAlignmentNum != -1)
                {
                    if (asList.Count % iAlignmentNum != 0)
                    {
                        for (i = 0; i < iAlignmentNum - (asList.Count % iAlignmentNum); i++) asList.Append((BYTE)0);
                    }
                }

                return asList.ToArray();
                */

            return bytes;
        }

        public static BYTE[]
        UTIL_SubBytes(
            BYTE[]  array,
        ref INT     iOffset,
            INT     iLength         = -1,
            INT     iAlignmentNum   = -1
        )
        {
            BYTE[] bytes;
            INT i, iRealLength = iLength;

            if (iRealLength == -1)
            {
                iRealLength = array.Length - iOffset;
            }

            if (iAlignmentNum != -1 && iRealLength % iAlignmentNum != 0)
            {
                iRealLength += iAlignmentNum - (iRealLength + iAlignmentNum) % iAlignmentNum;
            }

            bytes = new BYTE[iRealLength];

            if (iLength == -1) iLength = iRealLength;
            for (i = 0; i < iLength; i++) bytes[i] = array[iOffset + i];

            iOffset += iLength;

            return bytes;
        }

        public static BYTE[]
        UTIL_SubBytes(
            BYTE[]          array,
            ref INT         iOffset,
            INT             iLength
        )
        {
            BYTE[] tmp = new ArraySegment<BYTE>(array, iOffset, iLength).ToArray();
            iOffset += iLength;

            return tmp;
        }

        public static LPSTR
        UTIL_GetFileCompletePath(
            LPSTR       lpszFileName
        )
        {
            return lpszGaemPath + PathDSC + lpszFileName;
        }

        public static INT
        UTIL_GetTypeSize(
            LPSTR       lpszType
        )
        {
            Pal_INT_SIZE     TypeSize = 0;

            switch (lpszType)
            {
                case "BOOL":
                case "CHAR":
                case "BYTE":
                    {
                        TypeSize = Pal_INT_SIZE.CHAR;
                    }
                    break;

                case "SHORT":
                case "WORD":
                    {
                        TypeSize = Pal_INT_SIZE.WORD;
                    }
                    break;

                case "INT":
                case "UINT":
                    {
                        TypeSize = Pal_INT_SIZE.UINT;
                    }
                    break;

                case "SDWORD":
                case "DWORD":
                    {
                        TypeSize = Pal_INT_SIZE.DWORD;
                    }
                    break;

                case "SQWORD":
                case "QWORD":
                    {
                        TypeSize = Pal_INT_SIZE.QWORD;
                    }
                    break;
            }

            return (INT)TypeSize;
        }

        public static ushort
        UTIL_SwapLE16(
            ushort value
        )
        {
            return BitConverter.ToUInt16(BitConverter.GetBytes(value), 0);
        }

        public static uint
        UTIL_SwapLE32(
            uint value
        )
        {
            return BitConverter.ToUInt32(BitConverter.GetBytes(value), 0);
        }

        public static INT
        UTIL_TextBoxTextIsMatch(
            TextBox     textBox,
            INT         iDataExchange
        )
        {
            if (textBox         != NULL &&
                textBox.Text    != NULL &&
                !LPSTR.IsNullOrEmpty(textBox.Text))
            {
                //
                // 判断字符串是否完全是数字
                //
                if (!Regex.IsMatch(textBox.Text, @"^\d+$"))
                {
                    //
                    // 用户输入了无效的数值
                    //
                    textBox.Text = iDataExchange.ToString();
                }
                else
                {
                    //
                    // 用户输入了有效的数值
                    //
                    return INT.Parse(textBox.Text);
                }
            }

            return PALSN_ERROR;
        }

        private static Regex m_Regex = new Regex("^[0-9]+$");

        public static void
        UTIL_TextBox_Num_PreviewTextInput(
            TextCompositionEventArgs    e
        )
        {
            e.Handled = !m_Regex.IsMatch(e.Text);
        }

        public static void
        UTIL_MapViewportScale_TextChanged(
            INT             iMapViewportScal,
            Canvas          cvsMapViewport_Canvas,
            ScaleTransform  stMapViewport_ScaleTransform,
            TextBox         tbMapViewportScale_TextBox = (TextBox)NULL
        )
        {
            //
            // 修改样式表中 <Transform Scale> 的 <ScaleX> 和 <ScaleY> （缩放比）
            //
            stMapViewport_ScaleTransform.ScaleX = iMapViewportScal / (double)100;
            stMapViewport_ScaleTransform.ScaleY = iMapViewportScal / (double)100;

            //
            // 调整 <Map Viewport Canvas> 尺寸（画布）
            //
            cvsMapViewport_Canvas.Width     = Pal_Map.mc_wMapWidth  * stMapViewport_ScaleTransform.ScaleX;
            cvsMapViewport_Canvas.Height    = Pal_Map.mc_wMapHeight * stMapViewport_ScaleTransform.ScaleY;

            if (tbMapViewportScale_TextBox != NULL)
            {
                //
                // 更新 <Map Viewport Scale> 缩放百分比显示
                //
                tbMapViewportScale_TextBox.Text = iMapViewportScal.ToString();
            }
        }

        public static void
        UTIL_MapViewportScale_TextBox_TextChanged(
            object          sender,
        ref INT             ref_iMapViewportScale,
            Canvas          cvsMapViewport_Canvas,
            ScaleTransform  stMapViewport_ScaleTransform
        )
        {
            INT             iMapViewportScale;
            TextBox         textBox = sender as TextBox;

            if (textBox != NULL)
            {
                //
                // 判断用户输入的数值是否合法
                //
                if ((iMapViewportScale = UTIL_TextBoxTextIsMatch(textBox, ref_iMapViewportScale)) == PALSN_ERROR) return;

                //
                // 数值未变动，退出函数
                //
                if (iMapViewportScale == ref_iMapViewportScale) return;

                //
                // 最大输入值不得超过 <999> %
                //
                ref_iMapViewportScale = Math.Max(iMapViewportScale,     Pal_Map.mc_iMapViewportScaleMin);
                ref_iMapViewportScale = Math.Min(ref_iMapViewportScale, Pal_Map.mc_iMapViewportScaleMax);
                if (iMapViewportScale >= Pal_Map.mc_iMapViewportScaleMin) textBox.Text = ref_iMapViewportScale.ToString();

                //
                // 应用缩放倍数
                //
                UTIL_MapViewportScale_TextChanged(ref_iMapViewportScale, cvsMapViewport_Canvas, stMapViewport_ScaleTransform);
            }
        }

        public static void
        UTIL_MapViewportScale_TextBox_LostFocus(
            object          sender,
            INT             iMapViewportScale,
            Canvas          cvsMapViewport_Canvas,
            ScaleTransform  stMapViewport_ScaleTransform
        )
        {
            INT         iScale;
            TextBox     textBox = sender as TextBox;

            if (textBox != NULL)
            {
                //
                // 判断用户输入的数值是否合法
                //
                if ((iScale = UTIL_TextBoxTextIsMatch(textBox, iMapViewportScale)) == PALSN_ERROR)
                {
                    //
                    // 用户输入了错误的百分值
                    //
                    goto tagEnd;
                }

                if (iScale >= Pal_Map.mc_iMapViewportScaleMin)
                {
                    //
                    // 用户输入百分值值正确
                    // 直接退出函数
                    //
                    return;
                }

tagEnd:
                //
                // 将缩放倍数还原到 <100> %
                //
                UTIL_MapViewportScale_TextChanged(iMapViewportScale, cvsMapViewport_Canvas, stMapViewport_ScaleTransform, textBox);
            }
        }
    }
}
