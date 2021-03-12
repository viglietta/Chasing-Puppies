using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows;

namespace Puppy
{
    public class GraphicsData
    {
        public bool drawArrow = false;
        public bool drawTangent = false;
        public bool drawNormal = false;
        public double normalX = -1;

        private TrackForm tf;
        private Vector center;

        private double tfScale;
        private double drawScale;
        private double featureScale;
        public double pfScale = 1;
        public double pfDiagramScale = 1;
        public double hfScale = 1;
        public double hfDiagramScale = 1;

        private Bitmap hBitmap = new Bitmap("h.png");
        private Bitmap pBitmap = new Bitmap("p.png");
        private double bitmapSize;

        private Pen linePen = new Pen(Program.TRACK_COLOR);
        private Pen dashPenT = new Pen(Program.TANGENT_COLOR);
        private Pen dashPenN = new Pen(Program.NORMAL_COLOR);
        private AdjustableArrowCap arrowCap;

        private Pen diagramPen = new Pen(Program.STABLE_COLOR);
        private SolidBrush circleBrush = new SolidBrush(Program.NORMAL_CIRCLE_COLOR);

        public bool refreshingTf = false;
        public bool refreshingPf = false;
        public bool refreshingHf = false;

        /*        private Font tokenFont;
                private Brush tokenFontBrush = new SolidBrush(Color.Black);
                private StringFormat tokenFormat = new StringFormat()
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center,
                };*/

        /*private double arrowLength;
        private double arrowWidth;
        private Brush arrowBrush = new SolidBrush(Color.Green);*/

        public GraphicsData(TrackForm tf)
        {
            this.tf = tf;
            UpdateSize();
            SetFeatureScale(Program.INITIAL_FEATURE_SCALE);
            SetScale(Program.INITIAL_TRACK_SCALE);
            linePen.LineJoin = LineJoin.Round;
            dashPenT.SetLineCap(LineCap.Flat, LineCap.Flat, DashCap.Flat);
            dashPenT.DashPattern = new float[] { 2, 2 };
            dashPenN.SetLineCap(LineCap.Flat, LineCap.Flat, DashCap.Flat);
            dashPenN.DashPattern = new float[] { 2, 2 };
            diagramPen.SetLineCap(LineCap.Round, LineCap.Round, DashCap.Flat);
            arrowCap = new AdjustableArrowCap((float)(Program.ARROW_W_SIZE / Program.ARROW_LINE_THICKNESS), (float)(Program.ARROW_H_SIZE / Program.ARROW_LINE_THICKNESS));
        }

        public void UpdateSize()
        {
            int w = tf.ClientSize.Width;
            int h = tf.ClientSize.Height;
            center = new Vector(w * 0.5, h * 0.5); // consider margins
            Vector avail = new Vector(w, h); // subtract margins
            if (avail.X <= 0) avail.X = 0;
            if (avail.Y <= 0) avail.Y = 0;
            Vector draw = tf.t.size; // size of drawing in drawing space
            drawScale = draw.X * avail.Y > avail.X * draw.Y ? avail.X / draw.X : avail.Y / draw.Y;
            drawScale *= tfScale;
            UpdateFeatureScale();
        }

        public void SetScale(double s)
        {
            tfScale = s;
            //tokenFont = new Font("Verdana", (float)(50 * s), FontStyle.Regular, GraphicsUnit.Pixel);
            UpdateLayout();
        }

        public void UpdateFeatureScale()
        {
            double s = featureScale * drawScale;
            linePen.Width = (float)(Program.TRACK_LINE_THICKNESS * s);
            dashPenT.Width = (float)(Program.ARROW_LINE_THICKNESS * s);
            dashPenN.Width = (float)(Program.NORMAL_LINE_THICKNESS * s);
            bitmapSize = Program.TRACK_PORTRAIT_SIZE * s;
        }

        public void SetFeatureScale(double s)
        {
            featureScale = s;
            UpdateFeatureScale();
            Refresh(true, false, false);
        }

