namespace PalStudio.NET
{
    partial class PalStudio
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PalStudio));
            MainPanel = new Panel();
            MainTabCtrl = new TabControl();
            TabPage_ReadMe = new TabPage();
            ReadMe_Label = new Label();
            MainWorld_TabPage = new TabPage();
            MainSplit_MainWorld = new SplitContainer();
            MapMode_MainWorld_StatusBar = new StatusStrip();
            EditMode_DropDownMenu_StatusBar = new ToolStripSplitButton();
            SelectElement_EditMode_StatusBar_MenuItem = new ToolStripMenuItem();
            EditElement_EditMode_StatusBar_MenuItem = new ToolStripMenuItem();
            DeleteElement_EditMode_StatusBar_MenuItem = new ToolStripMenuItem();
            ElementTarget_DropDownMenu_StatusBar = new ToolStripSplitButton();
            EventLayer_ElementTarget_DropDownMenu_StatusBar = new ToolStripMenuItem();
            ObstacleLayer_ElementTarget_DropDownMenu_StatusBar = new ToolStripMenuItem();
            TileHighLayer_ElementTarget_DropDownMenu_StatusBar = new ToolStripMenuItem();
            TileLowLayer_ElementTarget_DropDownMenu_StatusBar = new ToolStripMenuItem();
            MainWorld_SplitCtrl = new SplitContainer();
            panel1 = new Panel();
            MapTile_MainWorld_Panel = new Panel();
            Map_MainWorld_StatusBar = new StatusStrip();
            PosX_Label_StatusBar = new ToolStripStatusLabel();
            PosX_Value_StatusBar = new ToolStripStatusLabel();
            PosY_Label_StatusBar = new ToolStripStatusLabel();
            PosY_Value_StatusBar = new ToolStripStatusLabel();
            PosXBlock_Label_StatusBar = new ToolStripStatusLabel();
            PosXBlock_Value_StatusBar = new ToolStripStatusLabel();
            PosYBlock_Label_StatusBar = new ToolStripStatusLabel();
            PosYBlock_Value_StatusBar = new ToolStripStatusLabel();
            Half_Label_StatusBar = new ToolStripStatusLabel();
            toolStripStatusLabel9 = new ToolStripStatusLabel();
            MainPanel.SuspendLayout();
            MainTabCtrl.SuspendLayout();
            TabPage_ReadMe.SuspendLayout();
            MainWorld_TabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)MainSplit_MainWorld).BeginInit();
            MainSplit_MainWorld.Panel1.SuspendLayout();
            MainSplit_MainWorld.Panel2.SuspendLayout();
            MainSplit_MainWorld.SuspendLayout();
            MapMode_MainWorld_StatusBar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)MainWorld_SplitCtrl).BeginInit();
            MainWorld_SplitCtrl.Panel1.SuspendLayout();
            MainWorld_SplitCtrl.Panel2.SuspendLayout();
            MainWorld_SplitCtrl.SuspendLayout();
            panel1.SuspendLayout();
            Map_MainWorld_StatusBar.SuspendLayout();
            SuspendLayout();
            // 
            // MainPanel
            // 
            MainPanel.Controls.Add(MainTabCtrl);
            MainPanel.Dock = DockStyle.Fill;
            MainPanel.Font = new Font("Microsoft YaHei UI", 12.1008406F);
            MainPanel.Location = new Point(0, 0);
            MainPanel.Name = "MainPanel";
            MainPanel.Size = new Size(1395, 739);
            MainPanel.TabIndex = 0;
            // 
            // MainTabCtrl
            // 
            MainTabCtrl.Controls.Add(TabPage_ReadMe);
            MainTabCtrl.Controls.Add(MainWorld_TabPage);
            MainTabCtrl.Dock = DockStyle.Fill;
            MainTabCtrl.Font = new Font("Microsoft YaHei UI", 12.1008406F);
            MainTabCtrl.Location = new Point(0, 0);
            MainTabCtrl.Name = "MainTabCtrl";
            MainTabCtrl.SelectedIndex = 0;
            MainTabCtrl.Size = new Size(1395, 739);
            MainTabCtrl.SizeMode = TabSizeMode.Fixed;
            MainTabCtrl.TabIndex = 0;
            // 
            // TabPage_ReadMe
            // 
            TabPage_ReadMe.Controls.Add(ReadMe_Label);
            TabPage_ReadMe.Font = new Font("Microsoft YaHei UI", 12.1008406F);
            TabPage_ReadMe.Location = new Point(4, 36);
            TabPage_ReadMe.Name = "TabPage_ReadMe";
            TabPage_ReadMe.Padding = new Padding(3);
            TabPage_ReadMe.Size = new Size(1387, 699);
            TabPage_ReadMe.TabIndex = 0;
            TabPage_ReadMe.Text = "关于";
            TabPage_ReadMe.UseVisualStyleBackColor = true;
            // 
            // ReadMe_Label
            // 
            ReadMe_Label.Dock = DockStyle.Fill;
            ReadMe_Label.Font = new Font("Microsoft YaHei UI", 12.1008406F);
            ReadMe_Label.Location = new Point(3, 3);
            ReadMe_Label.Name = "ReadMe_Label";
            ReadMe_Label.Size = new Size(1381, 693);
            ReadMe_Label.TabIndex = 0;
            ReadMe_Label.Text = resources.GetString("ReadMe_Label.Text");
            // 
            // MainWorld_TabPage
            // 
            MainWorld_TabPage.Controls.Add(MainSplit_MainWorld);
            MainWorld_TabPage.Font = new Font("Microsoft YaHei UI", 12.1008406F);
            MainWorld_TabPage.Location = new Point(4, 36);
            MainWorld_TabPage.Name = "MainWorld_TabPage";
            MainWorld_TabPage.Padding = new Padding(3);
            MainWorld_TabPage.Size = new Size(1387, 699);
            MainWorld_TabPage.TabIndex = 1;
            MainWorld_TabPage.Text = "主世界";
            MainWorld_TabPage.UseVisualStyleBackColor = true;
            // 
            // MainSplit_MainWorld
            // 
            MainSplit_MainWorld.BorderStyle = BorderStyle.Fixed3D;
            MainSplit_MainWorld.Dock = DockStyle.Fill;
            MainSplit_MainWorld.FixedPanel = FixedPanel.Panel1;
            MainSplit_MainWorld.Location = new Point(3, 3);
            MainSplit_MainWorld.Name = "MainSplit_MainWorld";
            MainSplit_MainWorld.Orientation = Orientation.Horizontal;
            // 
            // MainSplit_MainWorld.Panel1
            // 
            MainSplit_MainWorld.Panel1.Controls.Add(MapMode_MainWorld_StatusBar);
            // 
            // MainSplit_MainWorld.Panel2
            // 
            MainSplit_MainWorld.Panel2.Controls.Add(MainWorld_SplitCtrl);
            MainSplit_MainWorld.Size = new Size(1381, 693);
            MainSplit_MainWorld.SplitterDistance = 37;
            MainSplit_MainWorld.TabIndex = 0;
            // 
            // MapMode_MainWorld_StatusBar
            // 
            MapMode_MainWorld_StatusBar.Font = new Font("Microsoft YaHei UI", 12.1008406F);
            MapMode_MainWorld_StatusBar.ImageScalingSize = new Size(20, 20);
            MapMode_MainWorld_StatusBar.Items.AddRange(new ToolStripItem[] { EditMode_DropDownMenu_StatusBar, ElementTarget_DropDownMenu_StatusBar });
            MapMode_MainWorld_StatusBar.Location = new Point(0, 0);
            MapMode_MainWorld_StatusBar.Name = "MapMode_MainWorld_StatusBar";
            MapMode_MainWorld_StatusBar.Size = new Size(1377, 33);
            MapMode_MainWorld_StatusBar.TabIndex = 1;
            MapMode_MainWorld_StatusBar.Text = "statusStrip2";
            // 
            // EditMode_DropDownMenu_StatusBar
            // 
            EditMode_DropDownMenu_StatusBar.BackColor = Color.IndianRed;
            EditMode_DropDownMenu_StatusBar.DropDownItems.AddRange(new ToolStripItem[] { SelectElement_EditMode_StatusBar_MenuItem, EditElement_EditMode_StatusBar_MenuItem, DeleteElement_EditMode_StatusBar_MenuItem });
            EditMode_DropDownMenu_StatusBar.Font = new Font("Microsoft YaHei UI", 12.1008406F);
            EditMode_DropDownMenu_StatusBar.Name = "EditMode_DropDownMenu_StatusBar";
            EditMode_DropDownMenu_StatusBar.Size = new Size(111, 31);
            EditMode_DropDownMenu_StatusBar.Text = "选择元素";
            EditMode_DropDownMenu_StatusBar.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // SelectElement_EditMode_StatusBar_MenuItem
            // 
            SelectElement_EditMode_StatusBar_MenuItem.Font = new Font("Microsoft YaHei UI", 12.1008406F);
            SelectElement_EditMode_StatusBar_MenuItem.Name = "SelectElement_EditMode_StatusBar_MenuItem";
            SelectElement_EditMode_StatusBar_MenuItem.Size = new Size(178, 32);
            SelectElement_EditMode_StatusBar_MenuItem.Text = "选择元素";
            // 
            // EditElement_EditMode_StatusBar_MenuItem
            // 
            EditElement_EditMode_StatusBar_MenuItem.Font = new Font("Microsoft YaHei UI", 12.1008406F);
            EditElement_EditMode_StatusBar_MenuItem.Name = "EditElement_EditMode_StatusBar_MenuItem";
            EditElement_EditMode_StatusBar_MenuItem.Size = new Size(178, 32);
            EditElement_EditMode_StatusBar_MenuItem.Text = "编辑元素";
            // 
            // DeleteElement_EditMode_StatusBar_MenuItem
            // 
            DeleteElement_EditMode_StatusBar_MenuItem.Font = new Font("Microsoft YaHei UI", 12.1008406F);
            DeleteElement_EditMode_StatusBar_MenuItem.Name = "DeleteElement_EditMode_StatusBar_MenuItem";
            DeleteElement_EditMode_StatusBar_MenuItem.Size = new Size(178, 32);
            DeleteElement_EditMode_StatusBar_MenuItem.Text = "删除元素";
            // 
            // ElementTarget_DropDownMenu_StatusBar
            // 
            ElementTarget_DropDownMenu_StatusBar.BackColor = Color.IndianRed;
            ElementTarget_DropDownMenu_StatusBar.DropDownItems.AddRange(new ToolStripItem[] { EventLayer_ElementTarget_DropDownMenu_StatusBar, ObstacleLayer_ElementTarget_DropDownMenu_StatusBar, TileHighLayer_ElementTarget_DropDownMenu_StatusBar, TileLowLayer_ElementTarget_DropDownMenu_StatusBar });
            ElementTarget_DropDownMenu_StatusBar.Font = new Font("Microsoft YaHei UI", 12.1008406F);
            ElementTarget_DropDownMenu_StatusBar.Name = "ElementTarget_DropDownMenu_StatusBar";
            ElementTarget_DropDownMenu_StatusBar.Size = new Size(91, 31);
            ElementTarget_DropDownMenu_StatusBar.Text = "事件层";
            ElementTarget_DropDownMenu_StatusBar.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // EventLayer_ElementTarget_DropDownMenu_StatusBar
            // 
            EventLayer_ElementTarget_DropDownMenu_StatusBar.Font = new Font("Microsoft YaHei UI", 12.1008406F);
            EventLayer_ElementTarget_DropDownMenu_StatusBar.Name = "EventLayer_ElementTarget_DropDownMenu_StatusBar";
            EventLayer_ElementTarget_DropDownMenu_StatusBar.Size = new Size(178, 32);
            EventLayer_ElementTarget_DropDownMenu_StatusBar.Text = "事件层";
            // 
            // ObstacleLayer_ElementTarget_DropDownMenu_StatusBar
            // 
            ObstacleLayer_ElementTarget_DropDownMenu_StatusBar.Font = new Font("Microsoft YaHei UI", 12.1008406F);
            ObstacleLayer_ElementTarget_DropDownMenu_StatusBar.Name = "ObstacleLayer_ElementTarget_DropDownMenu_StatusBar";
            ObstacleLayer_ElementTarget_DropDownMenu_StatusBar.Size = new Size(178, 32);
            ObstacleLayer_ElementTarget_DropDownMenu_StatusBar.Text = "障碍层";
            // 
            // TileHighLayer_ElementTarget_DropDownMenu_StatusBar
            // 
            TileHighLayer_ElementTarget_DropDownMenu_StatusBar.Font = new Font("Microsoft YaHei UI", 12.1008406F);
            TileHighLayer_ElementTarget_DropDownMenu_StatusBar.Name = "TileHighLayer_ElementTarget_DropDownMenu_StatusBar";
            TileHighLayer_ElementTarget_DropDownMenu_StatusBar.Size = new Size(178, 32);
            TileHighLayer_ElementTarget_DropDownMenu_StatusBar.Text = "地形上层";
            // 
            // TileLowLayer_ElementTarget_DropDownMenu_StatusBar
            // 
            TileLowLayer_ElementTarget_DropDownMenu_StatusBar.Name = "TileLowLayer_ElementTarget_DropDownMenu_StatusBar";
            TileLowLayer_ElementTarget_DropDownMenu_StatusBar.Size = new Size(178, 32);
            TileLowLayer_ElementTarget_DropDownMenu_StatusBar.Text = "地形下层";
            // 
            // MainWorld_SplitCtrl
            // 
            MainWorld_SplitCtrl.BorderStyle = BorderStyle.Fixed3D;
            MainWorld_SplitCtrl.Dock = DockStyle.Fill;
            MainWorld_SplitCtrl.FixedPanel = FixedPanel.Panel1;
            MainWorld_SplitCtrl.Font = new Font("Microsoft YaHei UI", 12.1008406F);
            MainWorld_SplitCtrl.Location = new Point(0, 0);
            MainWorld_SplitCtrl.Name = "MainWorld_SplitCtrl";
            // 
            // MainWorld_SplitCtrl.Panel1
            // 
            MainWorld_SplitCtrl.Panel1.BackColor = Color.Transparent;
            MainWorld_SplitCtrl.Panel1.Controls.Add(panel1);
            // 
            // MainWorld_SplitCtrl.Panel2
            // 
            MainWorld_SplitCtrl.Panel2.Controls.Add(Map_MainWorld_StatusBar);
            MainWorld_SplitCtrl.Size = new Size(1381, 652);
            MainWorld_SplitCtrl.SplitterDistance = 234;
            MainWorld_SplitCtrl.TabIndex = 1;
            // 
            // panel1
            // 
            panel1.AutoScroll = true;
            panel1.Controls.Add(MapTile_MainWorld_Panel);
            panel1.Dock = DockStyle.Fill;
            panel1.Font = new Font("Microsoft YaHei UI", 12.1008406F);
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(230, 648);
            panel1.TabIndex = 1;
            // 
            // MapTile_MainWorld_Panel
            // 
            MapTile_MainWorld_Panel.Dock = DockStyle.Top;
            MapTile_MainWorld_Panel.Font = new Font("Microsoft YaHei UI", 12.1008406F);
            MapTile_MainWorld_Panel.Location = new Point(0, 0);
            MapTile_MainWorld_Panel.Name = "MapTile_MainWorld_Panel";
            MapTile_MainWorld_Panel.Size = new Size(209, 800);
            MapTile_MainWorld_Panel.TabIndex = 1;
            // 
            // Map_MainWorld_StatusBar
            // 
            Map_MainWorld_StatusBar.Font = new Font("Microsoft YaHei UI", 12.1008406F);
            Map_MainWorld_StatusBar.ImageScalingSize = new Size(20, 20);
            Map_MainWorld_StatusBar.Items.AddRange(new ToolStripItem[] { PosX_Label_StatusBar, PosX_Value_StatusBar, PosY_Label_StatusBar, PosY_Value_StatusBar, PosXBlock_Label_StatusBar, PosXBlock_Value_StatusBar, PosYBlock_Label_StatusBar, PosYBlock_Value_StatusBar, Half_Label_StatusBar, toolStripStatusLabel9 });
            Map_MainWorld_StatusBar.Location = new Point(0, 615);
            Map_MainWorld_StatusBar.Name = "Map_MainWorld_StatusBar";
            Map_MainWorld_StatusBar.Size = new Size(1139, 33);
            Map_MainWorld_StatusBar.TabIndex = 0;
            Map_MainWorld_StatusBar.Text = "statusStrip1";
            // 
            // PosX_Label_StatusBar
            // 
            PosX_Label_StatusBar.Font = new Font("Microsoft YaHei UI", 12.1008406F);
            PosX_Label_StatusBar.Name = "PosX_Label_StatusBar";
            PosX_Label_StatusBar.Size = new Size(30, 27);
            PosX_Label_StatusBar.Text = "X:";
            // 
            // PosX_Value_StatusBar
            // 
            PosX_Value_StatusBar.Font = new Font("Microsoft YaHei UI", 12.1008406F);
            PosX_Value_StatusBar.Name = "PosX_Value_StatusBar";
            PosX_Value_StatusBar.Size = new Size(130, 27);
            PosX_Value_StatusBar.Text = "FFFF(65535)";
            PosX_Value_StatusBar.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // PosY_Label_StatusBar
            // 
            PosY_Label_StatusBar.Font = new Font("Microsoft YaHei UI", 12.1008406F);
            PosY_Label_StatusBar.Name = "PosY_Label_StatusBar";
            PosY_Label_StatusBar.Size = new Size(29, 27);
            PosY_Label_StatusBar.Text = "Y:";
            // 
            // PosY_Value_StatusBar
            // 
            PosY_Value_StatusBar.Font = new Font("Microsoft YaHei UI", 12.1008406F);
            PosY_Value_StatusBar.Name = "PosY_Value_StatusBar";
            PosY_Value_StatusBar.Size = new Size(130, 27);
            PosY_Value_StatusBar.Text = "FFFF(65535)";
            PosY_Value_StatusBar.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // PosXBlock_Label_StatusBar
            // 
            PosXBlock_Label_StatusBar.Font = new Font("Microsoft YaHei UI", 12.1008406F);
            PosXBlock_Label_StatusBar.Name = "PosXBlock_Label_StatusBar";
            PosXBlock_Label_StatusBar.Size = new Size(82, 27);
            PosXBlock_Label_StatusBar.Text = "XBlock:";
            // 
            // PosXBlock_Value_StatusBar
            // 
            PosXBlock_Value_StatusBar.Font = new Font("Microsoft YaHei UI", 12.1008406F);
            PosXBlock_Value_StatusBar.Name = "PosXBlock_Value_StatusBar";
            PosXBlock_Value_StatusBar.Size = new Size(130, 27);
            PosXBlock_Value_StatusBar.Text = "FFFF(65535)";
            PosXBlock_Value_StatusBar.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // PosYBlock_Label_StatusBar
            // 
            PosYBlock_Label_StatusBar.Font = new Font("Microsoft YaHei UI", 12.1008406F);
            PosYBlock_Label_StatusBar.Name = "PosYBlock_Label_StatusBar";
            PosYBlock_Label_StatusBar.Size = new Size(81, 27);
            PosYBlock_Label_StatusBar.Text = "YBlock:";
            // 
            // PosYBlock_Value_StatusBar
            // 
            PosYBlock_Value_StatusBar.Font = new Font("Microsoft YaHei UI", 12.1008406F);
            PosYBlock_Value_StatusBar.Name = "PosYBlock_Value_StatusBar";
            PosYBlock_Value_StatusBar.Size = new Size(130, 27);
            PosYBlock_Value_StatusBar.Text = "FFFF(65535)";
            PosYBlock_Value_StatusBar.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // Half_Label_StatusBar
            // 
            Half_Label_StatusBar.Font = new Font("Microsoft YaHei UI", 12.1008406F);
            Half_Label_StatusBar.Name = "Half_Label_StatusBar";
            Half_Label_StatusBar.Size = new Size(55, 27);
            Half_Label_StatusBar.Text = "Half:";
            // 
            // toolStripStatusLabel9
            // 
            toolStripStatusLabel9.Font = new Font("Microsoft YaHei UI", 12.1008406F);
            toolStripStatusLabel9.Name = "toolStripStatusLabel9";
            toolStripStatusLabel9.Size = new Size(24, 27);
            toolStripStatusLabel9.Text = "1";
            toolStripStatusLabel9.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // PalStudio
            // 
            AutoScaleDimensions = new SizeF(9F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1395, 739);
            Controls.Add(MainPanel);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "PalStudio";
            Text = "Pal Studio";
            MainPanel.ResumeLayout(false);
            MainTabCtrl.ResumeLayout(false);
            TabPage_ReadMe.ResumeLayout(false);
            MainWorld_TabPage.ResumeLayout(false);
            MainSplit_MainWorld.Panel1.ResumeLayout(false);
            MainSplit_MainWorld.Panel1.PerformLayout();
            MainSplit_MainWorld.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)MainSplit_MainWorld).EndInit();
            MainSplit_MainWorld.ResumeLayout(false);
            MapMode_MainWorld_StatusBar.ResumeLayout(false);
            MapMode_MainWorld_StatusBar.PerformLayout();
            MainWorld_SplitCtrl.Panel1.ResumeLayout(false);
            MainWorld_SplitCtrl.Panel2.ResumeLayout(false);
            MainWorld_SplitCtrl.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)MainWorld_SplitCtrl).EndInit();
            MainWorld_SplitCtrl.ResumeLayout(false);
            panel1.ResumeLayout(false);
            Map_MainWorld_StatusBar.ResumeLayout(false);
            Map_MainWorld_StatusBar.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel MainPanel;
        private TabControl MainTabCtrl;
        private TabPage TabPage_ReadMe;
        private TabPage MainWorld_TabPage;
        private Label ReadMe_Label;
        private SplitContainer MainSplit_MainWorld;
        private SplitContainer MainWorld_SplitCtrl;
        private Panel panel1;
        private Panel MapTile_MainWorld_Panel;
        private StatusStrip Map_MainWorld_StatusBar;
        private ToolStripStatusLabel PosX_Label_StatusBar;
        private ToolStripStatusLabel PosX_Value_StatusBar;
        private ToolStripStatusLabel PosY_Label_StatusBar;
        private ToolStripStatusLabel PosY_Value_StatusBar;
        private ToolStripStatusLabel PosXBlock_Label_StatusBar;
        private ToolStripStatusLabel PosXBlock_Value_StatusBar;
        private ToolStripStatusLabel PosYBlock_Label_StatusBar;
        private ToolStripStatusLabel PosYBlock_Value_StatusBar;
        private ToolStripStatusLabel Half_Label_StatusBar;
        private ToolStripStatusLabel toolStripStatusLabel9;
        private StatusStrip MapMode_MainWorld_StatusBar;
        private ToolStripSplitButton EditMode_DropDownMenu_StatusBar;
        private ToolStripMenuItem SelectElement_EditMode_StatusBar_MenuItem;
        private ToolStripMenuItem EditElement_EditMode_StatusBar_MenuItem;
        private ToolStripMenuItem DeleteElement_EditMode_StatusBar_MenuItem;
        private ToolStripSplitButton ElementTarget_DropDownMenu_StatusBar;
        private ToolStripMenuItem EventLayer_ElementTarget_DropDownMenu_StatusBar;
        private ToolStripMenuItem ObstacleLayer_ElementTarget_DropDownMenu_StatusBar;
        private ToolStripMenuItem TileHighLayer_ElementTarget_DropDownMenu_StatusBar;
        private ToolStripMenuItem TileLowLayer_ElementTarget_DropDownMenu_StatusBar;
    }
}
