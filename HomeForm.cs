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
    public partial class HomeForm : Form
    {
        private Label lblTitle;
        private Label lblBoard;
        private Label lblDifficulty;
        private ComboBox cboBoard;
        private ComboBox cboDifficulty;
        private Button btnSinglePlay;
        private Button btnMultiPlay;

        public HomeForm()
        {
            InitializeComponent();
            BuildUI();
        }
        private void BuildUI()
        {
            this.BackColor = Color.White;

            // 제목
            lblTitle = new Label();
            lblTitle.Text = "DOTS  BOXES GAME !";
            lblTitle.Font = new Font("맑은 고딕", 24, FontStyle.Bold);
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(200, 50);

            // Single Play 버튼
            btnSinglePlay = new Button();
            btnSinglePlay.Text = "Single Play";
            btnSinglePlay.Font = new Font("맑은 고딕", 14, FontStyle.Bold);
            btnSinglePlay.Size = new Size(200, 60);
            btnSinglePlay.Location = new Point(200, 270);
            btnSinglePlay.Click += BtnSinglePlay_Click;

            // Multi Play 버튼
            btnMultiPlay = new Button();
            btnMultiPlay.Text = "Multi Play";
            btnMultiPlay.Font = new Font("맑은 고딕", 14, FontStyle.Bold);
            btnMultiPlay.Size = new Size(200, 60);
            btnMultiPlay.Location = new Point(200, 350);
            btnMultiPlay.Click += BtnMultiPlay_Click;

            // 폼에 추가
            this.Controls.Add(lblTitle);
            this.Controls.Add(btnSinglePlay);
            this.Controls.Add(btnMultiPlay);
        }

        // ====== 버튼 이벤트 ======

        // Single Play 클릭
        private void BtnSinglePlay_Click(object sender, EventArgs e)
        {
            MainForm main = (MainForm)this.ParentForm;

            // 나중에 보드/난이도 값을 넘기고 싶으면 여기서 읽어서 인자로 전달하면 됨
            // string board = cboBoard.SelectedItem.ToString();
            // string difficulty = cboDifficulty.SelectedItem.ToString();

            main.LoadChildForm(new SingleOptionForm());
        }

        // Multi Play 클릭
        private void BtnMultiPlay_Click(object sender, EventArgs e)
        {
            MainForm main = (MainForm)this.ParentForm;
            main.LoadChildForm(new MultiOptionForm());
        }
    }
}
