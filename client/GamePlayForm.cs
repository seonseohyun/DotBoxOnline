using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Text;

namespace DotsAndBoxes
{
    public partial class GamePlayForm : Form
    {
        int N;
        float spacing = 80f;
        float startX = 50f;
        float startY = 100f;

        private PointF[,] dots;
        private bool[,] hLines;
        private bool[,] vLines;
        private int[,] hLineOwner;
        private int[,] vLineOwner;
        private int[,] boxes;

        // 접속 플레이어 (최대 3명)
        private List<string> _players = new List<string>();

        // 실제 게임에 참여하는 인원 수
        private int _activePlayerCount;

        // 현재 턴인 플레이어 인덱스 (0, 1, 2)
        private int _currentPlayerIndex;

        // 멀티 모드용 playerId 리스트 (서버 ID)
        private List<string> _playerIds = new List<string>();

        private int _maxPlayers = 3; // 기본값 3
        private bool isInitialized = false;

        private readonly Color _p1Color = Color.FromArgb(70, 110, 160);
        private readonly Color _p2Color = Color.FromArgb(160, 90, 90);
        private readonly Color _p3Color = Color.FromArgb(90, 130, 100);

        private enum PlayerType { Player1, Player2, AI }
        private PlayerType currentPlayer;

        public enum AIDifficulty { Easy, Normal, Hard }
        private AIDifficulty aiDifficulty = AIDifficulty.Hard;
        private bool isAIMode = false;
        private Random rand = new Random();

        // 모드 구분 & 멀티용 필드
        private enum GameMode
        {
            Single,      // 싱글 (Player vs AI)
            MultiOnline  // 서버 연동 멀티
        }

        // 기본은 싱글
        private GameMode _mode = GameMode.Single;

        // 멀티 모드용 : 방/플레이어 정보
        private string _roomId;              
        private string _myPlayerId;            

        // /draw 폴링용
        private long _lastSeq = 0;                  
        private System.Windows.Forms.Timer _drawTimer;

        // 게임끝 결과 처리
        private bool _gameEnded = false;   

        // GamePlayForm이 담당하는 라운드 번호
        private int _gameRound;

        // 싱글모드 생성자
        public GamePlayForm(int boardSize, AIDifficulty difficulty)
        {
            _mode = GameMode.Single;   // [멀티추가] 싱글 모드

            N = boardSize;
            aiDifficulty = difficulty;
            isAIMode = true;
            currentPlayer = PlayerType.AI;

            // 싱글 모드는 항상 2인 (Player + AI)
            _activePlayerCount = 2;
            _currentPlayerIndex = 1; // 0: Player, 1: AI (시작 턴을 AI로)

            // 상단 이름 설정 (AI 모드)
            _players = new List<string>
            {
                "Player", 
                "AI"      
            };

            NormalizePlayers();   // 항상 3칸 맞추는 함수
            InitializeGame();
        }

        // 멀티모드 생성자
        public GamePlayForm(int boardSize, List<string> players, List<string> playerIds, string roomId, string myPlayerId, int gameRound)
        {
            _mode = GameMode.MultiOnline;   // [멀티추가] 멀티 모드

            N = boardSize;
            isAIMode = false;
            currentPlayer = PlayerType.Player1;

            _players = players ?? new List<string>();
            _playerIds = playerIds ?? new List<string>();

            // 실제 플레이 인원 = 방에 들어온 사람 수 (최대 3명)
            _activePlayerCount = Math.Min(_players.Count, 3);

            // 멀티는 0번 플레이어부터 시작
            _currentPlayerIndex = 0;

            // 방/내 플레이어 ID 저장
            _roomId = roomId;
            _myPlayerId = myPlayerId;

            // 이번 게임 라운드 번호 저장
            _gameRound = gameRound;

            _maxPlayers = AppSession.MaxPlayers;
            _activePlayerCount = Math.Min(_players.Count, 3); // 실제 플레이 인원

            NormalizePlayers();   // 내부적으로 3칸에 맞춰줌
            InitializeGame();
            ApplyMaxPlayersToScoreUI();
        }

