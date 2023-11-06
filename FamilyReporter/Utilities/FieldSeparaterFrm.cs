using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FamilyReporter
{
    public partial class FieldSeparaterFrm : Form
    {
        char m_fieldSeparater;
        public char FieldSeparater { get { return m_fieldSeparater; } }

        public FieldSeparaterFrm()
        {
            InitializeComponent();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (textBox1.Text != string.Empty)
            {
                m_fieldSeparater = textBox1.Text[0];
                radioButton1.Checked = radioButton2.Checked = radioButton3.Checked = false;
            }

            else
            {
                radioButton1.Checked = true;
            }
        }

        private void btnSubmit_Click_1(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                m_fieldSeparater = ';';
            }
            else if (radioButton2.Checked)
            {
                m_fieldSeparater = '\t';
            }
            else if (radioButton3.Checked)
            {
                m_fieldSeparater = ',';
            }

            else
            {
                m_fieldSeparater = textBox1.Text[0];
            }

            this.Close();
        }
    }
}
