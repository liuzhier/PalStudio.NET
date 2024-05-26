using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

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

using LPSTR     = System.String;
using FILE      = System.IO.File;
using PAL_Rect  = System.Windows.Int32Rect;

using static PalGlobal.Pal_Global;

namespace PalMap
{
    public class Pal_Map
    {
        public class Pal_Map_Tile
        {
            public BOOL     fIsNoPassBlock  = FALSE;
            public SHORT    LowTile_Num     = 0;
            public BYTE     LowTile_Layer   = 0;
            public SHORT    HighTile_Num    = 0;
            public BYTE     HighTile_Layer  = 0;
        }

        public const  WORD m_MapWidth = 2064, m_MapHeight = 2055, m_MapTileWidth = 32, m_MapTileHeight = 16, wOffsetX_H = 16, wOffsetY_H = 8;

        public static readonly PAL_Rect m_MapRect       = new PAL_Rect(0, 0, m_MapWidth, m_MapHeight);
        public static readonly PAL_Rect m_MapTileRect   = new PAL_Rect(0, 0, m_MapTileWidth, m_MapTileHeight);

        public static INT               m_iMapNum   = -1, m_iSceneNum = -1;
        public static Pal_Map_Tile[,,]  Tiles       = new Pal_Map_Tile[128, 64, 2];
        public static BYTE[]            TileSprite  = (BYTE[])NULL;
    }
}
