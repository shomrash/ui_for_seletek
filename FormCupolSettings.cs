using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UIforSeletek
{
    public partial class FormCupolSettings : Form
    {
        public FormCupolSettings()
        {
            InitializeComponent();
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.cupolOpen = (int)numericUpDownOpen.Value;
            Properties.Settings.Default.cupolClose = (int)numericUpDownClose.Value;
            Properties.Settings.Default.Save();
        }

        private void FormCupolSettings_Load(object sender, EventArgs e)
        {
            numericUpDownOpen.Value = Properties.Settings.Default.cupolOpen;
            numericUpDownClose.Value = Properties.Settings.Default.cupolClose;
        }
    }
}
