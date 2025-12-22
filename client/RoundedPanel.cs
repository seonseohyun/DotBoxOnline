using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace DotsAndBoxes
{
    public class RoundedPanel : Panel
    {
        public int CornerRadius { get; set; } = 40;
        public int BorderThickness { get; set; } = 5;
        public Color BorderColor { get; set; } = Color.FromArgb(240, 200, 255);

        public RoundedPanel()
        {
            // 깜빡임 방지(더블버퍼)
            this.DoubleBuffered = true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            // 패널 내부 영역 (테두리 두께만큼 줄임)
            Rectangle rect = this.ClientRectangle;
            rect.Width -= 1;
            rect.Height -= 1;

            int radius = CornerRadius;
            using (GraphicsPath path = CreateRoundRectPath(rect, radius))
            {
                // 1) 둥근 모양으로 클리핑 (자식 컨트롤도 둥글게 잘림)
                this.Region = new Region(path);

                // 2) 테두리 그리기
                if (BorderThickness > 0)
                {
                    using (Pen pen = new Pen(BorderColor, BorderThickness))
                    {
                        pen.Alignment = PenAlignment.Inset;
                        e.Graphics.DrawPath(pen, path);
                    }
                }
            }
        }

        private GraphicsPath CreateRoundRectPath(Rectangle r, int radius)
        {
            int d = radius * 2;
            GraphicsPath path = new GraphicsPath();

            path.AddArc(r.X, r.Y, d, d, 180, 90);                         // 좌상
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);                 // 우상
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);          // 우하
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);                 // 좌하
            path.CloseFigure();

            return path;
        }
    }
}
