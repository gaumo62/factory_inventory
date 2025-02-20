﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Factory_Inventory.Factory_Classes;

namespace Factory_Inventory
{
    public partial class O_U_usersUC : UserControl
    {
        private DbConnect c;
        public string currentUser;
        //private int selectedRowIndex = -1;
        public O_U_usersUC()
        {
            InitializeComponent();
            this.c = new DbConnect();
            //Create a drop-down list
            var dataSource = new List<string>();
            string s1 = "Super User";
            string s2 = "Normal User";
            string s3 = "";
            dataSource.Add(s3);
            dataSource.Add(s1);
            dataSource.Add(s2);
            //Setup data binding
            this.comboBox1.DataSource = dataSource;
            this.comboBox1.DisplayMember = "Name";

            // make it readonly
            this.comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
        }

        //callbacks
        private void confirmButton_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("Confirm Changes?", "Message", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dialogResult == DialogResult.Yes)
            {
                if (deleteUserCheckbox.Checked == true)
                {
                    if (this.currentUser == usernameTextbox.Text)
                    {
                        c.ErrorBox("Cannot delete self user", "Error");
                    }
                    else
                    {
                        c.deleteUser(usernameTextbox.Text);
                    }
                }
                else
                {
                    if (passwordTextbox.Text != conformPasswordTextbox.Text)
                    {
                        c.ErrorBox("Passwords Do Not Match", "Error");
                        return;
                    }
                    else
                    {
                       if(this.comboBox1.SelectedIndex==0)
                        {
                            c.ErrorBox("Access Level Cannot be left empty", "Error");
                            return;
                        }
                       else
                        {
                            c.updateUser(usernameTextbox.Text, passwordTextbox.Text, this.comboBox1.SelectedIndex);
                        }
                    }
                }
                //this.selectedRowIndex = -1;
                this.comboBox1.SelectedIndex = 0;
                this.usernameTextbox.Text = "";
                this.passwordTextbox.Text = "";
                this.conformPasswordTextbox.Text = "";
                this.deleteUserCheckbox.Checked = false;
                loadDatabase();
            }
            else if (dialogResult == DialogResult.No)
            {
            }
            
        }
        private void userDataView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            usernameTextbox.Text = userDataView.Rows[e.RowIndex].Cells[1].Value.ToString();
            if(this.currentUser==usernameTextbox.Text)
            {
                comboBox1.Enabled = false;
            }
            else
            {
                comboBox1.Enabled = true;
            }
            comboBox1.SelectedIndex = Convert.ToInt32(userDataView.Rows[e.RowIndex].Cells[2].Value);
        }
        private void comboBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Enter)
            {
                if(this.comboBox1.DroppedDown==false)
                {
                    this.comboBox1.DroppedDown = true;
                    e.Handled = true; 
                }
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            O_U_SignupForm f2 = new O_U_SignupForm(c);
            f2.ShowDialog();
            loadDatabase();
        }

        //user functions
        public void loadDatabase()
        {
            DataTable d = c.getUserData();
            DataColumn dc = new DataColumn("Access Level", typeof(string));
            d.Columns.Add(dc);
            d.Columns.Add("SLNO", typeof(int)).SetOrdinal(0);
            for (int i = 0; i < d.Rows.Count; i++)
            {
                d.Rows[i][0] = i + 1;
                if (d.Rows[i][2].ToString() == "1")
                {
                    d.Rows[i][3] = "Super User";
                }
                else
                {
                    d.Rows[i][3] = "Normal User";
                }
            }
            //DataRow row = d.Rows[d.Rows.Count];
            //d.Rows.Remove(row);
            //d.DefaultView.Sort = "SLNO ASC";
            userDataView.DataSource = d;
            this.userDataView.Columns["AccessLevel"].Visible = false;
        }
    }
}
