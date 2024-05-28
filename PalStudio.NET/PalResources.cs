using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

using PAL_POS   = System.UInt32;

using PalGlobal;

using static PalGlobal.Pal_Global;
using static PalConfig.Pal_Config;
using static PalCommon.Pal_Common;

namespace PalResources
{
    public class Pal_Resources
    {
        public BYTE[]   m_bySpirit;
        public PAL_POS  m_pos;
        public SHORT    m_sLayer;

        private Pal_Resources() { }

        public Pal_Resources(
            BYTE[]          bySpirit,
            PAL_POS         pos,
            SHORT           sLayer
        )
        {
            this.m_bySpirit = bySpirit;
            this.m_pos      = pos;
            this.m_sLayer   = sLayer;
        }

        public static BYTE[]
        PAL_GetEventObjectSprite(
            WORD            wEventObjectID
        )
        /*++
          Purpose:

            Get the sprite of the specified event object.

          Parameters:

            [IN]  wEventObjectID - the ID of event object.

          Return value:

            Pointer to the sprite.

        --*/
        {
            BYTE[]         binEventSprite = null;

            if (PAL_MKFDecompressChunk(ref binEventSprite, wEventObjectID, Pal_File.Pal_File_GetFile(lpszEventBMP).bufFile) == -1) return null;

            return binEventSprite;
        }
    }
}