        public void SetPScale(double s)
        {
            pfScale = s;
            UpdatePDiagramScale();
        }

        public void UpdatePDiagramScale()
        {
            //double s = pfDiagramScale * pfScale;
            Refresh(false, true, false);
        }

        public void SetPDiagramScale(double s)
        {
            pfDiagramScale = s;
            UpdatePDiagramScale();
        }

        public void SetHScale(double s)
        {
            hfScale = s;
            UpdateHDiagramScale();
        }

        public void UpdateHDiagramScale()
        {
            //double s = hfDiagramScale * hfScale;
            Refresh(false, false, true);
        }

        public void SetHDiagramScale(double s)
        {
            hfDiagramScale = s;
            UpdateHDiagramScale();
        }

        public void ScaleUp() => SetScale(tfScale * Program.TRACK_RESIZE_FACTOR);

        public void ScaleDown() => SetScale(tfScale / Program.TRACK_RESIZE_FACTOR);

        public void FeatureScaleUp() => SetFeatureScale(featureScale * Program.FEATURE_RESIZE_FACTOR);

        public void FeatureScaleDown() => SetFeatureScale(featureScale / Program.FEATURE_RESIZE_FACTOR);

        public void PDiagramScaleUp() => SetPDiagramScale(pfDiagramScale * Program.FEATURE_RESIZE_FACTOR);

        public void PDiagramScaleDown() => SetPDiagramScale(pfDiagramScale / Program.FEATURE_RESIZE_FACTOR);

        public void HDiagramScaleUp() => SetHDiagramScale(hfDiagramScale * Program.FEATURE_RESIZE_FACTOR);

        public void HDiagramScaleDown() => SetHDiagramScale(hfDiagramScale / Program.FEATURE_RESIZE_FACTOR);

        public void UpdateLayout()
        {
            UpdateSize();

            /*            button1.Size = new Size(buttonWidth, buttonHeight);

                        button1.Location = new Point(buttonMargin, h - button1.Size.Height - buttonMargin);*/

            Refresh(true, false, false);
        }

        private Vector Trans(Vector v)
        {
            return Vector.Add(Vector.Multiply(v, drawScale), center);
        }

        public Vector InvTrans(Vector v)
        {
            return Vector.Divide(Vector.Subtract(v, center), drawScale);
        }

        public void DrawLine(Graphics g, Vector p1, Vector p2) => g.DrawLine(linePen, Trans(p1).ToPointF(), Trans(p2).ToPointF());

        public void DrawPolygon(Graphics g, List<Vector> p)
        {
            PointF[] q = new PointF[p.Count];
            for (int i = 0; i < p.Count; i++) q[i] = Trans(p[i]).ToPointF();
            g.DrawPolygon(linePen, q);
        }

        public void DiagramCircle(Graphics g, Vector p)
        {
            float r = (float)(Program.DIAGRAM_CIRCLE_SIZE * pfDiagramScale * pfScale);
            float d = r * 2;
            RectangleF a = new RectangleF(Vector.Subtract(p, new Vector(r, r)).ToPointF(), new SizeF(d, d));
            circleBrush.Color = Program.PUPPY_CIRCLE_COLOR;
            g.FillEllipse(circleBrush, a);
        }

        /*        public void DrawToken(double x, double y, int num)
                {
                    Rectangle r = new Rectangle((int)(x - tokenSize), (int)(y - tokenSize), (int)(tokenSize * 2), (int)(tokenSize * 2));
                    g.FillEllipse(tokenBrush, r);
                    g.DrawEllipse(tokenPen, r);
                    g.DrawString(num.ToString(), tokenFont, tokenFontBrush, x, y, tokenFormat);
                }*/

        public void DrawPuppy(Graphics g, Vector p)
        {
            p = Trans(p);
            double sizey = bitmapSize;
            double sizex = sizey * pBitmap.Width / pBitmap.Height;
            RectangleF dest = new RectangleF((float)(p.X - sizex / 2 + sizex * Program.PUPPY_PORTRAIT_OFFSET), (float)(p.Y - sizex / 2), (float)(sizex), (float)(sizey));
            g.DrawImage(pBitmap, dest);
        }

