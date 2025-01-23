using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LibraryManagementSystem
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            this.WindowState = FormWindowState.Maximized; 
            this.FormBorderStyle = FormBorderStyle.None;   
            this.StartPosition = FormStartPosition.CenterScreen;
          this.Load += new EventHandler(MainForm_Load);

        }
        
    
        private void label1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void logout_btn_Click(object sender, EventArgs e)
        {
            DialogResult check = MessageBox.Show("Are you sure you want to logout?", "Confirmation Message", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if(check == DialogResult.Yes)
            {
                LoginForm lForm = new LoginForm();
                lForm.Show();
                this.Hide();
            }

        }

        private void dashboard_btn_Click(object sender, EventArgs e)
        {
            dashboard1.Visible = true;
            addBooks2.Visible = false;
            returnBooks1.Visible = false;
            issueBooks1.Visible = false;
            regmember1.Visible = false;
            viewLibmember1.Visible = false;
            AddBooks aForm = addBooks2 as AddBooks;
            if (aForm != null)
            {
                aForm.refreshData();
            }
        }

        private void addBooks_btn_Click(object sender, EventArgs e)
        {
            dashboard1.Visible = false;
            addBooks2.Visible = true;
            returnBooks1.Visible = false;
            issueBooks1.Visible = false;
            regmember1.Visible = false;
            viewLibmember1.Visible = false;
            AddBooks aForm = addBooks2 as AddBooks;
            if(aForm != null)
            {
                aForm.refreshData();
            }
        }

        private void issueBooks_btn_Click(object sender, EventArgs e)
        {
            dashboard1.Visible = false;
            addBooks2.Visible = false;
            returnBooks1.Visible = false;
            issueBooks1.Visible = true;
            regmember1.Visible = false;
            viewLibmember1.Visible = false;
            ReturnBooks rForm = returnBooks1 as ReturnBooks;
            if (rForm != null)
            {
                rForm.refreshData();
            }
        }

        private ReturnBooks issueUserControl = new ReturnBooks();
        public void MainControl(ReturnBooks control)
        {
            InitializeComponent();
            this.issueUserControl = control;
        }
        private void returnBooks_btn_Click(object sender, EventArgs e)
        {
            dashboard1.Visible = false;
            addBooks2.Visible = false;
            returnBooks1.Visible = true;
            issueBooks1.Visible = false;
            regmember1.Visible = false;
            viewLibmember1.Visible = false;
            ReturnBooks iForm = returnBooks1 as ReturnBooks;
            if (iForm != null)
            {
                iForm.refreshData();
            }
            DataIssueBooks dataIssueBooks = new DataIssueBooks();
            List<DataIssueBooks> returnBooksData = dataIssueBooks.ReturnIssueBooksData();

            // Convert List<DataIssueBooks> to DataTable
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("ID");
            dataTable.Columns.Add("IssueID");
            dataTable.Columns.Add("Name");
            dataTable.Columns.Add("BookID");
            dataTable.Columns.Add("BookTitle");
            dataTable.Columns.Add("DateIssue");
            dataTable.Columns.Add("DateReturn");
            dataTable.Columns.Add("Status");
            ;

            foreach (var item in returnBooksData)
            {
                DataRow row = dataTable.NewRow();
                row["ID"] = item.ID;
                row["IssueID"] = item.IssueID;
                row["Name"] = item.Name;
                row["BookID"] = item.BookID;
                row["BookTitle"] = item.BookTitle;
                row["DateIssue"] = item.DateIssue;
                row["DateReturn"] = item.DateReturn;
                row["Status"] = item.Status;

                dataTable.Rows.Add(row);
            }

            issueUserControl.RefreshGridView();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            dashboard1.Visible = false;
            addBooks2.Visible = false;
            returnBooks1.Visible = false;
            issueBooks1.Visible = false;
            regmember1.Visible = true;
            viewLibmember1.Visible = false;
            ReturnBooks iForm = returnBooks1 as ReturnBooks;
            if (iForm != null)
            {
                iForm.refreshData();
            }
        }

        private void greet_label_Click(object sender, EventArgs e)
        {

        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void dashboard1_Load(object sender, EventArgs e)
        {

        }
        private void MainForm_Load(object sender, EventArgs e)
        {
           
            this.label1.Location = new System.Drawing.Point(panel1.Width - this.label1.Width - 10, 10);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.dashboard1.Location = new System.Drawing.Point(-6, -6); 
            this.addBooks2.Location = new System.Drawing.Point(-6, -6); 
             this.returnBooks1.Location = new System.Drawing.Point(-6, -6);
             this.issueBooks1.Location = new System.Drawing.Point(-6, -6);
             this.regmember1.Location = new System.Drawing.Point(-6, -6);
             this.viewLibmember1.Location = new System.Drawing.Point(-6, -6); 
              int additionalStretchPixels = 114; 

            int stretchWidth = this.ClientSize.Width - this.dashboard1.Left + 10 + additionalStretchPixels; 
            this.dashboard1.Size = new System.Drawing.Size(stretchWidth, this.ClientSize.Height - 30);
            this.addBooks2.Size = new System.Drawing.Size(stretchWidth, this.ClientSize.Height - 30);
            this.returnBooks1.Size = new System.Drawing.Size(stretchWidth, this.ClientSize.Height - 30);
            this.issueBooks1.Size = new System.Drawing.Size(stretchWidth, this.ClientSize.Height - 30);
            this.regmember1.Size = new System.Drawing.Size(stretchWidth, this.ClientSize.Height - 30);
            this.viewLibmember1.Size = new System.Drawing.Size(stretchWidth, this.ClientSize.Height - 30);
            
            this.dashboard1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            this.addBooks2.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            this.returnBooks1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            this.issueBooks1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            this.regmember1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            this.viewLibmember1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;

        }

        private void addBooks1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            dashboard1.Visible = false;
            addBooks2.Visible = false;
            returnBooks1.Visible = false;
            issueBooks1.Visible = false;
            regmember1.Visible = false;
            viewLibmember1.Visible = true;
            viewLibmember iForm = viewLibmember1 as viewLibmember;
            if (iForm != null)
            {
                iForm.LoadMembers();
            }
            
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void logout_btn_Click_1(object sender, EventArgs e)
        {
            DialogResult check = MessageBox.Show("Are you sure you want to logout?", "Confirmation Message", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (check == DialogResult.Yes)
            {
                LoginForm lForm = new LoginForm();
                lForm.Show();
                this.Hide();
            }
        }

        private void dashboard_btn_Click_1(object sender, EventArgs e)
        {

        }

        private void addBooks_btn_Click_1(object sender, EventArgs e)
        {

        }

        private void issueBooks_btn_Click_1(object sender, EventArgs e)
        {

        }

        private void returnBooks_btn_Click_1(object sender, EventArgs e)
        {

        }

        private void regMemBTN_Click(object sender, EventArgs e)
        {

        }

        private void btnview_Click(object sender, EventArgs e)
        {

        }

        private void logout_btn_Click_2(object sender, EventArgs e)
        {

        }

        private void label1_Click_1(object sender, EventArgs e)
        {

        }
     
 
        }

    }
//by 2019-2020 third year senior student group(UCSICONIC) 
//THET PAING PHYO(LEADER)
//AIN JINN MOE
//KHIN MYO THIRI
//THU SHIN NWE
//THU HTOO SAN
//MAY YADANAR BO