using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Threading;

namespace DotsAndBoxes
{
    public partial class GamePlayForm : Form
    {
        int N;
        float spacing = 80f;
        float startX = 50f;
        float startY = 50f;

        private PointF[,] dots;
        private bool[,] hLines;
        private bool[,] vLines;
        private int[,] hLineOwner;
        private int[,] vLineOwner;
        private int[,] boxes;

        private Label lblPlayer1Score;
        private Label lblPlayer2Score;

        private bool isInitialized = false;

        private enum PlayerType { Player1, Player2, AI }
        private PlayerType currentPlayer;

        public enum AIDifficulty { Easy, Normal, Hard }
        private AIDifficulty aiDifficulty = AIDifficulty.Hard;
        private bool isAIMode = false;
        private Random rand = new Random();

        public GamePlayForm(int boardSize, AIDifficulty difficulty)
        {
            N = boardSize;
            aiDifficulty = difficulty;
            isAIMode = true;
            currentPlayer = PlayerType.AI;
            InitializeGame();
        }

        public GamePlayForm(int boardSize)
        {
            N = boardSize;
            isAIMode = false;
            currentPlayer = PlayerType.Player1;
            InitializeGame();
        }

        private void InitializeGame()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            BuildUI();

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
                if (isAIMode && currentPlayer == PlayerType.AI)
                    AIMoveLoop();
            };
        }

        private void BuildUI()
        {
            this.Text = "Dots & Boxes";
            this.ClientSize = new Size(600, 700);
            this.StartPosition = FormStartPosition.CenterScreen;

            TableLayoutPanel tlp = new TableLayoutPanel
            {
                RowCount = 1,
                ColumnCount = 2,
                Dock = DockStyle.Top,
                Height = 50,
            };
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            lblPlayer1Score = new Label
            {
                Text = "Player1: 0",
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill,
                Font = new Font("Arial", 12, FontStyle.Bold)
            };
            lblPlayer2Score = new Label
            {
                Text = isAIMode ? "AI: 0" : "Player2: 0",
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill,
                Font = new Font("Arial", 12, FontStyle.Bold)
            };

            tlp.Controls.Add(lblPlayer1Score, 0, 0);
            tlp.Controls.Add(lblPlayer2Score, 1, 0);

            this.Controls.Add(tlp);
        }

        private void GenerateDots()
        {
            for (int r = 0; r < N; r++)
                for (int c = 0; c < N; c++)
                    dots[r, c] = new PointF(startX + c * spacing, startY + r * spacing);
        }

        private void GamePlayForm_Paint(object sender, PaintEventArgs e)
        {
            if (!isInitialized) return;
            Graphics g = e.Graphics;
            Brush dotBrush = Brushes.Black;
            Brush player1BoxBrush = Brushes.LightBlue;
            Brush player2BoxBrush = isAIMode ? Brushes.LightCoral : Brushes.LightGreen;

            for (int r = 0; r < N - 1; r++)
            {
                for (int c = 0; c < N - 1; c++)
                {
                    if (boxes[r, c] == 0) g.FillRectangle(player1BoxBrush, dots[r, c].X + 4, dots[r, c].Y + 4, spacing - 8, spacing - 8);
                    else if (boxes[r, c] == 1) g.FillRectangle(player2BoxBrush, dots[r, c].X + 4, dots[r, c].Y + 4, spacing - 8, spacing - 8);
                }
            }

            for (int r = 0; r < N; r++)
            {
                for (int c = 0; c < N; c++)
                {
                    if (c < N - 1 && hLines[r, c]) g.DrawLine(hLineOwner[r, c] == 0 ? Pens.Blue : Pens.Red, dots[r, c], dots[r, c + 1]);
                    if (r < N - 1 && vLines[r, c]) g.DrawLine(vLineOwner[r, c] == 0 ? Pens.Blue : Pens.Red, dots[r, c], dots[r + 1, c]);
                }
            }

            for (int r = 0; r < N; r++)
                for (int c = 0; c < N; c++)
                    g.FillEllipse(dotBrush, dots[r, c].X - 4, dots[r, c].Y - 4, 8, 8);
        }

        private void GamePlayForm_MouseClick(object sender, MouseEventArgs e)
        {
            if (isAIMode && currentPlayer != PlayerType.Player1) return;

            var nearest = GetNearestLine(e.Location);
            if (!nearest.HasValue) return;

            int playerIndex = currentPlayer == PlayerType.Player1 ? 0 : 1;
            bool madeBox = PlaceLine(nearest.Value.isH, nearest.Value.r, nearest.Value.c, playerIndex);

            UpdateScores();
            this.Invalidate();

            if (!madeBox)
            {
                currentPlayer = isAIMode ? PlayerType.AI : (currentPlayer == PlayerType.Player1 ? PlayerType.Player2 : PlayerType.Player1);
            }

            if (isAIMode && currentPlayer == PlayerType.AI) AIMoveLoop();
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
                { boxes[r - 1, c] = player; madeBox = true; }
                if (r < N - 1 && hLines[r + 1, c] && vLines[r, c] && vLines[r, c + 1] && boxes[r, c] == -1)
                { boxes[r, c] = player; madeBox = true; }
            }
            else
            {
                if (c > 0 && vLines[r, c - 1] && hLines[r, c - 1] && hLines[r + 1, c - 1] && boxes[r, c - 1] == -1)
                { boxes[r, c - 1] = player; madeBox = true; }
                if (c < N - 1 && vLines[r, c + 1] && hLines[r, c] && hLines[r + 1, c] && boxes[r, c] == -1)
                { boxes[r, c] = player; madeBox = true; }
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
            }
        }

        private void UpdateScores()
        {
            int score1 = 0, score2 = 0;
            for (int r = 0; r < N - 1; r++)
                for (int c = 0; c < N - 1; c++)
                {
                    if (boxes[r, c] == 0) score1++;
                    else if (boxes[r, c] == 1) score2++;
                }
            lblPlayer1Score.Text = $"Player1: {score1}";
            lblPlayer2Score.Text = isAIMode ? $"AI: {score2}" : $"Player2: {score2}";
        }
    }
}
