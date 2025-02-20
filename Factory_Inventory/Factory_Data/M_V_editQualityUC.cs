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
using HatchStyleComboBox;
using System.Drawing.Drawing2D;

namespace Factory_Inventory
{
    public partial class editQuality : UserControl
    {
        private DbConnect c = new DbConnect();
        public string currentUser;
        List<string> l = new List<string>();
        //private int selectedRowIndex = -1;
        public editQuality()
        {
            InitializeComponent();
            l.Add("");
            l.Add("Percent10");
            l.Add("Percent90");
            l.Add("Horizontal");
            l.Add("Vertical");
            l.Add("WideDownwardDiagonal");
            l.Add("Cross");
            l.Add("DiagonalCross");
            l.Add("DashedUpwardDiagonal");
            l.Add("DashedHorizontal");
            l.Add("DashedVertical");
            l.Add("LargeConfetti");
            l.Add("ZigZag");
            l.Add("DiagonalBrick");
            l.Add("HorizontalBrick");
            l.Add("Plaid");
            l.Add("Shingle");
            l.Add("LargeCheckerBoard");
            l.Add("SolidDiamond");

            //All
            //l.Add ("Horizontal");
            //l.Add("Min");
			//l.Add("Vertical");
			//l.Add("ForwardDiagonal");
			//l.Add("BackwardDiagonal");
			//l.Add("Cross");
			//l.Add("LargeGrid");
			//l.Add("Max");
			//l.Add("DiagonalCross");
			//l.Add("Percent05");
			//l.Add("Percent10");
			//l.Add("Percent20");
			//l.Add("Percent25");
			//l.Add("Percent30");
			//l.Add("Percent40");
			//l.Add("Percent50");
			//l.Add("Percent60");
			//l.Add("Percent70");
			//l.Add("Percent75");
			//l.Add("Percent80");
			//l.Add("Percent90");
			//l.Add("LightDownwardDiagonal");
			//l.Add("LightUpwardDiagonal");
			//l.Add("DarkDownwardDiagonal");
			//l.Add("DarkUpwardDiagonal");
			//l.Add("WideDownwardDiagonal");
			//l.Add("WideUpwardDiagonal");
			//l.Add("LightVertical");
			//l.Add("LightHorizontal");
			//l.Add("NarrowVertical");
			//l.Add("NarrowHorizontal");
			//l.Add("DarkVertical");
			//l.Add("DarkHorizontal");
			//l.Add("DashedDownwardDiagonal");
			//l.Add("DashedUpwardDiagonal");
			//l.Add("DashedHorizontal");
			//l.Add("DashedVertical");
			//l.Add("SmallConfetti");
			//l.Add("LargeConfetti");
			//l.Add("ZigZag");
			//l.Add("Wave");
			//l.Add("DiagonalBrick");
			//l.Add("HorizontalBrick");
			//l.Add("Weave");
			//l.Add("Plaid");
			//l.Add("Divot");
			//l.Add("DottedGrid");
			//l.Add("DottedDiamond");
			//l.Add("Shingle");
			//l.Add("Trellis");
			//l.Add("Sphere");
			//l.Add("SmallGrid");
			//l.Add("SmallCheckerBoard");
			//l.Add("LargeCheckerBoard");
			//l.Add("OutlinedDiamond");
            //l.Add("SolidDiamond");


            List<string> l2 = new List<string>(l);;
            this.hsComboBox2.DataSource = l;
            this.hsComboBox3.DataSource = l2;

            DataTable dt = c.runQuery("SELECT * FROM Quality_Before_Twist");
            List<string> datasource = new List<string>();
            datasource.Add("---Select---");
            for(int i=0;i<dt.Rows.Count;i++)
            {
                datasource.Add(dt.Rows[i]["Quality_Before_Twist"].ToString());
            }
            this.addQBTCB.DataSource = datasource;
            this.addQBTCB.DropDownStyle = ComboBoxStyle.DropDownList;//Create a drop-down list
            this.addQBTCB.AutoCompleteSource = AutoCompleteSource.ListItems;
            this.addQBTCB.AutoCompleteMode = AutoCompleteMode.SuggestAppend;

            this.editQBTCB.DataSource = datasource;
            this.editQBTCB.DropDownStyle = ComboBoxStyle.DropDownList;//Create a drop-down list
            this.editQBTCB.AutoCompleteSource = AutoCompleteSource.ListItems;
            this.editQBTCB.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
        }
        public void loadDatabase()
        {
            DataTable d = c.getQC('q');
            dataGridView1.DataSource = d;
            this.dataGridView1.Columns[2].HeaderText = "Print Pattern";
            c.auto_adjust_dgv(dataGridView1);
        }
        private void confirmButton_Click_1(object sender, EventArgs e)  
        {
            if (dataGridView1.SelectedRows.Count <= 0) return;
            int RowIndex = this.dataGridView1.SelectedRows[0].Index;
            DialogResult dialogResult = MessageBox.Show("Confirm Changes?", "Message", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dialogResult == DialogResult.Yes)
            {
                if (dataGridView1.SelectedRows.Count <= 0) return;
                int row = dataGridView1.SelectedRows[0].Index;
                if (deleteUserCheckboxCK.Checked == true)
                {
                    c.deleteQuality(dataGridView1.Rows[row].Cells[0].Value.ToString());
                }
                else
                {
                    if (editedQualityTextboxTB.Text == "" || editHSNNoTB.Text == "" || this.hsComboBox3.SelectedIndex <= 0 || editQBTCB.SelectedIndex == 0)
                    {
                        c.ErrorBox("Enter all values", "Error");
                        return;
                    }
                    c.editQuality(editQBTCB.Text, editHSNNoTB.Text, this.hsComboBox3.SelectedItem.ToString(), editedQualityTextboxTB.Text, dataGridView1.Rows[row].Cells[0].Value.ToString());
                }
                
                //this.selectedRowIndex = -1;
                this.editedQualityTextboxTB.Text = "";
                editHSNNoTB.Text = "";
                editQBTCB.SelectedIndex = 0;
                this.deleteUserCheckboxCK.Checked = false;
                loadDatabase();
                if (RowIndex >= 0 && RowIndex<=dataGridView1.Rows.Count-1)
                {
                    this.dataGridView1.Rows[RowIndex].Selected = true;
                }
            }
        }
        private void addQualityButton_Click_1(object sender, EventArgs e)
        {
            if (newQualityTextboxTB.Text == "" || addHSNNoTB.Text == "" || this.hsComboBox2.SelectedIndex <= 0 || addQBTCB.SelectedIndex == 0)
            {
                c.ErrorBox("Enter all values", "Error");
                return;
            }
            c.addQuality(addQBTCB.Text, addHSNNoTB.Text, this.hsComboBox2.SelectedItem.ToString(), this.newQualityTextboxTB.Text); 
            this.newQualityTextboxTB.Text = "";
            this.addHSNNoTB.Text = "";
            this.addQBTCB.SelectedIndex = 0;
            loadDatabase();
        }
        private void hsComboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (hsComboBox3.SelectedIndex > 0)
            {
                this.Invalidate();
            }
        }
        private void editQuality_Paint(object sender, PaintEventArgs e)
        {
            if (hsComboBox2.SelectedIndex > 0)
            {
                HatchStyle hs = (HatchStyle)Enum.Parse(typeof(HatchStyle), hsComboBox2.SelectedItem.ToString(), true);

                using (HatchBrush hbr2 = new HatchBrush(hs, Color.Black, Color.White))
                {
                    using (TextureBrush tbr = c.TBrush(hbr2))
                    {
                        tbr.ScaleTransform(2.50F,2.50F);
                        e.Graphics.FillRectangle(tbr, new Rectangle(pictureBox2.Location, pictureBox2.Size));
                    }
                }
            }
            if (hsComboBox3.SelectedIndex > 0)
            {
                HatchStyle hs = (HatchStyle)Enum.Parse(typeof(HatchStyle), hsComboBox3.SelectedItem.ToString(), true);

                using (HatchBrush hbr2 = new HatchBrush(hs, Color.Black, Color.White))
                {
                    using (TextureBrush tbr = c.TBrush(hbr2))
                    {
                        tbr.ScaleTransform(2.50F, 2.50F);
                        e.Graphics.FillRectangle(tbr, new Rectangle(pictureBox1.Location, pictureBox1.Size));
                    }
                }
            }

        }
        private void hsComboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (hsComboBox2.SelectedIndex > 0)
            {
                this.Invalidate();
            }
        }
        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count <= 0) return;
            int RowIndex = this.dataGridView1.SelectedRows[0].Index;
            if (RowIndex >= 0)
            {
                editedQualityTextboxTB.Text = dataGridView1.Rows[RowIndex].Cells[0].Value.ToString();
                editHSNNoTB.Text = dataGridView1.Rows[RowIndex].Cells[1].Value.ToString();
                hsComboBox3.SelectedIndex = this.hsComboBox3.FindStringExact(this.dataGridView1.Rows[RowIndex].Cells[2].Value.ToString());
                editQBTCB.SelectedIndex = editQBTCB.FindStringExact(dataGridView1.Rows[RowIndex].Cells[3].Value.ToString());
            }
        }
    }
}
