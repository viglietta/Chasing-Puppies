using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows;
using System.Windows.Forms;

namespace Puppy
{
    public partial class HumanForm : Form
    {
        private TrackForm tf;
        private Track t;
        private DirectBitmap bb;
        private double windowWScale = 1;
        private double windowHScale = 1;
        public double xOffset = 0;
        private bool quality = true;
        private bool drawPortraits = true;
        private System.Drawing.Point mouseOffset = new System.Drawing.Point(0, 0);
        private int mouseX = 0;

        public HumanForm(TrackForm tf)
        {
            this.tf = tf;
            t = tf.t;
            InitializeComponent();
            Recreate(t);
            Location = new System.Drawing.Point(0, tf.Size.Height);
            Show();
        }

        public void Recreate(Track t)
        {
            this.t = t;
            ResizeWindow(windowWScale, windowHScale);
        }

        private void ResizeWindow(double sw, double sh)
        {
            int nx = (int)(sw * Program.HUMAN_WINDOW_WIDTH);
            int ny = (int)(sh * Program.HUMAN_WINDOW_HEIGHT);
            if (nx > Screen.FromControl(this).Bounds.Width) return;
            if (ny > Screen.FromControl(this).Bounds.Height) return;
            int ox = ClientSize.Width;
            int oy = ClientSize.Height;
            ClientSize = new System.Drawing.Size(nx, ny);
            Location = new System.Drawing.Point(Location.X + (ox - ClientSize.Width) / 2, Location.Y + (oy - ClientSize.Height) / 2);
            windowWScale = sw;
            windowHScale = sh;
            tf.gd.hfScale = sh;
            RedrawBackground();
        }

        private void RedrawBackground()
        {
            if (bb != null) bb.Dispose();
            bb = new DirectBitmap(ClientSize.Width, ClientSize.Height);
            bb.Clear(Program.HUMAN_BACKGROUND_COLOR);
            using (var g = Graphics.FromImage(bb.Bitmap))
            {
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
                tf.gd.DrawHSegmentList(g, t.humanDiagram, ClientSize.Width, ClientSize.Height, Program.HUMAN_LINE_COLOR);
                tf.gd.DrawHSegmentList(g, t.humanDiagramBase, ClientSize.Width, ClientSize.Height, Program.HUMAN_BASELINE_COLOR);
            }
            tf.gd.Refresh(false, false, true);
        }

        private void HumanForm_Paint(object sender, PaintEventArgs e)
        {
            if (bb == null) return;
            tf.gd.refreshingHf = false;
            Graphics g = e.Graphics;
            g.CompositingMode = CompositingMode.SourceCopy;
            g.CompositingQuality = CompositingQuality.HighSpeed;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.DrawImage(bb.Bitmap, new RectangleF((float)(xOffset * ClientSize.Width), 0, ClientSize.Width, ClientSize.Height));
            g.DrawImage(bb.Bitmap, new RectangleF((float)((xOffset - 1) * ClientSize.Width), 0, ClientSize.Width, ClientSize.Height));
            g.CompositingMode = CompositingMode.SourceOver;
            if (quality)
            {
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.InterpolationMode = InterpolationMode.HighQualityBilinear;
            }
            else
            {
                g.SmoothingMode = SmoothingMode.HighSpeed;
                g.CompositingQuality = CompositingQuality.HighSpeed;
                g.InterpolationMode = InterpolationMode.Low;
            }
            tf.gd.DrawHNormal(g, ClientSize.Width, ClientSize.Height, Program.NORMAL_COLOR);
            if (drawPortraits)
            {
                //g.PixelOffsetMode = PixelOffsetMode.Default;
                //var (pv, hv) = t.HumanDiagramCoordinates(t.reached ? t.DrawPuppy : t.Puppy, t.DrawHuman);
                var (pv, hv) = t.HumanDiagramCoordinates(t.DrawPuppy, t.DrawHuman);
                if (tf.pf.extendedDiagram && tf.pf.mousePressed) pv.X = hv.X = tf.pf.mouseY;
                if (tf.mousePressed) pv.X = hv.X = tf.mouseAttracted;
                tf.gd.DrawHBitmaps(g, pv, hv, xOffset, ClientSize.Width, ClientSize.Height);
            }
        }