        public void DrawHuman(Graphics g, Vector p)
        {
            p = Trans(p);
            double sizey = bitmapSize;
            double sizex = sizey * hBitmap.Width / hBitmap.Height;
            RectangleF dest = new RectangleF((float)(p.X - sizex / 2), (float)(p.Y - sizex / 2), (float)(sizex), (float)(sizey));
            g.DrawImage(hBitmap, dest);
        }

        public void DrawArrowFrame(Graphics g, Vector p1, Vector p2, Color col)
        {
            double f = bitmapSize / (2 * drawScale);
            double d = p1.Distance(p2);
            if (d < f * 2) return;
            if (drawArrow)
            {
                dashPenT.Color = col;
                Vector p3 = Vector.Add(p1, Vector.Multiply(Vector.Subtract(p2, p1), (d - f) / d));
                dashPenT.CustomStartCap = arrowCap;
                g.DrawLine(dashPenT, Trans(p3).ToPointF(), Trans(p1).ToPointF());
                dashPenT.StartCap = LineCap.Flat;
            }
            if (drawTangent)
            {
                dashPenT.Color = col;
                Vector p3 = Vector.Multiply(new Vector(p1.Y - p2.Y, p2.X - p1.X), Program.TANGENT_LENGTH / d);
                g.DrawLine(dashPenT, Trans(Vector.Add(p1, p3)).ToPointF(), Trans(p1).ToPointF());
                g.DrawLine(dashPenT, Trans(Vector.Subtract(p1, p3)).ToPointF(), Trans(p1).ToPointF());
            }
        }

        public void DrawPSegmentList(Graphics g, List<Vector> p, double sw, double sh, Color col)
        {
            diagramPen.Color = col;
            diagramPen.Width = (float)(Program.DIAGRAM_LINE_THICKNESS * pfDiagramScale * pfScale);
            int i = 0;
            while (i < p.Count)
            {
                Vector p1 = new Vector(p[i].X * sw, (1 - p[i].Y) * sh);
                i++;
                Vector p2 = new Vector(p[i].X * sw, (1 - p[i].Y) * sh);
                i++;
                g.DrawLine(diagramPen, p1.ToPointF(), p2.ToPointF());
            }
        }

        public void DrawHSegmentList(Graphics g, List<Vector> p, double sw, double sh, Color col)
        {
            diagramPen.Color = col;
            diagramPen.Width = (float)(Program.DIAGRAM_LINE_THICKNESS * hfDiagramScale * hfScale);
            int i = 0;
            while (i < p.Count)
            {
                Vector p1 = new Vector(p[i].X * sw, (0.5 - p[i].Y * Program.HUMAN_DIAGRAM_H_SCALE) * sh);
                i++;
                Vector p2 = new Vector(p[i].X * sw, (0.5 - p[i].Y * Program.HUMAN_DIAGRAM_H_SCALE) * sh);
                i++;
                g.DrawLine(diagramPen, p1.ToPointF(), p2.ToPointF());
            }
        }

