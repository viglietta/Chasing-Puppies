using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows;
using System.Windows.Forms;

namespace Puppy
{
    public partial class PuppyForm : Form
    {
        private TrackForm tf;
        private Track t;
        private DirectBitmap b;
        private DirectBitmap bb;
        private double windowWScale = 1;
        private double windowHScale = 1;
        private bool quality = true;
        private System.Drawing.Point mouseOffset = new System.Drawing.Point(0, 0);
        private bool squareLayout = true;
        public bool extendedDiagram = false;
        private PointF[] ClipPath;
        byte[] ClipBytes;
        public bool mousePressed = false;
        public double mouseX, mouseY;

        public PuppyForm(TrackForm tf)
        {
            this.tf = tf;
            t = tf.t;
            InitializeComponent();
            Recreate(t);
            Location = new System.Drawing.Point(tf.Size.Width, 0);
            Show();
        }

        private int WindowBaseSizeX() => squareLayout ? Program.PUPPY_WINDOW_SIZE_1 : (extendedDiagram ? Program.PUPPY_WINDOW_SIZE_3 : Program.PUPPY_WINDOW_SIZE_2);

        private int WindowBaseSizeY() => squareLayout ? Program.PUPPY_WINDOW_SIZE_1 : (extendedDiagram ? Program.PUPPY_WINDOW_SIZE_1 : Program.PUPPY_WINDOW_SIZE_2);

        private (int, int) ModeScale()
        {
            if (squareLayout) return (ClientSize.Width, ClientSize.Height);
            if (!extendedDiagram) return (ClientSize.Width * Program.PUPPY_WINDOW_SIZE_1 / Program.PUPPY_WINDOW_SIZE_2, ClientSize.Height * Program.PUPPY_WINDOW_SIZE_1 / Program.PUPPY_WINDOW_SIZE_2);
            return (ClientSize.Width * Program.PUPPY_WINDOW_SIZE_1 / Program.PUPPY_WINDOW_SIZE_3, ClientSize.Height);
        }

        public void Recreate(Track t, bool toggleExtended = false)
        {
            this.t = t;
            if (toggleExtended)
            {
                extendedDiagram = !extendedDiagram;
                t.ComputeCritical(extendedDiagram);
            }
            int n = Program.PUPPY_BITMAP_BASE_RESOLUTION;
            if (b != null) b.Dispose();
            b = new DirectBitmap(n, n);
            if (extendedDiagram) for (int x = 0; x < n; x++) t.PlotColumnExtended(b, x);
            else for (int x = 0; x < n; x++) t.PlotColumn(b, x);
            ResizeWindow(windowWScale, windowHScale);
        }

        public void ResizeWindow(double sw, double sh, bool toggleSquare = false)
        {
            if (toggleSquare)
            {
                squareLayout = !squareLayout;
                sw = windowWScale;
                sh = windowHScale;
            }
            int sizeX = (int)(sw * WindowBaseSizeX());
            int sizeY = (int)(sh * WindowBaseSizeY());
            if (sizeX > Screen.FromControl(this).Bounds.Width || sizeY > Screen.FromControl(this).Bounds.Height)
            {
                if (toggleSquare) squareLayout = !squareLayout;
                return;
            }
            int ox = ClientSize.Width;
            int oy = ClientSize.Height;
            ClientSize = new System.Drawing.Size(sizeX, sizeY);
            Location = new System.Drawing.Point(Location.X + (ox - ClientSize.Width) / 2, Location.Y + (oy - ClientSize.Height) / 2);
            windowWScale = sw;
            windowHScale = sh;
            tf.gd.pfScale = Math.Min(sw, sh);
            CreateClip();
            RedrawBackground();
        }

        private void CreateClip()
        {
            var (sw, sh) = ModeScale();
            if (!extendedDiagram) {
                ClipPath = new PointF[4] { new PointF(sw, 0), new PointF(0, sh), new PointF(sw * 0.5f, ClientSize.Height + 0.5f), new PointF(ClientSize.Width + 0.5f, sh * 0.5f) };
                ClipBytes = new byte[4] { 0, 1, 1, 129 };
                return;
            }
            int n = t.puppyDiagonal.Count / 2;
            int m = 2 * n + 2;
            ClipPath = new PointF[m];
            ClipBytes = new byte[m];
            for(int i = 0; i < n; i++)
            {
                PointF q = t.puppyDiagonal[i * 2].ToPointF();
                ClipPath[i] = new PointF(q.X * sw, (1 - q.Y) * sh);
                ClipPath[m - 1 - i] = new PointF((q.X + 1) * sw + 0.5f, (1 - q.Y) * sh);
                ClipBytes[i + 1] = 1;
                ClipBytes[n + i + 1] = 1;
            }
            ClipPath[n] = new PointF(sw, 0);
            ClipPath[n + 1] = new PointF(2 * sw + 0.5f, 0);
            ClipBytes[0] = 0;
            ClipBytes[m - 1] = 129;
        }

