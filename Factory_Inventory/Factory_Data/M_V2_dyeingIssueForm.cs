﻿using Factory_Inventory.Factory_Classes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Factory_Inventory
{
    public partial class M_V2_dyeingIssueForm : Form
    {
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Tab &&
                dataGridView1.EditingControl != null &&
                //msg.HWnd == dataGridView1.EditingControl.Handle &&
                dataGridView1.SelectedCells
                    .Cast<DataGridViewCell>()
                    .Any(x => x.ColumnIndex == 1))
            {
                this.edit_cmd_send = true;
                Console.WriteLine("Sending process tab");
                SendKeys.Send("{Tab}");
                return false;
            }
            if (keyData == Keys.F2)
            {
                Console.WriteLine("dgv1");
                this.dataGridView1.Focus();
                this.ActiveControl = dataGridView1;
                this.dataGridView1.CurrentCell = dataGridView1[0, 0];
                return false;
            }
            if (keyData == Keys.F3)
            {
                Console.WriteLine("cb");
                this.comboBox4CB.Focus();
                this.ActiveControl = comboBox4CB;
                return false;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
        private DbConnect c;
        private bool edit_cmd_send = false;
        private bool edit_form = false;
        private List<string> tray_details;  //Tray_No (TrayID)
        private string[] tray_no_this, tray_id_this;
        private M_V_history v1_history;
        private int voucherID;
        private int batch_state;
        private string old_fiscal_year;
        struct fetch_data
        {
            public float net_wt;
            public string mno;
            public string grade;
            public fetch_data(float net_wt, string mno, string grade)
            {
                this.net_wt = net_wt;
                this.mno = mno;
                this.grade = grade;
            }
        }
        Dictionary<string, fetch_data> tray_fetch_data = new Dictionary<string, fetch_data>(); //"Tray_No (Tray_ID)" -> fetch_data

        public M_V2_dyeingIssueForm()
        {
            InitializeComponent();
            this.c = new DbConnect();
            this.tray_details = new List<string>();
            this.tray_details.Add("");
            this.saveButton.Enabled = false;

            //Create drop-down Quality lists
            var dataSource1 = new List<string>();
            DataTable d1 = c.getQC('q');
            dataSource1.Add("---Select---");

            for (int i = 0; i < d1.Rows.Count; i++)
            {
                dataSource1.Add(d1.Rows[i][0].ToString());
            }
            this.comboBox1CB.DataSource = dataSource1;
            this.comboBox1CB.DisplayMember = "Quality";
            this.comboBox1CB.DropDownStyle = ComboBoxStyle.DropDownList;//Create a drop-down list
            this.comboBox1CB.AutoCompleteSource = AutoCompleteSource.ListItems;
            this.comboBox1CB.AutoCompleteMode = AutoCompleteMode.SuggestAppend;


            //Create drop-down Company lists
            var dataSource2 = new List<string>();
            DataTable d2 = c.getQC('c');
            dataSource2.Add("---Select---");

            for (int i = 0; i < d2.Rows.Count; i++)
            {
                dataSource2.Add(d2.Rows[i][0].ToString());
            }
            this.comboBox2CB.DataSource = dataSource2;
            this.comboBox2CB.DisplayMember = "Company_Names";
            this.comboBox2CB.DropDownStyle = ComboBoxStyle.DropDownList;//Create a drop-down list
            this.comboBox2CB.AutoCompleteSource = AutoCompleteSource.ListItems;
            this.comboBox2CB.AutoCompleteMode = AutoCompleteMode.SuggestAppend;



            //Create drop-down Dyeing Company lists
            var dataSource3 = new List<string>();
            DataTable d3 = c.getQC('d');
            dataSource3.Add("---Select---");

            for (int i = 0; i < d3.Rows.Count; i++)
            {
                dataSource3.Add(d3.Rows[i][0].ToString());
            }
            this.comboBox3CB.DataSource = dataSource3;
            this.comboBox3CB.DisplayMember = "Dyeing_Company_Names";
            this.comboBox3CB.DropDownStyle = ComboBoxStyle.DropDownList;//Create a drop-down list
            this.comboBox3CB.AutoCompleteSource = AutoCompleteSource.ListItems;
            this.comboBox3CB.AutoCompleteMode = AutoCompleteMode.SuggestAppend;


            //Create drop-down Colour lists
            var dataSource4 = new List<string>();
            DataTable d4 = c.getQC('l');
            dataSource4.Add("---Select---");

            for (int i = 0; i < d4.Rows.Count; i++)
            {
                dataSource4.Add(d4.Rows[i][0].ToString());
            }
            List<string> final_list = dataSource4.Distinct().ToList();
            this.comboBox4CB.DataSource = final_list;
            this.comboBox4CB.DisplayMember = "Colours";
            this.comboBox4CB.DropDownStyle = ComboBoxStyle.DropDownList;//Create a drop-down list
            this.comboBox4CB.AutoCompleteSource = AutoCompleteSource.ListItems;
            this.comboBox4CB.AutoCompleteMode = AutoCompleteMode.SuggestAppend;


            //DatagridView
            dataGridView1.Columns.Add("Sl_No", "Sl_No");
            dataGridView1.Columns[0].ReadOnly = true;
            DataGridViewComboBoxColumn dgvCmb = new DataGridViewComboBoxColumn();
            dgvCmb.DataSource = this.tray_details;

            dgvCmb.HeaderText = "Tray Number";
            dataGridView1.Columns.Insert(1, dgvCmb);
            dataGridView1.Columns[1].Width = 250;
            dataGridView1.Columns.Add("Weight", "Weight");
            dataGridView1.Columns[2].ReadOnly = true;
            dataGridView1.Columns.Add("Machine_Number", "Machine Number");
            dataGridView1.Columns[3].ReadOnly = true;
            dataGridView1.Columns.Add("Grade", "Grade");
            dataGridView1.Columns[4].ReadOnly = true;
            dataGridView1.Columns.Add("Tray_ID", "Tray ID");
            dataGridView1.Columns[5].Visible = false;
            dataGridView1.RowCount = 10;

            c.set_dgv_column_sort_state(this.dataGridView1, DataGridViewColumnSortMode.NotSortable);

        }
        public M_V2_dyeingIssueForm(DataRow row, bool isEditable, M_V_history v1_history)
        {

            InitializeComponent();
            this.edit_form = true;
            this.v1_history = v1_history;
            this.c = new DbConnect();
            this.tray_details = new List<string>();
            this.tray_details.Add("");
            this.saveButton.Enabled = false;

            //Create drop-down Quality lists
            var dataSource1 = new List<string>();
            DataTable d1 = c.getQC('q');
            dataSource1.Add("---Select---");

            for (int i = 0; i < d1.Rows.Count; i++)
            {
                dataSource1.Add(d1.Rows[i][0].ToString());
            }
            this.comboBox1CB.DataSource = dataSource1;
            this.comboBox1CB.DisplayMember = "Quality";
            this.comboBox1CB.DropDownStyle = ComboBoxStyle.DropDownList;//Create a drop-down list
            this.comboBox1CB.AutoCompleteSource = AutoCompleteSource.ListItems;
            this.comboBox1CB.AutoCompleteMode = AutoCompleteMode.SuggestAppend;


            //Create drop-down Company lists
            var dataSource2 = new List<string>();
            DataTable d2 = c.getQC('c');
            dataSource2.Add("---Select---");
            for (int i = 0; i < d2.Rows.Count; i++)
            {
                dataSource2.Add(d2.Rows[i][0].ToString());
            }
            this.comboBox2CB.DataSource = dataSource2;
            this.comboBox2CB.DisplayMember = "Company_Names";
            this.comboBox2CB.DropDownStyle = ComboBoxStyle.DropDownList;//Create a drop-down list
            this.comboBox2CB.AutoCompleteSource = AutoCompleteSource.ListItems;
            this.comboBox2CB.AutoCompleteMode = AutoCompleteMode.SuggestAppend;



            //Create drop-down Dyeing Company lists
            var dataSource3 = new List<string>();
            DataTable d3 = c.getQC('d');
            dataSource3.Add("---Select---");

            for (int i = 0; i < d3.Rows.Count; i++)
            {
                dataSource3.Add(d3.Rows[i][0].ToString());
            }
            this.comboBox3CB.DataSource = dataSource3;
            this.comboBox3CB.DisplayMember = "Dyeing_Company_Names";
            this.comboBox3CB.DropDownStyle = ComboBoxStyle.DropDownList;//Create a drop-down list
            this.comboBox3CB.AutoCompleteSource = AutoCompleteSource.ListItems;
            this.comboBox3CB.AutoCompleteMode = AutoCompleteMode.SuggestAppend;


            //Create drop-down Colour lists
            var dataSource4 = new List<string>();
            DataTable d4 = c.getQC('l');
            dataSource4.Add("---Select---");

            for (int i = 0; i < d4.Rows.Count; i++)
            {
                dataSource4.Add(d4.Rows[i][0].ToString());
            }
            List<string> final_list = dataSource4.Distinct().ToList();
            this.comboBox4CB.DataSource = final_list;
            this.comboBox4CB.DisplayMember = "Colours";
            this.comboBox4CB.DropDownStyle = ComboBoxStyle.DropDownList;//Create a drop-down list
            this.comboBox4CB.AutoCompleteSource = AutoCompleteSource.ListItems;
            this.comboBox4CB.AutoCompleteMode = AutoCompleteMode.SuggestAppend;


            //DatagridView
            dataGridView1.Columns.Add("Sl_No", "Sl_No");
            dataGridView1.Columns[0].ReadOnly = true;
            DataGridViewComboBoxColumn dgvCmb = new DataGridViewComboBoxColumn();
            dgvCmb.DataSource = this.tray_details;

            dgvCmb.HeaderText = "Tray Number";
            dataGridView1.Columns.Insert(1, dgvCmb);
            dataGridView1.Columns[1].Width = 250;
            dataGridView1.Columns.Add("Weight", "Weight");
            dataGridView1.Columns["Weight"].ReadOnly = true;
            dataGridView1.Columns.Add("Machine_Number", "Machine Number");
            dataGridView1.Columns["Machine_Number"].ReadOnly = true;
            dataGridView1.Columns.Add("Grade", "Grade");
            dataGridView1.Columns[4].ReadOnly = true;
            dataGridView1.Columns.Add("Tray_ID", "Tray ID");
            dataGridView1.Columns[5].Visible = false;
            dataGridView1.RowCount = 10;

            bool invalid_edit= false;
            //make isEditable false if batch is in future states
            this.batch_state = c.getBatchState(int.Parse(row["Batch_No"].ToString()), row["Batch_Fiscal_Year"].ToString());
            if (batch_state == 2)
            {
                dynamicEditableLabel.Text = "This voucher is not editable as the Batch has been recieved from dyeing";
                invalid_edit= true;
            }
            else if (batch_state == 3)
            {
                dynamicEditableLabel.Text = "This voucher is not editable as the Batch has been packed";
                invalid_edit= true;
            }
            else if (batch_state == 4)
            {
                dynamicEditableLabel.Text = "This voucher is not editable as the Batch has been split for redyeing";
                invalid_edit= true;
            }
            if (isEditable == false)
            {
                if (invalid_edit == true) this.deleteButton.Enabled = false; 
                else this.deleteButton.Enabled = true;
                this.deleteButton.Visible = true;
            }
            if (isEditable == false || invalid_edit==true)
            {
                this.Text += "(View Only)";
                this.disable_form_edit();
            }
            else
            { 
                //no option to edit company name and quality
                this.Text += "(Edit)";
                this.comboBox1CB.Enabled = false;
                this.comboBox2CB.Enabled = false;
                this.comboBox3CB.Enabled = true;
                this.comboBox4CB.Enabled = true;
                this.saveButton.Enabled = true;
                this.loadCartonButton.Enabled = false;
                this.dataGridView1.ReadOnly = false;
            }
            this.inputDateDTP.Value = Convert.ToDateTime(row["Date_Of_Input"].ToString());
            this.issueDateDTP.Value = Convert.ToDateTime(row["Date_Of_Issue"].ToString());
            this.comboBox1CB.SelectedIndex = this.comboBox1CB.FindStringExact(row["Quality"].ToString());
            this.comboBox2CB.SelectedIndex = this.comboBox2CB.FindStringExact(row["Company_Name"].ToString());
            this.comboBox3CB.SelectedIndex = this.comboBox3CB.FindStringExact(row["Dyeing_Company_Name"].ToString());
            this.comboBox4CB.SelectedIndex = this.comboBox4CB.FindStringExact(row["Colour"].ToString());
            this.rateTextBoxTB.Text = row["Dyeing_Rate"].ToString();
            this.old_fiscal_year = row["Batch_Fiscal_Year"].ToString();
            batchNumberTextboxTB.Text = row["Batch_No"].ToString();

            this.voucherID = int.Parse(row["Voucher_ID"].ToString());
            this.tray_no_this = c.csvToArray(row["Tray_No_Arr"].ToString());
            this.tray_id_this = c.csvToArray(row["Tray_ID_Arr"].ToString());
            
            for (int i=0; i< tray_no_this.Length; i++)
            {
                this.tray_details.Add(tray_no_this[i] + " (" + tray_id_this[i] + ")");
            }
            DataTable tray_data = c.getTrayDataBothTables("Tray_No, Net_Weight, Machine_No, Grade, Tray_ID", "Tray_ID IN (" + c.removecom(row["Tray_ID_Arr"].ToString()) + ")");
            for (int i = 0; i < tray_data.Rows.Count; i++)
            {
                this.tray_fetch_data[tray_data.Rows[i]["Tray_No"].ToString() + " (" + tray_data.Rows[i]["Tray_ID"].ToString() + ")"] = new fetch_data(float.Parse(tray_data.Rows[i]["Net_Weight"].ToString()), tray_data.Rows[i]["Machine_No"].ToString(), tray_data.Rows[i]["Grade"].ToString());
            }
            this.loadData(row["Quality"].ToString(), row["Company_Name"].ToString());
            dataGridView1.RowCount = tray_no_this.Length + 1;

            for (int i = 0; i < tray_no_this.Length; i++)
            {
                dataGridView1.Rows[i].Cells[1].Value = tray_no_this[i] + " (" + tray_id_this[i] + ")";
            }

            string voucher_fiscal_year = c.getFinancialYear(this.issueDateDTP.Value);
            string today_fiscal_year = c.getFinancialYear(DateTime.Now);
            List<int> years = c.getFinancialYearArr(voucher_fiscal_year);
            if (today_fiscal_year == voucher_fiscal_year)
            {
                this.issueDateDTP.MinDate = new DateTime(years[0], 04, 01);
                this.issueDateDTP.MaxDate = DateTime.Now;
            }
            else
            {
                this.issueDateDTP.MinDate = new DateTime(years[0], 04, 01);
                this.issueDateDTP.MaxDate = new DateTime(years[1], 03, 31);
            }

            c.set_dgv_column_sort_state(this.dataGridView1, DataGridViewColumnSortMode.NotSortable);
        }
        private void M_V2_dyeingIssueForm_Load(object sender, EventArgs e)
        {
            if (Global.access == 2) this.deleteButton.Visible = false;
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

        //Own Functions
        public void disable_form_edit()
        {
            this.inputDateDTP.Enabled = false;
            this.issueDateDTP.Enabled = false;
            this.comboBox1CB.Enabled = false;
            this.comboBox2CB.Enabled = false;
            this.comboBox3CB.Enabled = false;
            this.comboBox4CB.Enabled = false;
            this.loadCartonButton.Enabled = false;
            this.saveButton.Enabled = false;
            this.dataGridView1.ReadOnly = true;
            this.rateTextBoxTB.Enabled = false;
            this.deleteToolStripMenuItem.Enabled = false;
        }
        private float CellSum()
        {
            float sum = 0;
            try
            {
                if (dataGridView1.Rows.Count == 0)
                {
                    return sum;
                }
                for (int i = 0; i < dataGridView1.Rows.Count; ++i)
                {
                    if (dataGridView1.Rows[i].Cells[2].Value != null)
                        sum += float.Parse(dataGridView1.Rows[i].Cells[2].Value.ToString());
                }
                return sum;
            }
            catch
            {
                return sum;
            }
        }
        private void fillRate()
        {
            if (comboBox1CB.SelectedIndex == 0 || comboBox4CB.SelectedIndex == 0)
            {
                rateTextBoxTB.Text = "";
                return;
            }
            float f = c.getDyeingRate(comboBox4CB.SelectedItem.ToString(), comboBox1CB.SelectedItem.ToString());
            if (f == -1F)
            {
                rateTextBoxTB.Text = "";
                return;
            }
            rateTextBoxTB.Text = (c.getDyeingRate(comboBox4CB.SelectedItem.ToString(), comboBox1CB.SelectedItem.ToString())).ToString();
        }
        private void loadData(string quality, string company)
        {
            DataTable d = c.getTableData("Tray_Active", "Tray_No, Tray_ID, Net_Weight, Machine_No, Grade", "Tray_State = 1 AND Quality = '" + quality + "' AND Company_Name = '" + company + "'");

            //DataTable d = c.getTrayStateQualityCompany(1, quality, company);
            Console.WriteLine(d.Rows.Count);
            for (int i = 0; i < d.Rows.Count; i++)
            {
                string trayno = d.Rows[i]["Tray_No"].ToString();
                int trayid = int.Parse(d.Rows[i]["Tray_ID"].ToString());
                this.tray_details.Add(trayno + " (" + trayid.ToString() + ")");
                this.tray_fetch_data[trayno + " (" + trayid.ToString() + ")"] = new fetch_data(float.Parse(d.Rows[i]["Net_Weight"].ToString()), d.Rows[i]["Machine_No"].ToString(), d.Rows[i]["Grade"].ToString());   
            }
        }

        //Clicks, Index changes
        private void saveButton_Click(object sender, EventArgs e)
        {
            if (c.check_login_val() == false) return;
            //checks
            if (comboBox1CB.SelectedIndex == 0)
            {
                c.ErrorBox("Enter Select Quality", "Error");
                return;
            }
            if (comboBox2CB.SelectedIndex == 0)
            {
                c.ErrorBox("Enter Select Company Name", "Error");
                return;
            }
            if (comboBox3CB.SelectedIndex == 0)
            {
                c.ErrorBox("Enter Select Dyeing Company Name", "Error");
                return;
            }
            if (comboBox4CB.SelectedIndex == 0)
            {
                c.ErrorBox("Enter Select Colour", "Error");
                return;
            }
            if (batchNumberTextboxTB.Text == null || batchNumberTextboxTB.Text == "")
            {
                c.ErrorBox("Enter Batch Number", "Error");
            }
            try
            {
                int.Parse(batchNumberTextboxTB.Text);
            }
            catch
            {
                c.ErrorBox("Enter numeric Batch Number only", "Error");
                return;
            }
            if (dataGridView1.Rows[0].Cells[1].Value == null)
            {
                c.ErrorBox("Please enter Tray Numbers", "Error");
                return;
            }
            try
            {
                float.Parse(rateTextBoxTB.Text);
            }
            catch
            {
                c.ErrorBox("Enter numeric rate only", "Error");
                return;
            }
            if (this.inputDateDTP.Value.Date < this.issueDateDTP.Value.Date)
            {
                c.ErrorBox("Issue Date is in the future", "Error");
                return;
            }
            string trayno = "", trayid = "";
            int number = 0;
            
            string grade = dataGridView1.Rows[0].Cells[4].Value.ToString();
            for(int i=0;i<dataGridView1.Rows.Count-1;i++)
            {
                if (dataGridView1.Rows[i].Cells[1].Value == null || dataGridView1.Rows[i].Cells[1].Value.ToString() == "")
                {
                    continue;
                }
                if (grade != dataGridView1.Rows[i].Cells[4].Value.ToString())
                {
                    c.ErrorBox("All tray grades are not same");
                    return;
                }
            }

            List<string> temp = new List<string>();
            for (int i = 0; i < dataGridView1.Rows.Count - 1; i++)
            {

                //ComboBox c = (ComboBox)dataGridView1.EditingControl;
                if (dataGridView1.Rows[i].Cells[1].Value == null || dataGridView1.Rows[i].Cells[1].Value.ToString() == "")
                {
                    continue;
                }
                else
                {
                    string tray_details = dataGridView1.Rows[i].Cells[1].Value.ToString();
                    string[] split = tray_details.Split(' ');
                    trayno += split[0] + ",";
                    trayid += split[1].Substring(1, split[1].Length - 2) + ',';
                    number++;
                    //to check for all different tray_nos
                    temp.Add(dataGridView1.Rows[i].Cells[1].Value.ToString());
                    var distinctBytes = new HashSet<string>(temp);
                    bool allDifferent = distinctBytes.Count == temp.Count;
                    if (allDifferent == false)
                    {
                        c.ErrorBox("Please Enter Distinct Tray Nos at Row: " + (i + 1).ToString(), "Error");
                        return;
                    }

                }
            }

            if (this.edit_form == false)
            {
                bool added = c.addDyeingIssueVoucher(inputDateDTP.Value, issueDateDTP.Value, comboBox1CB.SelectedItem.ToString(), comboBox2CB.SelectedItem.ToString(), trayno, number, comboBox4CB.SelectedItem.ToString(), comboBox3CB.SelectedItem.ToString(), int.Parse(batchNumberTextboxTB.Text), trayid, CellSum(), float.Parse(rateTextBoxTB.Text), grade);
                if (added == true)
                {
                    dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = Color.LawnGreen;
                    disable_form_edit();
                }
                else return;
            }
            else
            {
                bool edited = c.editDyeingIssueVoucher(this.voucherID, this.old_fiscal_year, inputDateDTP.Value, issueDateDTP.Value, comboBox1CB.SelectedItem.ToString(), comboBox2CB.SelectedItem.ToString(), trayno, number, comboBox4CB.SelectedItem.ToString(), comboBox3CB.SelectedItem.ToString(), int.Parse(batchNumberTextboxTB.Text), trayid, CellSum(), float.Parse(rateTextBoxTB.Text), trayid, grade);
                if (edited == true)
                {
                    dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = Color.LawnGreen;
                    disable_form_edit();
                    this.v1_history.loadData();
                }
                else return;
            }
            dataGridView1.EnableHeadersVisualStyles = false;
        }
        private void loadCartonButton_Click(object sender, EventArgs e)
        {
            if (comboBox1CB.SelectedIndex == 0)
            {
                c.ErrorBox("Enter Select Quality", "Error");
                return;
            }
            if (comboBox2CB.SelectedIndex == 0)
            {
                c.ErrorBox("Enter Select Company Name", "Error");
                return;
            }
            if (this.edit_form == false)
            {
                int batch_no = int.Parse(c.getNextNumber_FiscalYear("Highest_Batch_No", c.getFinancialYear(issueDateDTP.Value)));
                if (batch_no == -1)
                {
                    batchNumberTextboxTB.Text = "Error";
                }
                else
                {
                    batchNumberTextboxTB.Text = batch_no.ToString();
                }
            }
            this.loadData(this.comboBox1CB.SelectedItem.ToString(), this.comboBox2CB.SelectedItem.ToString());
            if (this.tray_details.Count - 1 == 0)
            {
                c.WarningBox("No Trays Loaded");
                return;
            }
            else
            {
                if (this.edit_form == false)
                {
                    c.SuccessBox("Loaded " + (this.tray_details.Count - 1).ToString() + " Trays");
                }
            }
            this.saveButton.Enabled = true;
            this.loadCartonButton.Enabled = false;
            this.comboBox1CB.Enabled = false;
            this.comboBox2CB.Enabled = false;
            this.saveButton.Enabled = true;
            string fiscal_year = c.getFinancialYear(this.issueDateDTP.Value);
            List<int> years = c.getFinancialYearArr(fiscal_year);
            this.issueDateDTP.MinDate = new DateTime(years[0], 04, 01);
            this.issueDateDTP.MaxDate = new DateTime(years[1], 03, 31);
            //this.issueDateDTP.Enabled = false; //because the next batch number is coming from the date
        }
        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int count = dataGridView1.SelectedRows.Count;
            for (int i = 0; i < count; i++)
            {
                if (dataGridView1.SelectedRows[0].Index == dataGridView1.Rows.Count - 1)
                {
                    dataGridView1.SelectedRows[0].Selected = false;
                    continue;
                }
                dataGridView1.Rows.RemoveAt(dataGridView1.SelectedRows[0].Index);
            }
            dynamicWeightLabel.Text = CellSum().ToString("F3");
        }
        private void deleteButton_Click(object sender, EventArgs e)
        {
            if (c.check_login_val() == false) return;
            DialogResult dialogResult = MessageBox.Show("Confirm Delete", "Message", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dialogResult == DialogResult.Yes)
            {
                bool deleted = c.deleteDyeingIssueVoucher(this.voucherID);
                if (deleted == true)
                {
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
        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            fillRate();
        }
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            fillRate();
        }

        //DataGridView 1
        private void dataGridView1_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            if (e.Control is DataGridViewComboBoxEditingControl)
            {
                ((ComboBox)e.Control).DropDownStyle = ComboBoxStyle.DropDown;
                ((ComboBox)e.Control).AutoCompleteSource = AutoCompleteSource.ListItems;
                ((ComboBox)e.Control).AutoCompleteMode = AutoCompleteMode.Append;
            }
        }
        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 1 && e.RowIndex >= 0)
            {
                if (dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value == null)
                {
                    dataGridView1.Rows[e.RowIndex].Cells["Weight"].Value = null;
                    dataGridView1.Rows[e.RowIndex].Cells["Machine_Number"].Value = null;
                    dynamicWeightLabel.Text = CellSum().ToString("F3");
                    return;
                }
                fetch_data temp = new fetch_data(-1F, "", "");
                string traydetails = dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();

                this.tray_fetch_data.TryGetValue(traydetails, out temp);
                dataGridView1.Rows[e.RowIndex].Cells[2].Value = temp.net_wt;
                this.dataGridView1.Rows[e.RowIndex].Cells[3].Value = temp.mno;
                this.dataGridView1.Rows[e.RowIndex].Cells[4].Value = temp.grade;
                dynamicWeightLabel.Text = CellSum().ToString("F3");
            }
        }
        private void dataGridView1_RowPostPaint_1(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            if (e.RowIndex != dataGridView1.Rows.Count - 1)
            {
                dataGridView1.Rows[e.RowIndex].Cells[0].Value = e.RowIndex + 1;
            }
        }
        private void dataGridView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (dataGridView1.Enabled == false || dataGridView1.ReadOnly == true)
            {
                return;
            }
            int col_ind = dataGridView1.SelectedCells[0].ColumnIndex;
            if (e.KeyCode == Keys.Tab &&
                ((col_ind != 0) || this.edit_cmd_send == true))
            {
                bool edit_cmd_local = this.edit_cmd_send;
                this.edit_cmd_send = false;
                int rowindex_tab = dataGridView1.SelectedCells[0].RowIndex;
                if (edit_cmd_local == true) rowindex_tab--;

                if (rowindex_tab < 0)
                {
                    SendKeys.Send("{tab}");
                    return;                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                     
                }
                if (dataGridView1.Rows.Count - 2 == rowindex_tab)
                {
                    DataGridViewRow row = (DataGridViewRow)dataGridView1.Rows[rowindex_tab].Clone();
                    dataGridView1.Rows.Add(row);
                }
                if(col_ind==1)
                {
                    Console.WriteLine("Col 1"+ edit_cmd_local);

                    SendKeys.Send("{tab}");
                    if (edit_cmd_local == true)
                    {
                        SendKeys.Send("{tab}");
                    }
                }
                if (col_ind == 2)
                {
                    Console.WriteLine("Col 2" );
                    
                    SendKeys.Send("{tab}");
                }
            }
            if (e.KeyCode == Keys.Tab &&
               (dataGridView1.SelectedCells.Cast<DataGridViewCell>().Any(x => x.ColumnIndex == 3)))
            {
                int rowindex_tab = dataGridView1.SelectedCells[0].RowIndex;
                if (rowindex_tab < 0)
                {
                    SendKeys.Send("{tab}");
                    return;
                }
                if (dataGridView1.Rows.Count - 2 == rowindex_tab)
                {
                    DataGridViewRow row = (DataGridViewRow)dataGridView1.Rows[rowindex_tab].Clone();
                    dataGridView1.Rows.Add(row);
                }
                if (dataGridView1.Rows.Count - 1 < rowindex_tab + 1)
                {
                    dataGridView1.Rows.Add();
                }
                SendKeys.Send("{tab}");
            }
            if (e.KeyCode == Keys.Enter &&
               (dataGridView1.SelectedCells.Cast<DataGridViewCell>().Any(x => x.ColumnIndex == 1) || this.edit_cmd_send == true))
            {
                dataGridView1.BeginEdit(true);
                ComboBox c = (ComboBox)dataGridView1.EditingControl;
                if(c!=null) c.DroppedDown = true;
                e.Handled = true;
            }
        }
        private void dataGridView1_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            if (e.RowIndex != dataGridView1.Rows.Count - 1)
            {
                dataGridView1.Rows[e.RowIndex].Cells[0].Value = e.RowIndex + 1;
            }
        }
        private void dataGridView1_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            //Console.WriteLine("Inside cell begin edit");
            //if(e.RowIndex >=0 && e.ColumnIndex==1)
            //{
            //    Console.WriteLine("Valid cell begin edit");
            //    Console.WriteLine(dataGridView1.Rows[e.RowIndex].Cells[1].Value);
            //    if (dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value != null)
            //    {
            //        Console.WriteLine("Inside inside cell begin edit");
            //        this.old_quality = dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
            //        Console.WriteLine(this.old_quality);
            //    }
            //}
        }
        private void dataGridView1_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (dataGridView1.IsCurrentCellDirty)
            {
                dataGridView1.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }
        private void dataGridView1_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && e.RowIndex >= 0)
            {
                dataGridView1.Rows[e.RowIndex].Selected = true;
            }
        }
        private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            //if (e.ColumnIndex == 1 && e.RowIndex >= 0)
            //{
            //    if (dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value == null)
            //    {
            //        dataGridView1.Rows[e.RowIndex].Cells[2].Value = null;
            //        dynamicWeightLabel.Text = CellSum().ToString("F3");
            //        return;
            //    }
            //    string trayno = dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
            //    DataTable dt;
            //    if (edit_form == true)
            //    {
            //        dt = c.getTrayWeightMachineNo(int.Parse(this.tray_id_this[e.RowIndex]), this.batch_state);
            //    }
            //    else
            //    {
            //        int trayid = c.getTrayID(trayno);
            //        dt = c.getTrayWeightMachineNo(trayid, 1);

            //    }
            //    dataGridView1.Rows[e.RowIndex].Cells[2].Value = float.Parse(dt.Rows[0]["Net_Weight"].ToString()).ToString("F3");
            //    this.dataGridView1.Rows[e.RowIndex].Cells[3].Value = dt.Rows[0]["Machine_No"].ToString();
            //    dynamicWeightLabel.Text = CellSum().ToString("F3");
            //}
        }
        
        
    }
}
