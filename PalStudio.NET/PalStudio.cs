namespace PalStudio.NET
{
    public partial class PalStudio : Form
    {
        public PalStudio()
        {
            InitializeComponent();
        }

        private void toolStripStatus_DropdownMenu_DefaultItemChanged(object sender, ToolStripItemClickedEventArgs e)
        {
            // Get the currently selected option.
            ToolStripItem? TSMI_Default = sender as ToolStripItem;
            ToolStripItem? TSMI_Selected = e.ClickedItem;

            // Change the default text of TSCBOX_This
            TSMI_Default.Text = TSMI_Selected.Text;
        }
    }
}
