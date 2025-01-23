using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.IO;
using ZXing;
namespace LibraryManagementSystem
{
    public partial class viewLibmember : UserControl
    {
        public viewLibmember()
        {
            InitializeComponent();
        }

        private void viewLibmember_Load(object sender, EventArgs e)
        {
            LoadMembers();
        }

 // Add this for ZXing support

public void LoadMembers()
{
    // Clear existing controls in the FlowLayoutPanel
    flowLayoutPanel1.Controls.Clear();

    string connectionString = @"Data Source=(LocalDB)\v11.0;AttachDbFilename=C:\Users\hp\OneDrive\Documents\LibraryDB.mdf;Integrated Security=True;Connect Timeout=30";
    using (SqlConnection connect = new SqlConnection(connectionString))
    {
        string query = "SELECT name, year, major, phone, email, qr FROM LibMember";
        using (SqlCommand cmd = new SqlCommand(query, connect))
        {
            connect.Open();
            SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                // Convert QR code from byte array to Image
                byte[] qrBytes = (byte[])reader["qr"];
                Image qrImage = null;
                using (MemoryStream ms = new MemoryStream(qrBytes))
                {
                    qrImage = Image.FromStream(ms);
                }

                // Decode QR code image to text
                string qrText = DecodeQRCode(qrImage);

                // Create and display the card with QR code and text
                Panel card = CreateCard(
                    reader["name"].ToString(),
                    reader["year"].ToString(),
                    reader["major"].ToString(),
                    reader["phone"].ToString(),
                    reader["email"].ToString(),
                    qrImage,
                    qrText  // Use the decoded QR content text
                );

                flowLayoutPanel1.Controls.Add(card); // Add card to the FlowLayoutPanel
            }

            reader.Close();
        }
    }
}


        // Create a card-like panel for each member
        // Create a card-like panel for each member
        private Panel CreateCard(string name, string year, string major, string phoneNo, string email, Image qrImage, string qrText)
        {
            Panel card = new Panel
            {
                Width = 200,
                Height = 300, // Adjusted height to accommodate the QR text
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(10),
                Margin = new Padding(10),
            };

            // Create labels for text information
            Label lblName = new Label { Text = "Name: " + name, Dock = DockStyle.Top };
            Label lblYear = new Label { Text = "Year: " + year, Dock = DockStyle.Top };
            Label lblMajor = new Label { Text = "Major: " + major, Dock = DockStyle.Top };
            Label lblPhoneNo = new Label { Text = "Phone: " + phoneNo, Dock = DockStyle.Top };
            Label lblEmail = new Label { Text = "Email: " + email, Dock = DockStyle.Top };

            // Label for QR content text
            Label lblQRText = new Label { Text = "QR Content: " + qrText, Dock = DockStyle.Top };

            // Create PictureBox for QR code
            PictureBox qrPictureBox = new PictureBox
            {
                Image = qrImage,
                SizeMode = PictureBoxSizeMode.Zoom,
                Dock = DockStyle.Bottom,
                Height = 100 // Adjust height as needed
            };

            // Add controls to the card panel
            card.Controls.Add(qrPictureBox);
            card.Controls.Add(lblQRText);  // Add QR content text
            card.Controls.Add(lblEmail);
            card.Controls.Add(lblPhoneNo);
            card.Controls.Add(lblMajor);
            card.Controls.Add(lblYear);
            card.Controls.Add(lblName);

            return card;
        }

        private string DecodeQRCode(Image qrImage)
        {
            // Convert the Image to a Bitmap for ZXing to process
            Bitmap bitmap = new Bitmap(qrImage);

            // Initialize the barcode reader
            var barcodeReader = new BarcodeReader();

            // Decode the QR code
            var result = barcodeReader.Decode(bitmap);

            // Return the decoded text or an empty string if decoding fails
            return result != null ? result.Text : string.Empty;
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }


    }
}
