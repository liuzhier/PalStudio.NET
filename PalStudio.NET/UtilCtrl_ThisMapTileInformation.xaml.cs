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

using PAL_POS   = System.UInt32;
using PAL_Rect  = System.Windows.Int32Rect;

using PalGlobal;
using PalMap;
using PalVideo;

using static PalGlobal.Pal_Global;
using static PalUtil.Pal_Util;
using static PalVideo.Pal_Video;
using static PalMap.Pal_Map;
using static PalCommon.Pal_Common;

namespace PalStudio.NET
{
    /// <summary>
    /// UserControl1.xaml 的交互逻辑
    /// </summary>
    public partial class UtilCtrl_ThisMapTileInformation : UserControl
    {
        private INT                 m_iThisMapTile, m_DefaultMapTile = PALSN_ERROR;
        private WORD                m_wSelectPosX, m_wSelectPosY;
        private BYTE                m_bySelectPosXB, m_bySelectPosYB, m_bySelectPosHB;
        private BOOL                m_fIsBindingBoth = FALSE;
        private Image               m_MapViewportImage;
        private Surface             m_MapViewportSurface;
        private PAL_Rect            m_ThisMapTileRect;
        private MapLayerType        m_mltMapLayerType;
        private UIElement           m_MapTilesList;

        public UtilCtrl_ThisMapTileInformation()
        {
            InitializeComponent();
        }

        public void
        SetDefaultMapTile(
            INT         DefaultMapTile
        )
        {
            m_DefaultMapTile = DefaultMapTile;
        }

        public void
        SetPoxXYH(
            BYTE        byPoxXB,
            BYTE        byPoxYB,
            BYTE        byPoxHB
        )
        {
            PAL_POS     posMapTile = PAL_XYH_TO_POS(byPoxXB, byPoxYB, byPoxHB);

            //
            // 设置当前选中的 <Map Tile> 的块坐标
            //
            m_bySelectPosXB = byPoxXB;
            m_bySelectPosYB = byPoxYB;
            m_bySelectPosHB = byPoxHB;

            m_wSelectPosX   = PAL_X(posMapTile);
            m_wSelectPosY   = PAL_Y(posMapTile);
        }

        private void
        SetTitle(
            LPSTR       lpszTitle
        )
        {
            ThisMapTileIndex_Label.Content = lpszTitle;
        }

        private void
        SetText(
            LPSTR       lpszTitle
        )
        {
            ThisMapTileIndex_TextBox.Text = lpszTitle;
        }

        private void
        ReplaceImage()
        {
            if (m_iThisMapTile < (m_MapTilesList as DockPanel).Children.Count && m_iThisMapTile >= 0)
            {
                ThisMapTile_Image.Source = ((m_MapTilesList as DockPanel).Children[m_iThisMapTile] as UtilCtrl_MapTileList_Item).GetMapTileImage().Source;
            }
            else
            {
                ThisMapTile_Image.Source = (ImageSource)NULL;
                VIDEO_DrawSurfaceToImage(Pal_Map.mc_sfMapTileCursor[0], ThisMapTile_Image, Pal_Map.m_MapTileRect);
            }
        }

        public void
        SetThisMapTile(
            INT         iThisMapTile
        )
        {
            //
            // 设置 <标题> 和 <图像> 为当前选中的 <Map Tile>
            //
            m_iThisMapTile = iThisMapTile;
            this.SetTitle($"[0x{m_iThisMapTile:X4}]");
            this.SetText(m_iThisMapTile.ToString());
            this.ReplaceImage();
        }

        public void
        BindMapTilesList(
            UIElement   MapTilesList
        )
        {
            //
            // 绑定 <Map Tiles List>
            //
            m_MapTilesList = MapTilesList;
        }

        public void
        SetBindingBoth(
            BOOL        fIsBindingBoth
        )
        {
            //
            // 设置是否 <双向绑定>
            // 该属性决定了当用户修改 <TextBox> 数据后，
            // 是否自动同步选择 <Map Tiles List> 的子项
            //
            m_fIsBindingBoth = fIsBindingBoth;
        }

