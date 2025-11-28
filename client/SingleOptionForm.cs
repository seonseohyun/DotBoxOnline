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
    public partial class SingleOptionForm : Form
    {
        public SingleOptionForm()
        {
            InitializeComponent();
            BuildUI();
        }

        private ComboBox cbBoardSize;
        private ComboBox cbDifficulty;
        private Button btnBack;

        private void BuildUI()
        {
            this.Text = "Single Play Options";
            this.Size = new System.Drawing.Size(500, 700);

            // ===== 뒤로가기 버튼 =====
            btnBack = new Button();
            btnBack.Text = "◀ Back";            // 원하는 텍스트로 변경 가능
            btnBack.Size = new Size(80, 30);
            btnBack.Location = new Point(10, 10);  // 좌측 상단
            btnBack.Click += BtnBack_Click;
            this.Controls.Add(btnBack);
            btnBack.BringToFront();

            // ===== 제목 =====
            Label lblTitle = new Label();
            lblTitle.Text = "Single Play Options";
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(100, 60);
            lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            lblTitle.Font = new System.Drawing.Font("Arial", 20, System.Drawing.FontStyle.Bold);
            this.Controls.Add(lblTitle);

            // ===== 맵 크기 설정 =====
            Label lblBoard = new Label();
            lblBoard.Text = "Board Size";
            lblBoard.Location = new System.Drawing.Point(60, 150);
            lblBoard.Font = new System.Drawing.Font("Arial", 14);
            this.Controls.Add(lblBoard);

            cbBoardSize = new ComboBox();
            cbBoardSize.Location = new System.Drawing.Point(200, 150);
            cbBoardSize.Items.Add("5 x 5");
            cbBoardSize.Items.Add("6 x 6");
            cbBoardSize.Items.Add("7 x 7");
            cbBoardSize.SelectedIndex = 0;
            this.Controls.Add(cbBoardSize);

            // ===== 난이도 설정 =====
            Label lblDiff = new Label();
            lblDiff.Text = "Difficulty";
            lblDiff.Location = new System.Drawing.Point(60, 230);
            lblDiff.Font = new System.Drawing.Font("Arial", 14);
            this.Controls.Add(lblDiff);

            cbDifficulty = new ComboBox();
            cbDifficulty.Location = new System.Drawing.Point(200, 230);
            cbDifficulty.Items.Add("Easy");
            cbDifficulty.Items.Add("Normal");
            cbDifficulty.Items.Add("Hard");
            cbDifficulty.SelectedIndex = 1;
            this.Controls.Add(cbDifficulty);

            // ===== 싱글 게임시작 버튼 =====
            Button btnStart = new Button();
            btnStart.Text = "Start Game";
            btnStart.Font = new System.Drawing.Font("Arial", 14);
            btnStart.Size = new System.Drawing.Size(200, 50);
            btnStart.Location = new System.Drawing.Point(150, 400);

            btnStart.Click += BtnStartGame_Click;
            this.Controls.Add(btnStart);
        }

        // ====== 버튼 이벤트 함수 ======

        // 게임시작 클릭
        private void BtnStartGame_Click(object sender, EventArgs e)
        {
            // TODO: 나중에 여기서 옵션 값들을 모아서 GamePlayForm에 넘겨줄 수 있음
            // var selectedOption = ...;

            // MainForm을 찾아서 화면전환 함수 호출
            MainForm main = (MainForm)this.ParentForm;
            main.LoadChildForm(new GamePlayForm());
        }

        // 뒤로가기 클릭
        private void BtnBack_Click(object sender, EventArgs e)
        {
            // ParentForm 은 항상 MainForm 이므로 캐스팅
            MainForm main = (MainForm)this.ParentForm;

            // 초기 화면으로 전환
            main.LoadChildForm(new HomeForm());
        }

    }
}
