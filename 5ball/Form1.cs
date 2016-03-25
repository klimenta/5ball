using System;
using System.Drawing;
using System.Windows.Forms;

namespace _5ball
{
    public partial class Form1 : Form
    {

        const int cGridElements = 9;
        const int cLeftPadding = 32;
        const int cTopPadding = 32;
        const int cPictureSizeWidth = 50;
        const int cPictureSizeHeight = 50;
        int i, j;

        public static PictureBox[,] arrPictureBox = new PictureBox[cGridElements, cGridElements];

        public Form1()
        {
            InitializeComponent();            
            for (i = 0; i < cGridElements; i++)                
            {                                                
                for (j = 0; j < cGridElements; j++)
                {
                    arrPictureBox[i, j] = new PictureBox();
                    arrPictureBox[i, j].Parent = this;
                    arrPictureBox[i, j].Top = cTopPadding + cPictureSizeHeight * j;
                    arrPictureBox[i, j].Left = cLeftPadding + cPictureSizeWidth * i;
                    arrPictureBox[i, j].Width = cPictureSizeWidth;
                    arrPictureBox[i, j].Height = cPictureSizeHeight;
                    //arrPictureBox[i, j].Image = ((System.Drawing.Image)(Properties.Resources._100Wx75H));
                    arrPictureBox[i, j].MouseDown += pictureBox_MouseDown;
                    arrPictureBox[i, j].AllowDrop = true;
                    arrPictureBox[i, j].DragEnter += pictureBox_DragEnter;
                    arrPictureBox[i, j].DragDrop += pictureBox_DragDrop;
                    //arrPictureBox[i, j].GiveFeedback += pictureBox_GiveFeedback;
                }
            }
            arrPictureBox[0, 0].Image = ((System.Drawing.Image)(Properties.Resources.green));
            arrPictureBox[3, 5].Image = ((System.Drawing.Image)(Properties.Resources.green));
            arrPictureBox[6, 8].Image = ((System.Drawing.Image)(Properties.Resources.green));

        }

        private void pictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                var dragImage = (Bitmap)(sender as PictureBox).Image;
                IntPtr icon = dragImage.GetHicon();
                Cursor.Current = new Cursor(icon);
                if (DoDragDrop((sender as PictureBox).Image, DragDropEffects.Copy) == DragDropEffects.Copy)
                {
                    (sender as PictureBox).Image = null;
                }
                DestroyIcon(icon);
            }

        }

        private void pbMinimize_MouseLeave(object sender, EventArgs e)
        {
            pbMinimize.Image = ((System.Drawing.Image)(Properties.Resources.min_off));
        }

        private void pbMinimize_MouseEnter(object sender, EventArgs e)
        {
            pbMinimize.Image = ((System.Drawing.Image)(Properties.Resources.min_on));
        }

        private void pbExit_MouseEnter(object sender, EventArgs e)
        {
            pbExit.Image = ((System.Drawing.Image)(Properties.Resources.close_on));
        }

        private void pbExit_MouseLeave(object sender, EventArgs e)
        {
            pbExit.Image = ((System.Drawing.Image)(Properties.Resources.close_off));
        }

        private void pbMinimize_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        private void pbExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        protected override void OnGiveFeedback(GiveFeedbackEventArgs e)
        {
            e.UseDefaultCursors = false;
        }
        void pictureBox_DragEnter (object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(Bitmap))) e.Effect = DragDropEffects.Copy;
        }
        void pictureBox_DragDrop(object sender, DragEventArgs e)
        {
            var bmp = (Bitmap)e.Data.GetData(typeof(Bitmap));
            //var pb = new PictureBox();
            //pb.Image = (Bitmap)e.Data.GetData(typeof(Bitmap));
            //pb.Size = pb.Image.Size;
            //pb.Location = this.PointToClient(new Point(e.X - pb.Width / 2, e.Y - pb.Height / 2));
            //this.Controls.Add(pb);
            (sender as PictureBox).Image = bmp;
        }


        [System.Runtime.InteropServices.DllImport("user32.dll")]
        extern static bool DestroyIcon(IntPtr handle);

    }
}