        private void InitializeGame()
        {
            InitializeComponent();
            //this.DoubleBuffered = true;
            //BuildUI();

            dots = new PointF[N, N];
            hLines = new bool[N, N - 1];
            vLines = new bool[N - 1, N];
            hLineOwner = new int[N, N - 1];
            vLineOwner = new int[N - 1, N];
            boxes = new int[N - 1, N - 1];

            for (int r = 0; r < N; r++)
                for (int c = 0; c < N - 1; c++) hLineOwner[r, c] = -1;
            for (int r = 0; r < N - 1; r++)
                for (int c = 0; c < N; c++) vLineOwner[r, c] = -1;
            for (int r = 0; r < N - 1; r++)
                for (int c = 0; c < N - 1; c++) boxes[r, c] = -1;

            GenerateDots();
            isInitialized = true;

            this.Paint += GamePlayForm_Paint;
            this.MouseClick += GamePlayForm_MouseClick;

            this.Load += (s, e) =>
            {
                UpdateScores();
                this.Invalidate();

                // 싱글 모드에서만 AI 루프 시작
                if (_mode == GameMode.Single &&
                    isAIMode && currentPlayer == PlayerType.AI)
                {
                    AIMoveLoop();
                }

                // [멀티추가] 멀티 모드에서는 /draw 폴링 시작
                if (_mode == GameMode.MultiOnline)
                {
                    StartDrawPolling();
                }
            };

            // 폼 크기가 바뀌면 보드도 다시 중앙 정렬
            this.Resize += (s, e) =>
            {
                if (!isInitialized) return;

                GenerateDots();   
                Invalidate();   
            };
        }

        // 상단 접속자 3칸 맞춰주는 메서드
        private void NormalizePlayers()
        {
            // 최대 3명까지만 사용
            if (_players.Count > 3)
            {
                _players = _players.GetRange(0, 3);
            }
            while (_players.Count < 3)
            {
                _players.Add(" ");
            }
        }
        
        // 2P/3P 설정에 따라 상단 UI 조정
        private void ApplyMaxPlayersToScoreUI() 
        {
            if (_maxPlayers == 2)
            {
                // 2인 모드 → Player3 슬롯 비활성화 
                if (lblPlayer3Score != null)
                {
                    lblPlayer3Score.BackColor = Color.White;
                }
            }
            else
            {
                // 3인 모드 → 원래 색으로 복구 
                if (lblPlayer3Score != null) 
                {
                    lblPlayer3Score.ForeColor = Color.Black;
                }
            }
        }

        private void GenerateDots()
        {
            // 보드의 실제 픽셀 크기 계산
            float boardWidth = (N - 1) * spacing;
            float boardHeight = (N - 1) * spacing;

            // 중앙 정렬용 시작 좌표 계산
            startX = (this.ClientSize.Width - boardWidth) / 2f;
            startY = (this.ClientSize.Height - boardHeight) / 2f;

            for (int r = 0; r < N; r++)
                for (int c = 0; c < N; c++)
                    dots[r, c] = new PointF(startX + c * spacing, startY + r * spacing);
        }

        private void GamePlayForm_Paint(object sender, PaintEventArgs e)
        {
            if (!isInitialized) return;
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            Brush dotBrush = new SolidBrush(Color.FromArgb(85, 85, 85));

            Brush player1BoxBrush = new SolidBrush(Color.FromArgb(170, 200, 225));
            Brush player2BoxBrush = new SolidBrush(Color.FromArgb(225, 185, 185));
            Brush player3BoxBrush = new SolidBrush(Color.FromArgb(135, 180, 135));

            float dotRadius = 6f;      
            float lineThickness = 5f;
            Color[] playerLineColors = new Color[]
            {
                Color.FromArgb(70, 110, 160), 
                Color.FromArgb(160, 90, 90),  
                Color.FromArgb(90, 130, 100) 
            };


            for (int r = 0; r < N - 1; r++)
            {
                for (int c = 0; c < N - 1; c++)
                {
                    int owner = boxes[r, c];
                    if (owner == 1)
                        g.FillRectangle(player1BoxBrush, dots[r, c].X + 4, dots[r, c].Y + 4, spacing - 8, spacing - 8);
                    else if (owner == 2)
                        g.FillRectangle(player2BoxBrush, dots[r, c].X + 4, dots[r, c].Y + 4, spacing - 8, spacing - 8);
                    else if (owner == 3)
                        g.FillRectangle(player3BoxBrush, dots[r, c].X + 4, dots[r, c].Y + 4, spacing - 8, spacing - 8);
                }
            }

            for (int r = 0; r < N; r++)
            {
                for (int c = 0; c < N; c++)
                {
                    // 가로선
                    if (c < N - 1 && hLines[r, c])
                    {
                        int owner = hLineOwner[r, c];

                        Color lineColor = (owner >= 0 && owner < playerLineColors.Length)
                            ? playerLineColors[owner]
                            : Color.Gray;

                        using (var pen = new Pen(lineColor, lineThickness))
                        {
                            pen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
                            pen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                            g.DrawLine(pen, dots[r, c], dots[r, c + 1]);
                        }
                    }

                    // 세로선
                    if (r < N - 1 && vLines[r, c])
                    {
                        int owner = vLineOwner[r, c];

                        Color lineColor = (owner >= 0 && owner < playerLineColors.Length)
                            ? playerLineColors[owner]
                            : Color.Gray;               

                        using (var pen = new Pen(lineColor, lineThickness))
                        {
                            pen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
                            pen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                            g.DrawLine(pen, dots[r, c], dots[r + 1, c]);
                        }
                    }
                }
            }
            // 점 그리기
            for (int r = 0; r < N; r++)
            {
                for (int c = 0; c < N; c++)
                {
                    g.FillEllipse(dotBrush,
                        dots[r, c].X - dotRadius,
                        dots[r, c].Y - dotRadius,
                        dotRadius * 2,
                        dotRadius * 2);
                }
            }
            ((SolidBrush)player1BoxBrush).Dispose();
            ((SolidBrush)player2BoxBrush).Dispose();
            ((SolidBrush)player3BoxBrush).Dispose();
            ((SolidBrush)dotBrush).Dispose();
        }

