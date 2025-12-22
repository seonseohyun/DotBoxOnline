using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace DotsAndBoxes
{
    public class OutlinedTextControl : Control
    {
        private float _letterSpacing = 3f;    
        private float _outlineThickness = 3f;  
        private Color _outlineColor = Color.FromArgb(25, 25, 25);

        // 중앙 정렬 기본
        private ContentAlignment _textAlign = ContentAlignment.MiddleCenter;

        [Category("Appearance")]
        public float LetterSpacing
        {
            get => _letterSpacing;
            set { _letterSpacing = Math.Max(0f, value); Invalidate(); }
        }

        [Category("Appearance")]
        public float OutlineThickness
        {
            get => _outlineThickness;
            set { _outlineThickness = Math.Max(0f, value); Invalidate(); }
        }

        [Category("Appearance")]
        public Color OutlineColor
        {
            get => _outlineColor;
            set { _outlineColor = value; Invalidate(); }
        }

        [Category("Appearance")]
        public ContentAlignment TextAlign
        {
            get => _textAlign;
            set { _textAlign = value; Invalidate(); }
        }

        // 품질 옵션: 기본 ON 
        [Category("Appearance")]
        public bool UseHighQuality { get; set; } = true;

        public OutlinedTextControl()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.UserPaint |
                     ControlStyles.SupportsTransparentBackColor, true);

            BackColor = Color.Transparent;
            ForeColor = Color.White;
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            Invalidate();
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (string.IsNullOrEmpty(Text))
                return;

            var g = e.Graphics;

            if (UseHighQuality)
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            }
            else
            {
                g.SmoothingMode = SmoothingMode.None;
                g.PixelOffsetMode = PixelOffsetMode.Default;
                g.TextRenderingHint = TextRenderingHint.SystemDefault;
            }

            // 글자별 폭 측정해서 전체 너비 계산
            float totalWidth = 0f;
            float maxHeight = 0f;

            using (var fmt = StringFormat.GenericTypographic)
            {
                fmt.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces;

                for (int i = 0; i < Text.Length; i++)
                {
                    string ch = Text[i].ToString();
                    var size = g.MeasureString(ch, Font, int.MaxValue, fmt);
                    totalWidth += size.Width;
                    if (i < Text.Length - 1) totalWidth += LetterSpacing;
                    if (size.Height > maxHeight) maxHeight = size.Height;
                }
            }

            // 시작 위치 계산 (정렬 반영)
            float startX = GetAlignedX(totalWidth);
            float startY = GetAlignedY(maxHeight);

            // 실제 렌더링: 글자 하나씩 path 만들어 outline + fill
            float x = startX;
            var brush = new SolidBrush(ForeColor);
            var pen = new Pen(OutlineColor, OutlineThickness)
            {
                LineJoin = LineJoin.Round
            };

            float emSize = Font.SizeInPoints * g.DpiY / 72f;

            for (int i = 0; i < Text.Length; i++)
            {
                string ch = Text[i].ToString();

                var path = new GraphicsPath();
                path.AddString(
                    ch,
                    Font.FontFamily,
                    (int)Font.Style,
                    emSize,
                    new PointF(x, startY),
                    StringFormat.GenericTypographic
                );

                // Outline 먼저
                if (OutlineThickness > 0.01f)
                    g.DrawPath(pen, path);

                // Fill
                g.FillPath(brush, path);

                // 다음 글자 위치로 이동 (MeasureString 기반)
                float chWidth;
                using (var fmt = StringFormat.GenericTypographic)
                {
                    fmt.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces;
                    chWidth = g.MeasureString(ch, Font, int.MaxValue, fmt).Width;
                }

                x += chWidth + LetterSpacing;
            }
        }

        private float GetAlignedX(float textWidth)
        {
            // Left / Center / Right 계열만 처리 (위/아래는 Y에서)
            bool isRight =
                TextAlign == ContentAlignment.TopRight ||
                TextAlign == ContentAlignment.MiddleRight ||
                TextAlign == ContentAlignment.BottomRight;

            bool isCenter =
                TextAlign == ContentAlignment.TopCenter ||
                TextAlign == ContentAlignment.MiddleCenter ||
                TextAlign == ContentAlignment.BottomCenter;

            float pad = OutlineThickness + 2f;

            if (isRight) return Width - textWidth - pad;
            if (isCenter) return (Width - textWidth) / 2f;
            return pad;
        }

        private float GetAlignedY(float textHeight)
        {
            bool isBottom =
                TextAlign == ContentAlignment.BottomLeft ||
                TextAlign == ContentAlignment.BottomCenter ||
                TextAlign == ContentAlignment.BottomRight;

            bool isMiddle =
                TextAlign == ContentAlignment.MiddleLeft ||
                TextAlign == ContentAlignment.MiddleCenter ||
                TextAlign == ContentAlignment.MiddleRight;

            float pad = OutlineThickness + 2f;

            if (isBottom) return Height - textHeight - pad;
            if (isMiddle) return (Height - textHeight) / 2f;
            return pad;
        }
    }
}
