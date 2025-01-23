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
using System.IO;
using AForge.Video;
using AForge.Video.DirectShow;
using ZXing;
using iTextSharp.text;
using iTextSharp.text.pdf;


namespace LibraryManagementSystem
{
    public partial class AddBooks : UserControl
    {
        SqlConnection connect = new SqlConnection(@"Data Source=(LocalDB)\v11.0;AttachDbFilename=C:\Users\hp\OneDrive\Documents\LibraryDB.mdf;Integrated Security=True;Connect Timeout=30");

        public AddBooks()
        {
            InitializeComponent();

            displayBooks();

        }
        private FilterInfoCollection videoDevices; // Collection of webcam devices
        private VideoCaptureDevice videoSource; 

        public void refreshData()
        {
            if (InvokeRequired)
            {
                Invoke((MethodInvoker)refreshData);
                return;
            }

            displayBooks();
        }

        private string CaptureBarcode()
{
    // Initialize webcam devices
    videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);

    if (videoDevices.Count == 0)
    {
        MessageBox.Show("No webcam found.");
        return null;
    }

    // Select the first available device
    videoSource = new VideoCaptureDevice(videoDevices[1].MonikerString);

    // Hook into the NewFrame event to capture frames from the webcam
    videoSource.NewFrame += new NewFrameEventHandler(videoSource_NewFrame);

    // Start capturing video
    videoSource.Start();

    // Wait for the user to scan the barcode (you can add a timeout or other logic here)
    string scannedBarcode = WaitForBarcode();

    // Stop capturing video
    videoSource.SignalToStop();
    videoSource.WaitForStop();