        private void RedrawBackground()
        {
            if (b == null) return;
            if (bb != null) bb.Dispose();
            int sw = (int)(windowWScale * Program.PUPPY_WINDOW_SIZE_1);
            int sh = (int)(windowHScale * Program.PUPPY_WINDOW_SIZE_1);
            bb = new DirectBitmap(sw, sh);
            using (var g = Graphics.FromImage(bb.Bitmap))
            {
                g.CompositingMode = CompositingMode.SourceCopy;
                g.CompositingQuality = CompositingQuality.HighSpeed;
                g.PixelOffsetMode = PixelOffsetMode.Half;
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.DrawImage(b.Bitmap, new RectangleF(0, 0, sw, sh));
                g.CompositingMode = CompositingMode.SourceOver;
                if (quality)
                {
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.CompositingQuality = CompositingQuality.HighQuality;
                }
                else
                {
                    g.SmoothingMode = SmoothingMode.HighSpeed;
                    g.CompositingQuality = CompositingQuality.HighSpeed;
                }
                tf.gd.DrawPSegmentList(g, t.unstableCrit, sw, sh, Program.UNSTABLE_COLOR);
                tf.gd.DrawPSegmentList(g, t.stableCrit, sw, sh, Program.STABLE_COLOR);
                tf.gd.DrawDiagramDiag(g, t.puppyDiagonal, sw, sh, Program.STABLE_COLOR, squareLayout, extendedDiagram);
            }
            tf.gd.Refresh(false, true, false);
        }

        private void PuppyForm_Paint(object sender, PaintEventArgs e)
        {
            if (bb == null) return;
            tf.gd.refreshingPf = false;
            Graphics g = e.Graphics;
            g.CompositingMode = CompositingMode.SourceCopy;
            g.CompositingQuality = CompositingQuality.HighSpeed;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            var (sw, sh) = ModeScale();
            if (!squareLayout)
            {
                g.Clip.Dispose();
                g.Clip = new Region(new GraphicsPath(ClipPath, ClipBytes));
            }
            g.DrawImage(bb.Bitmap, new RectangleF(0, 0, sw, sh));
            if (!squareLayout)
            {
                g.DrawImage(bb.Bitmap, new RectangleF(sw, 0, sw, sh));
                if (!extendedDiagram) g.DrawImage(bb.Bitmap, new RectangleF(0, sh, sw, sh));
            }
            g.CompositingMode = CompositingMode.SourceOver;
            if (quality)
            {
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.CompositingQuality = CompositingQuality.HighQuality;
            }
            else
            {
                g.SmoothingMode = SmoothingMode.HighSpeed;
                g.CompositingQuality = CompositingQuality.HighSpeed;
            }

            if (mousePressed && extendedDiagram)
            {
                double px = (mouseX.Frac() * sw);
                double py = ((1 - mouseY.Frac()) * sh);
                tf.gd.DiagramCircle(g, new Vector(px, py));
                if (!squareLayout) tf.gd.DiagramCircle(g, new Vector(px + sw, py));
            }
            else
            {
                double px = t.DrawHuman;
                double py = t.captured ? t.DrawHuman : t.Puppy;
                //double py = t.reached ? t.DrawPuppy : t.Puppy;
                //double py = t.Human == t.DrawHuman ? t.DrawPuppy : t.Puppy;

                bool diag = px == py;
                bool left = px < py;

                if (extendedDiagram)
                {
                    if (!squareLayout && py == 0) left = true;
                    if (tf.mousePressed) py = tf.mouseAttracted;
                    else py = t.ExtendedConversion(py, px);
                }

                double cx = px;
                double cy = 1 - py;
                if (!squareLayout && left)
                {
                    if (!extendedDiagram && cy < cx) cy += 1;
                    else cx += 1;
                }
                tf.gd.DiagramCircle(g, new Vector(cx * sw, cy * sh));
                if (!squareLayout && diag)
                {
                    tf.gd.DiagramCircle(g, new Vector((cx + 1) * sw, cy * sh));
                    if (!extendedDiagram)
                    {
                        tf.gd.DiagramCircle(g, new Vector(cx * sw, (cy + 1) * sh));
                        tf.gd.DiagramCircle(g, new Vector((cx + 1) * sw, (cy - 1) * sh));
                        tf.gd.DiagramCircle(g, new Vector((cx - 1) * sw, (cy + 1) * sh));
                    }
                }
            }
        }