        public void DrawDiagramDiag(Graphics g, List<Vector> p, double sw, double sh, Color col, bool squareLayout, bool extendedDiagram)
        {
            diagramPen.Color = col;
            float pw = (float)(Program.DIAGRAM_LINE_THICKNESS * pfDiagramScale * pfScale * (squareLayout ? 1 : 2));
            diagramPen.Width = pw;
            pw *= 2;
            if (extendedDiagram) {
                int i = 0;
                while (i < p.Count)
                {
                    float x1 = (float)(p[i].X * sw);
                    float y1 = (float)((1 - p[i++].Y) * sh);
                    float x2 = (float)(p[i].X * sw);
                    float y2 = (float)((1 - p[i++].Y) * sh);
                    g.DrawLine(diagramPen, new PointF(x1, y1), new PointF(x2, y2));
                    if (!squareLayout)
                    {
                        if (x1 < pw) g.DrawLine(diagramPen, new PointF((float)(x1 + sw), y1), new PointF((float)(x2 + sw), y2));
                        if (x2 > 1 - pw) g.DrawLine(diagramPen, new PointF((float)(x1 - sw), y1), new PointF((float)(x2 - sw), y2));
                    }
                }
            }
            else
            {
                float ofs = squareLayout ? 0 : 0;
                g.DrawLine(diagramPen, new PointF(0, ofs + (float)sh), new PointF(ofs + (float)sw, 0));
                if (!squareLayout)
                {
                    g.DrawLine(diagramPen, new PointF(ofs - (float)sw, ofs + (float)sh), new PointF(ofs + (float)sw, ofs - (float)sh));
                    g.DrawLine(diagramPen, new PointF(0, ofs + (float)(sh * 2)), new PointF(ofs + (float)(sw * 2), 0));
                }
            }
        }

        public void DrawHNormal(Graphics g, int w, int h, Color col)
        {
            if (!drawNormal) return;
            diagramPen.Color = col;
            diagramPen.Width = (float)(Program.DIAGRAM_NORMAL_THICKNESS * hfDiagramScale * hfScale);
            float x = (float)(normalX * w);
            g.DrawLine(diagramPen, new PointF(x, 0), new PointF(x, h - 1));
        }

        public void DrawTNormal(Graphics g, Vector p, Vector nor, Color col)
        {
            if (!drawNormal) return;
            dashPenN.Color = col;
            nor = Vector.Multiply(nor, Program.NORMAL_LENGTH);
            g.DrawLine(dashPenN, Trans(Vector.Add(p, nor)).ToPointF(), Trans(p).ToPointF());
            g.DrawLine(dashPenN, Trans(Vector.Subtract(p, nor)).ToPointF(), Trans(p).ToPointF());
            float r = (float)(Program.TRACK_CIRCLE_SIZE * featureScale * drawScale);
            float d = (float)(r * 2);
            RectangleF a = new RectangleF(Vector.Subtract(Trans(p), new Vector(r, r)).ToPointF(), new SizeF(d, d));
            circleBrush.Color = Program.NORMAL_CIRCLE_COLOR;
            g.FillEllipse(circleBrush, a);
        }

        public void DrawHBitmaps(Graphics g, Vector pv, Vector hv, double ofs, double sw, double sh)
        {
            double px = (pv.X + ofs).Frac() * sw;
            double py = (0.5 - pv.Y * Program.HUMAN_DIAGRAM_H_SCALE) * sh;
            double sizey = Program.DIAGRAM_PORTRAIT_SIZE * hfDiagramScale * hfScale;
            double sizex = sizey * pBitmap.Width / pBitmap.Height;
            g.DrawImage(pBitmap, new RectangleF((float)(px - sizex / 2 + sizex * Program.PUPPY_PORTRAIT_OFFSET), (float)(py - sizex / 2), (float)(sizex), (float)(sizey)));
            if (tf.t.attract)
            {
                double hx = (hv.X + ofs).Frac() * sw;
                double hy = (0.5 - hv.Y * Program.HUMAN_DIAGRAM_H_SCALE) * sh;
                sizex = sizey * hBitmap.Width / hBitmap.Height;
                g.DrawImage(hBitmap, new RectangleF((float)(hx - sizex / 2), (float)(hy - sizex / 2), (float)(sizex), (float)(sizey)));
            }
        }

        public void Refresh(bool r1, bool r2, bool r3)
        {
            if (tf == null) return;
            if (r1 && !refreshingTf)
            {
                refreshingTf = true;
                tf.Invalidate(false);
            }
            if (r2 && !refreshingPf && tf.pf != null)
            {
                refreshingPf = true;
                tf.pf.Invalidate(false);
            }
            if (r3 && !refreshingHf && tf.hf != null)
            {
                refreshingHf = true;
                tf.hf.Invalidate(false);
            }
        }
    }
}
