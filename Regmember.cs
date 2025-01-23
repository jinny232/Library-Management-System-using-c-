
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
using Zen;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace LibraryManagementSystem
{
    public partial class Regmember : UserControl
    {
        public event Action MemberAdded;
        public Regmember()
        {
            InitializeComponent();
        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void addBooks_addBtn_Click(object sender, EventArgs e)
        {
            string selectedMajor = "";

            if (cmbYear.SelectedItem != null && cmbYear.SelectedItem.ToString() == "1st Year")
            {
                CST.Checked = true;
                CS.Checked = false;
                CT.Checked = false;
            }
            if (CS.Checked)
            {
                selectedMajor = CS.Text;
            }
            else if (CT.Checked)
            {
                selectedMajor = CT.Text;
            }
            else if (CST.Checked)
            {
                selectedMajor = CST.Text;
            }

          
            if (string.IsNullOrEmpty(textBoxrollno.Text))
            {
                MessageBox.Show("Please enter a valid roll number.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                textBoxrollno.Focus();
                return;
            }
            // Validate Email Format
            if (!IsValidEmail(txtEmail.Text))
            {
                MessageBox.Show("Please enter a valid email address.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtEmail.Focus();
                return;
            }

            // Generate the QR Code with a Random String
            string qrContent = GenerateRandomString(10);  // Generate a 10-character random string
            Zen.Barcode.CodeQrBarcodeDraw qrCodeDraw = Zen.Barcode.BarcodeDrawFactory.CodeQr;
            System.Drawing.Image qrImage = qrCodeDraw.Draw(qrContent, 40);

            // Convert QR Code image to byte array
            byte[] qrCodeBytes;
            using (MemoryStream ms = new MemoryStream())
            {
                qrImage.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                qrCodeBytes = ms.ToArray();
            }

            string connectionString = @"Data Source=(LocalDB)\v11.0;AttachDbFilename=C:\Users\hp\OneDrive\Documents\LibraryDB.mdf;Integrated Security=True;Connect Timeout=30";
            string query = @"INSERT INTO [dbo].[LibMember]
            ([name], [year], [major], [phone], [email], [qr], [rollno],[qrcode])
            VALUES
            (@Name, @Year, @Major, @Phone, @Email, @QR, @rno,@qrcode)";

            using (SqlConnection connect = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, connect))
            {
                cmd.Parameters.AddWithValue("@Name", txtName.Text);
                cmd.Parameters.AddWithValue("@Year", cmbYear.SelectedItem.ToString());
                cmd.Parameters.AddWithValue("@Major", selectedMajor);
                cmd.Parameters.AddWithValue("@Phone", txtphoneno.Text);
                cmd.Parameters.AddWithValue("@Email", txtEmail.Text);
                cmd.Parameters.AddWithValue("@QR", qrCodeBytes);  // Save QR code as byte array in the database
                cmd.Parameters.AddWithValue("@rno", textBoxrollno.Text);
                cmd.Parameters.AddWithValue("@qrcode", qrContent);


                connect.Open();
                cmd.ExecuteNonQuery();
                connect.Close();
            }

            MessageBox.Show("Registered Successfully");

            // Update UI with member details
            label5.Text = txtName.Text;
            label13.Text = cmbYear.SelectedItem.ToString();
            label14.Text = selectedMajor;
            label15.Text = txtphoneno.Text;
            label16.Text = txtEmail.Text;
            label20.Text = textBoxrollno.Text;
            label22.Text = qrContent;  // Show the random string used in the QR code
            QRpictureBox.Image = qrImage;

            GenerateMemberPdf(txtName.Text, cmbYear.SelectedItem.ToString(), selectedMajor, txtphoneno.Text, txtEmail.Text, textBoxrollno.Text, qrContent, qrImage);
            // Clear inputs after successful registration
            txtName.Clear();
            txtEmail.Clear();
            txtphoneno.Clear();
            textBoxrollno.Clear();
            CS.Checked = false;
            CT.Checked = false;
            CT.Checked = false;
            cmbYear.SelectedIndex = -1;
            panel2.Visible = true;
            alertp.Visible = false;

        }
     
        private void GenerateMemberPdf(string name, string year, string major, string phone, string email, string rollno, string qrContent, System.Drawing.Image qrImage)
{
    string pdfPath = "C://Users//hp//OneDrive//Desktop//PDF//" + rollno + ".pdf";
    Document pdfDoc = new Document(PageSize.A4, 25, 25, 25, 25);

    using (FileStream stream = new FileStream(pdfPath, FileMode.Create))
    {
        PdfWriter.GetInstance(pdfDoc, stream);
        pdfDoc.Open();
        pdfDoc.Add(new Paragraph("Member Registration Details"));
        pdfDoc.Add(new Paragraph("Name: " + name));
        pdfDoc.Add(new Paragraph("Year: " + year));
        pdfDoc.Add(new Paragraph("Major: " + major));
        pdfDoc.Add(new Paragraph("Phone: " + phone));
        pdfDoc.Add(new Paragraph("Email: " + email));
        pdfDoc.Add(new Paragraph("Roll No: " + rollno));
        pdfDoc.Add(new Paragraph("QR Content: " + qrContent));

        // Add QR Code to PDF
        using (MemoryStream ms = new MemoryStream())
        {
            qrImage.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            iTextSharp.text.Image pdfImage = iTextSharp.text.Image.GetInstance(ms.ToArray());
            pdfImage.ScaleToFit(100f, 100f); // Adjust size as needed
            pdfDoc.Add(pdfImage);
        }

        pdfDoc.Close();
    }

    MessageBox.Show("PDF saved successfully at: " + pdfPath);
}
       
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private void addBooks_clearBtn_Click(object sender, EventArgs e)
        {
            // Clear all textboxes
            txtName.Clear();
            txtEmail.Clear();
            txtphoneno.Clear();

            // Clear ComboBox selections
            cmbYear.SelectedIndex = -1;

            // Uncheck all radio buttons
            CS.Checked = false;
            CT.Checked = false;
            CST.Checked = false;

            // Optionally reset focus to the first input field
            txtName.Focus();
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void label17_Click(object sender, EventArgs e)
        {

        }

        private void addBooks_updateBtn_Click(object sender, EventArgs e)
        {

        }

        private void Regmember_Load(object sender, EventArgs e)
        {

        }
        private string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            Random random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void cmbYear_SelectedIndexChanged(object sender, EventArgs e)
        {
     
        }

       
    }
}
