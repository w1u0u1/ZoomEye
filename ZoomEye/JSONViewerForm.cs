using EPocalipse.Json.Viewer;
using System.Windows.Forms;

namespace ZoomEye
{
    public partial class JSONViewerForm : Form
    {
        public JSONViewerForm(string text)
        {
            InitializeComponent();

            this.jsonViewer1.ShowTab(Tabs.Viewer);
            this.jsonViewer1.Json = text;
        }
    }
}
