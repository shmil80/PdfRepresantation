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
        PdfImageWriter imageWriter = new PdfImageWriter();
        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                                  
                var details=PdfDetailsFactory.Create(openFileDialog1.FileName);
                var image=imageWriter.ConvertToImage(details);
                this.pictureBox1.Image = image;
                pictureBox1.Width = image.Width;
                pictureBox1.Height = image.Height;
            }
        }
    }
}
