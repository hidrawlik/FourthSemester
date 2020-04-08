﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DB_Requester
{
    public partial class MainForm : Form
    {
        public DBRequest UserReq;

        private DataTable RequestTab;

        private DataTable StructTab;

        private string LastTabName;

        public MainForm()
        {
            InitializeComponent();
        }

        public void UpdateDatGridDBTables()
        {
            StructTab.Clear();
            string sql = "SELECT [Name] FROM sys.tables WHERE type_desc = 'USER_TABLE'";
            SqlDataAdapter adapter = UserReq.GetAdapter(sql);
            DataSet ds = new DataSet();
            adapter.Fill(ds);
            StructTab = new DataTable("TabFields");
            StructTab = ds.Tables[0];
            StructTab.Columns[0].ColumnName = "Tables";
            StructTab.Columns.Add("Fields", Type.GetType("System.String"));

            for (int i = 0; i < StructTab.Rows.Count; i++)
            {
                StructTab.Rows[i][1] = UserReq.GetTableFields((string)StructTab.Rows[i][0]);
            }

            datGridDBTables.DataSource = StructTab;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            UserReq = new DBRequest();
            StructTab = new DataTable("TabFields");
            //DataColumn NewDatCol = new DataColumn("Tables", Type.GetType("System.String"));
            //NewDatCol.AllowDBNull = false;
            //NewDatCol.Unique = true;
            //StructTab.Columns.Add(NewDatCol);
            //NewDatCol = new DataColumn("Fields", Type.GetType("System.String"));
            //NewDatCol.AllowDBNull = false;
            //NewDatCol.DefaultValue = "none;";
            //StructTab.Columns.Add(NewDatCol);
            //datGridDBTables.DataSource = StructTab;
            //datGridDBTables.ReadOnly = false;
            //datGridSQLResult.DataSource = RequestTab;
            StructTab.RowChanged += new DataRowChangeEventHandler(StructTab_OnRowChanged);
        }

        private void StructTab_OnRowChanged(object sender, DataRowChangeEventArgs e)
        {
            try
            {
                if (LastTabName != (string)e.Row["Tables"])
                {
                    LastTabName = (string)e.Row["Tables"];
                    string Fields = UserReq.GetTableFields(LastTabName);
                    e.Row["Fields"] = Fields;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Connection Error", MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            }
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            bool Connected = false;
            Cursor = Cursors.WaitCursor;

            try
            {
                UserReq.Disconnect();
                UserReq.ConnectTo(tbDatSource.Text, tbInitCat.Text);
                Connected = true;
            }
            catch (Exception e1)
            {
                MessageBox.Show(this, e1.Message, "Connection Error", MessageBoxButtons.OK,
                MessageBoxIcon.Error);
                Connected = false;
            }


            if (Connected)
            {
                tbRequest.Enabled = true;
                btnRequest.Enabled = true;
                datGridSQLResult.Enabled = true;
                datGridDBTables.Enabled = true;

                UpdateDatGridDBTables();
            }
            else
            {
                tbRequest.Enabled = false;
                btnRequest.Enabled = false;
                datGridSQLResult.Enabled = false;
                datGridDBTables.Enabled = false;
            }
            Cursor = Cursors.Arrow;
            if (RequestTab != null)
            {
                try
                {
                    //StructTab.Clear();
                    RequestTab.Clear();
                }
                catch (Exception)
                {
                }
            }
        }

        private void btnRequest_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            try
            {
                RequestTab = UserReq.SQLRequest(tbRequest.Text);
                datGridSQLResult.DataSource = RequestTab;

                var ReqText = tbRequest.Text.ToLower();
                if (ReqText.StartsWith("create") || ReqText.StartsWith("drop") || ReqText.StartsWith("update"))
                {
                    UpdateDatGridDBTables();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Request Error", MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            }
            Cursor = Cursors.Arrow;
        }

        private void datGridDBTables_MouseUp(object sender, MouseEventArgs e)
        {
            DataGridView.HitTestInfo HitInfo = datGridDBTables.HitTest(e.X, e.Y);
            if ((HitInfo.RowIndex >= 0) && (HitInfo.RowIndex < StructTab.Rows.Count))
            {
                tbRequest.Text = "SELECT * FROM " + (string)StructTab.Rows[HitInfo.RowIndex]["Tables"];
                btnRequest_Click(this, null);
            }
        }

        private void TbDatSource_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                e.Handled = true;
                tbInitCat.Focus();
            }
        }

        private void TbInitCat_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                e.Handled = true;
                btnConnect.Focus();
                btnConnect.PerformClick();
            }
        }

        private void TbRequest_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                e.Handled = true;
                btnRequest.Focus();
                btnRequest.PerformClick();
            }
        }
    }
}
