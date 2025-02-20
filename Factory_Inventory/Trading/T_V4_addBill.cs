﻿using Factory_Inventory.Factory_Classes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Factory_Inventory
{
    public partial class T_V3_addBill : Form
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
                this.saveButton.Focus();
                this.ActiveControl = saveButton;
                return false;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
        private DbConnect c;
        private bool edit_cmd_send = false;
        private bool edit_form = false;
        private List<string> do_no;
        private M_V_history v1_history;
        private int voucher_id;
        //string tablename;
        private Dictionary<string, int> quality_dict = new Dictionary<string, int>();
        private Dictionary<string, int> customer_dict = new Dictionary<string, int>();

        Dictionary<string, bool> batch_editable = new Dictionary<string, bool>();
        
        //DO_no -> DataRow
        Dictionary<string, DataRow> DO_fetch_data = new Dictionary<string, DataRow>();
        //
        Dictionary<string, Tuple<DataRow, bool>> DO_fetch_data_edit = new Dictionary<string, Tuple<DataRow, bool>>();

        //Form functions
        public T_V3_addBill()
        {
            InitializeComponent();
            this.Name = "Add Bill";
            this.c = new DbConnect();
            this.do_no = new List<string>();
            //this.do_no.Add("");

            //Create rop down type list
            List<string> dataSource = new List<string>();
            dataSource.Add("---Select---");
            dataSource.Add("0");
            dataSource.Add("1");
            this.typeCB.DataSource = dataSource;
            this.typeCB.DisplayMember = "Type";
            this.typeCB.DropDownStyle = ComboBoxStyle.DropDownList;//Create a drop-down list
            this.typeCB.AutoCompleteSource = AutoCompleteSource.ListItems;
            this.typeCB.AutoCompleteMode = AutoCompleteMode.SuggestAppend;

            //Create drop-down lists
            var dataSource1 = new List<string>();
            DataTable d = c.getQC('f');

            for (int i = 0; i < d.Rows.Count; i++)
            {
                dataSource1.Add(d.Rows[i][0].ToString());
            }
            this.financialYearCB.DataSource = dataSource1;
            this.financialYearCB.DisplayMember = "Financial Year";
            this.financialYearCB.DropDownStyle = ComboBoxStyle.DropDownList;//Create a drop-down list
            this.financialYearCB.AutoCompleteSource = AutoCompleteSource.ListItems;
            this.financialYearCB.AutoCompleteMode = AutoCompleteMode.SuggestAppend;

            this.financialYearCB.SelectedIndex = this.financialYearCB.FindStringExact(c.getFinancialYear(this.inputDate.Value));

            //Create drop-down Quality list
            var dataSource2 = new List<string>();
            DataTable dt = c.runQuery("SELECT * FROM T_M_Quality_Before_Job");
            dataSource2.Add("---Select---");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                dataSource2.Add(dt.Rows[i]["Quality_Before_Job"].ToString());
                quality_dict[dt.Rows[i]["Quality_Before_Job"].ToString()] = int.Parse(dt.Rows[i]["Quality_Before_Job_ID"].ToString());
            }
            this.qualityCB.DataSource = dataSource2;
            this.qualityCB.DisplayMember = "Quality";
            this.qualityCB.DropDownStyle = ComboBoxStyle.DropDownList;//Create a drop-down list
            this.qualityCB.AutoCompleteSource = AutoCompleteSource.ListItems;
            this.qualityCB.AutoCompleteMode = AutoCompleteMode.SuggestAppend;


            //Create drop - down Customers list
            var dataSource3 = new List<string>();
            dt = c.runQuery("SELECT * FROM T_M_Customers");
            dataSource3.Add("---Select---");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                dataSource3.Add(dt.Rows[i]["Customer_Name"].ToString());
                customer_dict[dt.Rows[i]["Customer_Name"].ToString()] = int.Parse(dt.Rows[i]["Customer_ID"].ToString());
            }
            this.billCustomerNameCB.DataSource = dataSource3;
            this.billCustomerNameCB.DisplayMember = "Customers";
            this.billCustomerNameCB.DropDownStyle = ComboBoxStyle.DropDownList;//Create a drop-down list
            this.billCustomerNameCB.AutoCompleteSource = AutoCompleteSource.ListItems;
            this.billCustomerNameCB.AutoCompleteMode = AutoCompleteMode.SuggestAppend;


            //DatagridView
            dataGridView1.Columns.Add("Sl_No", "Sl_No");
            dataGridView1.Columns[0].ReadOnly = true;
            DataGridViewComboBoxColumn dgvCmb = new DataGridViewComboBoxColumn();
            dgvCmb.DataSource = this.do_no;
            dgvCmb.HeaderText = "DO Number";
            dataGridView1.Columns.Insert(1, dgvCmb);
            dataGridView1.Columns.Add("DO Weight", "DO Weight");
            dataGridView1.Columns.Add("DO Amount", "DO Amount");
            dataGridView1.Columns[2].ReadOnly = true;
            dataGridView1.Columns[3].ReadOnly = true;
            dataGridView1.Columns.Add("Comments", "Comments");
            dataGridView1.RowCount = 10;

            c.set_dgv_column_sort_state(this.dataGridView1, DataGridViewColumnSortMode.NotSortable);
        }
        public T_V3_addBill(DataRow row, bool isEditable, M_V_history v1_history)
        {
            InitializeComponent();
            //common initializations
            this.Name = "Add Bill";
            this.edit_form = true;
            this.v1_history = v1_history;
            this.c = new DbConnect();
            this.do_no = new List<string>();

            //Create rop down type list
            List<string> dataSource = new List<string>();
            dataSource.Add("---Select---");
            dataSource.Add("0");
            dataSource.Add("1");
            this.typeCB.DataSource = dataSource;
            this.typeCB.DisplayMember = "Type";
            this.typeCB.DropDownStyle = ComboBoxStyle.DropDownList;//Create a drop-down list
            this.typeCB.AutoCompleteSource = AutoCompleteSource.ListItems;
            this.typeCB.AutoCompleteMode = AutoCompleteMode.SuggestAppend;

            //Create drop-down lists
            var dataSource1 = new List<string>();
            DataTable d = c.getQC('f');

            for (int i = 0; i < d.Rows.Count; i++)
            {
                dataSource1.Add(d.Rows[i][0].ToString());
            }
            this.financialYearCB.DataSource = dataSource1;
            this.financialYearCB.DisplayMember = "Financial Year";
            this.financialYearCB.DropDownStyle = ComboBoxStyle.DropDownList;//Create a drop-down list
            this.financialYearCB.AutoCompleteSource = AutoCompleteSource.ListItems;
            this.financialYearCB.AutoCompleteMode = AutoCompleteMode.SuggestAppend;


            //Create drop-down Quality list
            var dataSource2 = new List<string>();
            DataTable dt = c.runQuery("SELECT * FROM T_M_Quality_Before_Job");
            dataSource2.Add("---Select---");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                dataSource2.Add(dt.Rows[i]["Quality_Before_Job"].ToString());
                quality_dict[dt.Rows[i]["Quality_Before_Job"].ToString()] = int.Parse(dt.Rows[i]["Quality_Before_Job_ID"].ToString());
            }
            this.qualityCB.DataSource = dataSource2;
            this.qualityCB.DisplayMember = "Quality";
            this.qualityCB.DropDownStyle = ComboBoxStyle.DropDownList;//Create a drop-down list
            this.qualityCB.AutoCompleteSource = AutoCompleteSource.ListItems;
            this.qualityCB.AutoCompleteMode = AutoCompleteMode.SuggestAppend;

            //Create drop - down Customers list
            var dataSource3 = new List<string>();
            dt = c.runQuery("SELECT * FROM T_M_Customers");
            dataSource3.Add("---Select---");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                dataSource3.Add(dt.Rows[i]["Customer_Name"].ToString());
                customer_dict[dt.Rows[i]["Customer_Name"].ToString()] = int.Parse(dt.Rows[i]["Customer_ID"].ToString());
            }
            this.billCustomerNameCB.DataSource = dataSource3;
            this.billCustomerNameCB.DisplayMember = "Customers";
            this.billCustomerNameCB.DropDownStyle = ComboBoxStyle.DropDownList;//Create a drop-down list
            this.billCustomerNameCB.AutoCompleteSource = AutoCompleteSource.ListItems;
            this.billCustomerNameCB.AutoCompleteMode = AutoCompleteMode.SuggestAppend;


            //DatagridView
            dataGridView1.Columns.Add("Sl_No", "Sl_No");
            dataGridView1.Columns[0].ReadOnly = true;
            DataGridViewComboBoxColumn dgvCmb = new DataGridViewComboBoxColumn();
            dgvCmb.DataSource = this.do_no;
            dgvCmb.HeaderText = "DO Number";
            dataGridView1.Columns.Insert(1, dgvCmb);
            dataGridView1.Columns.Add("DO Weight", "DO Weight");
            dataGridView1.Columns.Add("DO Amount", "DO Amount");
            dataGridView1.Columns[2].ReadOnly = true;
            dataGridView1.Columns[3].ReadOnly = true;
            dataGridView1.Columns.Add("Comments", "Comments");
            dataGridView1.RowCount = 10;

            //if only in view mode
            if (isEditable == false)
            {
                this.Text += "(View Only)";
                this.deleteButton.Visible = true;
                this.deleteButton.Enabled = true;
                this.disable_form_edit();
            }
            else
            {
                this.Text += "(Edit)";
                this.typeCB.Enabled = false;
                this.financialYearCB.Enabled = false;
                this.qualityCB.Enabled = false;
                this.loadDOButton.Enabled = false;
                this.saveButton.Enabled = true;
                if (typeCB.Text == "0")   this.billCustomerNameCB.Enabled = true;
            }

            //Fill in required values
            this.inputDate.Value = Convert.ToDateTime(row["Date_Of_Input"].ToString());
            this.billDateDTP.Value = Convert.ToDateTime(row["Sale_Bill_Date"].ToString());
            this.qualityCB.SelectedIndex = this.qualityCB.FindStringExact(row["Quality_Before_Job"].ToString());
            this.billNumberTextboxTB.Text = row["Sale_Bill_No"].ToString();
            this.voucher_id = int.Parse(row["Voucher_ID"].ToString());  
            this.financialYearCB.SelectedIndex = this.financialYearCB.FindStringExact(row["DO_Fiscal_Year"].ToString());
            this.typeCB.SelectedIndex = this.typeCB.FindStringExact(row["Type_Of_Sale"].ToString());
            this.billWeightTB.Text = row["Sale_Bill_Weight"].ToString();
            this.billAmountTB.Text = row["Sale_Bill_Amount"].ToString();
            this.netDOWeightTB.Text = row["Sale_Bill_Weight_Calc"].ToString();
            this.netDOAmountTB.Text = row["Sale_Bill_Amount_Calc"].ToString();
            this.narrationTB.Text = row["Narration"].ToString();
            this.billCustomerNameCB.SelectedIndex = this.billCustomerNameCB.FindStringExact(row["Customer_Name"].ToString());
            this.billCustomerNameCB.Visible = true;
            this.billCustomerNameCB.TabIndex = 8;
            this.billCustomerNameCB.TabStop = true;
            this.label13.Visible = true;

            string sql = "SELECT Sale_DO_No, Net_Weight, Sale_Rate, Voucher_ID FROM T_Sales_Voucher WHERE SalesBillNos_Voucher_ID = " + this.voucher_id + " ORDER BY SalesBillNos_Display_Order ASC";
            DataTable temp = c.runQuery(sql);
            for (int i = 0; i < temp.Rows.Count; i++)
            {
                this.do_no.Add(temp.Rows[i]["Sale_DO_No"].ToString());
                DO_fetch_data[temp.Rows[i]["Sale_DO_No"].ToString()] = temp.Rows[i];
                DO_fetch_data_edit[temp.Rows[i]["Sale_DO_No"].ToString()] = new Tuple<DataRow, bool>(temp.Rows[i], false);
            }
            dataGridView1.RowCount = this.do_no.Count + 1;
            dgvCmb.DataSource = this.do_no;
            //Fill data in datagridview
            for (int i = 0; i < this.do_no.Count; i++)
            {
                dataGridView1.Rows[i].Cells[1].Value = this.do_no[i];
            }

            //Load data in datagridview dropdown
            this.loadData(row["Quality_Before_Job"].ToString(), row["DO_Fiscal_Year"].ToString(), row["Type_Of_Sale"].ToString());
            string[] do_nos = c.csvToArray(row["DO_No_Arr"].ToString());
            for (int i = 0; i < do_nos.Length; i++)
            {
                this.do_no.Add(do_nos[i]);
            }
            dgvCmb.DataSource = this.do_no;

            c.set_dgv_column_sort_state(this.dataGridView1, DataGridViewColumnSortMode.NotSortable);
        }
        private void M_V2_dyeingInwardForm_Load(object sender, EventArgs e)
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

            this.billDateDTP.Focus();
            if (Global.access == 2)
            {
                this.deleteButton.Visible = false;
            }
        }

        //Own functions
        public void disable_form_edit()
        {
            this.inputDate.Enabled = false;
            this.qualityCB.Enabled = false;
            this.loadDOButton.Enabled = false;
            this.saveButton.Enabled = false;
            this.dataGridView1.ReadOnly = true;
            this.billNumberTextboxTB.Enabled = false;
            this.billDateDTP.Enabled = false;
            this.billWeightTB.Enabled = false;
            this.billAmountTB.Enabled = false;
            this.deleteToolStripMenuItem.Enabled = false;
            this.typeCB.Enabled = false;
            this.financialYearCB.Enabled = false;
            this.billCustomerNameCB.Enabled = false;
            this.narrationTB.ReadOnly = true;
        }
        private float CellSum(int column)
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
                    if (c.Cell_Not_NullOrEmpty(this.dataGridView1, i, column))
                    {
                        //float dyeing_rate = float.Parse(c.getColumnBatchNo("Dyeing_Rate", int.Parse(this.dataGridView1.Rows[i].Cells[1].Value.ToString()), this.comboBox3CB.Text));
                        //Console.WriteLine(dyeing_rate.ToString());
                        //sum += float.Parse(dataGridView1.Rows[i].Cells[column].Value.ToString())*dyeing_rate;
                        sum += float.Parse(dataGridView1.Rows[i].Cells[column].Value.ToString());
                    }
                }
                return sum;
            }
            catch
            {
                Console.WriteLine("Excep");
                return sum;
            }
        }
        private void loadData(string quality, string do_fiscal_year, string type)
        {
            string sql;
            if (type == "0")
            {
                sql = "SELECT Sale_DO_No, Net_Weight, Sale_Rate, Voucher_ID FROM T_Sales_Voucher WHERE Quality_ID = '" + quality_dict[quality] + "' AND Fiscal_Year='" + do_fiscal_year + "' AND Type_of_Sale='" + type + "' AND Sale_Bill_No IS NULL";
            }
            else 
            {
                sql = "SELECT Sale_DO_No, Net_Weight, Sale_Rate, Voucher_ID FROM T_Sales_Voucher WHERE Quality_ID = '" + quality_dict[quality] + "' AND Fiscal_Year='" + do_fiscal_year + "' AND Type_of_Sale='" + type + "' AND Sale_Bill_No IS NULL AND Customer_ID = '" + customer_dict[billCustomerNameCB.Text].ToString() + "'";
            }
            DataTable dt = c.runQuery(sql);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                this.do_no.Add(dt.Rows[i]["Sale_DO_No"].ToString());
                DO_fetch_data[dt.Rows[i]["Sale_DO_No"].ToString()] = dt.Rows[i];
            }
        }

        //Clicks
        private void saveButton_Click(object sender, EventArgs e)
        {
            if (c.check_login_val() == false) return;
            //checks
            if (!c.Cell_Not_NullOrEmpty(this.dataGridView1, 0, 1))
            {
                c.ErrorBox("Please enter DO Numbers", "Error");
                return;
            }
            try
            {
                float.Parse(this.billWeightTB.Text);
            }
            catch
            {
                c.ErrorBox("Enter numeric bill weight", "Error");
                return;
            }
            try
            {
                float.Parse(this.billAmountTB.Text);
            }
            catch
            {
                c.ErrorBox("Enter numeric bill amount", "Error");
                return;
            }
            if (Math.Abs(Double.Parse(billWeightTB.Text) - Double.Parse(netDOWeightTB.Text)) > 1D)
            {
                c.ErrorBox("Bill Weight is does not match total DO weight", "Error");
                return;
            }
            string customer = null;
            if (typeCB.SelectedItem.ToString() == "1")
            {
                
                if (Math.Abs(float.Parse(this.netDOAmountTB.Text) - float.Parse(this.billAmountTB.Text)) > 5F)
                {
                    c.ErrorBox("Bill Amount and Net DO Amount have a difference greater than ₹5", "Error");
                    return;
                }
            }
            if (typeCB.SelectedItem.ToString() == "0")
            {
                if(this.billCustomerNameCB.SelectedIndex==0)
                {
                    c.ErrorBox("Please Select Bill Customer Name");
                    return;
                }
                if (float.Parse(this.netDOAmountTB.Text) < float.Parse(this.billAmountTB.Text))
                {
                    c.ErrorBox("Bill Amount is more than total DO Amount", "Error");
                    return;
                }
            }
            customer = this.billCustomerNameCB.Text;
            List<string> do_nos = new List<string>();
            List<string> temp = new List<string>();

            //Used for edit only
            List<Tuple<string, int>> add = new List<Tuple<string, int>>(); //<DO No, Display Order>
            List<Tuple<string, int>> edit = new List<Tuple<string, int>>();
            List<string> delete = new List<string>();
            for (int i = 0; i < dataGridView1.Rows.Count - 1; i++)
            {
                if (!c.Cell_Not_NullOrEmpty(this.dataGridView1, i, 1))
                {
                    continue;
                }
                else
                {
                    do_nos.Add(dataGridView1.Rows[i].Cells[1].Value.ToString());
                    //to check for all different do_nos
                    temp.Add(dataGridView1.Rows[i].Cells[1].Value.ToString());
                    var distinctBytes = new HashSet<string>(temp);
                    bool allDifferent = distinctBytes.Count == temp.Count;
                    if (allDifferent == false)
                    {
                        c.ErrorBox("Please Enter Distinct DO Numbers at Row: " + (i + 1).ToString(), "Error");
                        return;
                    }
                }
                if(DO_fetch_data_edit.ContainsKey(dataGridView1.Rows[i].Cells[1].Value.ToString()))
                {
                    edit.Add(new Tuple<string, int>(dataGridView1.Rows[i].Cells[1].Value.ToString(), i));
                    DataRow temp_row = DO_fetch_data_edit[dataGridView1.Rows[i].Cells[1].Value.ToString()].Item1;
                    DO_fetch_data_edit[dataGridView1.Rows[i].Cells[1].Value.ToString()] = new Tuple<DataRow, bool>(temp_row, true);
                }
                else
                {
                    add.Add(new Tuple<string, int>(dataGridView1.Rows[i].Cells[1].Value.ToString(), i));
                }
            }
            foreach(KeyValuePair<string, Tuple<DataRow,bool>> entry in DO_fetch_data_edit)
            {
                if (entry.Value.Item2 == false) delete.Add(entry.Key);
            }
            if (this.edit_form == true)
            {
                //bool editbill = c.editSalesBillNosVoucher(this.voucher_id, inputDate.Value, billDateDTP.Value, do_nos, this.financialYearCB.SelectedItem.ToString(), billNumberTextboxTB.Text, float.Parse(billWeightTB.Text), float.Parse(billAmountTB.Text), float.Parse(netDOWeightTB.Text), float.Parse(netDOAmountTB.Text), this.tablename, customer);

                string bill_date = billDateDTP.Value.Date.ToString("MM-dd-yyyy").Substring(0, 10);
                string fiscal_year = c.getFinancialYear(billDateDTP.Value);

                string sql = "begin transaction; begin try;\n";
                //Delete pervious DOs
                for (int i = 0; i < delete.Count; i++)
                {
                    sql += "UPDATE T_Sales_Voucher SET Sale_Bill_Date = NULL, Sale_Bill_No = NULL, SalesBillNos_Voucher_ID = NULL, SalesBillNos_Display_Order = NULL, Bill_Comments = NULL WHERE Sale_DO_No = '" + delete[i] + "' AND SalesBillNos_Voucher_ID = " + this.voucher_id.ToString() + ";\n";
                }

                for (int i = 0; i < add.Count; i++)
                {
                    string bill_comments = "";
                    if (c.Cell_Not_NullOrEmpty(dataGridView1, add[i].Item2, -1, "Comments")) bill_comments = dataGridView1.Rows[add[i].Item2].Cells["Comments"].Value.ToString();
                    sql += "UPDATE T_Sales_Voucher SET Sale_Bill_No='" + billNumberTextboxTB.Text + "', Sale_Bill_Date='" + bill_date + "', SalesBillNos_Voucher_ID = '" + this.voucher_id + "', SalesBillNos_Display_Order = '" + add[i].Item2 + "', Bill_Comments = '" + bill_comments + "' WHERE Sale_DO_No = '" + add[i].Item1 + "' AND Fiscal_Year='" + this.financialYearCB.SelectedItem.ToString() + "';\n";
                }

                for (int i = 0; i < edit.Count; i++)
                {
                    string bill_comments = "";
                    if (c.Cell_Not_NullOrEmpty(dataGridView1, edit[i].Item2, -1, "Comments")) bill_comments = dataGridView1.Rows[edit[i].Item2].Cells["Comments"].Value.ToString();
                    sql += "UPDATE T_Sales_Voucher SET Sale_Bill_No='" + billNumberTextboxTB.Text + "', Sale_Bill_Date='" + bill_date + "', SalesBillNos_Display_Order = '" + edit[i].Item2 + "', Bill_Comments = '" + bill_comments + "' WHERE Sale_DO_No = '" + edit[i].Item1 + "' AND Fiscal_Year='" + this.financialYearCB.SelectedItem.ToString() + "' AND SalesBillNos_Voucher_ID = '" + this.voucher_id + "';\n";
                }

                sql += "UPDATE T_SalesBillNos_Voucher SET Sale_Bill_Date='" + bill_date + "', Fiscal_Year='" + fiscal_year + "', Sale_Bill_No='" + billNumberTextboxTB.Text + "', Sale_Bill_Weight=" + billWeightTB.Text + ", Sale_Bill_Amount=" + billAmountTB.Text + ", Sale_Bill_Weight_Calc=" + netDOWeightTB.Text + ", Sale_Bill_Amount_Calc=" + netDOAmountTB.Text + ", Bill_Customer_ID = '" + customer_dict[customer] + "', Narration = '" + narrationTB.Text + "' WHERE Voucher_ID=" + this.voucher_id + ";\n";
                
                //catch
                sql += "commit transaction; end try BEGIN CATCH rollback transaction; \n";
                sql += "DECLARE @ErrorMessage NVARCHAR(4000); DECLARE @ErrorSeverity INT; DECLARE @ErrorState INT; SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE(); \n";
                sql += "RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState); END CATCH; \n";
                DataTable editbill = c.runQuery(sql); 
                if (editbill != null)
                {
                    dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = Color.LawnGreen;
                    disable_form_edit();
                    this.v1_history.loadData();
                    c.SuccessBox("Voucher Edited Successfully");
                }
                return;
            }
            else
            {
                string input_date = inputDate.Value.Date.ToString("MM-dd-yyyy").Substring(0, 10);
                string bill_date = billDateDTP.Value.Date.ToString("MM-dd-yyyy").Substring(0, 10);
                string fiscal_year = c.getFinancialYear(billDateDTP.Value);

                string sql = "begin transaction; begin try; DECLARE @voucherID int;\n";
                sql += "INSERT INTO T_SalesBillNos_Voucher (Date_Of_Input, Sale_Bill_Date, Quality_ID, DO_Fiscal_Year, Fiscal_Year, Type_Of_Sale, Sale_Bill_No, Sale_Bill_Weight, Sale_Bill_Amount, Sale_Bill_Weight_Calc, Sale_Bill_Amount_Calc, Bill_Customer_ID) VALUES ('" + input_date + "','" + bill_date + "'," + quality_dict[qualityCB.SelectedItem.ToString()].ToString() + ",'" + this.financialYearCB.SelectedItem.ToString() + "', '" + fiscal_year + "', " + typeCB.Text + ", '" + billNumberTextboxTB.Text + "', " + billWeightTB.Text + ", " + billAmountTB.Text + ", " + netDOWeightTB.Text + ", " + netDOAmountTB.Text + ", '" + customer_dict[customer].ToString() + "'); SELECT @voucherID = SCOPE_IDENTITY();\n";
                //add bill nos to DOs
                for(int i=0;i<do_nos.Count;i++)
                {
                    string bill_comments = "";
                    if (c.Cell_Not_NullOrEmpty(dataGridView1, i, -1, "Comments")) bill_comments = dataGridView1.Rows[i].Cells["Comments"].Value.ToString();
                    sql += "UPDATE T_Sales_Voucher SET Sale_Bill_No='" + billNumberTextboxTB.Text + "', Sale_Bill_Date='" + bill_date + "', SalesBillNos_Voucher_ID = @voucherID1, SalesBillNos_Display_Order = '" + (i + 1).ToString() + "', Bill_Comments = '"+ bill_comments +"' WHERE Sale_DO_No = '" + do_nos[i] + "' AND Fiscal_Year='" + this.financialYearCB.SelectedItem.ToString() + "';\n";
                }

                //catch
                sql += "commit transaction; end try BEGIN CATCH rollback transaction; \n";
                sql += "DECLARE @ErrorMessage NVARCHAR(4000); DECLARE @ErrorSeverity INT; DECLARE @ErrorState INT; SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE(); \n";
                sql += "RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState); END CATCH; \n";
                DataTable added = c.runQuery(sql);
                if(added != null)
                {
                    dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = Color.LawnGreen;
                    disable_form_edit();
                    c.SuccessBox("Voucher Added Successfully");
                }
            }
            dataGridView1.EnableHeadersVisualStyles = false;
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
            this.netDOWeightTB.Text = CellSum(2).ToString("F3");
            this.netDOAmountTB.Text = CellSum(3).ToString("F2");
        }
        private void loadDOButton_Click(object sender, EventArgs e)
        {
            if (qualityCB.SelectedIndex == 0)
            {
                c.ErrorBox("Enter Quality", "Error");
                return;
            }
            if (financialYearCB.SelectedIndex < 0)
            {
                c.ErrorBox("Please select Batch Financial Year", "Error");
                return;
            }
            if (typeCB.SelectedIndex == 0)
            {
                c.ErrorBox("Please select type of DOs", "Error");
                return;
            }
            if(typeCB.Text=="1")
            {
                if(billCustomerNameCB.SelectedIndex==0)
                {
                    c.ErrorBox("Please select bill customer name", "Error");
                    return;
                }
            }
            this.loadData(this.qualityCB.SelectedItem.ToString(), financialYearCB.SelectedItem.ToString(), this.typeCB.Text);
            Console.WriteLine(this.do_no.Count);
            if (this.do_no.Count <= 0)
            {
                c.WarningBox("No DOs Loaded");
                return;
            }
            else
            {
                if (this.edit_form == false)
                {
                    c.SuccessBox("Loaded " + (this.do_no.Count).ToString() + " DOs");
                }
            }
            this.saveButton.Enabled = true;
            this.loadDOButton.Enabled = false;
            this.qualityCB.Enabled = false;
            this.saveButton.Enabled = true;
            this.financialYearCB.Enabled = false;
            this.typeCB.Enabled = false;

            this.label13.Visible = true;
            this.billCustomerNameCB.Visible = true;
            this.billCustomerNameCB.TabIndex = 8;
            this.billCustomerNameCB.TabStop = true;
            //if(typeCB.Text=="1")
            //{
            //    this.billDateDTP.MinDate = this.inputDate.Value.Date.AddDays(-2);
            //    this.billDateDTP.MaxDate = this.inputDate.Value.Date.AddDays(2);
            //}
        }
        private void deleteButton_Click(object sender, EventArgs e)
        {
            if (c.check_login_val() == false) return;
            DialogResult dialogResult = MessageBox.Show("Confirm Delete", "Message", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dialogResult == DialogResult.Yes)
            {
                bool deleted = c.deleteSalesBillNosVoucher(this.voucher_id);
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
                if(dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value == null)
                {
                    dataGridView1.Rows[e.RowIndex].Cells["DO Weight"].Value = null;
                    dataGridView1.Rows[e.RowIndex].Cells["DO Amount"].Value = null;
                    this.netDOWeightTB.Text = CellSum(2).ToString("F3");
                    this.netDOAmountTB.Text = CellSum(3).ToString("F2");
                    return;
                }
                string do_no = dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
                DataRow row = DO_fetch_data[do_no];
                dataGridView1.Rows[e.RowIndex].Cells["DO Weight"].Value = row["Net_Weight"].ToString();
                dataGridView1.Rows[e.RowIndex].Cells["DO Amount"].Value = (float.Parse(row["Sale_Rate"].ToString())*float.Parse(row["Net_Weight"].ToString())).ToString("F2");
                this.netDOWeightTB.Text = CellSum(2).ToString("F3");
                this.netDOAmountTB.Text = CellSum(3).ToString("F2");
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
            int col = dataGridView1.SelectedCells[0].ColumnIndex;
            if (e.KeyCode == Keys.Tab &&
               ( (col!=0) || this.edit_cmd_send == true))
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
                //if (dataGridView1.Rows.Count - 2 == rowindex_tab && ((col==6 && this.addBill==false)|| (col == 5 && this.addBill == true)))
                //{
                //    DataGridViewRow row = (DataGridViewRow)dataGridView1.Rows[rowindex_tab].Clone();
                //    dataGridView1.Rows.Add(row);
                //}
                if(col==1)
                {
                    Console.WriteLine("col1 " + edit_cmd_local);
                    SendKeys.Send("{tab}");
                    if (edit_cmd_local == true)
                    {
                        SendKeys.Send("{tab}");
                    }
                }
                if(col==2)
                {
                    Console.WriteLine("col2");
                    SendKeys.Send("{tab}");
                }
                if(col==3)
                {
                    Console.WriteLine("col3");
                    SendKeys.Send("{tab}");
                }
                if (col == 4)
                {
                    Console.WriteLine("col4");
                    SendKeys.Send("{tab}");
                }
                //if (col == 5 && this.addBill == true)
                //{
                //    Console.WriteLine("col4");
                //    SendKeys.Send("{tab}");
                //}
                //if (col == 5 && this.addBill == false && this.billcheckBoxCK.Checked==false)
                //{
                //    Console.WriteLine("col5 no slip");
                //    SendKeys.Send("{tab}");
                //}
                //if (col == 6 && this.addBill==false)
                //{
                //    Console.WriteLine("col4");
                //    SendKeys.Send("{tab}");
                //}

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
        private void dataGridView1_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && e.RowIndex >= 0)
            {
                dataGridView1.Rows[e.RowIndex].Selected = true;
            }
        }
        private void dataGridView1_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (dataGridView1.IsCurrentCellDirty)
            {
                dataGridView1.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        //Combobox
        private void typeCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (typeCB.Text == "1")
            {
                this.label13.Visible = true;
                this.billCustomerNameCB.Visible = true;
                this.billCustomerNameCB.TabIndex = 8;
                this.billCustomerNameCB.TabStop = true;
            }
            if (typeCB.Text == "0")
            {
                this.label13.Visible = false;
                this.billCustomerNameCB.Visible = false;
            }
        }
    }
}