    return scannedBarcode;
}

private string WaitForBarcode()
{
    // This method waits for a barcode to be scanned (you can implement a more sophisticated wait mechanism)
    while (scannedBarcode == null)
    {
        Application.DoEvents(); // Keep UI responsive while waiting
    }
    return scannedBarcode;
}

private string scannedBarcode = null;

private void videoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
{
    // Grab the current frame
    Bitmap bitmap = (Bitmap)eventArgs.Frame.Clone();

    // Use ZXing to decode the barcode
    BarcodeReader reader = new BarcodeReader();
    var result = reader.Decode(bitmap);

    if (result != null)
    {
        // Barcode found
        scannedBarcode = result.Text;
        videoSource.SignalToStop(); // Stop capturing video once barcode is found
    }

    // Display the frame (optional)
    pictureBox1.Image = bitmap; // Assuming you have a PictureBox to show the camera feed
}

private void addBooks_addBtn_Click(object sender, EventArgs e)
{
    if (addBooks_bookTitle.Text == ""
        || addBooks_author.Text == ""
        || addBooks_published.Value == null
        || addBooks_status.Text == "")
    {
        MessageBox.Show("Please fill all blank fields", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
    else
    {
        if (connect.State == ConnectionState.Closed)
        {
            try
            {
                // Generate a random numeric string
                Random random = new Random();
                string brContent = random.Next(1000000, 9999999).ToString(); // 7-digit random number

                Zen.Barcode.Code128BarcodeDraw brCodeDraw = Zen.Barcode.BarcodeDrawFactory.Code128WithChecksum;
                System.Drawing.Image brImage = brCodeDraw.Draw(brContent, 40);

                // Convert barcode image to byte array
                byte[] brCodeBytes;
                using (MemoryStream ms = new MemoryStream())
                {
                    brImage.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    brCodeBytes = ms.ToArray();
                }

                DateTime today = DateTime.Today;
                connect.Open();
                string insertData = "INSERT INTO books " +
                    "(book_title, author, published_date, status, date_insert, br,barcode) " +
                    "VALUES(@bookTitle, @Author, @PublishedDate, @Status, @DateInsert, @BrCode,@barcode)";

                using (SqlCommand cmd = new SqlCommand(insertData, connect))
                {
                    cmd.Parameters.AddWithValue("@bookTitle", addBooks_bookTitle.Text.Trim());
                    cmd.Parameters.AddWithValue("@Author", addBooks_author.Text.Trim());
                    cmd.Parameters.AddWithValue("@PublishedDate", addBooks_published.Value);
                    cmd.Parameters.AddWithValue("@Status", addBooks_status.Text.Trim());
                    cmd.Parameters.AddWithValue("@BrCode", brCodeBytes);
                    cmd.Parameters.AddWithValue("@DateInsert", today);
                    cmd.Parameters.AddWithValue("@barcode", brContent);


                    cmd.ExecuteNonQuery();

                    displayBooks();

                   // barpicture.Image = brImage;
                    MessageBox.Show("Added successfully!", "Information Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    GenerateMemberPdf(brContent,brImage);

                    clearFields();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                connect.Close();
            }
        }
    }
}

private void GenerateMemberPdf(string barcode ,System.Drawing.Image qrImage)
{
    string pdfPath = "C://Users//hp//OneDrive//Desktop//PDF//books//" + barcode + ".pdf";
    Document pdfDoc = new Document(PageSize.A4, 25, 25, 25, 25);

    using (FileStream stream = new FileStream(pdfPath, FileMode.Create))
    {
        PdfWriter.GetInstance(pdfDoc, stream);
        pdfDoc.Open();
       
      

        // Add QR Code to PDF
        using (MemoryStream ms = new MemoryStream())
        {
            qrImage.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            iTextSharp.text.Image pdfImage = iTextSharp.text.Image.GetInstance(ms.ToArray());
            pdfImage.ScaleToFit(100f, 100f); // Adjust size as needed
            pdfDoc.Add(pdfImage);
        }
        pdfDoc.Add(new Paragraph("" + barcode));
        pdfDoc.Close();
    }

    MessageBox.Show("PDF saved successfully at: " + pdfPath);
}
       
        public void clearFields()
        {
            addBooks_bookTitle.Text = "";
            addBooks_author.Text = "";
            barcodeTextBox.Text = "";
            addBooks_status.SelectedIndex = -1;
            pictureBox1.Image = null;
        }

        public void displayBooks()
        {
            DataAddBooks dab = new DataAddBooks();
            List<DataAddBooks> listData = dab.addBooksData();

            dataGridView1.DataSource = listData;

        }

        private int bookID = 0;
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex != -1)
            {
                StopScanning();
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];
                Zen.Barcode.Code128BarcodeDraw brCodeDraw = Zen.Barcode.BarcodeDrawFactory.Code128WithChecksum;
                System.Drawing.Image brImage = brCodeDraw.Draw( row.Cells[5].Value.ToString(), 70,2);             
                bookID = (int)row.Cells[0].Value;
                addBooks_bookTitle.Text = row.Cells[1].Value.ToString();
                addBooks_author.Text = row.Cells[2].Value.ToString();
                addBooks_published.Text = row.Cells[3].Value.ToString();
                addBooks_status.Text = row.Cells[4].Value.ToString();
                barcodeTextBox.Text = row.Cells[5].Value.ToString();
                pictureBox1.Image = brImage;             

            }
        }

        private void addBooks_clearBtn_Click(object sender, EventArgs e)
        {
            clearFields();
        }

        private void addBooks_updateBtn_Click(object sender, EventArgs e)
        {
            if (addBooks_bookTitle.Text == ""
                || addBooks_author.Text == ""
                || addBooks_published.Value == null
                || addBooks_status.Text == ""
              )
            {
                MessageBox.Show("Please select item first", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                if (connect.State != ConnectionState.Open)
                {
                    DialogResult check = MessageBox.Show("Are you sure you want to UPDATE Book ID:"
                        + bookID + "?", "Confirmation Message", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (check == DialogResult.Yes)
                    {
                        try
                        {
                            connect.Open();
                            DateTime today = DateTime.Today;
                            string updateData = "UPDATE books SET book_title = @bookTitle" +
                                ", author = @author, published_date = @published" +
                                ", status = @status, date_update = @dateUpdate WHERE id = @id";

                            using (SqlCommand cmd = new SqlCommand(updateData, connect))
                            {
                                cmd.Parameters.AddWithValue("@bookTitle", addBooks_bookTitle.Text.Trim());
                                cmd.Parameters.AddWithValue("@author", addBooks_author.Text.Trim());
                                cmd.Parameters.AddWithValue("@published", addBooks_published.Value);
                                cmd.Parameters.AddWithValue("@status", addBooks_status.Text.Trim());
                                cmd.Parameters.AddWithValue("@dateUpdate", today);
                                cmd.Parameters.AddWithValue("@id", bookID);

                                cmd.ExecuteNonQuery();

                                displayBooks();

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

        private void addBooks_deleteBtn_Click(object sender, EventArgs e)
        {
            if (addBooks_bookTitle.Text == ""
                || addBooks_author.Text == ""
                || addBooks_published.Value == null
                || addBooks_status.Text == "")
            {
                MessageBox.Show("Please select item first", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                if (connect.State != ConnectionState.Open)
                {
                    DialogResult check = MessageBox.Show("Are you sure you want to DELETE Book ID:"
                        + bookID + "?", "Confirmation Message", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (check == DialogResult.Yes)
                    {
                        try
                        {
                            connect.Open();
                            DateTime today = DateTime.Today;
                            string updateData = "UPDATE books SET date_delete = @dateDelete WHERE id = @id";

                            using (SqlCommand cmd = new SqlCommand(updateData, connect))
                            {
                                cmd.Parameters.AddWithValue("@dateDelete", today);
                                cmd.Parameters.AddWithValue("@id", bookID);

                                cmd.ExecuteNonQuery();

                                displayBooks();

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

        private void btnscan_Click(object sender, EventArgs e)
        {
            // Stop any ongoing barcode scanning process
            StopScanning();

            // Clear PictureBox image before starting a new scan
            pictureBox1.Image = null;

            // Start a new barcode scan
            string scannedBarcode = CaptureBarcode();  // Method to capture the barcode using the webcam

            if (string.IsNullOrEmpty(scannedBarcode))
            {
                MessageBox.Show("No barcode detected. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Database connection string
            string connectionString = @"Data Source=(LocalDB)\v11.0;AttachDbFilename=C:\Users\hp\OneDrive\Documents\LibraryDB.mdf;Integrated Security=True;Connect Timeout=30";

            // Query to check if the scanned barcode exists in the database
            string query = "SELECT book_title, author, barcode, published_date, status FROM [dbo].[books] WHERE barcode = @ScannedBarcode";

            using (SqlConnection connect = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, connect))
            {
                cmd.Parameters.AddWithValue("@ScannedBarcode", scannedBarcode);

                connect.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    Zen.Barcode.Code128BarcodeDraw brCodeDraw = Zen.Barcode.BarcodeDrawFactory.Code128WithChecksum;
                    System.Drawing.Image brImage = brCodeDraw.Draw(reader["barcode"].ToString(), 70, 2);

                    // Retrieve the title and order from the database
                    string title = reader["book_title"].ToString();
                    string order = reader["author"].ToString();
                    pictureBox1.Image = brImage;

                    // Display title and order in your form
                    addBooks_bookTitle.Text = title;
                    addBooks_author.Text = order;
                    addBooks_published.Text = reader["published_date"].ToString();
                    addBooks_status.Text = reader["status"].ToString();
                    barcodeTextBox.Text = reader["barcode"].ToString();
                }
                else
                {
                    // If no match is found, show the "Not Registered" message
                    MessageBox.Show("Not Registered", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                scannedBarcode = null;
                reader.Close();
                connect.Close();
            }
        }


        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }
        private void StopScanning()
        {
            if (videoSource != null && videoSource.IsRunning)
            {
                videoSource.SignalToStop();  // Stop capturing video
                videoSource.WaitForStop();   // Wait for the video to stop
                videoSource.NewFrame -= new NewFrameEventHandler(videoSource_NewFrame);  // Unsubscribe from the NewFrame event
                videoSource = null;          // Clear the video source
                pictureBox1.Image = null;    // Clear the PictureBox if displaying the feed
            }
        }

        public void RefreshGridView()
        {
            // Assuming you're binding data to the GridView from a database
            string query = "SELECT * FROM books"; // Adjust the query as needed

            using (SqlConnection conn = new SqlConnection(@"Data Source=(LocalDB)\v11.0;AttachDbFilename=C:\Users\hp\OneDrive\Documents\LibraryDB.mdf;Integrated Security=True;Connect Timeout=30"))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                dataGridView1.DataSource = dt;
            }
        }

    }
}
