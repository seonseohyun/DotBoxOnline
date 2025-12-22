using System;
using System.Drawing;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Drawing2D;


namespace DotsAndBoxes
{
    // 서버 세션 정보 보관용 (전역)
    public static class AppSession
    {
        public static string PlayerId { get; set; }
        public static string PlayerName { get; set; }
        public static DateTime? ConnectedAt { get; set; }
        public static int MaxPlayers { get; set; }   // 옵션창에서 인원수 선택시
    }

    public partial class HomeForm : Form
    {

        public HomeForm()
        {
            InitializeComponent();

            this.DoubleBuffered = true; // 깜빡임 방지
            Theme.ApplyForm(this);       
            Theme.ApplyCard(pnlCard);
            Theme.ApplyButton(btnSinglePlay);
            Theme.ApplyButton(btnMultiPlay);

            ApplyTitleStyle();


            this.Resize += (s, e) =>
            {
                CenterCard();
                LayoutCard();
            };

            this.Shown += (s, e) => 
            {
                LayoutCard();
                CenterCard();
                pnlCard.Focus();
            };
        }

        // /connect: 최초 1회만 호출
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

        // Single Play 클릭
        private async void BtnSinglePlay_Click(object sender, EventArgs e)
        {
            // 최초 진입 시 /connect
            await EnsureConnectedAsync();
            // 연결 실패시 넘어가지 않음
            if (string.IsNullOrEmpty(AppSession.PlayerId))
                return;

            MainForm main = (MainForm)this.ParentForm;
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

        // 디자인 : 중앙정렬
        private void CenterCard()
        {
            pnlCard.Left = (this.ClientSize.Width - pnlCard.Width) / 2;
            pnlCard.Top = (this.ClientSize.Height - pnlCard.Height) / 2;
        }

        // 디자인 : 컨트롤러 중앙정렬
        private void LayoutCard()
        {
            // 타이틀 위쪽 중앙
            lblTitle.AutoSize = true;
            lblTitle.Left = (pnlCard.Width - lblTitle.Width) / 2;
            lblTitle.Top = 40;

            // 버튼 2개 세로 중앙 정렬
            btnSinglePlay.Width = 220;
            btnSinglePlay.Height = 60;
            btnMultiPlay.Width = 220;
            btnMultiPlay.Height = 60;

            btnSinglePlay.Left = (pnlCard.Width - btnSinglePlay.Width) / 2;
            btnMultiPlay.Left = (pnlCard.Width - btnMultiPlay.Width) / 2;

            btnSinglePlay.Top = 170;
            btnMultiPlay.Top = 270;
        }

        // 디자인 : 백그라운드
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            e.Graphics.Clear(Color.White);
        }

        // 디자인 : 타이틀
        private void ApplyTitleStyle()
        {
            lblTitle.Text = "DOTS & BOXES GAME !";
            lblTitle.Font = new Font("Impact", 28f, FontStyle.Underline);
            lblTitle.OutlineColor = Color.FromArgb(0, 0, 0);
            lblTitle.OutlineThickness = 3f;
            lblTitle.LetterSpacing = 3f;
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;
        }
    }
}
