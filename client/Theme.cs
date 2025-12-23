using System.Drawing;
using System.Windows.Forms;

namespace DotsAndBoxes
{
    public static class Theme
    {
        //public static readonly Color C_BG = Color.White;
        public static readonly Color C_TEXT_DEFAULT = Color.Black;
        public static readonly Color C_TEXT_HOVER = Color.White;

        public static readonly Color C_CARD_BG = Color.White;
        public static readonly Color C_HOVER = Color.FromArgb(45, 45, 45);
        public static readonly Color C_PRESSED = Color.FromArgb(30, 30, 30);

        public static readonly Color C_BORDER = Color.Black;
        public static readonly Color C_TEXT = Color.Black;
        public static readonly Color C_TEXT_DIM = Color.FromArgb(80, 80, 80);

        public const int CARD_BORDER_THICKNESS = 4;
        public const int CARD_CORNER_RADIUS = 28;

        // 디자인 : 폼
        public static void ApplyForm(Form form)
        {
            form.BackColor = C_CARD_BG;
            form.ForeColor = C_TEXT;
        }
        // 디자인 : 버튼효과 기본으로 초기화
        public static void ResetButtonStyle(Button btn)
        {
            btn.BackColor = C_CARD_BG;
            btn.ForeColor = C_TEXT_DEFAULT;
        }
        // 디자인 : 카드
        public static void ApplyCard(RoundedPanel card)
        {
            card.BackColor = C_CARD_BG;
            card.BorderColor = C_BORDER;
            card.BorderThickness = CARD_BORDER_THICKNESS;
            card.CornerRadius = CARD_CORNER_RADIUS;
        }
        // 디자인 : 버튼
        public static void ApplyButton(Button btn)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.UseVisualStyleBackColor = false;
            btn.FlatAppearance.BorderSize = 2;
            btn.FlatAppearance.BorderColor = C_BORDER;

            btn.BackColor = C_CARD_BG;
            btn.ForeColor = C_TEXT_DEFAULT;
            btn.Cursor = Cursors.Hand;
            btn.Font = new Font("Segoe UI", 12f, FontStyle.Bold);

            ResetButtonStyle(btn);

            // hover/press 최소 연출
            btn.MouseEnter += (_, __) =>
            {
                btn.BackColor = C_HOVER;
                btn.ForeColor = Color.White;
            };
            btn.MouseLeave += (_, __) =>
            {
                btn.BackColor = C_CARD_BG;
                btn.ForeColor = C_TEXT_DEFAULT;
            };
            btn.MouseDown += (_, __) =>
            {
                btn.BackColor = C_PRESSED;
                btn.ForeColor = Color.White;
            };
            btn.MouseUp += (_, __) =>
            {
                btn.BackColor = C_HOVER;
                btn.ForeColor = Color.White;
            };
        }
    }
}
