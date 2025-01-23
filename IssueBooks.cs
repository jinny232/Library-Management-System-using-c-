using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using AForge.Video;
using AForge.Video.DirectShow;
using ZXing;
using System.IO;

namespace LibraryManagementSystem
{
    public partial class IssueBooks : UserControl
    {
        SqlConnection connect = new SqlConnection(@"Data Source=(LocalDB)\v11.0;AttachDbFilename=C:\Users\hp\OneDrive\Documents\LibraryDB.mdf;Integrated Security=True;Connect Timeout=30");
        
        private AddBooks otherUserControl=new AddBooks();
        public void RefreshGridView()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(@"Data Source=(LocalDB)\v11.0;AttachDbFilename=C:\Users\hp\OneDrive\Documents\LibraryDB.mdf;Integrated Security=True;Connect Timeout=30"))
                {
                    string query = "SELECT * FROM issues"; // Adjust this query as needed
                    using (SqlDataAdapter da = new SqlDataAdapter(query, conn))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        dataGridView1.DataSource = dt;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while refreshing the grid view: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public  void MainControl(AddBooks control)
{
    InitializeComponent();
    this.otherUserControl = control;
}
        public IssueBooks()
        {
            InitializeComponent();

            displayBookIssueData();
           

        }

        public void refreshData()
        {
            if (InvokeRequired)
            {
                Invoke((MethodInvoker)refreshData);
                return;
            }

            displayBookIssueData();
            DataBookTitle();
        }

        public void displayBookIssueData()
        {

            DataIssueBooks dib = new DataIssueBooks();
            List<DataIssueBooks> listData = dib.IssueBooksData();

            dataGridView1.DataSource = listData;
        }
    

        private void bookIssue_addBtn_Click(object sender, EventArgs e)
        {
            if (bookIssue_id.Text == ""
                || bookIssue_name.Text == ""
                || bookIssue_contact.Text == ""
                || bookIssue_email.Text == ""
                || txtrno.Text == ""
                || cboyear.Text == ""
                || cbomajor.Text == ""
                || txtbarcode.Text == ""
                || bookIssue_bookTitle.Text == ""
                || bookIssue_author.Text == ""
                || bookIssue_issueDate.Value == null
                || bookIssue_returnDate.Value == null
                || bookIssue_status.Text == ""
        )
            {
                MessageBox.Show("Please fill all blank fields", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                if (connect.State != ConnectionState.Open)
                {
                    try
                    {
                        DateTime today = DateTime.Today;

                        connect.Open();

                        string insertData = "INSERT INTO issues " +
                            "(issue_id, full_name, contact, email, book_title, author, status, issue_date, return_date, date_insert,rollno,year,major,book_id) " +
                            "VALUES(@issueID, @fullname, @contact, @email, @bookTitle, @author, @status, @issueDate, @returnDate, @dateInsert,@rollno,@year,@major,@book_id)";

                        using (SqlCommand cmd = new SqlCommand(insertData, connect))
                        {
                            cmd.Parameters.AddWithValue("@issueID", bookIssue_id.Text.Trim());
                            cmd.Parameters.AddWithValue("@fullname", bookIssue_name.Text.Trim());
                            cmd.Parameters.AddWithValue("@contact", bookIssue_contact.Text.Trim());
                            cmd.Parameters.AddWithValue("@email", bookIssue_email.Text.Trim());
                            cmd.Parameters.AddWithValue("@bookTitle", bookIssue_bookTitle.Text.Trim());
                            cmd.Parameters.AddWithValue("@author", bookIssue_author.Text.Trim());
                            cmd.Parameters.AddWithValue("@status", bookIssue_status.Text.Trim());
                            cmd.Parameters.AddWithValue("@issueDate", bookIssue_issueDate.Value);
                            cmd.Parameters.AddWithValue("@returnDate", bookIssue_returnDate.Value); ;
                            cmd.Parameters.AddWithValue("@dateInsert", today);
                            cmd.Parameters.AddWithValue("@rollno", txtrno.Text.Trim());
                            cmd.Parameters.AddWithValue("@year", cboyear.Text.Trim());
                            cmd.Parameters.AddWithValue("@major", cbomajor.Text.Trim());
                            cmd.Parameters.AddWithValue("@book_id", txtbarcode.Text.Trim());

                            cmd.ExecuteNonQuery();
                        }
                        string updateBooksData = "UPDATE books SET status = 'Not Available' WHERE barcode = @book_id";
                
                using (SqlCommand cmdUpdate = new SqlCommand(updateBooksData, connect))
                {
                    cmdUpdate.Parameters.AddWithValue("@book_id", txtbarcode.Text.Trim());
                    cmdUpdate.ExecuteNonQuery();
                }
                if (otherUserControl != null)
                {
                    otherUserControl.RefreshGridView();
                }
                else
                {
                    MessageBox.Show("Other User Control not initialized.", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
  
                        displayBookIssueData();

                            MessageBox.Show("Issued successfully!", "Information Message", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            clearFields();

                        }
                    
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: " + ex, "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    }
                    finally
                    {
                        connect.Close();
                    }
                }
            }
        }

        public void clearFields()
        {
            bookIssue_id.Text = "";
            bookIssue_name.Text = "";
            bookIssue_contact.Text = "";
            bookIssue_email.Text = "";
            txtrno.Text = "";
            cbomajor.Text = "";
            cboyear.Text = "";
            txtbarcode.Text = "";
            bookIssue_bookTitle.SelectedIndex = -1;
            bookIssue_bookTitle.Text = ""; 
            bookIssue_author.SelectedIndex = -1;
            bookIssue_author.Text = ""; 
            bookIssue_status.SelectedIndex = -1;
            bookIssue_status.Text = ""; 
            

        }

        public void DataBookTitle()
        {
            if (connect.State == ConnectionState.Closed)
            {
                try
                {
                    connect.Open();
                    string selectData = "SELECT id, book_title FROM books WHERE status = 'Available' AND date_delete IS NULL";

                    using (SqlCommand cmd = new SqlCommand(selectData, connect))
                    {
                        SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                        DataTable table = new DataTable();
                        adapter.Fill(table);

                        bookIssue_bookTitle.DataSource = table;
                        bookIssue_bookTitle.DisplayMember = "book_title";
                        bookIssue_bookTitle.ValueMember = "id";

                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex, "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);

                }
                finally
                {
                    connect.Close();
                }
            }

        }

        private void bookIssue_bookTitle_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (connect.State != ConnectionState.Open)
            {
                if (bookIssue_bookTitle.SelectedValue != null)
                {
                    DataRowView selectedRow = (DataRowView)bookIssue_bookTitle.SelectedItem;
                    int selectID = Convert.ToInt32(selectedRow["id"]);
                    try
                    {
                        connect.Open();

                        string selectData = "SELECT * FROM books WHERE id = @id";

                        using (SqlCommand cmd = new SqlCommand(selectData, connect))
                        {
                            cmd.Parameters.AddWithValue("@id", selectID);

                            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                            DataTable table = new DataTable();
                            adapter.Fill(table);

                            if (table.Rows.Count > 0)
                            {
                                bookIssue_author.Text = table.Rows[0]["author"].ToString();
                                txtbarcode.Text = table.Rows[0]["barcode"].ToString(); 
                                bookIssue_status.Text = table.Rows[0]["status"].ToString(); 

                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: " + ex, "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    finally
                    {
                        connect.Close();
                    }
                }
            }

        }

        private void dataGridView1_CellClick_1(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex != -1)
            {
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];
                bookIssue_id.Text = row.Cells[1].Value.ToString();
                bookIssue_name.Text = row.Cells[2].Value.ToString();
                bookIssue_contact.Text = row.Cells[3].Value.ToString();
                bookIssue_email.Text = row.Cells[4].Value.ToString();
                bookIssue_bookTitle.Text = row.Cells[5].Value.ToString();
                bookIssue_author.Text = row.Cells[6].Value.ToString();
                bookIssue_issueDate.Text = row.Cells[7].Value.ToString();
                bookIssue_returnDate.Text = row.Cells[8].Value.ToString();
                bookIssue_status.Text = row.Cells[9].Value.ToString();
                txtrno.Text=row.Cells[10].Value.ToString();
                cboyear.Text = row.Cells[11].Value.ToString();
                cbomajor.Text = row.Cells[12].Value.ToString();
                label15.Text = row.Cells[0].Value.ToString();
                txtbarcode.Text = row.Cells[13].Value.ToString();


            }
        }

        private void bookIssue_updateBtn_Click(object sender, EventArgs e)
        {
            if (bookIssue_id.Text == ""
                || bookIssue_name.Text == ""
                || bookIssue_contact.Text == ""
                || bookIssue_email.Text == ""
                || cboyear.Text == ""
                 || cbomajor.Text == ""
                || bookIssue_bookTitle.Text == ""
                || bookIssue_author.Text == ""
                || bookIssue_issueDate.Value == null
                || bookIssue_returnDate.Value == null
                || bookIssue_status.Text == ""
             )
            {
                MessageBox.Show("Please select item first", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                if (connect.State != ConnectionState.Open)
                {
                    DialogResult check = MessageBox.Show("Are you sure you want to UPDATE Issue ID:"
                        + bookIssue_id + "?", "Confirmation Message", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (check == DialogResult.Yes)
                    {
                        try
                        {
                            connect.Open();
                            DateTime today = DateTime.Today;
                            string updateData = "UPDATE issues SET full_name = @fullName, contact = @contact, email = @email" +
                                ", book_title = @bookTitle, author = @author, status = @status, issue_date = @issueDate" +
                                ", return_date = @returnDate, date_update = @dateUpdate ,issue_id=@issue_id,book_id=@book_id   WHERE id = @id";

                            using (SqlCommand cmd = new SqlCommand(updateData, connect))
                            {
                                cmd.Parameters.AddWithValue("@fullName", bookIssue_name.Text.Trim());
                                cmd.Parameters.AddWithValue("@contact", bookIssue_contact.Text.Trim());
                                cmd.Parameters.AddWithValue("@email", bookIssue_email.Text.Trim());
                                cmd.Parameters.AddWithValue("@bookTitle", bookIssue_bookTitle.Text.Trim());
                                cmd.Parameters.AddWithValue("@author", bookIssue_author.Text.Trim());
                                cmd.Parameters.AddWithValue("@status", bookIssue_status.Text.Trim());
                                cmd.Parameters.AddWithValue("@issueDate", bookIssue_issueDate.Value);
                                cmd.Parameters.AddWithValue("@returnDate", bookIssue_returnDate.Value);
                                cmd.Parameters.AddWithValue("@dateUpdate", today);
                                cmd.Parameters.AddWithValue("@issue_id", bookIssue_id.Text.Trim());
                                cmd.Parameters.AddWithValue("@book_id", txtbarcode.Text.Trim());

                                cmd.Parameters.AddWithValue("@id", label15.Text.Trim());



                                cmd.ExecuteNonQuery();

                                displayBookIssueData();

                                MessageBox.Show("Updated successfully!", "Information Message", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                clearFields();
                            }

                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error: " + ex, "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        finally
                        {
                            connect.Close();
                        }
                    }
                    else
                    {
                        MessageBox.Show("Cancelled.", "Information Message", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }

                }
            }
        }

        private void bookIssue_deleteBtn_Click(object sender, EventArgs e)
        {
            if (bookIssue_id.Text == ""
                || bookIssue_name.Text == ""
                || bookIssue_contact.Text == ""
                || bookIssue_email.Text == ""
                || cboyear.Text == ""
                 || cbomajor.Text == ""
                || bookIssue_bookTitle.Text == ""
                || bookIssue_author.Text == ""
                || bookIssue_issueDate.Value == null
                || bookIssue_returnDate.Value == null
                || bookIssue_status.Text == ""
               )
            {
                MessageBox.Show("Please select item first", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                if (connect.State != ConnectionState.Open)
                {
                    DialogResult check = MessageBox.Show("Are you sure you want to DELETE Issue ID:"
                        + bookIssue_id.Text.Trim() + "?", "Confirmation Message", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (check == DialogResult.Yes)
                    {
                        try
                        {
                            connect.Open();
                            DateTime today = DateTime.Today;
                            string deleteData = "DELETE FROM issues WHERE issue_id = @issueID";

                            using (SqlCommand cmd = new SqlCommand(deleteData, connect))
                            {
                                cmd.Parameters.AddWithValue("@dateDelete", today);
                                cmd.Parameters.AddWithValue("@issueID", bookIssue_id.Text.Trim());

                                cmd.ExecuteNonQuery();

                                displayBookIssueData();

                                MessageBox.Show("Deleted successfully!", "Information Message", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                clearFields();
                            }

                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error: " + ex, "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        finally
                        {
                            connect.Close();
                        }
                    }
                    else
                    {
                        MessageBox.Show("Cancelled.", "Information Message", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
        }

        private void bookIssue_clearBtn_Click(object sender, EventArgs e)
        {
            clearFields();
        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }


        private FilterInfoCollection videoDevices;
        private VideoCaptureDevice videoSource;
        private bool qrCodeFound = false;

        private void btnqr_Click(object sender, EventArgs e)
        {
            btnclose.Visible = true;
            pbforqrscan.Visible = true;
            pbforbarscan.Visible = false;
            try
            {
                videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                if (videoDevices.Count > 0)
                {
                    videoSource = new VideoCaptureDevice(videoDevices[1].MonikerString);
                    videoSource.NewFrame += new NewFrameEventHandler(CaptureNewFrame);
                    videoSource.Start();
                    qrCodeFound = false; 
                }
                else
                {
                    MessageBox.Show("No webcam detected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("not scanned'" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CaptureNewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            // Check if QR code has been found to prevent further processing
            if (qrCodeFound) return;

            try
            {
                // Create a new bitmap from the current frame
                using (Bitmap bitmap = new Bitmap(eventArgs.Frame))
                {
                    // Ensure the PictureBox image is disposed of before setting a new one
                    if (pbforqrscan.Image != null)
                    {
                        pbforqrscan.Image.Dispose();
                    }

                    // Display the current frame in the PictureBox
                    pbforqrscan.Image = (Bitmap)bitmap.Clone();

                    // Decode the QR code using ZXing.Net
                    ZXing.BarcodeReader reader = new ZXing.BarcodeReader();
                    var result = reader.Decode(bitmap);

                    // Check if the QR code is decoded successfully
                    if (result != null)
                    {
                        string qrCodeContent = result.Text;
                        MessageBox.Show("Scanned QR Code Content: " + qrCodeContent, "QR Code Scanned", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Match the QR code with the database
                        using (SqlConnection con = new SqlConnection(@"Data Source=(LocalDB)\v11.0;AttachDbFilename=C:\Users\hp\OneDrive\Documents\LibraryDB.mdf;Integrated Security=True;Connect Timeout=30"))
                        {
                           string query = "SELECT * FROM LibMember WHERE qrcode = @qrCodeContent";
                            using (SqlCommand cmd = new SqlCommand(query, con))
                            {
                                cmd.Parameters.AddWithValue("@qrCodeContent", qrCodeContent);
                                con.Open();
                                SqlDataReader sqlReader = cmd.ExecuteReader();
                           
                                if (sqlReader.Read())
                                {
                                    string scannedQrCode = sqlReader["qrcode"].ToString();
                                    string scannedName = sqlReader["name"].ToString();
                                    string scannedPhone = sqlReader["phone"].ToString();
                                    string scannedEmail = sqlReader["email"].ToString();
                                    string scannedRollNo = sqlReader["rollno"].ToString();
                                    string scannedYear = sqlReader["year"].ToString();
                                    string scannedMajor = sqlReader["major"].ToString();

                                    // Call the method to populate fields
                                    PopulateScannedQRData(scannedQrCode, scannedName, scannedPhone, scannedEmail, scannedRollNo, scannedYear, scannedMajor);

                                    // Stop the webcam capture if a match is found
                                    videoSource.SignalToStop();
                                    videoSource.WaitForStop();
                                   
                                    qrCodeFound = true; // Set flag to prevent further frame processing
                                }
                                else
                                {
                                    videoSource.SignalToStop();
                                    videoSource.WaitForStop();
                                    // Log the QR code content and indicate no match
                                    MessageBox.Show("No match found in database for QR Code: " + qrCodeContent, "Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                 
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void PopulateScannedQRData(string qrCode, string name, string phone, string email, string rollNo, string year, string major)
        {
            if (this.InvokeRequired)
            {
                // If called from another thread, use Invoke to update UI elements safely
                this.Invoke(new Action(() =>
                {
                    PopulateScannedQRData(qrCode, name, phone, email, rollNo, year, major);
                }));
            }
            else
            {
                // Populate the fields with the data
                bookIssue_id.Text = qrCode;
                bookIssue_name.Text = name;
                bookIssue_contact.Text = phone;
                bookIssue_email.Text = email;
                txtrno.Text = rollNo;
                cboyear.Text = year;
                cbomajor.Text = major;
                pbforqrscan.Visible = false;
                btnclose.Visible = false;
                MessageBox.Show("Fields populated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnbarcode_Click(object sender, EventArgs e)
        {
            btnclose.Visible = true;
            pbforqrscan.Visible = false;
            pbforbarscan.Visible = true;
            try
            {
                videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                if (videoDevices.Count > 0)
                {
                    videoSource = new VideoCaptureDevice(videoDevices[1].MonikerString); // Use the first available device
                    videoSource.NewFrame += new NewFrameEventHandler(CaptureNewBarcodeFrame);
                    videoSource.Start();
                    qrCodeFound = false;
                }
                else
                {
                    MessageBox.Show("No webcam detected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while starting barcode scanning: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CaptureNewBarcodeFrame(object sender, NewFrameEventArgs eventArgs)
        {
            // Check if the barcode has been found to prevent further processing
            if (qrCodeFound) return;

            try
            {
                // Create a new bitmap from the current frame
                using (Bitmap bitmap = new Bitmap(eventArgs.Frame))
                {
                    // Ensure the PictureBox image is disposed of before setting a new one
                    if (pbforbarscan.Image != null)
                    {
                        pbforbarscan.Image.Dispose();
                    }

                    // Display the current frame in the PictureBox
                    pbforbarscan.Image = (Bitmap)bitmap.Clone();

                    // Decode the barcode using ZXing.Net
                    ZXing.BarcodeReader reader = new ZXing.BarcodeReader();
                    var result = reader.Decode(bitmap);

                    // Check if the barcode is decoded successfully
                    if (result != null)
                    {
                        string barcodeContent = result.Text;
                        MessageBox.Show("Scanned Barcode Content: " + barcodeContent, "Barcode Scanned", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Match the barcode with the database (similar to QR code logic)
                        using (SqlConnection con = new SqlConnection(@"Data Source=(LocalDB)\v11.0;AttachDbFilename=C:\Users\hp\OneDrive\Documents\LibraryDB.mdf;Integrated Security=True;Connect Timeout=30"))
                        {
                            string query = "SELECT * FROM books WHERE barcode = @barcodeContent";
                            using (SqlCommand cmd = new SqlCommand(query, con))
                            {
                                cmd.Parameters.AddWithValue("@barcodeContent", barcodeContent);
                                con.Open();
                                SqlDataReader sqlReader = cmd.ExecuteReader();

                                if (sqlReader.Read())
                                {
                              
                                    string scannedBarcode = sqlReader["barcode"].ToString();
                                    string scannedbt = sqlReader["book_title"].ToString();
                                    string scannedauthor= sqlReader["author"].ToString();
                                    string scannedstatus = sqlReader["status"].ToString();
                                 
                                    // Call the method to populate fields
                                    PopulateScannedBarcodeData(scannedBarcode, scannedbt, scannedauthor, scannedstatus);

                                    // Stop the webcam capture if a match is found
                                    videoSource.SignalToStop();
                                    videoSource.WaitForStop();
                             
                                    qrCodeFound = true; // Set flag to prevent further frame processing
                                }
                                else
                                {
                                    videoSource.SignalToStop();
                                    videoSource.WaitForStop();
                                    // Log the barcode content and indicate no match
                                    MessageBox.Show("No match found in database for Barcode: " + barcodeContent, "Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PopulateScannedBarcodeData(string barcode, string bookTitle, string author, string status)
        {
            if (this.InvokeRequired)
            {
                // If called from another thread, use Invoke to update UI elements safely
                this.Invoke(new Action(() =>
                {
                    PopulateScannedBarcodeData(barcode, bookTitle, author, status);
                }));
            }
            else
            {
                // Populate the fields with the data
                txtbarcode.Text = barcode;      // Assuming txtBarcode is the textbox for barcode
                bookIssue_bookTitle.Text = bookTitle;  // Assuming txtBookTitle is the textbox for book title
                bookIssue_author.Text = author;        // Assuming txtAuthor is the textbox for author
                bookIssue_status.Text = status;        // Assuming txtStatus is the textbox for status
                pbforbarscan.Visible = false;
                btnclose.Visible = false;
                MessageBox.Show("Fields populated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnrefreshqr_Click(object sender, EventArgs e)
        {
            
            pbforbarscan.Visible = false;
            pbforqrscan.Visible = false;
            btnclose.Visible = false;
            videoSource.SignalToStop();
            videoSource.WaitForStop();

        }

        private void btnrefreshbarcode_Click(object sender, EventArgs e)
        {
        }

        private void bookIssue_id_TextChanged(object sender, EventArgs e)
        {
            if (bookIssue_id.Text.Length >= 10)
            {
                string query = "SELECT * FROM LibMember WHERE qrcode = @userQR";

                using (SqlConnection conn = new SqlConnection(@"Data Source=(LocalDB)\v11.0;AttachDbFilename=C:\Users\hp\OneDrive\Documents\LibraryDB.mdf;Integrated Security=True;Connect Timeout=30"))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@userQR", bookIssue_id.Text);
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            bookIssue_id.Text = reader["qrcode"].ToString();


                            bookIssue_name.Text = reader["name"].ToString();
                            bookIssue_contact.Text = reader["phone"].ToString();
                            bookIssue_email.Text = reader["email"].ToString();

                            txtrno.Text = reader["rollno"].ToString();
                            cboyear.Text = reader["year"].ToString();
                            cbomajor.Text = reader["major"].ToString();
                        }else
                        {
                            MessageBox.Show("No record found.");
                        }
                    }
                }
            }
            
        }

        private void txtbarcode_TextChanged(object sender, EventArgs e)
        {
            if (txtbarcode.Text.Length >= 7)
            {
                string query = "SELECT * FROM books WHERE barcode = @barcode";

                using (SqlConnection conn = new SqlConnection(@"Data Source=(LocalDB)\v11.0;AttachDbFilename=C:\Users\hp\OneDrive\Documents\LibraryDB.mdf;Integrated Security=True;Connect Timeout=30"))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@barcode", txtbarcode.Text);
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            txtbarcode.Text = reader["barcode"].ToString();


                            bookIssue_bookTitle.Text = reader["book_title"].ToString();
                            bookIssue_author.Text = reader["author"].ToString();
                            bookIssue_status.Text = reader["status"].ToString();
                          }
                        else
                        {
                            MessageBox.Show("No record found.");
                        }
                    }
                }
            }
        }

  
    }
}