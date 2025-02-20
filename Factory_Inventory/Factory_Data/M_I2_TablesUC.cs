﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Factory_Inventory
{
    public partial class M_I2_TablesUC : UserControl
    {
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            Console.WriteLine(keyData.ToString());
            if (keyData == (Keys.D1) || keyData == (Keys.NumPad1))
            {
                this.button1.Focus();
                this.button1.PerformClick();
                return false;
            }

            if (keyData == (Keys.D2) || keyData == (Keys.NumPad2))
            {
                this.button2.Focus();
                this.button2.PerformClick();
                return false;
            }
           
            if (keyData == (Keys.D3) || keyData == (Keys.NumPad3))
            {
                this.button6.Focus();
                this.button6.PerformClick();
                return false;
            }
            
            if (keyData == (Keys.D4) || keyData == (Keys.NumPad4))
            {
                this.button3.Focus();
                this.button3.PerformClick();
                return false;
            }
            
            return base.ProcessCmdKey(ref msg, keyData);
        }
        public M_I2_TablesUC()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            M_I2_Tables f = new M_I2_Tables("SearchInCarton");
            Global.background.show_form(f);
            f.Text = "Tables - Carton";
        }

        private void button6_Click(object sender, EventArgs e)
        {
            M_I2_Tables f = new M_I2_Tables("SearchInBatch");
            Global.background.show_form(f);
            f.Text = "Tables - Batch";
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            M_I2_Tables f = new M_I2_Tables("SearchInTray");
            Global.background.show_form(f);
            f.Text = "Tables - Tray";
        }

        private void button3_Click(object sender, EventArgs e)
        {
            M_I2_Tables f = new M_I2_Tables("SearchInCartonProduced");
            Global.background.show_form(f);
            f.Text = "Tables - Carton Produced";
        }
    }
}