        private void PuppyForm_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Back: tf.RecreateTrack(t.trackType); break;
                case Keys.D1: tf.RecreateTrack(1); break;
                case Keys.D2: tf.RecreateTrack(2); break;
                case Keys.D3: tf.RecreateTrack(3); break;
                case Keys.D4: tf.RecreateTrack(4); break;
                case Keys.D5: tf.RecreateTrack(5); break;
                case Keys.D6: tf.RecreateTrack(6); break;
                case Keys.D7: tf.RecreateTrack(7); break;
                case Keys.D8: tf.RecreateTrack(8); break;
                case Keys.D9: tf.RecreateTrack(9); break;
                case Keys.D0: tf.RecreateTrack(0); break;
                case Keys.P: Hide(); break;
                case Keys.H: TrackForm.ToggleForm(tf.hf); break;
                case Keys.C: tf.ChamferTrack(); break;
                case Keys.Q: quality = !quality; RedrawBackground(); break;
                case Keys.Enter: ResizeWindow(0, 0, true); break;
                case Keys.Space: Recreate(t, true); break;
                case Keys.PageUp: ResizeWindow(windowWScale * Program.PUPPY_DIAGRAM_RESIZE_FACTOR, windowHScale * Program.PUPPY_DIAGRAM_RESIZE_FACTOR); break;
                case Keys.PageDown: ResizeWindow(windowWScale / Program.PUPPY_DIAGRAM_RESIZE_FACTOR, windowHScale / Program.PUPPY_DIAGRAM_RESIZE_FACTOR); break;
                case Keys.S: ResizeWindow(windowWScale, windowHScale * Program.PUPPY_DIAGRAM_RESIZE_FACTOR); break;
                case Keys.A: ResizeWindow(windowWScale, windowHScale / Program.PUPPY_DIAGRAM_RESIZE_FACTOR); break;
                case Keys.X: ResizeWindow(windowWScale * Program.PUPPY_DIAGRAM_RESIZE_FACTOR, windowHScale); break;
                case Keys.Z: ResizeWindow(windowWScale / Program.PUPPY_DIAGRAM_RESIZE_FACTOR, windowHScale); break;
                case Keys.R: ResizeWindow(1, 1); break;
                case Keys.Up: tf.gd.pfDiagramScale *= Program.FEATURE_RESIZE_FACTOR; RedrawBackground(); break;
                case Keys.Down: tf.gd.pfDiagramScale /= Program.FEATURE_RESIZE_FACTOR; RedrawBackground(); break;
            }
        }

        private void HandleMouse(MouseEventArgs e)
        {
            if (tf.gd == null || t == null) return;
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left) {
                var (sw, sh) = ModeScale();
                mousePressed = true;
                mouseX = (double)e.X / sw;
                mouseY = 1 - (double)e.Y / sh;
                t.RelocateHuman(mouseX, true);
                t.RelocatePuppy(extendedDiagram ? t.ExtendedConversion2(mouseY, mouseX).Item1 : mouseY, 0);
            }
        }

        private void PuppyForm_MouseMove(object sender, MouseEventArgs e)
        {
            HandleMouse(e);
            if ((e.Button & MouseButtons.Right) == MouseButtons.Right)
            {
                System.Drawing.Point mousePos = Control.MousePosition;
                mousePos.Offset(mouseOffset);
                Location = mousePos;
            }
        }

        private void PuppyForm_MouseDown(object sender, MouseEventArgs e)
        {
            HandleMouse(e);
            if ((e.Button & MouseButtons.Right) == MouseButtons.Right)
                mouseOffset = new System.Drawing.Point(-e.X, -e.Y);
        }

        private void PuppyForm_MouseUp(object sender, MouseEventArgs e)
        {
            if (tf.gd == null || t == null) return;
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
            {
                var (sw, sh) = ModeScale();
                mousePressed = false;
                mouseY = 1 - (double)e.Y / sh;
                if (extendedDiagram)
                {
                    mouseX = (double)e.X / sw;
                    var (w, dir) = t.ExtendedConversion2(mouseY, mouseX);
                    t.RelocatePuppy(w, dir, true);
                }
                else t.RelocatePuppy(mouseY, 0, true);
            }
        }
    }
}
