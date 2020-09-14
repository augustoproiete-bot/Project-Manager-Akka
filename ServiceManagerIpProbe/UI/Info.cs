using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ServiceManagerIpProbe.UI
{
    public partial class Info : Form
    {
        private readonly Action<Action<string>> _runner;

        public Info(Action<Action<string>> runner)
        {
            _runner = runner;
            InitializeComponent();
        }

        private void Info_Shown(object sender, EventArgs e)
        {
            _runner(s => Invoke(new Action(() => textConsole.AppendText(s))));
        }
    }
}
