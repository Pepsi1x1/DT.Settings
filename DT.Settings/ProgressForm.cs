using System.Windows.Forms;

namespace DT.Common
{
    public partial class ProgressForm : Form
    {
        private string _progressText;

        public string ProgressText
        {
            get
            {
                return _progressText;
            }

            set
            {
                _progressText = value;
                progressTextBox.Text = _progressText;
            }
        }

        public ProgressForm()
        {
            InitializeComponent();
            loadingCircle1.Active = true;
        }

        public ProgressForm(string title)
        {
            InitializeComponent();
            Text = title;
            loadingCircle1.Active = true;
        }
    }
}