        // 멀티 모드용 MouseClick
        private void GamePlayForm_MouseClick(object sender, MouseEventArgs e)
        {
            if (_mode == GameMode.Single)
                HandleMouseClick_Single(e);
            else
                _ = HandleMouseClick_MultiAsync(e); // async 호출
        }

        // 싱글 모드용 클릭 처리 
        private void HandleMouseClick_Single(MouseEventArgs e)
        {
            if (isAIMode && currentPlayer != PlayerType.Player1) return;

            var nearest = GetNearestLine(e.Location);
            if (!nearest.HasValue) return;

            int playerIndex;

            // 싱글: Player1(0), AI(1)
            playerIndex = (currentPlayer == PlayerType.Player1) ? 0 : 1;

            bool madeBox = PlaceLine(nearest.Value.isH, nearest.Value.r, nearest.Value.c, playerIndex);

            UpdateScores();
            this.Invalidate();

            if (!madeBox)
            {
                // AI 모드: 한 칸 그렸는데 박스 못 만들면 AI 턴으로 넘김
                currentPlayer = PlayerType.AI;
                _currentPlayerIndex = 1; // AI 인덱스
            }

            // 플레이어가 한 수 둔 뒤 게임 종료 체크
            CheckGameEnd();

            if (isAIMode && currentPlayer == PlayerType.AI) AIMoveLoop();
        }

        // 멀티 모드용 클릭 처리 (/choice)
        private async System.Threading.Tasks.Task HandleMouseClick_MultiAsync(MouseEventArgs e)
        {
            // 1) 어느 선인지 찾기 (기존 로직 재사용)
            var nearest = GetNearestLine(e.Location);
            if (!nearest.HasValue) return;

            bool isH = nearest.Value.isH;
            int r = nearest.Value.r;
            int c = nearest.Value.c;

            // 2) ChoiceRequest 구성
            var req = new ChoiceRequest    // [멀티추가] DTO는 아래에 정의
            {
                roomId = _roomId,
                playerId = _myPlayerId,
                isHorizontal = isH,
                row = r,
                col = c
            };

            ChoiceResponse res;
            try
            {
                res = await ServerApi.SendChoiceAsync(req); // [멀티추가] ServerApi.cs에 구현
            }
            catch (Exception ex)
            {
                MessageBox.Show($"서버 통신 오류: {ex.Message}");
                return;
            }

            if (res == null)
            {
                return;
            }


            if (res.status != "ok")
            {
                // 에러 처리 (필요하면 코드별로 메시지)
                if (res.errorCode == "NOT_YOUR_TURN" && !string.IsNullOrEmpty(res.currentTurnPlayerId))
                {
                    MessageBox.Show($"지금은 {res.currentTurnPlayerId} 차례입니다.");
                }
                else
                {
                    MessageBox.Show($"선 그리기 실패: {res.errorCode}");
                }
                return;
            }

            // 3) 서버가 인정한 move를 로컬 보드에 반영
            if (res.move != null)
            {
                int ownerIndex = GetPlayerIndexById(res.move.playerId);
                bool madeBox = PlaceLine(res.move.isHorizontal, res.move.row, res.move.col, ownerIndex);
            }

            // 4) 마지막 시퀀스 업데이트
            _lastSeq = res.moveSeq;

            // 5) 점수 / 화면 갱신
            UpdateScores();
            this.Invalidate();

            // 6) 게임 종료 여부는 일단 로컬 기준으로 체크
            CheckGameEnd();
        }