        private void HumanForm_KeyDown(object sender, KeyEventArgs e)
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
                case Keys.P: TrackForm.ToggleForm(tf.pf); break;
                case Keys.H: Hide(); break;
                case Keys.C: tf.ChamferTrack(); break;
                case Keys.Q: quality = !quality; RedrawBackground(); break;
                case Keys.D: drawPortraits = !drawPortraits; tf.gd.Refresh(false, false, true); break;
                case Keys.Enter: tf.pf.ResizeWindow(0, 0, true); break;
                case Keys.Space: tf.pf.Recreate(t, true); break;
                case Keys.PageUp: ResizeWindow(windowWScale * Program.HUMAN_DIAGRAM_RESIZE_FACTOR, windowHScale * Program.HUMAN_DIAGRAM_RESIZE_FACTOR); break;
                case Keys.PageDown: ResizeWindow(windowWScale / Program.HUMAN_DIAGRAM_RESIZE_FACTOR, windowHScale / Program.HUMAN_DIAGRAM_RESIZE_FACTOR); break;
                case Keys.S: ResizeWindow(windowWScale, windowHScale * Program.HUMAN_DIAGRAM_RESIZE_FACTOR); break;
                case Keys.A: ResizeWindow(windowWScale, windowHScale / Program.HUMAN_DIAGRAM_RESIZE_FACTOR); break;
                case Keys.X: ResizeWindow(windowWScale * Program.HUMAN_DIAGRAM_RESIZE_FACTOR, windowHScale); break;
                case Keys.Z: ResizeWindow(windowWScale / Program.HUMAN_DIAGRAM_RESIZE_FACTOR, windowHScale); break;
                case Keys.R: ResizeWindow(1, 1); break;
                case Keys.Up: tf.gd.hfDiagramScale *= Program.FEATURE_RESIZE_FACTOR; RedrawBackground(); break;
                case Keys.Down: tf.gd.hfDiagramScale /= Program.FEATURE_RESIZE_FACTOR; RedrawBackground(); break;
            }
        }

        private void LineOnMouse(MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left && !TrackForm.IsControlDown())
            {
                if (tf.gd == null || t == null) return;
                tf.gd.drawNormal = true;
                tf.gd.normalX = ((double)e.X / ClientSize.Width).Frac();
                tf.gd.Refresh(true, false, true);
            }
            else if (tf.gd.drawNormal)
            {
                mouseX = e.X;
                tf.gd.drawNormal = false;
            }
        }

        private void HumanForm_MouseMove(object sender, MouseEventArgs e)
        {
            LineOnMouse(e);
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left && TrackForm.IsControlDown())
            {
                xOffset = (xOffset + ((double)e.X - mouseX) / ClientSize.Width).Frac();
                mouseX = e.X;
                tf.gd.Refresh(false, false, true);
            }
            if ((e.Button & MouseButtons.Right) == MouseButtons.Right)
            {
                System.Drawing.Point mousePos = Control.MousePosition;
                mousePos.Offset(mouseOffset);
                Location = mousePos;
            }
        }

        private void HumanForm_MouseDown(object sender, MouseEventArgs e)
        {
            LineOnMouse(e);
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left && TrackForm.IsControlDown())
                mouseX = e.X;
            if ((e.Button & MouseButtons.Right) == MouseButtons.Right)
                mouseOffset = new System.Drawing.Point(-e.X, -e.Y);
        }

        private void HumanForm_MouseUp(object sender, MouseEventArgs e)
        {
            if (tf.gd == null || t == null) return;
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
            {
                tf.gd.drawNormal = false;
                tf.gd.Refresh(true, false, true);
            }
        }
    }
}
