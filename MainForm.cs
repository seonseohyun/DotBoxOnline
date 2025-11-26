using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DotsAndBoxes
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            BuildUI();
        }

        private void BuildUI()
        {
            this.Text = "Dots and Boxes";
            this.Size = new System.Drawing.Size(600, 800);

            //제목
            Label lblTitle = new Label();
            lblTitle.Text = "DOTS & BOXES GAME !";
            lblTitle.Dock = DockStyle.Top;
            lblTitle.Height = 120;
            lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            lblTitle.Font = new System.Drawing.Font("Arial", 28, System.Drawing.FontStyle.Bold);
            this.Controls.Add(lblTitle);

            // 컴퓨터와 게임하기 버튼 (Single Play)
            Button btnSingle = new Button();
            btnSingle.Text = "Single Play";
            btnSingle.Font = new System.Drawing.Font("Arial", 16);
            btnSingle.Size = new System.Drawing.Size(250, 60);
            btnSingle.Location = new System.Drawing.Point(170, 300);

            btnSingle.Click += new System.EventHandler(this.BtnSingle_Click);
            this.Controls.Add(btnSingle);

            // 다른사람과 게임하기 버튼 (Online Play)
            Button btnOnline = new Button();
            btnOnline.Text = "Multi Play";
            btnOnline.Font = new System.Drawing.Font("Arial", 16);
            btnOnline.Size = new System.Drawing.Size(250, 60);
            btnOnline.Location = new System.Drawing.Point(170, 380);

            btnOnline.Click += BtnOnline_Click;
            this.Controls.Add(btnOnline);
        }

        //클릭시 실행되는 이벤트함수들
        private void BtnSingle_Click(object sender, EventArgs e)
        {
            SingleOptionsForm f = new SingleOptionsForm();
            f.Show();
        }
        private void BtnOnline_Click(object sender, EventArgs e)
        {
            MultiOptionForm f = new MultiOptionForm();
            f.Show();
        }
    }
}
