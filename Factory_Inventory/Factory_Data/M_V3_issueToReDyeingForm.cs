﻿using Factory_Inventory.Factory_Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Factory_Inventory
{
    public partial class M_V3_issueToReDyeingForm : Form
    {
        private DbConnect c = new DbConnect();
        private DataTable dt, all_trays;
        private List<string> batch_nos = new List<string>();
        private M_V2_trayInputForm f;
        public Label dummyLabel;
        private DataRow old_batch_row;
        private M_V_history v1_history;
        private bool edit_form = false;
        private int voucher_id;
        private DataRow redyeing_batch, non_redyeing_batch;
        //Form functions
        public M_V3_issueToReDyeingForm()
        {
            InitializeComponent();
            c = new DbConnect();
            all_trays = new DataTable();
            //Create drop-down Batches lists
            this.dt = c.getBatchFiscalYearWeight_StateDyeingCompanyColourQuality(2, null, null, null);
            List<string> batch_no_arr = new List<string>();
            if(this.dt == null)
            {
                c.ErrorBox("There are no recieved batches", "Error");
                return;
            }
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                batch_no_arr.Add(dt.Rows[i]["Batch_No"].ToString());
            }
            for (int i = 0; i < batch_no_arr.Count; i++)
            {
                batch_no_arr[i] = dt.Rows[i]["Batch_No"].ToString() + "  (" + dt.Rows[i]["Fiscal_Year"].ToString() + ")";
            }
            for (int i = 0; i < batch_no_arr.Count; i++)
            {
                this.batch_nos.Add(batch_no_arr[i]);
            }
            batch_no_arr.Insert(0, "---Select---");
            this.batchNoCB.DataSource = batch_no_arr;
            this.batchNoCB.DisplayMember = "Batches";
            this.batchNoCB.DropDownStyle = ComboBoxStyle.DropDownList;//Create a drop-down list
            this.batchNoCB.AutoCompleteSource = AutoCompleteSource.ListItems;
            this.batchNoCB.AutoCompleteMode = AutoCompleteMode.SuggestAppend;

            c.set_dgv_column_sort_state(this.dataGridView1, DataGridViewColumnSortMode.NotSortable);
            dataGridView1.DefaultCellStyle.SelectionBackColor = Color.White;
            dataGridView1.DefaultCellStyle.SelectionForeColor = Color.Blue;
        }
        public M_V3_issueToReDyeingForm(DataRow row, bool isEditable, M_V_history v1_history)
        {
            InitializeComponent();
            this.edit_form = true;
            this.v1_history = v1_history;
            this.all_trays = new DataTable();
            this.c = new DbConnect();
            this.voucher_id = int.Parse(row["Voucher_ID"].ToString());
            //load data
            this.inputDateDTP.Value= Convert.ToDateTime(row["Date_Of_Input"].ToString());
            this.issueDateDTP.Value = Convert.ToDateTime(row["Date_Of_Issue"].ToString());
            this.batchNoCB.Items.Add("");
            this.batchNoCB.Items.Add(row["Old_Batch_No"].ToString() +"  ("+ row["Old_Batch_Fiscal_Year"].ToString()+")");
            this.batchNoCB.SelectedIndex = 1;
            load_batch();

            this.redyeing_batch = c.getBatchRow_BatchNo(int.Parse(row["Redyeing_Batch_No"].ToString()), row["Redyeing_Batch_Fiscal_Year"].ToString());
            this.non_redyeing_batch = null;
            float non_redyeing_wt = -1F;
            string non_redyeing_batch_no = "NA";
            
            if (row["Non_Redyeing_Batch_No"].ToString() != "-1")
            {
                non_redyeing_batch = c.getBatchRow_BatchNo(int.Parse(row["Non_Redyeing_Batch_No"].ToString()), row["Old_Batch_Fiscal_Year"].ToString());
                non_redyeing_wt= float.Parse(batchWeightTB.Text) - float.Parse(redyeing_batch["Net_Weight"].ToString());
                non_redyeing_batch_no= row["Non_Redyeing_Batch_No"].ToString();
            }
            else
            {
                this.fullRedyeCK.Checked = true;
            }
            this.redyeingBatchNoTB.Text = row["Redyeing_Batch_No"].ToString();
            this.nonRedyeingBatchNoTB.Text = non_redyeing_batch_no;
            
            this.redyeingBatchWeightTB.Text = float.Parse(redyeing_batch["Net_Weight"].ToString()).ToString("F3");
            if (non_redyeing_wt != -1F) this.nonRedyeingBatchWeightTB.Text = non_redyeing_wt.ToString("F3");
            else this.nonRedyeingBatchWeightTB.Text = "NA";

            loadButton_Click(null, null);
            this.redyeingColourCB.SelectedIndex = this.redyeingColourCB.FindStringExact(redyeing_batch["Colour"].ToString());
            this.rateTextBoxTB.Text = redyeing_batch["Dyeing_Rate"].ToString();
            
            this.all_trays.Columns.Add("Sl No");
            this.all_trays.Columns.Add("Date Of Production");
            this.all_trays.Columns.Add("Tray No");
            this.all_trays.Columns.Add("Spring");
            this.all_trays.Columns.Add("No Of Springs");
            this.all_trays.Columns.Add("Tray Tare");
            this.all_trays.Columns.Add("Gross Weight");
            this.all_trays.Columns.Add("Quality");
            this.all_trays.Columns.Add("Company Name");
            this.all_trays.Columns.Add("Machine No");
            this.all_trays.Columns.Add("Net Weight");
            this.all_trays.Columns.Add("Quality Before Twist");
            this.all_trays.Columns.Add("Grade");
            this.all_trays.Columns.Add("Redyeing");
            this.all_trays.Columns.Add("No Of Springs RD");


            string[] tray_ids = c.csvToArray(redyeing_batch["Tray_ID_Arr"].ToString());
            for(int i=0;i<tray_ids.Length;i++)
            {
                DataRow tray = c.getTrayRow_TrayID(int.Parse(tray_ids[i]));
                DataRow to_add = this.all_trays.NewRow();
                to_add["Sl No"] = i + 1;
                to_add["Date Of Production"] = tray["Tray_Production_Date"].ToString();
                to_add["Tray No"] = tray["Tray_No"].ToString();
                to_add["Spring"] = tray["Spring"].ToString();
                to_add["No Of Springs"] = tray["Number_of_Springs"].ToString();
                to_add["Tray Tare"] = tray["Tray_Tare"].ToString();
                to_add["Gross Weight"] = tray["Gross_Weight"].ToString();
                to_add["Quality"] = tray["Quality"].ToString();
                to_add["Company Name"] = tray["Company_Name"].ToString();
                to_add["Machine No"] = tray["Machine_No"].ToString();
                to_add["Net Weight"] = tray["Net_Weight"].ToString();
                to_add["Net Weight"] = tray["Net_Weight"].ToString();
                to_add["Quality Before Twist"] = tray["Quality_Before_Twist"].ToString();
                to_add["Grade"] = tray["Grade"].ToString();
                to_add["Redyeing"] = tray["Redyeing"].ToString();
                to_add["No Of Springs RD"] = tray["No_Of_Springs_RD"].ToString();
                this.all_trays.Rows.Add(to_add);
            }

            this.dataGridView1.DataSource = this.all_trays;
            this.dataGridView1.Columns.OfType<DataGridViewColumn>().ToList().ForEach(col => col.Visible = false);
            this.dataGridView1.Columns["Sl No"].Visible = true;
            this.dataGridView1.Columns["Sl No"].DisplayIndex = 0;
            this.dataGridView1.Columns["Sl No"].Width = 80;
            this.dataGridView1.Columns["Tray No"].Visible = true;
            this.dataGridView1.Columns["Tray No"].HeaderText = "Tray No";
            this.dataGridView1.Columns["Tray No"].DisplayIndex = 2;
            this.dataGridView1.Columns["Quality"].Visible = true;
            this.dataGridView1.Columns["Quality"].DisplayIndex = 4;
            this.dataGridView1.Columns["Quality"].Width = 150;
            this.dataGridView1.Columns["Net Weight"].Visible = true;
            this.dataGridView1.Columns["Net Weight"].DisplayIndex = 6;
            this.dataGridView1.Columns["No Of Springs RD"].Visible = true;
            this.dataGridView1.Columns["No Of Springs RD"].DisplayIndex = 8;
            this.dataGridView1.Columns["No Of Springs RD"].HeaderText = "No of redyeing springs";
            this.dataGridView1.Columns["No of Springs"].Visible = true;
            this.dataGridView1.Columns["No of Springs"].DisplayIndex = 10;
            c.auto_adjust_dgv(this.dataGridView1);
            this.CellSum();

            if (this.dataGridView1.SelectedRows.Count > 0)
            {
                this.dataGridView1.SelectedRows[0].Selected = false;
            }

            if (isEditable == false)
            {
                this.Text += "(View Only)";
                this.disable_form_edit();
                this.deleteButton.Visible = true;
                this.deleteButton.Enabled = true;
            }
            else
            {
                this.Text += "(Edit)";
            }

            if (int.Parse(redyeing_batch["Batch_State"].ToString()) > 1)
            {
                string prefix = "This voucher is not editable/deletable as batch " + this.redyeingBatchNoTB.Text + " has ";
                int state = int.Parse(redyeing_batch["Batch_State"].ToString());
                if(state == 2)
                {
                    this.label7.Text = prefix + "already come from redyeing";
                }
                else if(state == 3)
                {
                    if(string.IsNullOrEmpty(redyeing_batch["Date_OF_Production"].ToString()))
                    {
                        this.label7.Text = prefix + "already come from redyeing and is in production";
                    }
                    else
                    {
                        this.label7.Text = prefix + "already come from redyeing and has been produced";
                    }
                }
                else if(state==4)
                {
                    this.label7.Text = prefix + "gone to redyeing again";
                }
                this.label7.ForeColor = Color.Red;
                this.deleteButton.Visible = true;
                this.deleteButton.Enabled = false;
                disable_form_edit();
            }
            if(non_redyeing_batch!=null)
            {
                if (int.Parse(non_redyeing_batch["Batch_State"].ToString()) > 2)
                {
                    string prefix = "This voucher is not editable/deletable as batch " + this.nonRedyeingBatchNoTB.Text + " has ";
                    int state = int.Parse(non_redyeing_batch["Batch_State"].ToString());
                    if (state == 3)
                    {
                        if (string.IsNullOrEmpty(non_redyeing_batch["Date_OF_Production"].ToString()))
                        {
                            this.label7.Text = prefix + "already come from redyeing and is in production";
                        }
                        else
                        {
                            this.label7.Text = prefix + "already come from redyeing and has been produced";
                        }
                    }
                    else if (state == 4)
                    {
                        this.label7.Text = prefix + "gone to redyeing again";
                    }
                    this.label7.ForeColor = Color.Red;
                    this.deleteButton.Visible = true;
                    this.deleteButton.Enabled = false;
                    disable_form_edit();
                }
            }
        }
        private void M_V3_issueToReDyeingForm_Load(object sender, EventArgs e)
        {
            if (Global.access == 2) this.deleteButton.Visible = false;
            this.Location = new System.Drawing.Point(Screen.PrimaryScreen.Bounds.Width/16, Screen.PrimaryScreen.Bounds.Height/8);


            var comboBoxes = this.Controls
                 .OfType<ComboBox>()
                 .Where(x => x.Name.EndsWith("CB"));

            foreach (var cmbBox in comboBoxes)
            {
                c.comboBoxEvent(cmbBox);
            }

            var textBoxes = this.Controls
                  .OfType<TextBox>()
                  .Where(x => x.Name.EndsWith("TB"));

            foreach (var txtBox in textBoxes)
            {
                c.textBoxEvent(txtBox);
            }

            var dtps = this.Controls
                  .OfType<DateTimePicker>()
                  .Where(x => x.Name.EndsWith("DTP"));

            foreach (var dtp in dtps)
            {
                c.DTPEvent(dtp);
            }

            var buttons = this.Controls
                  .OfType<Button>()
                  .Where(x => x.Name.EndsWith("Button"));

            foreach (var button in buttons)
            {
                Console.WriteLine(button.Name);
                c.buttonEvent(button);
            }

            this.issueDateDTP.Focus();
            if (Global.access == 2)
            {
                this.deleteButton.Visible = false;
            }
        }

        //Own functions
        private void load_batch()
        {
            int index = this.batchNoCB.SelectedIndex;
            string[] batch_details = c.repeated_batch_csv(this.batchNoCB.Text);
            int batchno = int.Parse(batch_details[0]);
            string fiscal_year = batch_details[1];
            this.old_batch_row = c.getBatchRow_BatchNo(batchno, fiscal_year);
            this.qualityTB.Text = this.old_batch_row["Quality"].ToString();
            this.colourTB.Text = this.old_batch_row["Colour"].ToString();
            this.batchWeightTB.Text = this.old_batch_row["Net_Weight"].ToString();
            this.companyNameTB.Text = this.old_batch_row["Company_Name"].ToString();
            this.dyeingCompanyNameTB.Text = this.old_batch_row["Dyeing_Company_Name"].ToString();
        }
        private void disable_form_edit()
        {
            this.redyeingColourCB.Enabled = false;
            this.rateTextBoxTB.Enabled = false;
            this.addTrayButton.Enabled = false;
            this.editTrayButton.Enabled = false;
            this.saveButton.Enabled = false;
            this.dataGridView1.ReadOnly = true;
            this.addTrayButton.Enabled = false;
            this.editTrayButton.Enabled = false;
            this.saveButton.Enabled = false;
            this.deleteToolStripMenuItem.Enabled = false;
            this.issueDateDTP.Enabled = false;
        }
        public void CellSum()
        {
            float sum_pure = 0;
            float sum_net= 0;
            try
            {
                if (dataGridView1.Rows.Count == 0)
                {
                    this.redyeingBatchWeightTB.Text = "0";
                    this.pureRDWeightTB.Text = "0";
                    this.nonRedyeingBatchWeightTB.Text = (float.Parse(this.batchWeightTB.Text) - float.Parse(this.pureRDWeightTB.Text)).ToString("F3");
                    return;
                }
                for (int i = 0; i < dataGridView1.Rows.Count; ++i)
                {
                    if (c.Cell_Not_NullOrEmpty(this.dataGridView1, i, 3) == true)
                    {
                        sum_pure += float.Parse(dataGridView1.Rows[i].Cells["Net Weight"].Value.ToString()) * float.Parse(dataGridView1.Rows[i].Cells["Redyeing"].Value.ToString()) / 100F;
                        sum_net += float.Parse(dataGridView1.Rows[i].Cells["Net Weight"].Value.ToString());
                    }
                }
                this.pureRDWeightTB.Text = sum_pure.ToString("F3");
                this.redyeingBatchWeightTB.Text = sum_net.ToString("F3");
                if(this.fullRedyeCK.Checked == false)
                {
                    this.nonRedyeingBatchWeightTB.Text = (float.Parse(this.batchWeightTB.Text) - float.Parse(this.pureRDWeightTB.Text)).ToString("F3");
                }
                else
                {
                    this.nonRedyeingBatchWeightTB.Text = "0";
                }
            }
            catch
            {
                return;
            }
        }


        //Clicks or index changed
        private void loadButton_Click(object sender, EventArgs e)
        {
            Console.WriteLine("Load button clicked");
            if(this.batchNoCB.SelectedIndex==0)
            {
                c.ErrorBox("Please Select Batch Number", "Error");
                return;
            }
            load_batch();
            string fiscal_year = c.getFinancialYear(this.issueDateDTP.Value);
            if (this.edit_form==false)
            {
                this.redyeingBatchNoTB.Text = c.getNextNumber_FiscalYear("Highest_Batch_No", c.getFinancialYear(this.issueDateDTP.Value));
                if (fiscal_year == c.repeated_batch_csv(this.batchNoCB.Text)[1])
                {
                    this.nonRedyeingBatchNoTB.Text = (int.Parse(this.redyeingBatchNoTB.Text) + 1).ToString();
                }
                else
                {
                    this.nonRedyeingBatchNoTB.Text = c.getNextNumber_FiscalYear("Highest_Batch_No", c.repeated_batch_csv(this.batchNoCB.Text)[1]);
                }
            }
            this.redyeingColourCB.Enabled = true;
            this.rateTextBoxTB.ReadOnly = false;
            this.saveButton.Enabled = true;
            this.addTrayButton.Enabled = true;
            this.editTrayButton.Enabled = true;
            this.redyeingBatchWeightTB.Text = "0";
            this.nonRedyeingBatchWeightTB.Text = (float.Parse(this.batchWeightTB.Text) - float.Parse(this.redyeingBatchWeightTB.Text)).ToString("F3");
            //Create drop-down Colour list
            var dataSource1 = new List<string>();
            DataTable d = c.getQC('l');
            dataSource1.Add("---Select---");
            for (int i = 0; i < d.Rows.Count; i++)
            {
                dataSource1.Add(d.Rows[i][0].ToString());
            }
            List<string> final_list = dataSource1.Distinct().ToList();
            this.redyeingColourCB.DataSource = final_list;
            this.redyeingColourCB.DisplayMember = "Colour";
            this.redyeingColourCB.DropDownStyle = ComboBoxStyle.DropDownList;//Create a drop-down list
            this.redyeingColourCB.AutoCompleteSource = AutoCompleteSource.ListItems;
            this.redyeingColourCB.AutoCompleteMode = AutoCompleteMode.SuggestAppend;

            List<int> years = c.getFinancialYearArr(fiscal_year);
            this.issueDateDTP.MinDate = new DateTime(years[0], 04, 01);
            this.issueDateDTP.MaxDate = new DateTime(years[1], 03, 31);

            this.batchNoCB.Enabled = false;
            this.loadButton.Enabled = false;
            this.fullRedyeCK.Enabled = false;

            if(this.fullRedyeCK.Checked == true)
            {
                this.nonRedyeingBatchNoTB.Text = "NA";
                this.nonRedyeingBatchWeightTB.Text = "0";
            }
        }
        private void addTrayButton_Click(object sender, EventArgs e)
        {
            f = new M_V2_trayInputForm(this.issueDateDTP.Value.Date.ToString().Substring(0,10), null, "135", -1, -1F, -1F, this.qualityTB.Text, this.companyNameTB.Text, null, this.old_batch_row["Grade"].ToString(), -1F, -1, -1, this);
            Point global_loc = Global.background.PointToScreen(Global.background.ClientRectangle.Location);
            f.Location = new System.Drawing.Point(global_loc.X+ this.Location.X +this.Width, global_loc.Y+ this.Location.Y+30);
            f.ShowDialog();
            this.all_trays = f.tray_details;
            
        }
        private void editTrayButton_Click(object sender, EventArgs e)
        {
            if (this.dataGridView1.SelectedRows.Count == 0)
            {
                return;
            }
            DataRow row = this.all_trays.Rows[this.dataGridView1.CurrentRow.Index];
            f = new M_V2_trayInputForm(row["Date Of Production"].ToString(), row["Tray No"].ToString(), row["Spring"].ToString(), int.Parse(row["No Of Springs"].ToString()), float.Parse(row["Tray Tare"].ToString()), float.Parse(row["Gross Weight"].ToString()), row["Quality"].ToString(), row["Company Name"].ToString(), row["Machine No"].ToString(), row["Grade"].ToString(), float.Parse(row["Redyeing"].ToString()), int.Parse(row["No Of Springs RD"].ToString()), this.dataGridView1.CurrentRow.Index, this);
            Point global_loc = Global.background.PointToScreen(Global.background.ClientRectangle.Location);
            f.Location = new System.Drawing.Point(global_loc.X + this.Location.X + this.Width, global_loc.Y + this.Location.Y + 30);
            f.ShowDialog();
            this.all_trays = f.tray_details;
        }
        private void saveButton_Click(object sender, EventArgs e)
        {
            if (c.check_login_val() == false) return;
            //checks
            if (this.dataGridView1.Rows.Count==0)
            {
                c.ErrorBox("Please enter atleast one tray");
                return;
            }
            if (float.Parse(this.pureRDWeightTB.Text) > float.Parse(this.batchWeightTB.Text))
            {
                c.ErrorBox("Redyeing weight of batch being sent for redyeing is greater than original batch weight");
                return;
            }
            if (redyeingColourCB.SelectedIndex == 0)
            {
                c.ErrorBox("Please select redyeing colour");
                return;
            }
            try
            {
                float.Parse(this.rateTextBoxTB.Text);
            }
            catch
            {
                c.ErrorBox("Please enter numeric dyeing rate");
                return;
            }
            DateTime inward_date = Convert.ToDateTime(this.old_batch_row["Dyeing_In_Date"].ToString());
            if (inward_date > this.issueDateDTP.Value.Date)
            {
                c.ErrorBox("Redyeing issue date should be more or equal to than batch dyeing inward date (" + inward_date.Date.ToString("dd-MM-yyyy").Substring(0, 10) + ")");
                return;
            }
            bool full = false;
            int nrd_bno;
            if(this.fullRedyeCK.Checked == true)
            {
                if(float.Parse(pureRDWeightTB.Text) != float.Parse(batchWeightTB.Text))
                {
                    c.ErrorBox("Pure redyeing weight sould be equal to old batch weight for full redyeing");
                    return;
                }
                full = true;
                nrd_bno = -1;
            }
            else
            {
                nrd_bno = int.Parse(this.nonRedyeingBatchNoTB.Text);
            }
            
            if (this.edit_form == false)
            {
                bool added;
                added = c.addRedyeingVoucher(this.inputDateDTP.Value, this.issueDateDTP.Value, this.old_batch_row, nrd_bno, int.Parse(this.redyeingBatchNoTB.Text), float.Parse(nonRedyeingBatchWeightTB.Text), float.Parse(redyeingBatchWeightTB.Text), c.getFinancialYear(this.issueDateDTP.Value), this.dataGridView1.DataSource as DataTable, this.redyeingColourCB.Text, float.Parse(this.rateTextBoxTB.Text), full);
                if (added == true)
                {
                    dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = Color.LawnGreen;
                    this.disable_form_edit();
                }
            }
            else
            {
                string redyeing = this.old_batch_row["Batch_No"].ToString() + "," + this.old_batch_row["Fiscal_Year"].ToString();
                bool edited = c.editRedyeingVoucher(this.issueDateDTP.Value, this.old_batch_row, this.dataGridView1.DataSource as DataTable, this.redyeingColourCB.Text, float.Parse(this.rateTextBoxTB.Text), this.redyeing_batch, this.non_redyeing_batch, float.Parse(this.nonRedyeingBatchWeightTB.Text), float.Parse(this.redyeingBatchWeightTB.Text), full);
                if (edited == true)
                {
                    dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = Color.LawnGreen;
                    this.disable_form_edit();
                }
            }
            dataGridView1.EnableHeadersVisualStyles = false;
        }
        private void deleteButton_Click(object sender, EventArgs e)
        {
            if (c.check_login_val() == false) return;
            DialogResult dialogResult = MessageBox.Show("Confirm Delete", "Message", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dialogResult == DialogResult.Yes)
            {
                bool deleted = c.deleteRedyeingVoucher(this.voucher_id, this.redyeing_batch);
                if (deleted == true)
                {
                    c.SuccessBox("Voucher Deleted Successfully");
                    this.deleteButton.Enabled = false;
                    this.v1_history.loadData();
                }
                else return;
            }
            else if (dialogResult == DialogResult.No)
            {
                return;
            }
        }
        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int count = dataGridView1.SelectedRows.Count;
            for (int i = 0; i < count; i++)
            {
                dataGridView1.Rows.RemoveAt(dataGridView1.SelectedRows[0].Index);

            }
            CellSum();
        }
        private void redyeingColourCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.redyeingColourCB.SelectedIndex != 0)
            {
                float rate = c.getDyeingRate(this.redyeingColourCB.Text, this.qualityTB.Text);
                if (rate != -1F)
                {
                    this.rateTextBoxTB.Text = rate.ToString();
                }
            }
            else
            {
                this.rateTextBoxTB.Text = "";
            }
        }
        private void label8_Click(object sender, EventArgs e)
        {

        }
        private void pureRDWeightTB_TextChanged(object sender, EventArgs e)
        {

        }

        //DataGridView 1
        private void dataGridView1_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            if (e.RowIndex != dataGridView1.Rows.Count - 1)
            {
                //dataGridView1.Rows[e.RowIndex].Cells[0].Value = e.RowIndex + 1;
            }
        }
    }
}
