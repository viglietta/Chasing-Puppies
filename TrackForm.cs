using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows;
using System.Windows.Forms;

namespace Puppy
{
    public partial class TrackForm : Form
    {
        private const int INITIAL_TRACK = 0;

        public GraphicsData gd;
        private bool quality = true;
        public Track t;
        public PuppyForm pf;
        public HumanForm hf;
        public bool mousePressed = false;
        public double mouseAttracted;
        private List<Vector> defaultTrack;

        public TrackForm()
        {
            InitializeComponent();
            defaultTrack = Program.ReadTrack();
            if (defaultTrack != null) t = new Track(this, -1, defaultTrack, false);
            else t = new Track(this, INITIAL_TRACK, false);
            gd = new GraphicsData(this);
            Size = new System.Drawing.Size(Program.TRACK_WINDOW_SIZE, Program.TRACK_WINDOW_SIZE);
            Location = new System.Drawing.Point(0, 0);
            pf = new PuppyForm(this);
            hf = new HumanForm(this);
        }

        public void RecreateTrack(int n)
        {
            if (t != null) t.Dispose();
            if (n == -1) t = new Track(this, -1, defaultTrack, pf != null ? pf.extendedDiagram : false);
            else t = new Track(this, n, pf != null ? pf.extendedDiagram : false);
            pf.Recreate(t);
            hf.Recreate(t);
            t.Refresh();
        }

        public void ChamferTrack()
        {
            if (t == null) return;
            List<Vector> c = t.Chamfer();
            int n = t.trackType;
            double human = t.DrawHuman;
            double puppy = t.DrawPuppy;
            t.Dispose();
            t = new Track(this, n, c, pf != null ? pf.extendedDiagram : false, human, puppy);
            pf.Recreate(t);
            hf.Recreate(t);
            t.Refresh();
        }

        public static void ToggleForm(Form f)
        {
            if (f.Visible) f.Hide();
            else f.Show();
        }

        private void TrackForm_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Back: RecreateTrack(t.trackType); break;
                case Keys.D1: RecreateTrack(1); break;
                case Keys.D2: RecreateTrack(2); break;
                case Keys.D3: RecreateTrack(3); break;
                case Keys.D4: RecreateTrack(4); break;
                case Keys.D5: RecreateTrack(5); break;
                case Keys.D6: RecreateTrack(6); break;
                case Keys.D7: RecreateTrack(7); break;
                case Keys.D8: RecreateTrack(8); break;
                case Keys.D9: RecreateTrack(9); break;
                case Keys.D0: RecreateTrack(0); break;
                case Keys.P: ToggleForm(pf); break;
                case Keys.H: ToggleForm(hf); break;
                case Keys.C: ChamferTrack(); break;
                case Keys.Q: quality = !quality; gd.Refresh(true, false, false); break;
                case Keys.D: gd.drawArrow = !gd.drawArrow; gd.Refresh(true, false, false); break;
                case Keys.F: gd.drawTangent = !gd.drawTangent; gd.Refresh(true, false, false); break;
                case Keys.Z: t.RelocateHuman(t.DrawHuman - 0.25); break;
                case Keys.X: t.RelocateHuman(t.DrawHuman + 0.25); break;
                case Keys.ShiftKey: t.RelocateHuman(t.DrawHuman); break;
                //case Keys.Escape: UndoList(); break;
                case Keys.Enter: pf.ResizeWindow(0, 0, true); break;
                case Keys.Space: pf.Recreate(t, true); break;
                case Keys.Up: gd.FeatureScaleUp(); break;
                case Keys.Down: gd.FeatureScaleDown(); break;
                case Keys.Right: t.SpeedUp(); break;
                case Keys.Left: t.SlowDown(); break;
                case Keys.PageUp: gd.ScaleUp(); break;
                case Keys.PageDown: gd.ScaleDown(); break;
                case Keys.R: gd.SetScale(Program.INITIAL_TRACK_SCALE); break;
            }
        }

        private void TrackForm_Paint(object sender, PaintEventArgs e)
        {
            if (gd == null || t == null) return;
            gd.refreshingTf = false;
            Graphics g = e.Graphics;
            if (quality)
            {
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.InterpolationMode = InterpolationMode.HighQualityBilinear;
                //g.TextRenderingHint = TextRenderingHint.AntiAlias;
            }
            else
            {
                g.SmoothingMode = SmoothingMode.HighSpeed;
                g.CompositingQuality = CompositingQuality.HighSpeed;
                g.InterpolationMode = InterpolationMode.Low;
                //g.TextRenderingHint = TextRenderingHint.SingleBitPerPixel;
            }
            gd.UpdateSize();
            t.Draw(e.Graphics);
        }

        private void TrackForm_Resize(object sender, EventArgs e)
        {
            if (gd == null) return;
            gd.UpdateLayout();
        }

        public static bool IsControlDown() => (Control.ModifierKeys & Keys.Control) == Keys.Control;

        private void MoveToMouse(MouseEventArgs e)
        {
            if (gd == null || t == null) return;
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
                t.RelocateHuman(t.ClosestOnTrack(gd.InvTrans(new Vector(e.X, e.Y)), false).Item1, IsControlDown());
            if ((e.Button & MouseButtons.Right) == MouseButtons.Right)
            {
                mousePressed = true;
                double w;
                (w, mouseAttracted) = t.ClosestOnTrack(gd.InvTrans(new Vector(e.X, e.Y)), true);
                t.RelocatePuppy(w, 0, false, true);
            }
        }

        private void TrackForm_MouseMove(object sender, MouseEventArgs e) => MoveToMouse(e);

        private void TrackForm_MouseDown(object sender, MouseEventArgs e) => MoveToMouse(e);

        private void TrackForm_MouseUp(object sender, MouseEventArgs e)
        {
            if (gd == null || t == null) return;
            if ((e.Button & MouseButtons.Right) == MouseButtons.Right)
            {
                mousePressed = false;
                double w;
                (w, mouseAttracted) = t.ClosestOnTrack(gd.InvTrans(new Vector(e.X, e.Y)), true);
                t.RelocatePuppy(w, t.ExtendedConversion2(mouseAttracted, t.Human).Item2, true);
            }
        }
    }
}
