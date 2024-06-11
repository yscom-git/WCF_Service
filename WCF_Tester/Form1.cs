using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WCF_Tester
{
    public partial class Form1 : Form
    {
        YMES.FX.DB.WCFHelper m_WCF = null;
        public Form1()
        {
            InitializeComponent();
        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if(DesignMode == false)
            {
                m_WCF.Open(textBox1.Text, "", "", "");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }
    }
}