        // 멀티 모드용 playerId -> 인덱스(0,1,2) 매핑 헬퍼
        private int GetPlayerIndexById(string playerId)
        {
            if (_playerIds == null || _playerIds.Count == 0)
                return 0;

            int idx = _playerIds.IndexOf(playerId);   // ★ 여기만 바뀜 (닉네임X, playerIdO)

            if (idx < 0) idx = 0;
            if (idx >= _activePlayerCount) idx = _activePlayerCount - 1;
            return idx;
        }

        private (bool isH, int r, int c)? GetNearestLine(Point location)
        {
            float threshold = spacing / 3f;

            for (int r = 0; r < N; r++)
                for (int c = 0; c < N - 1; c++)
                    if (!hLines[r, c] && DistancePointToLine(location, dots[r, c], dots[r, c + 1]) < threshold)
                        return (true, r, c);

            for (int r = 0; r < N - 1; r++)
                for (int c = 0; c < N; c++)
                    if (!vLines[r, c] && DistancePointToLine(location, dots[r, c], dots[r + 1, c]) < threshold)
                        return (false, r, c);

            return null;
        }

        private float DistancePointToLine(Point p, PointF a, PointF b)
        {
            float dx = b.X - a.X, dy = b.Y - a.Y;
            if (dx == 0 && dy == 0) return Distance(p, a);
            float t = ((p.X - a.X) * dx + (p.Y - a.Y) * dy) / (dx * dx + dy * dy);
            t = Math.Max(0, Math.Min(1, t));
            return Distance(p, new PointF(a.X + t * dx, a.Y + t * dy));
        }

        private float Distance(Point p, PointF a) => (float)Math.Sqrt((p.X - a.X) * (p.X - a.X) + (p.Y - a.Y) * (p.Y - a.Y));

        private bool PlaceLine(bool isH, int r, int c, int player)
        {
            if (isH)
            {
                if (hLines[r, c]) return false;
                hLines[r, c] = true;
                hLineOwner[r, c] = player;
            }
            else
            {
                if (vLines[r, c]) return false;
                vLines[r, c] = true;
                vLineOwner[r, c] = player;
            }

            bool madeBox = false;

            if (isH)
            {
                if (r > 0 && hLines[r - 1, c] && vLines[r - 1, c] && vLines[r - 1, c + 1] && boxes[r - 1, c] == -1)
                { boxes[r - 1, c] = player +1; madeBox = true; }
                if (r < N - 1 && hLines[r + 1, c] && vLines[r, c] && vLines[r, c + 1] && boxes[r, c] == -1)
                { boxes[r, c] = player +1; madeBox = true; }
            }
            else
            {
                if (c > 0 && vLines[r, c - 1] && hLines[r, c - 1] && hLines[r + 1, c - 1] && boxes[r, c - 1] == -1)
                { boxes[r, c - 1] = player +1; madeBox = true; }
                if (c < N - 1 && vLines[r, c + 1] && hLines[r, c] && hLines[r + 1, c] && boxes[r, c] == -1)
                { boxes[r, c] = player +1; madeBox = true; }
            }

            return madeBox;
        }

        private bool WouldMakeBox(bool isH, int r, int c, int player)
        {
            return PlaceLineSim(isH, r, c, player, out _);
        }

        private bool PlaceLineSim(bool isH, int r, int c, int player, out int[,] tempBoxes)
        {
            tempBoxes = (int[,])boxes.Clone();
            bool madeBox = false;

            if (isH)
            {
                if (r > 0 && hLines[r - 1, c] && vLines[r - 1, c] && vLines[r - 1, c + 1] && tempBoxes[r - 1, c] == -1) { tempBoxes[r - 1, c] = player; madeBox = true; }
                if (r < N - 1 && hLines[r + 1, c] && vLines[r, c] && vLines[r, c + 1] && tempBoxes[r, c] == -1) { tempBoxes[r, c] = player; madeBox = true; }
            }
            else
            {
                if (c > 0 && vLines[r, c - 1] && hLines[r, c - 1] && hLines[r + 1, c - 1] && tempBoxes[r, c - 1] == -1) { tempBoxes[r, c - 1] = player; madeBox = true; }
                if (c < N - 1 && vLines[r, c + 1] && hLines[r, c] && hLines[r + 1, c] && tempBoxes[r, c] == -1) { tempBoxes[r, c] = player; madeBox = true; }
            }

            return madeBox;
        }

