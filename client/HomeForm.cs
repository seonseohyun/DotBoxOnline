using System;
using System.Drawing;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace DotsAndBoxes
{
    // 서버 세션 정보 보관용 (전역)
    public static class AppSession
    {
        public static string PlayerId { get; set; }
        public static string PlayerName { get; set; }
        public static DateTime? ConnectedAt { get; set; }
    }

    public partial class HomeForm : Form
    {
        private Label lblTitle;
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

        // ====== /connect: 최초 1회만 호출 ======
        private async Task EnsureConnectedAsync()
        {
            // 이미 playerId가 있으면 재접속 안 함
            if (!string.IsNullOrEmpty(AppSession.PlayerId))
                return;

            // 임시 닉네임 자동 생성 (나중에 닉네임 UI로 바꿔도 됨)
            if (string.IsNullOrWhiteSpace(AppSession.PlayerName))
            {
                AppSession.PlayerName = "Player_" + Guid.NewGuid().ToString("N").Substring(0, 4);
            }

            try
            {
                var res = await ServerApi.ConnectAsync(AppSession.PlayerName);

                AppSession.PlayerId = res.playerId;
                AppSession.PlayerName = res.playerName;

                if (DateTime.TryParse(res.connectedAt, out var utc))
                {
                    AppSession.ConnectedAt = utc;
                }
            }
            catch (Exception ex)
            {
                // ServerApi에서 던진 모든 에러 메시지를 여기서 보여줌
                MessageBox.Show(
                    ex.Message,
                    "서버 연결 오류",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        // ====== 버튼 이벤트 ======

        // Single Play 클릭
        private async void BtnSinglePlay_Click(object sender, EventArgs e)
        {
            // 최초 진입 시 /connect
            await EnsureConnectedAsync();

            // 연결 실패했다면 넘어가지 않음
            if (string.IsNullOrEmpty(AppSession.PlayerId))
                return;

            MainForm main = (MainForm)this.ParentForm;

            // 나중에 보드/난이도 값을 넘기고 싶으면 여기서 읽어서 인자로 전달하면 됨
            // string board = cboBoard.SelectedItem.ToString();
            // string difficulty = cboDifficulty.SelectedItem.ToString();

            main.LoadChildForm(new SingleOptionForm());
        }

        // Multi Play 클릭
        private async void BtnMultiPlay_Click(object sender, EventArgs e)
        {
            // 최초 진입 시 /connect
            await EnsureConnectedAsync();

            if (string.IsNullOrEmpty(AppSession.PlayerId))
                return;

            MainForm main = (MainForm)this.ParentForm;
            main.LoadChildForm(new MultiOptionForm());
        }
    }
}
