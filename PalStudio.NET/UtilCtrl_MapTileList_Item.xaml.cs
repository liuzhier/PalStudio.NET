using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

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

using PalVideo;
using PalMap;

using static PalGlobal.Pal_Global;
using static PalVideo.Pal_Video;
using static PalCommon.Pal_Common;
using System.Windows.Media.Media3D;
using System.Security.Cryptography;
using System.Windows.Ink;

namespace PalStudio.NET
{
    /// <summary>
    /// UtilCtrl_MapTileList_Item.xaml 的交互逻辑
    /// </summary>
    public partial class UtilCtrl_MapTileList_Item : UserControl
    {
        private UIElement   m_Parent        = (UIElement)NULL;
        private Surface     m_Surface       = new Surface(Pal_Map.mc_wMapTileWidth, Pal_Map.mc_wMapTileHeight);
        private INT         m_iMapTileIndex = -1;

        public UtilCtrl_MapTileList_Item()
        {
            InitializeComponent();
        }

        public void
        Init(
            INT         iMapTileIndex
        )
        {
            this.Tag        = iMapTileIndex;
            m_iMapTileIndex = iMapTileIndex;

            //
            // 绘制 <Map Tile> 到 <Image>
            //
            DrawMapTileToImage();
        }

        public void
        SetText(
            LPSTR       lpszText
        )
        {
            //
            // 设置文本内容
            //
            Item_Text.Content = lpszText;
        }

        public void
        SetParent(
            UIElement   uieParent
        )
        {
            //
            // 设置父级元素
            //
            m_Parent = uieParent;
        }

        private void
        DrawMapTileToImage()
        {
            //
            // 绘制 <Map Tile> 到 <Surface>
            //
            PAL_RLEBlitToSurface(PAL_SpriteGetFrame(Pal_Map.TileSprite, m_iMapTileIndex), m_Surface, PAL_XY(0, 0));

            //
            // 开始将 <Surface> 转换为 <Image>
            //
            VIDEO_DrawSurfaceToImage(m_Surface, MapTile_Image, Pal_Map.m_MapTileRect);
        }

        private void
        SetBackground(
            SolidColorBrush scbColor
        )
        {
            Background_Border.Background = scbColor;
        }

        private void DockPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DockPanel Parent = m_Parent as DockPanel;

            if (sender  != NULL &&
                Parent  != NULL)
            {
                Parent.Tag = this.Tag;

                foreach (UtilCtrl_MapTileList_Item brother in Parent.Children)
                {
                    brother.SetBackground(Brushes.Pink);
                }

                //
                // 设置当前 <Map Tile Item> 背景色高亮
                //
                this.SetBackground(Brushes.LightSkyBlue);
            }
        }

        public void
        SimulateMouseDown()
        {
            //
            // 模拟鼠标按下当前 <UtilCtrl_MapTileList_Item>
            //
            DockPanel_MouseDown(this, (MouseButtonEventArgs)NULL);
        }

        public Image
        GetMapTileImage()
        {
            return MapTile_Image;
        }
    }
}