        private List<(bool isH, int r, int c)> GetAvailableMoves()
        {
            var list = new List<(bool isH, int r, int c)>();
            for (int r = 0; r < N; r++)
                for (int c = 0; c < N - 1; c++)
                    if (!hLines[r, c]) list.Add((true, r, c));
            for (int r = 0; r < N - 1; r++)
                for (int c = 0; c < N; c++)
                    if (!vLines[r, c]) list.Add((false, r, c));
            return list;
        }

        private (bool isH, int r, int c) GetBestMoveMinimax(int depth)
        {
            var moves = GetAvailableMoves();
            int bestScore = int.MinValue;
            (bool isH, int r, int c) bestMove = moves[0];

            foreach (var move in moves)
            {
                PlaceLineSim(move.isH, move.r, move.c, 1, out int[,] tempBoxes);
                int score = EvaluateBoard(tempBoxes, 1) - EvaluateBoard(tempBoxes, 0);
                if (score > bestScore) { bestScore = score; bestMove = move; }
            }
            return bestMove;
        }

        private int EvaluateBoard(int[,] tempBoxes, int player)
        {
            int count = 0;
            for (int r = 0; r < N - 1; r++)
                for (int c = 0; c < N - 1; c++)
                    if (tempBoxes[r, c] == player) count++;
            return count;
        }

        private void AIMove()
        {
            var moves = GetAvailableMoves();
            if (moves.Count == 0) return;

            (bool isH, int r, int c) move = moves[rand.Next(moves.Count)];

            if (aiDifficulty == AIDifficulty.Normal)
            {
                foreach (var m in moves)
                    if (WouldMakeBox(m.isH, m.r, m.c, 1)) { move = m; break; }
            }
            else if (aiDifficulty == AIDifficulty.Hard)
            {
                move = GetBestMoveMinimax(2);
            }

            bool madeBox = PlaceLine(move.isH, move.r, move.c, 1);
            if (!madeBox)
                currentPlayer = PlayerType.Player1;
                _currentPlayerIndex = 0; // 다음 턴: 사람(0번)
        }

        private void AIMoveLoop()
        {
            while (currentPlayer == PlayerType.AI && GetAvailableMoves().Count > 0)
            {
                AIMove();
                UpdateScores();
                this.Invalidate();
                Application.DoEvents();
                Thread.Sleep(200);

                // AI가 둔 뒤에도 게임 종료 체크
                CheckGameEnd();
            }
        }

        // 현재 보드 상태 기준으로 플레이어별 점수 계산
        private int[] CalculateScores()
        {
            // scores[0]=1P, scores[1]=2P, scores[2]=3P
            int[] scores = new int[3];

            for (int r = 0; r < N - 1; r++)
            {
                for (int c = 0; c < N - 1; c++)
                {
                    int owner = boxes[r, c];
                    if (owner >= 1 && owner <= 3)
                    {
                        scores[owner - 1]++; 
                    }
                }
            }

            return scores;
        }

        private void UpdateScores()
        {
            int[] scores = CalculateScores();

            string name1 = _players[0];
            string name2 = _players[1];
            string name3 = _players[2];

            // 0번 플레이어 점수
            int s1 = (_activePlayerCount > 0) ? scores[0] : 0;
            int s2 = (_activePlayerCount > 1) ? scores[1] : 0;
            int s3 = (_activePlayerCount > 2) ? scores[2] : 0;

            lblPlayer1Score.ForeColor = _p1Color;
            lblPlayer2Score.ForeColor = _p2Color;
            lblPlayer3Score.ForeColor = _p3Color;

            lblPlayer1Score.Text = $"{name1}: {s1}";
            lblPlayer2Score.Text = $"{name2}: {s2}";
            if (_activePlayerCount < 3)
            {
                lblPlayer3Score.Text = string.Empty; 
            }
            else
            {
                lblPlayer3Score.Text = $"{name3}: {s3}";
            }
        }

        // MainForm 찾는 함수
        private MainForm GetMainForm()
        {
            foreach (Form f in Application.OpenForms)
            {
                if (f is MainForm)
                    return (MainForm)f;
            }
            return null;
        }

        // 모든 수가 끝나면 결과창 띄우는 메서드
        private void ShowGameResult()
        {
            // UI 스레드 보장
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(ShowGameResult));
                return;
            }

