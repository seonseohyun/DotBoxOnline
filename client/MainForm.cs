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
            LoadChildForm(new HomeForm());
        }

        // 화면전환 함수
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
    }
}
