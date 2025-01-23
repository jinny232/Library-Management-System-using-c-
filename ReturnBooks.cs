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
    public partial class ReturnBooks : UserControl
    {
        SqlConnection connect = new SqlConnection(@"Data Source=(LocalDB)\v11.0;AttachDbFilename=C:\Users\hp\OneDrive\Documents\LibraryDB.mdf;Integrated Security=True;Connect Timeout=30");
        private IssueBooks issueUserControl = new IssueBooks();
        private AddBooks otherUserControl = new AddBooks();
        public ReturnBooks()
        {
            InitializeComponent();
            RefreshReturnedBooksGridView();
            dataGridView1.CellClick += DataGridView1_CellClick;

   
        }
        public void MainControl(AddBooks control)
        {
            InitializeComponent();
            this.otherUserControl = control;
        }
        public void MainControl(IssueBooks control)
        {
            InitializeComponent();
            this.issueUserControl = control;
           
        }
        public void refreshData()
        {
            if (InvokeRequired)
            {
                Invoke((MethodInvoker)refreshData);
                return;
            }

           
        }


        private void label16_Click(object sender, EventArgs e)
        {

        }

        private FilterInfoCollection videoDevices;
        private VideoCaptureDevice videoSource;
        private bool qrCodeFound = false;

       
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
                            string query = "SELECT * FROM issues WHERE issue_id = @qrCodeContent";
                            using (SqlCommand cmd = new SqlCommand(query, con))
                            {
                                cmd.Parameters.AddWithValue("@qrCodeContent", qrCodeContent);
                                con.Open();
                                SqlDataReader sqlReader = cmd.ExecuteReader();

                                if (sqlReader.Read())
                                {
                                    string scannedQrCode = sqlReader["issue_id"].ToString();
                                    string scannedName = sqlReader["full_name"].ToString();
                                    string scannedPhone = sqlReader["contact"].ToString();
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

                        }
                        else
                        {
                            MessageBox.Show("No record found.");
                        }
                    }
                }
            }
        }

        private void btnclose_Click(object sender, EventArgs e)
        {

            pbforbarscan.Visible = false;
            pbforqrscan.Visible = false;
            btnclose.Visible = false;
            videoSource.SignalToStop();
            videoSource.WaitForStop();

        }
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
                                    string scannedauthor = sqlReader["author"].ToString();
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

        private void Returnbtn_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(@"Data Source=(LocalDB)\v11.0;AttachDbFilename=C:\Users\hp\OneDrive\Documents\LibraryDB.mdf;Integrated Security=True;Connect Timeout=30"))
                {
                    conn.Open();

                    // Begin a transaction to ensure all steps succeed
                    SqlTransaction transaction = conn.BeginTransaction();

                    try
                    {
                        // Step 1: Save data to the returnedbook table
                        string insertReturnedBookQuery = "INSERT INTO returnedBooks (issue_id, book_id, book_title, name, return_date) " +
                                                         "VALUES (@issue_id, @book_id, @book_title, @full_name, @return_date)";
                        using (SqlCommand cmd = new SqlCommand(insertReturnedBookQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@issue_id", bookIssue_id.Text);
                            cmd.Parameters.AddWithValue("@book_id", txtbarcode.Text);
                            cmd.Parameters.AddWithValue("@book_title", bookIssue_bookTitle.Text);
                            cmd.Parameters.AddWithValue("@full_name", bookIssue_name.Text);
                            cmd.Parameters.AddWithValue("@return_date", DateTime.Now);

                            cmd.ExecuteNonQuery();
                        }

                        // Step 2: Update the issues table to mark the book as returned
                        string updateIssueTableQuery = "UPDATE issues SET return_date = @return_date, status = 'Returned' WHERE issue_id = @issue_id AND book_id = @book_id";
                        using (SqlCommand cmd = new SqlCommand(updateIssueTableQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@return_date", DateTime.Now);
                            cmd.Parameters.AddWithValue("@issue_id", bookIssue_id.Text);
                            cmd.Parameters.AddWithValue("@book_id", txtbarcode.Text); // Corrected to use book_id
                            cmd.ExecuteNonQuery();
                        }

                        // Step 3: Update the book status in the books table to indicate availability
                        string updateBookStatusQuery = "UPDATE books SET status = 'Available' WHERE barcode = @barcode";
                        using (SqlCommand cmd = new SqlCommand(updateBookStatusQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@barcode", txtbarcode.Text);

                            cmd.ExecuteNonQuery();
                        }

                        // Commit the transaction if all commands succeed
                        transaction.Commit();

                        // Refresh the user controls
                        issueUserControl.RefreshGridView();
                        otherUserControl.RefreshGridView();
                        RefreshReturnedBooksGridView();
                        MessageBox.Show("Book returned successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        // Rollback the transaction if any command fails
                        transaction.Rollback();
                        MessageBox.Show("An error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RefreshReturnedBooksGridView()
        {
            using (SqlConnection conn = new SqlConnection(@"Data Source=(LocalDB)\v11.0;AttachDbFilename=C:\Users\hp\OneDrive\Documents\LibraryDB.mdf;Integrated Security=True;Connect Timeout=30"))
            {
                string query = "SELECT * FROM returnedBooks"; // Adjust the query if needed
                using (SqlDataAdapter adapter = new SqlDataAdapter(query, conn))
                {
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);
                    dataGridView1.DataSource = dataTable;
                }
            }
        }

        private void CustomizeDataGridViewHeader()
        {
            dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = Color.LightBlue;
            dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = Color.DarkBlue;
            dataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font("Arial", 12, FontStyle.Bold);
        }

        public void RefreshGridView()
        {
            // Assuming you're binding data to the GridView from a database
            string query = "SELECT * FROM returnedBooks"; // Adjust the query as needed

            using (SqlConnection conn = new SqlConnection(@"Data Source=(LocalDB)\v11.0;AttachDbFilename=C:\Users\hp\OneDrive\Documents\LibraryDB.mdf;Integrated Security=True;Connect Timeout=30"))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                dataGridView1.DataSource = dt;
            }
        }


        private void ReturnBooks_Load(object sender, EventArgs e)
        {
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

            dataGridView1.DataSource = dataTable;
            CustomizeDataGridViewHeader();
        }

        private void panel1_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {

        }

        private void pbforqrscan_Click(object sender, EventArgs e)
        {

        }

        private void dataGridView1_CellContentClick(object sender, System.Windows.Forms.DataGridViewCellEventArgs e)
        {

        }
        private void DataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
{
    // Ensure the clicked row index is valid
    if (e.RowIndex >= 0)
    {
        // Get the clicked row
        DataGridViewRow row = dataGridView1.Rows[e.RowIndex];

        // Extract data from the row
        string issueId = row.Cells["issue_id"].Value.ToString();
        string bookId = row.Cells["book_id"].Value.ToString();
        string bookTitle = row.Cells["book_title"].Value.ToString();
        string memberName = row.Cells["name"].Value.ToString();
        string returnDate = row.Cells["return_date"].Value.ToString();
        string id = row.Cells["return_id"].Value.ToString();

        // Populate fields (assuming textboxes or labels)
        bookIssue_id.Text = issueId;
        txtbarcode.Text = bookId;
        bookIssue_bookTitle.Text = bookTitle;
        bookIssue_name.Text = memberName;
        label17.Text = id;
  }
}

      private void button1_Click(object sender, EventArgs e)
{
    // Get the return_id from the selected row or textbox
    int returnId;
    if (int.TryParse(label17.Text, out returnId)) // Assuming bookReturn_returnID is a textbox with the return_id
    {
        // Define the connection string (replace with your actual connection string)
        string connectionString = @"Data Source=(LocalDB)\v11.0;AttachDbFilename=C:\Users\hp\OneDrive\Documents\LibraryDB.mdf;Integrated Security=True;Connect Timeout=30";

        // SQL delete command
        string deleteQuery = "DELETE FROM returnedBooks WHERE return_id = @returnId";

        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            try
            {
                // Open the connection
                connection.Open();

                // Create the SQL command
                using (SqlCommand command = new SqlCommand(deleteQuery, connection))
                {
                    // Add parameter to prevent SQL injection
                    command.Parameters.AddWithValue("@returnId", returnId);

                    // Execute the command
                    int rowsAffected = command.ExecuteNonQuery();

                    // Check if the record was successfully deleted
                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Record deleted successfully.");
                        RefreshReturnedBooksGridView(); // Refresh the grid after deletion
                    }
                    else
                    {
                        MessageBox.Show("Record not found.");
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle any errors that might occur
                MessageBox.Show("An error occurred: {ex.Message}");
            }
        }
    }
    else
    {
        MessageBox.Show("Invalid return ID.");
    }
}


       

   
    
    }
}