            int[] scores = CalculateScores();

            MainForm main = GetMainForm();
            if (main == null)
            {
                MessageBox.Show("화면 전환 실패: MainForm을 찾지 못했습니다.");
                return;
            }

            // 싱글(AI) / 멀티를 구분해서 GameResultForm 생성자 호출
            GameResultForm resultForm;

            if (isAIMode)
            {
                // 싱글(AI) : 4개짜리 생성자 사용
                resultForm = new GameResultForm(
                    N,              // 보드 크기
                    true,           // AI 모드
                    _players,       // 플레이어 이름 리스트
                    aiDifficulty    // AI 난이도
                );
            }
            else
            {
                // 멀티 : 7개짜리 생성자 사용
                resultForm = new GameResultForm(
                    N,              // 보드 크기
                    false,          // AI 모드 아님
                    _players,       // 플레이어 이름 리스트
                    aiDifficulty,   // (멀티면 무시해도 됨)
                    _roomId,
                    _myPlayerId,
                    _gameRound
                );
            }

            // 승자 판정 (2명/3명 모두 공용)
            int winnerIndex = -1;
            int bestScore = -1;
            bool isTie = false;

            for (int i = 0; i < _activePlayerCount; i++)
            {
                if (scores[i] > bestScore)
                {
                    bestScore = scores[i];
                    winnerIndex = i;
                    isTie = false;
                }
                else if (scores[i] == bestScore)
                {
                    // 최고 점수와 동점이면 비김 처리
                    isTie = true;
                }
            }

            if (winnerIndex == -1 || isTie)
            {
                resultForm.SetResultText("Draw");
            }
            else
            {
                string winnerName = _players[winnerIndex];
                resultForm.SetResultText($"{winnerName} Win!");
            }

            // 점수 요약 문자열 만들어서 결과창에 넘김  
            var sb = new StringBuilder();                        
            for (int i = 0; i < _activePlayerCount; i++)         
            {                                                    
                if (i > 0) sb.Append("   |   ");                 
                string name = _players[i];                       
                int score = scores[i];                          
                sb.Append($"{name}: {score}");                   
            }                                                  
            resultForm.SetScoreSummary(sb.ToString());           

            // 5) 결과창 로드
            main.LoadChildForm(resultForm);
        }

        // 남은 수가 없는지 확인하고, 없으면 결과창 띄움
        private void CheckGameEnd()
        {
            if (_gameEnded) return;

            if (GetAvailableMoves().Count == 0)
            {
                _gameEnded = true;

                // 1.5초(1500ms) 후에 결과창 보여주기
                var delayTimer = new System.Windows.Forms.Timer();
                delayTimer.Interval = 1500;

                delayTimer.Tick += (s, e) =>
                {
                    delayTimer.Stop();
                    delayTimer.Dispose();

                    if (!this.IsDisposed && this.IsHandleCreated)
                    {
                        this.BeginInvoke(new Action(() =>
                        {
                            ShowGameResult();
                        }));
                    }
                };

                delayTimer.Start();
            }
        }

        // /draw 폴링용 타이머
        private void StartDrawPolling()
        {
            _drawTimer = new System.Windows.Forms.Timer();
            _drawTimer.Interval = 500; // 0.5초 간격
            _drawTimer.Tick += async (s, e) =>
            {
                await PollDrawEventsAsync();
            };
            _drawTimer.Start();
        }

        // /draw 호출해서 새로운 이벤트만 반영
        private async Task PollDrawEventsAsync()
        {
            if (string.IsNullOrEmpty(_roomId)) return;

            DrawResponse res;
            try
            {
                res = await ServerApi.GetDrawEventsAsync(_roomId, _lastSeq);
            }
            catch
            {
                return; // 조용히 무시 (네트워크 흔들릴 수 있으니)
            }

            if (res == null || res.gameRound != _gameRound) //이전 게임의 이벤트
                return;
            if (res == null || res.events == null || res.events.Count == 0)
                return;

            foreach (var ev in res.events)
            {
                int ownerIndex = GetPlayerIndexById(ev.playerId);
                bool madeBox = PlaceLine(ev.isHorizontal, ev.row, ev.col, ownerIndex);
                _lastSeq = ev.seq;
            }

            UpdateScores();
            this.Invalidate();

            // 상대가 그린 마지막 선까지 반영된 뒤, 게임 종료 여부 체크
            CheckGameEnd();
        }
    }
}
