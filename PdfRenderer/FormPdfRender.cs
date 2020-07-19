using PdfRepresantation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PdfRenderer
{
    public partial class FormPdfRender : Form
    {
        public FormPdfRender()
        {
            InitializeComponent();
        }
        public FormPdfRender(string[] args):this()
        {
         if(args.Length>0)
             ShowImage(args[0]);
        }
        PdfImageWriter imageWriter = new PdfImageWriter();
        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() != DialogResult.OK)
                return;
            ShowImage(openFileDialog1.FileName);
        }

        private void ShowImage(string fileName)
        {
            var originalCursor = this.Cursor;
            try
            {
                this.Cursor = Cursors.WaitCursor;
                var details = PdfDetailsFactory.Create(fileName);
                var image = imageWriter.ConvertToImage(details);
                this.pictureBox1.Image = image;
                this.toolStripStatusLabel1.Text = openFileDialog1.FileName;
                pictureBox1.Width = image.Width;
                pictureBox1.Height = image.Height;
            }
            finally
            {
                this.Cursor = originalCursor;
            }
        }
    }
}