        public void
        BindMapViewportImage(
            MapLayerType    mltMapLayerType,
            Image           MapViewportImage,
            Surface         MapViewportSurface
        )
        {
            //
            // 绑定 <Map Viewport Image>
            // 以便于实时更新地图
            //
            m_mltMapLayerType       = mltMapLayerType;
            m_MapViewportImage      = MapViewportImage;
            m_MapViewportSurface    = MapViewportSurface;
        }

        private void
        ReplaceMapViewportImage()
        {
            //
            // 更新 <Map Tiles Data> 列表中的数据
            //
            if (m_mltMapLayerType == MapLayerType.LowTile)
            {
                //
                // <Low Tile>
                //
                Pal_Map.Tiles[m_bySelectPosYB, m_bySelectPosXB, m_bySelectPosHB].LowTile_Num = (BYTE)m_iThisMapTile;
            }
            else
            {
                //
                // <High Tile>
                //
                Pal_Map.Tiles[m_bySelectPosYB, m_bySelectPosXB, m_bySelectPosHB].HighTile_Num = (BYTE)m_iThisMapTile;
            }

            //
            // 清空 <Map Viewport Image> 选中的区域的像素
            //
            PAL_MapTileCleanSpirit(m_MapViewportSurface, m_bySelectPosXB, m_bySelectPosYB, m_bySelectPosHB);

            //
            // 获取需要更新的图像区域（矩形）
            //
            m_ThisMapTileRect = new PAL_Rect(m_wSelectPosX, m_wSelectPosY, Pal_Map.mc_wMapTileWidth, Pal_Map.mc_wMapTileHeight);

            //
            // 更新 <Map Viewport Image>
            //
            if (m_iThisMapTile >= 0)
            {
                PAL_RLEBlitToSurface(PAL_SpriteGetFrame(Pal_Map.TileSprite, m_iThisMapTile), m_MapViewportSurface, PAL_XY(m_wSelectPosX, m_wSelectPosY));
            }

            VIDEO_DrawSurfaceToImage(m_MapViewportSurface, m_MapViewportImage, m_ThisMapTileRect, TRUE);
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e) => UTIL_TextBox_Num_PreviewTextInput(e);

        private void ThisMapTileIndex_TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            INT         iThisMapTile;
            DockPanel   MapTilesList = m_MapTilesList as DockPanel;

            //
            // 判断用户输入的数值是否合法
            //
            if ((iThisMapTile = UTIL_TextBoxTextIsMatch(ThisMapTileIndex_TextBox, m_iThisMapTile)) == PALSN_ERROR) return;

            //
            // 数值未变动，退出函数
            //
            if (iThisMapTile == m_iThisMapTile) return;

            //
            // 最大输入值不得超过 <Maximum Map Tiles Index>
            //
            this.SetThisMapTile(Math.Min(iThisMapTile, MapTilesList.Children.Count - 1));

            //
            // 控件双向绑定时
            // 模拟 <UtilCtrl_MapTileList_Item> 点击
            //
            if (m_fIsBindingBoth) ((UtilCtrl_MapTileList_Item)MapTilesList.Children[m_iThisMapTile]).SimulateMouseDown();

            if (m_MapViewportImage != NULL)
            {
                //
                // 如果绑定了 <Map Viewport Image> 则更新它
                //
                this.ReplaceMapViewportImage();
            }
        }

        private void ThisMapTileIndex_TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            INT iThisMapTile;
            TextBox textBox = sender as TextBox;

            if (textBox != NULL)
            {
                //
                // 判断用户输入的数值是否合法
                //
                if ((iThisMapTile = UTIL_TextBoxTextIsMatch(textBox, m_iThisMapTile)) == PALSN_ERROR)
                {
                    //
                    // 用户输入了错误的百分值
                    //
                    goto tagEnd;
                }

                if (iThisMapTile >= 0)
                {
                    //
                    // 用户输入百分值值正确
                    // 直接退出函数
                    //
                    return;
                }

            tagEnd:
                //
                // 撤回到上次输入的数值
                //
                if (m_DefaultMapTile != PALSN_ERROR)
                {
                    m_iThisMapTile  = m_DefaultMapTile;
                    this.ReplaceImage();
                    this.ReplaceMapViewportImage();
                }

                textBox.Text = m_iThisMapTile.ToString();
            }
        }
    }
}
