﻿using BarcodeLib;
using CoolPrintPreview;
using Factory_Inventory.Factory_Classes;
using Factory_Inventory.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Factory_Inventory
{
    public partial class M_V4_printCartonSlip : Form
    {
        private DbConnect c;
        DataTable dt2 = new DataTable();
        DataTable dt3 = new DataTable();
        DataTable dt4 = new DataTable();

        int topmargin, lrmargin;
        int slip_count = 0, printed_slips = 0;
        string carton_fiscal_year;
        List<int> cartons_to_print = new List<int>();
        List<string> cartons_to_print_fiscal_year = new List<string>();
        Color selected_color = Color.SteelBlue;
        Color printed_color= Global.printedColor;

        DataTable dt_printTypes;
        Dictionary<string, string> firmDetails = new Dictionary<string, string>();
        string font_name = "generic sans serif";
        
        public M_V4_printCartonSlip()
        {
            InitializeComponent();
            this.c = new DbConnect();

            //Load Data
            //Create drop-down lists 
            var dataSource = new List<string>();
            DataTable d = c.getQC('f');

            for (int i = 0; i < d.Rows.Count; i++)
            {
                dataSource.Add(d.Rows[i][0].ToString());
            }
            this.fiscalCombobox.DataSource = dataSource;
            this.fiscalCombobox.DisplayMember = "Financial Year";
            this.fiscalCombobox.DropDownStyle = ComboBoxStyle.DropDownList;//Create a drop-down list
            this.fiscalCombobox.AutoCompleteSource = AutoCompleteSource.ListItems;
            this.fiscalCombobox.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            this.fiscalCombobox.SelectedIndex = this.fiscalCombobox.FindStringExact(c.getFinancialYear(DateTime.Now));
            
            //Datagridviews

            DataTable dyeing_batches = c.getBatchTable_State(3);
            dataGridView1.DataSource = dyeing_batches;
            if (true)
            {
                dataGridView1.Columns.OfType<DataGridViewColumn>().ToList().ForEach(col => col.Visible = false);
                this.dataGridView1.Columns["Batch_No"].Visible = true;
                this.dataGridView1.Columns["Colour"].Visible = true;
                this.dataGridView1.Columns["Quality"].Visible = true;
                this.dataGridView1.Columns["Date_Of_Production"].Visible = true;
                this.dataGridView1.Columns["Net_Weight"].Visible = true;
                this.dataGridView1.Columns["Number_Of_Trays"].Visible = true;
                this.dataGridView1.Columns["Fiscal_Year"].Visible = true;

                this.dataGridView1.Columns["Fiscal_Year"].HeaderText = "Financial Year";
                this.dataGridView1.Columns["Number_Of_Trays"].HeaderText = "Number of Trays";
                this.dataGridView1.Columns["Batch_No"].HeaderText = "Batch Number";
                this.dataGridView1.Columns["Date_Of_Production"].HeaderText = "Date of Production";

                this.dataGridView1.Columns["Batch_No"].Width = 70;
                this.dataGridView1.Columns["Number_Of_Trays"].Width = 80;
            }
            dataGridView1.DefaultCellStyle.SelectionBackColor = Color.White;
            dataGridView1.DefaultCellStyle.SelectionForeColor = Color.Blue;
            dataGridView1.Columns["Fiscal_Year"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            dataGridView2.DataSource = dt2;
            dataGridView2.DefaultCellStyle.SelectionBackColor = Color.White;
            dataGridView2.DefaultCellStyle.SelectionForeColor = Color.Blue;
            c.set_dgv_column_sort_state(dataGridView2, DataGridViewColumnSortMode.NotSortable);

            dataGridView3.DataSource = this.dt3;
            dataGridView3.RowHeadersVisible = false;
            this.dt3.Columns.Clear();
            this.dt3.Columns.Add("SlNo");
            this.dt3.Columns.Add("Batch_No");
            this.dt3.Columns.Add("Financal Year");
            this.dt3.Columns.Add("Net Weight");
            dataGridView3.Columns["SlNo"].Width = 60;
            dataGridView3.DefaultCellStyle.SelectionBackColor = Color.White;
            dataGridView3.DefaultCellStyle.SelectionForeColor = Color.Black;
            dataGridView3.Columns["Net Weight"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            c.set_dgv_column_sort_state(dataGridView3, DataGridViewColumnSortMode.NotSortable);


            dataGridView4.DataSource = this.dt4;
            dataGridView4.RowHeadersVisible = false;
            this.dt4.Columns.Clear();
            this.dt4.Columns.Add("SlNo");
            this.dt4.Columns.Add("Carton No.");
            this.dt4.Columns.Add("Net Weight");
            this.dt4.Columns.Add("Financial Year");
            dataGridView4.Columns["SlNo"].Width = 70;
            dataGridView4.Columns["Carton No."].Width = 70;
            dataGridView4.Columns["Net Weight"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridView4.DefaultCellStyle.SelectionBackColor = Color.White;
            dataGridView4.DefaultCellStyle.SelectionForeColor = Color.Blue;
            c.set_dgv_column_sort_state(dataGridView4, DataGridViewColumnSortMode.NotSortable);


            dataGridView5.RowHeadersVisible = false;
            this.dataGridView5.Columns.Clear();
            this.dataGridView5.Columns.Add("SlNo", "SlNo");
            this.dataGridView5.Columns.Add("Carton No.", "Carton No.");
            this.dataGridView5.Columns.Add("Net Weight", "Net Weight");
            this.dataGridView5.Columns.Add("Financial Year", "Financial Year");
            dataGridView5.Columns["SlNo"].Width = 70;
            dataGridView5.Columns["Carton No."].Width = 70;
            dataGridView5.Columns["Net Weight"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridView5.DefaultCellStyle.SelectionBackColor = Color.White;
            dataGridView5.DefaultCellStyle.SelectionForeColor = Color.Blue;
            c.set_dgv_column_sort_state(dataGridView5, DataGridViewColumnSortMode.NotSortable);

            PrinterSettings ps = new PrinterSettings();
            printDocument1.PrinterSettings = ps;
            IEnumerable<PaperSize> paperSizes = ps.PaperSizes.Cast<PaperSize>();
            PaperSize sizeA4 = paperSizes.First<PaperSize>(size => size.Kind == PaperKind.A4); // setting paper size to A4 size
            printDocument1.DefaultPageSettings.PaperSize = sizeA4;

            //Load and Set defaults
            var print_types = new List<string>();
            dt_printTypes = c.runQuery("SELECT * FROM Print_Types");
            for (int i = 0; i < dt_printTypes.Rows.Count; i++)
            {
                string val = "";
                for(int j=0; j<dt_printTypes.Columns.Count-1; j++)
                {
                    val += dt_printTypes.Rows[i][j].ToString()+", ";
                }
                val += "ID:"+dt_printTypes.Rows[i]["Print_Type_ID"].ToString();
                print_types.Add(val);
            }
            this.firmDetailsCB.DataSource = print_types;
            this.firmDetailsCB.DisplayMember = "Firm Details";
            this.firmDetailsCB.DropDownStyle = ComboBoxStyle.DropDownList;//Create a drop-down list
            this.firmDetailsCB.AutoCompleteSource = AutoCompleteSource.ListItems;
            this.firmDetailsCB.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            this.firmDetailsCB.DropDownWidth = DropDownWidth(this.firmDetailsCB);
            this.firmDetailsCB.ItemHeight = 50; //set drawmode user to use
            //Load default
            DataTable dt = c.runQuery("SELECT * FROM Defaults WHERE Default_Type= 'Print:Carton_Slip' and default_name = 'Default Print Type'");
            int default_print_type_id = int.Parse(dt.Rows[0]["Default_Value"].ToString());
            this.firmDetailsCB.SelectedIndex= FindEndSubStringIndex(this.firmDetailsCB, "ID:" + default_print_type_id.ToString());

            this.printFormatCB.DropDownStyle = ComboBoxStyle.DropDownList;//Create a drop-down list
            this.printFormatCB.AutoCompleteSource = AutoCompleteSource.ListItems;
            this.printFormatCB.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            this.printFormatCB.SelectedIndex = 0; //default print type
        }   

        //user
        private void load_batch()
        {
            int batch_no = -1;
            int voucher_id = -1;
            if (dataGridView1.SelectedRows.Count > 0)
            {
                int index = this.dataGridView1.SelectedRows[0].Index;
                if (index >= this.dataGridView1.Rows.Count)
                {
                    c.ErrorBox("Please select valid voucher", "Error");
                    return;
                }
                DataRow row = (dataGridView1.Rows[index].DataBoundItem as DataRowView).Row;
                batch_no = int.Parse(row["Batch_No"].ToString());
                voucher_id = int.Parse(row["Production_Voucher_ID"].ToString());
            }
            if (dataGridView2.SelectedRows.Count > 0)
            {
                if (dataGridView2.Columns.Count <= 2) return;
                int index = this.dataGridView2.SelectedRows[0].Index;
                if (index >= this.dataGridView2.Rows.Count)
                {
                    c.ErrorBox("Please select valid voucher", "Error");
                    return;
                }
                DataRow row = (dataGridView2.Rows[index].DataBoundItem as DataRowView).Row;
                batch_no = int.Parse(row["Batch_No"].ToString());
                voucher_id = int.Parse(row["Production_Voucher_ID"].ToString());
            }
            if (batch_no == -1 || voucher_id == -1)
            {
                //c.ErrorBox("Invalid Batch, couldnt fetch voucher", "Error");
                return;
            }
            DataTable voucher = c.getProductionVoucherTable_VoucherID(voucher_id);

            //dt3
            this.dt3.Clear();
            string[] batch_nos = c.csvToArray(voucher.Rows[0]["Batch_No_Arr"].ToString());
            string[] batch_fiscal_years = c.csvToArray(voucher.Rows[0]["Batch_Fiscal_Year_Arr"].ToString());
            int batch_index = -1;
            for (int i = 0; i < batch_nos.Length; i++)
            {
                float batch_wt = float.Parse(c.getColumnBatchNo("Net_Weight", int.Parse(batch_nos[i]), batch_fiscal_years[i]));
                dt3.Rows.Add(i + 1, batch_nos[i], batch_fiscal_years[i], batch_wt);
                if (batch_no.ToString() == batch_nos[i]) batch_index = i;
            }
            dataGridView3.DataSource = this.dt3;
            this.dataGridView3.RowsDefaultCellStyle.BackColor = Color.White;
            this.dataGridView3.RowsDefaultCellStyle.SelectionBackColor = Color.White;

            if (batch_index >= 0)
            {
                DataGridViewRow r = (DataGridViewRow)dataGridView3.Rows[batch_index];
                r.DefaultCellStyle.BackColor = Color.LightGray;
                r.DefaultCellStyle.SelectionBackColor = Color.LightGray;
            }
            if (this.dataGridView3.SelectedRows.Count >= 0) this.dataGridView3.Rows[0].Selected = false;

            //dt4
            this.dt4.Clear();
            string[] carton_nos = c.csvToArray(voucher.Rows[0]["Carton_No_Production_Arr"].ToString());
            this.carton_fiscal_year = voucher.Rows[0]["Carton_Fiscal_Year"].ToString(); //set carton_fiscal_year

            DataTable carton_info = c.getTableData("Carton_Produced", "Net_Weight, Printed", "Carton_No IN (" +
                c.removecom(voucher.Rows[0]["Carton_No_Production_Arr"].ToString()) + ") AND Fiscal_Year='" + carton_fiscal_year + "'");

            for (int i = 0; i < carton_info.Rows.Count; i++)
            {
                float net_wt = float.Parse(carton_info.Rows[i]["Net_Weight"].ToString());
                dt4.Rows.Add(i + 1, carton_nos[i], net_wt, this.carton_fiscal_year);
                dataGridView4.DataSource = this.dt4;
                if (ispresent(dataGridView5, carton_nos[i], 1, this.carton_fiscal_year, 3) >= 0)
                {
                    dataGridView4.Rows[dataGridView4.Rows.Count - 1].DefaultCellStyle.BackColor = selected_color;
                    dataGridView4.Rows[dataGridView4.Rows.Count - 1].DefaultCellStyle.SelectionBackColor = selected_color;
                }
                int printed = 0;
                if(carton_info.Rows[i]["Printed"].ToString()!="")
                {
                    printed = int.Parse(carton_info.Rows[i]["Printed"].ToString());
                }
                if (printed > 0)
                {
                    dataGridView4.Rows[dataGridView4.Rows.Count - 1].DefaultCellStyle.BackColor = printed_color;
                    dataGridView4.Rows[dataGridView4.Rows.Count - 1].DefaultCellStyle.SelectionBackColor = printed_color;
                }
            }
            if (this.dataGridView4.SelectedRows.Count >= 0) this.dataGridView4.Rows[0].Selected = false;
        }
        int ispresent(DataGridView d, string carton_no, int cin, string fiscal_year, int fin)
        {
            for (int i = 0; i < d.Rows.Count; i++)
            {
                if (d.Rows[i].Cells[cin].Value.ToString() == carton_no && d.Rows[i].Cells[fin].Value.ToString() == fiscal_year)
                {
                    return i;
                }
            }
            return -1;
        }
        private void search_batch()
        {
            int batch_no;
            string fiscal_year;
            try
            {
                batch_no = int.Parse(batchnoTextbox.Text);
            }
            catch
            {
                c.ErrorBox("Please Enter Numeric Batch Number", "Error");
                return;
            }
            fiscal_year = fiscalCombobox.Text;
            DataTable result = c.getBatchTable_BatchNoState(batch_no, 3, fiscal_year);
            if (result.Rows.Count == 0)
            {
                this.dt2.Clear();
                this.dt2.Columns.Clear();
                this.dt2.Columns.Add("Not Found");
                this.dt2.Rows.Add("No Conned Batch Found with the following details: Batch Number=" + batch_no + " and Financial Year=" + fiscal_year);
                this.dataGridView2.Columns[0].Width = 800;
                this.dataGridView2.DataSource = dt2;
                dataGridView2.Columns["Not Found"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            }
            else
            {
                this.dt2 = result;
                this.dataGridView2.DataSource = dt2;
                dataGridView2.Columns.OfType<DataGridViewColumn>().ToList().ForEach(col => col.Visible = false);
                this.dataGridView2.Columns["Batch_No"].Visible = true;
                this.dataGridView2.Columns["Colour"].Visible = true;
                this.dataGridView2.Columns["Quality"].Visible = true;
                this.dataGridView2.Columns["Date_Of_Production"].Visible = true;
                this.dataGridView2.Columns["Net_Weight"].Visible = true;
                this.dataGridView2.Columns["Number_Of_Trays"].Visible = true;
                this.dataGridView2.Columns["Fiscal_Year"].Visible = true;

                this.dataGridView2.Columns["Fiscal_Year"].HeaderText = "Financial Year";
                this.dataGridView2.Columns["Number_Of_Trays"].HeaderText = "Number of Trays";
                this.dataGridView2.Columns["Batch_No"].HeaderText = "Batch Number";
                this.dataGridView2.Columns["Date_Of_Production"].HeaderText = "Date of Production";

                this.dataGridView2.Columns["Batch_No"].Width = 70;
                this.dataGridView2.Columns["Number_Of_Trays"].Width = 80;

                dataGridView2.Columns["Fiscal_Year"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            }

            this.dataGridView2.Rows[0].Selected = false;
            return;
        }
        private void load_cartons()
        {
            for (int i = 0; i < dataGridView4.Rows.Count; i++)
            {
                if (dataGridView4.Rows[i].Selected == false) continue;
                DataGridViewRow row = dataGridView4.Rows[i];
                if (ispresent(dataGridView5, row.Cells[1].Value.ToString(), 1, row.Cells[3].Value.ToString(), 3) == -1)
                {
                    this.dataGridView5.Rows.Add("", row.Cells[1].Value, row.Cells[2].Value, row.Cells[3].Value);
                    if (row.DefaultCellStyle.BackColor == printed_color)
                    {
                        dataGridView5.Rows[dataGridView5.RowCount - 1].DefaultCellStyle.BackColor = printed_color;
                        dataGridView5.Rows[dataGridView5.RowCount - 1].DefaultCellStyle.SelectionBackColor = printed_color;
                    }
                    else
                    {
                        //set selected rows in dgv4 
                        row.DefaultCellStyle.BackColor = selected_color;
                        row.DefaultCellStyle.SelectionBackColor = selected_color;
                    }
                    row.Selected = false;
                }
            }
        }
        int DropDownWidth(ComboBox myCombo)
        {
            int maxWidth = 0;
            int temp = 0;
            Label label1 = new Label();

            foreach (var obj in myCombo.Items)
            {
                label1.Text = obj.ToString();
                temp = label1.PreferredWidth;
                if (temp > maxWidth)
                {
                    maxWidth = temp;
                }
            }
            label1.Dispose();
            return maxWidth;
        }
        public int FindEndSubStringIndex(ComboBox combo, string subString)
        {
            // Sanity check parameters
            if (combo == null) throw new ArgumentNullException("combo");
            if (subString == null)
            {
                return -1;
            }

            for (int index = 0; index < combo.Items.Count; index++)
            {
                object obj = combo.Items[index];
                if (obj == null) continue;
                string item = obj.ToString();
                if (string.IsNullOrWhiteSpace(item) && string.IsNullOrWhiteSpace(subString)) //both null/white
                    return index;
                int indexInItem = item.IndexOf(subString);
                if (indexInItem >= 0)
                    return index;
            }

            return -1;
        }
        IEnumerable<string> WrapString(string str, Graphics g, Font font, int allowedWidth)
        {
            string[] arr = str.Split(' ');
            StringBuilder current = new StringBuilder();
            foreach (string token in arr)
            {
                // TODO: You'll have to fix this, might break in marginal cases
                int width =
                  (int)g.MeasureString(current.ToString() + " " + token, font).Width;
                if (width > allowedWidth)
                {
                    yield return current.ToString();
                    current.Clear();
                }
                current.Append(token + " ");
            }
            yield return current.ToString();
        }
        private void firmDetailsCB_DrawItem(object sender, DrawItemEventArgs e)
        {
            ComboBox box = sender as ComboBox;
            //Brush backgroundBrush, forgroundBrush;
            //if (e.State == (DrawItemState.Selected |
            //            DrawItemState.NoAccelerator | DrawItemState.NoFocusRect) ||
            //    e.State == DrawItemState.Selected)
            //{
            //    forgroundBrush = Brushes.Black;
            //    backgroundBrush = Brushes.White;
            //}
            //else
            //{
            //    forgroundBrush = Brushes.White;
            //    backgroundBrush = Brushes.Black;
            //}

            //// some way to wrap the string (on a space)           
            //string str = (string)box.Items[e.Index];

            //Rectangle rc = new Rectangle(e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height);
            //e.Graphics.FillRectangle(forgroundBrush, rc);

            //int stringHeight = (int)e.Graphics.MeasureString(str, box.Font).Height;
            //int lineNo = 0;
            //foreach (string line in WrapString(str, e.Graphics, box.Font, e.Bounds.Width))
            //{
            //    e.Graphics.DrawString(line, box.Font, backgroundBrush, new PointF(0, lineNo * stringHeight + 5));
            //    lineNo++;
            //}
            if (e.Index < 0)
                return;

            // draw a lighter blue selected BG colour, the dark blue default has poor contrast with black text on a dark blue background
            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
                e.Graphics.FillRectangle(Brushes.PowderBlue, e.Bounds);
            else
                e.Graphics.FillRectangle(Brushes.White, e.Bounds);

            // get the text of the item
            string text = box.Items[e.Index].ToString();

            // don't dispose the brush afterwards
            Brush b = Brushes.Black;
            e.Graphics.DrawString(text, this.Font, b, e.Bounds, StringFormat.GenericDefault);

            // draw a light grey border line to separate the items
            Pen p = new Pen(Brushes.Gainsboro, 1);
            e.Graphics.DrawLine(p, new Point(e.Bounds.Left, e.Bounds.Bottom - 1), new Point(e.Bounds.Right, e.Bounds.Bottom - 1));
            p.Dispose();

            e.DrawFocusRectangle();
        }

        private void searchButton_Click(object sender, EventArgs e)
        {
            search_batch();
        }
        private void printButton_Click(object sender, EventArgs e)
        {
            if (dataGridView5.RowCount== 0)
            {
                c.ErrorBox("Please Select Cartons to Print", "Error");
                return;
            }
            this.cartons_to_print.Clear();
            this.cartons_to_print_fiscal_year.Clear();
            int rows = dataGridView5.Rows.Count;
            string sql = "begin transaction; begin try;\n";
            for (int i = 0; i < dataGridView5.RowCount; i++)
            {
                int carton = int.Parse(dataGridView5.Rows[i].Cells[dataGridView5.Columns["Carton No."].Index].Value.ToString());
                string fiscal_year = dataGridView5.Rows[i].Cells[dataGridView5.Columns["Financial Year"].Index].Value.ToString();
                this.cartons_to_print.Add(carton);
                this.cartons_to_print_fiscal_year.Add(fiscal_year);
                sql += "UPDATE Carton_Produced SET Printed=1 WHERE Carton_No = '"+carton+"' AND Fiscal_Year = '"+fiscal_year+"';\n";
            }
            sql += "commit transaction; end try BEGIN CATCH rollback transaction; ";
            sql += "DECLARE @ErrorMessage NVARCHAR(4000); DECLARE @ErrorSeverity INT; DECLARE @ErrorState INT; SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE(); ";
            sql += "RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState); END CATCH; ";
            DataTable d = c.runQuery(sql); //set print 

            //Load firmDetails in dictionary
            int print_type_id = int.Parse(firmDetailsCB.SelectedItem.ToString().Split(':')[1]);
            for(int i=0; i< dt_printTypes.Rows.Count; i++)
            {
                if(dt_printTypes.Rows[i]["Print_Type_ID"].ToString()==print_type_id.ToString())
                {
                    for(int j=0; j<dt_printTypes.Columns.Count-1; j++)
                    {
                        firmDetails[dt_printTypes.Columns[j].ColumnName] = dt_printTypes.Rows[i][j].ToString();
                    }
                    break;
                }
            }

            this.slip_count = cartons_to_print.Count;
            using (var dlg = new CoolPrintPreviewDialog())
            {
                dlg.Document = this.printDocument1;
                dlg.WindowState = FormWindowState.Maximized;
                dlg.ShowDialog(this);
            }
            //printPreviewDialog1.ShowDialog();
        }
        private void batchButton_Click(object sender, EventArgs e)
        {
            load_batch();
        }
        private void batchnoTextbox_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode==Keys.Enter) search_batch();
        }
        private void loadcartonButton_Click(object sender, EventArgs e)
        {
            this.load_cartons();
        }

        //dgv
        private void dataGridView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                load_batch();
                e.Handled = true;
            }
        }
        private void dataGridView2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                load_batch();
                e.Handled = true;
            }
        }
        private void dataGridView4_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                load_cartons();
                e.Handled = true;
            }
        }
        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int count = dataGridView5.SelectedRows.Count;
            for (int i = 0; i < count; i++)
            {
                DataGridViewRow row = dataGridView5.SelectedRows[0];
                int index = ispresent(dataGridView4, row.Cells[1].Value.ToString(), 1, row.Cells[3].Value.ToString(), 3);
                if (index >= 0 && row.DefaultCellStyle.BackColor != printed_color)
                {
                    dataGridView4.Rows[index].DefaultCellStyle.BackColor = Color.White;
                    dataGridView4.Rows[index].DefaultCellStyle.SelectionBackColor = Color.White;
                }
                dataGridView5.Rows.RemoveAt(dataGridView5.SelectedRows[0].Index);
            }
        }
        private void dataGridView5_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            if (e.RowIndex <= dataGridView5.Rows.Count - 1 && e.RowIndex >= 0)
            {
                dataGridView5.Rows[e.RowIndex].Cells[0].Value = e.RowIndex + 1;
                return;
            }
        }
        private void dataGridView1_Click(object sender, EventArgs e)
        {
            if (dataGridView2.SelectedRows.Count != 0) dataGridView2.SelectedRows[0].Selected = false;
        }
        private void dataGridView2_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count != 0) dataGridView1.SelectedRows[0].Selected = false;
        }

        //print
        private void printDocument1_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            //set margins and initial setup
            this.topmargin = (int)(0.03 * e.PageBounds.Height);
            this.lrmargin = (int)(0.045 * e.PageBounds.Width);
            Graphics g = e.Graphics;
            int slip_width = (e.PageBounds.Width - 2 * lrmargin) / 2;
            int slip_height = (e.PageBounds.Height - 2 * topmargin) / 3;

            int[] w = new int[6]; //stores the x coordinates of the 6 slips
            w[0] = lrmargin - 5;
            w[1] = w[0] + slip_width + 10;
            w[2] = lrmargin - 5;
            w[3] = w[0] + slip_width + 10;
            w[4] = lrmargin - 5;
            w[5] = w[0] + slip_width + 10;
            int running_y = topmargin - 10;
            int local_print_count = 0;

            while (this.slip_count - this.printed_slips > 0)
            {
                write_slip(e, w[local_print_count], running_y, slip_width, slip_height, this.cartons_to_print[this.printed_slips], this.cartons_to_print_fiscal_year[this.printed_slips]);
                if (local_print_count == 1 || local_print_count == 3) running_y += slip_height + 10;
                if (local_print_count == 5)
                {
                    this.printed_slips++;
                    break;
                }
                local_print_count++;
                this.printed_slips++;
            }
            if (this.slip_count - this.printed_slips > 0) e.HasMorePages = true;
            else e.HasMorePages = false;
        }
        private void printDocument1_BeginPrint(object sender, PrintEventArgs e)
        {
            this.printed_slips = 0;
        }
        private void printDocument1_EndPrint(object sender, PrintEventArgs e)
        {
            this.printed_slips = 0;
        }
        int write_slip(System.Drawing.Printing.PrintPageEventArgs e, int x, int write_height, int width, int height, int carton_no, string fiscal_year)
        {
            if(this.printFormatCB.SelectedItem.ToString()=="1")
            {
                font_name = "generic sans serif";
                int y = write_height;
                const int basic_size = 9;
                int gap = 5;
                //Draw rect
                e.Graphics.DrawRectangle(Pens.Black, x, write_height, width, height);
                DataRow carton_data = c.getProducedCartonRow(carton_no.ToString(), fiscal_year);
                //header
                write_height += 6;
                write(e, x + (int)(0.02 * width), write_height, (int)(0.40 * width), "Phone No : " + this.firmDetails["Phone_Number"], basic_size - 2, 'r', 0, 0);
                write(e, x + (int)(0.42 * width), write_height - 2, (int)(0.16 * width), "||Shri||", basic_size, 'c', 0, 0);
                write_height += write(e, x + (int)(0.58 * width), write_height, (int)(0.40 * width), this.firmDetails["Email_ID"], basic_size - 2, 'l', 0) + gap;
                write_height += write(e, x, write_height, width, this.firmDetails["Firm_Name"], basic_size + 8, 'c', 1) + gap;
                e.Graphics.DrawLine(new Pen(Color.Black, 2), x, write_height, x + width, write_height);
                ////main
                write_height += gap;
                write(e, x + (int)(0.02 * width), write_height, (int)(0.40 * width), "CARTON NO : ", basic_size + 6, 'l', 0, 0);
                write_height += write(e, x + (int)(0.42 * width), write_height - 2, (int)(0.50 * width), carton_data["Carton_No"].ToString(), basic_size + 8, 'l', 1, 0);
                e.Graphics.DrawLine(new Pen(Color.Black, 1), x, write_height, x + width, write_height);

                write_height += gap;
                write(e, x + (int)(0.02 * width), write_height, (int)(0.17 * width), "Quality", basic_size + 3, 'l', 0, 0);
                write(e, x + (int)(0.19 * width), write_height - 1, (int)(0.51 * width), ": " + carton_data["Quality"].ToString(), basic_size + 4, 'l', 1, 0);
                write(e, x + (int)(0.70 * width), write_height, (int)(0.15 * width), "Grade", basic_size + 3, 'l', 0, 0);
                write_height += write(e, x + (int)(0.85 * width), write_height, (int)(0.15 * width), ":" + carton_data["Grade"].ToString(), basic_size + 3, 'l', 1, 0);
                //carton_data["Colour"].ToString()
                write(e, x + (int)(0.02 * width), write_height, (int)(0.23 * width), "Shade", basic_size + 3, 'l', 0, 0);
                write_height += write(e, x + (int)(0.25 * width), write_height - 1, (int)(0.73 * width), ": " + carton_data["Colour"].ToString(), basic_size + 4, 'l', 1, 0) - 3;

                string unformatted_batch_nos = carton_data["Batch_No_Arr"].ToString();
                string batch_nos = unformatted_batch_nos.Substring(0, unformatted_batch_nos.Length - 1).Replace(",", ", ");
                write(e, x + (int)(0.02 * width), write_height, (int)(0.23 * width), "Batch No", basic_size + 3, 'l', 0, 0);
                write_height += write(e, x + (int)(0.25 * width), write_height, (int)(0.73 * width), ":  " + batch_nos, basic_size + 3, 'l', 0, 0) + gap;
                e.Graphics.DrawLine(new Pen(Color.Black, 1), x, write_height, x + width, write_height);
                e.Graphics.DrawLine(new Pen(Color.Black, 1), x + (width / 2), write_height, x + (width / 2), y + height - 70);

                write_height += gap;
                write(e, x + (int)(0.02 * width), write_height, (int)(0.23 * width), "Cheese", basic_size + 3, 'l', 0, 0);
                write(e, x + (int)(0.25 * width), write_height, (int)(0.25 * width), ":  " + carton_data["Number_Of_Cones"].ToString(), basic_size + 3, 'l', 0, 0);
                write(e, x + (int)(0.52 * width), write_height, (int)(0.13 * width), "Date", basic_size + 3, 'l', 0, 0);
                write_height += write(e, x + (int)(0.65 * width), write_height, (int)(0.33 * width), ":  " + carton_data["Date_Of_Production"].ToString().Substring(0, 10), basic_size + 3, 'l', 0, 0) + gap;
                e.Graphics.DrawLine(new Pen(Color.Black, 1), x, write_height, x + width, write_height);
                Barcode barcode = new Barcode();
                //barcode.IncludeLabel = true;
                //barcode.StandardizeLabel = true;
                string barcode_data = fiscal_year + " " + carton_no;
                Image img = barcode.Encode(TYPE.CODE128, barcode_data, Color.Black, Color.White, (int)(0.48 * width), 70);
                e.Graphics.DrawImage(img, new Rectangle(x + (int)(0.51 * width), write_height + 5, (int)(0.48 * width), 70));

                write_height += gap;
                write(e, x + (int)(0.02 * width), write_height, (int)(0.23 * width), "Gross Wt", basic_size + 3, 'l', 0, 0);
                write_height += write(e, x + (int)(0.25 * width), write_height, (int)(0.25 * width), ":  " + carton_data["Gross_Weight"].ToString(), basic_size + 3, 'l', 0, 0);
                string tare_wt = (float.Parse(carton_data["Carton_Weight"].ToString()) + int.Parse(carton_data["Number_Of_Cones"].ToString()) * float.Parse(carton_data["Cone_Weight"].ToString())).ToString("F3");
                write(e, x + (int)(0.02 * width), write_height, (int)(0.23 * width), "Tare Wt", basic_size + 3, 'l', 0, 0);
                write_height += write(e, x + (int)(0.25 * width), write_height, (int)(0.25 * width), ":  " + tare_wt, basic_size + 3, 'l', 0, 0);
                write(e, x + (int)(0.02 * width), write_height, (int)(0.23 * width), "Net Wt", basic_size + 3, 'l', 0, 0);
                write_height += write(e, x + (int)(0.25 * width), write_height - 2, (int)(0.25 * width), ": " + float.Parse(carton_data["Net_Weight"].ToString()).ToString("F3"), basic_size + 5, 'l', 1, 0);

                string hatch = c.getQualityColour(carton_data["Quality"].ToString());
                if (hatch != "")
                {
                    HatchStyle hs = (HatchStyle)Enum.Parse(typeof(HatchStyle), hatch, true);
                    using (HatchBrush hbr2 = new HatchBrush(hs, Color.Black, Color.White))
                    {
                        using (TextureBrush tbr = c.TBrush(hbr2))
                        {
                            tbr.ScaleTransform(2.50F, 2.50F);
                            e.Graphics.DrawRectangle(new Pen(Color.Black, 2), x + 1, y + height - 75, width - 2, 55);
                            e.Graphics.FillRectangle(tbr, x + 1, y + height - 75, width - 2, 55);
                        }
                    }
                }
                write(e, x + (int)(0.02 * width), y + height - 20, (int)(0.96 * width), "Note: Please do not mix two different Batches", basic_size, 'c', 0, 0);
            }
            else if(this.printFormatCB.SelectedItem.ToString()=="2")
            {
                this.font_name = "Cascadia Mono";
                int y = write_height;
                const int basic_size = 9;
                int gap = 5;
                //Draw rect
                e.Graphics.DrawRectangle(Pens.Black, x, write_height, width, height);
                
                //Get data
                DataRow carton_data = c.getProducedCartonRow(carton_no.ToString(), fiscal_year);
                string unformatted_batch_nos = carton_data["Batch_No_Arr"].ToString();
                string batch_nos = unformatted_batch_nos.Substring(0, unformatted_batch_nos.Length - 1).Replace(",", ", ");

                //header
                write_height += 4;
                write_height+=write(e, x + (int)(0.02 * width), write_height - 2, (int)(0.96 * width), "----------------\\\\OM//---------------", basic_size, 'c', 0, 0)-5;
                write_height += write(e, x, write_height, width, this.firmDetails["Firm_Name"], basic_size + 8, 'c', 1) + gap-2;
                e.Graphics.DrawLine(new Pen(Color.Black, 2), x, write_height, x + width, write_height);
                write_height += gap;
                
                ////main
                write(e, x + (int)(0.02 * width), write_height, (int)(0.60 * width), "CARTON NO : ", basic_size + 6, 'r', 0, 0);
                write_height += write(e, x + (int)(0.62 * width), write_height - 2, (int)(0.36 * width), carton_data["Carton_No"].ToString(), basic_size + 8, 'l', 1, 0);
                e.Graphics.DrawLine(new Pen(Color.Black, 1), x, write_height, x + width, write_height);
                write_height += gap;
                Barcode barcode = new Barcode();
                string barcode_data = fiscal_year + " " + carton_no;
                Image img = barcode.Encode(TYPE.CODE128, barcode_data, Color.Black, Color.White, (int)(0.48 * width), 70);
                e.Graphics.DrawImage(img, new Rectangle(x + (int)(0.03 * width), write_height + 4, (int)(0.40 * width), 60));
                write(e, x + (int)(0.45 * width), write_height, (int)(0.18 * width), "Cheese", basic_size + 3, 'l', 0, 0);
                write_height+=write(e, x + (int)(0.63 * width), write_height, (int)(0.37 * width), ": " + carton_data["Number_Of_Cones"].ToString(), basic_size + 3, 'l', 0, 0);
                write(e, x + (int)(0.45 * width), write_height, (int)(0.18 * width), "Date", basic_size + 3, 'l', 0, 0);
                write_height += write(e, x + (int)(0.63 * width), write_height, (int)(0.37 * width), ": " + carton_data["Date_Of_Production"].ToString().Substring(0, 10), basic_size + 3, 'l', 0, 0);
                write(e, x + (int)(0.45 * width), write_height, (int)(0.18 * width), "Grade", basic_size + 3, 'l', 0, 0);
                write_height += write(e, x + (int)(0.63 * width), write_height, (int)(0.37 * width), ": " + carton_data["Grade"].ToString(), basic_size + 3, 'l', 1, 0);
                write_height += gap;
                e.Graphics.DrawLine(new Pen(Color.Black, 1), x, write_height, x + width, write_height);
                write_height += gap;

                write(e, x + (int)(0.02 * width), write_height, (int)(0.25 * width), "Quality", basic_size + 3, 'l', 0, 0);
                write_height+=write(e, x + (int)(0.26 * width), write_height - 1, (int)(0.70 * width), ": " + carton_data["Quality"].ToString(), basic_size + 4, 'l', 1, 0);
                write(e, x + (int)(0.02 * width), write_height, (int)(0.25 * width), "Shade", basic_size + 3, 'l', 0, 0);
                write_height+=write(e, x + (int)(0.26 * width), write_height - 1, (int)(0.70 * width), ": " + carton_data["Colour"].ToString(), basic_size + 4, 'l', 1, 0);
                write_height += gap;
                e.Graphics.DrawLine(new Pen(Color.Black, 1), x, write_height, x + width, write_height);
                write_height += gap;

                string tare_wt = (float.Parse(carton_data["Carton_Weight"].ToString()) + int.Parse(carton_data["Number_Of_Cones"].ToString()) * float.Parse(carton_data["Cone_Weight"].ToString())).ToString("F3");
                write(e, x + (int)(0.02 * width), write_height, (int)(0.24 * width), "Batch No", basic_size + 3, 'l', 0, 0);
                write_height += write(e, x + (int)(0.26 * width), write_height, (int)(0.74 * width), ": " + batch_nos, basic_size + 3, 'l', 0, 0);
                
                write(e, x + (int)(0.02 * width), write_height, (int)(0.24 * width), "Gross Wt", basic_size + 3, 'l', 0, 0);
                write(e, x + (int)(0.26 * width), write_height, (int)(0.27 * width), ": " + carton_data["Gross_Weight"].ToString(), basic_size + 3, 'l', 0, 0);
                
                write(e, x + (int)(0.53 * width), write_height, (int)(0.22 * width), "Tare Wt", basic_size + 3, 'r', 0, 0);
                write_height += write(e, x + (int)(0.75 * width), write_height, (int)(0.26 * width), ": " + tare_wt, basic_size + 3, 'l', 0, 0);
                
                write(e, x + (int)(0.02 * width), write_height, (int)(0.24 * width), "Net Wt", basic_size + 3, 'l', 0, 0);
                write_height += write(e, x + (int)(0.26 * width), write_height - 2, (int)(0.74* width), ": " + float.Parse(carton_data["Net_Weight"].ToString()).ToString("F3"), basic_size + 5, 'l', 1, 0);
                
                e.Graphics.DrawLine(new Pen(Color.Black, 1), x, write_height, x + width, write_height);
                write_height += gap;

                write_height += 4;

                write_height += write(e, x + (int)(0.02 * width), write_height, (int)(0.96 * width), "Note: Please do not mix two different Batches", basic_size, 'c', 0, 0);
                write_height += gap;
                e.Graphics.DrawLine(new Pen(Color.Black, 1), x, write_height, x + width, write_height);
                write_height += gap;
                write(e, x + (int)(0.02 * width), y + height - 20, (int)(0.48 * width), this.firmDetails["Email_ID"], basic_size - 2, 'l', 0);
                write_height+=write(e, x + (int)(0.52 * width), y + height - 20, (int)(0.48 * width), "Phone No : " + this.firmDetails["Phone_Number"], basic_size - 2, 'r', 0, 0);
            }
            return 0;
        }

        int drawDGV(int write_height, System.Drawing.Printing.PrintPageEventArgs e)
        {
            //inputs write_height(not including the margin)
            //returns the next write height(not including the margin)
            Graphics g = e.Graphics;
            //Print Datagridview
            #region
            StringFormat str = new StringFormat();
            str.Alignment = StringAlignment.Near;
            str.LineAlignment = StringAlignment.Center;
            str.Trimming = StringTrimming.EllipsisCharacter;
            Pen p = new Pen(Color.Black, 2.5f);

            //rescale widths of datagridview
            int total_width = 0;
            for (int j = 0; j < dataGridView1.Columns.Count; j++) total_width += dataGridView1.Columns[j].Width;

            int new_total = e.PageBounds.Width - 2 * lrmargin;
            float scale = (float)new_total / total_width;
            int[] column_widths = new int[dataGridView1.Columns.Count];
            int new_total_real = 0;
            for (int j = 0; j < dataGridView1.Columns.Count; j++)
            {
                column_widths[j] = (int)(scale * (float)dataGridView1.Columns[j].Width);
                new_total_real += column_widths[j];
            }
            column_widths[dataGridView1.Columns.Count - 1] += new_total - new_total_real;
            int start_height = topmargin + write_height;
            int start_width = lrmargin;
            int width = start_width;
            Font newFont = new Font(dataGridView1.Font.FontFamily, dataGridView1.Font.Size, FontStyle.Bold);
            for (int j = 0; j < dataGridView1.Columns.Count; j++)
            {
                str.Alignment = StringAlignment.Center;
                //draws the first cell of column
                g.FillRectangle(Brushes.LightGray, new Rectangle(width, start_height, column_widths[j], dataGridView1.Rows[0].Height));
                g.DrawRectangle(Pens.Black, width, start_height, column_widths[j], dataGridView1.Rows[0].Height);
                g.DrawString(dataGridView1.Columns[j].HeaderText, newFont, Brushes.Black, new RectangleF(width, start_height, column_widths[j], dataGridView1.Rows[0].Height), str);
                if (j == 0) write_height += dataGridView1.Rows[0].Height;
                newFont = new Font(dataGridView1.Font.FontFamily, dataGridView1.Font.Size, FontStyle.Regular);
                str.Alignment = StringAlignment.Far;
                int height = start_height;
                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    if (i == dataGridView1.Rows.Count - 1)
                    {
                        newFont = new Font(dataGridView1.Font.FontFamily, dataGridView1.Font.Size, FontStyle.Bold);
                    }
                    height += dataGridView1.Rows[i].Height;
                    if (j == 0) write_height += dataGridView1.Rows[i].Height;
                    g.DrawRectangle(Pens.Black, width, height, column_widths[j], dataGridView1.Rows[0].Height);
                    g.DrawString(dataGridView1.Rows[i].Cells[j].Value.ToString(), newFont, Brushes.Black, new RectangleF(width, height, column_widths[j], dataGridView1.Rows[0].Height), str);
                }
                width += column_widths[j];
            }
            #endregion
            return write_height;
        }
        private int write(System.Drawing.Printing.PrintPageEventArgs e, int x, int y, int width, string text, int size, char lr = 'c', int bold = 0, int drawrect = 0)
        {
            Graphics g = e.Graphics;
            StringFormat format = new StringFormat();
            format.LineAlignment = StringAlignment.Center;
            if (width == 0)
            {
                format.Alignment = StringAlignment.Center;
            }
            else if (lr == 'l')
            {
                format.Alignment = StringAlignment.Near;
            }
            else if (lr == 'r')
            {
                format.Alignment = StringAlignment.Far;
            }
            else if (lr == 'c')
            {
                format.Alignment = StringAlignment.Center;
            }

            SolidBrush myBrush = new SolidBrush(Color.Black);
            Font newFont;
            if (bold == 1)
            {
                newFont = new Font(this.font_name, size, FontStyle.Bold);
            }
            else
            {
                newFont = new Font(this.font_name, size, FontStyle.Regular);
            }
            if (width == 0) width = e.PageBounds.Width - 2 * lrmargin;

            g.DrawString(text, newFont, myBrush, new RectangleF(x, y, width, newFont.Height + 5), format);
            if (drawrect == 1)
            {
                g.DrawRectangle(Pens.Black, x, y, width, newFont.Height + 5);
            }
            return newFont.Height;
        }
    }
}
