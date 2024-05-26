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
    }
}
