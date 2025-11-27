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
        private Panel mainPanel; // 화면 전환용 패널

        public MainForm()
        {
            InitializeComponent();
            BuildUI();
            LoadChildForm(new HomeForm());      // 시작 화면 = HomeForm
        }

        private void BuildUI()
        {
            this.Text = "Dots and Boxes";
            this.ClientSize = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            // 화면 전환용 패널
            mainPanel = new Panel();
            mainPanel.Dock = DockStyle.Fill;     // MainForm 전체를 채우도록
            mainPanel.BackColor = Color.White;   // 임시 배경색
            this.Controls.Add(mainPanel);
        }

        // ====== 화면전환 하는 함수 (모든 폼 화면) ======
        public void LoadChildForm(Form childForm)
        {
            // 기존 화면 제거
            mainPanel.Controls.Clear();

            // 새 화면 설정
            childForm.TopLevel = false;     // 자식폼으로 사용
            childForm.FormBorderStyle = FormBorderStyle.None;
            childForm.Dock = DockStyle.Fill; // panel을 꽉 채움

            // panel에 부착
            mainPanel.Controls.Add(childForm);
            childForm.Show(); // 화면 표시
        }

        // ====== 버튼 이벤트 함수 ======
        private void BtnSingle_Click(object sender, EventArgs e)
        {
            // SingleOptionForm을 panel에 로드
            LoadChildForm(new SingleOptionForm());
        }
        private void BtnMulti_Click(object sender, EventArgs e)
        {
            // MultiOptionForm을 panel에 로드
            LoadChildForm(new MultiOptionForm());
        }
    }
}
