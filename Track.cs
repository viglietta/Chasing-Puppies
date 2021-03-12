using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;

namespace Puppy
{
    public class Track
    {
        private static double animationSpeed = 1;
        private static double margin = 0.01;

        public int trackType;
        private List<Vector> p = new List<Vector>();
        public Vector size = new Vector(2, 2);
        private double[] l;
        private double[] ls;
        private double tl;
        private Vector[] nor;
        private double[] an;
        private double[] ans;
        private double ta;
        private double[] hd;
        private double edgeFactor;
        private double angleFactor;
        private double human;
        private double puppy;
        private double drawHuman;
        private double drawPuppy;
        public bool captured = false;
        public bool reached = false;
        public bool attract = true;
        private TrackForm tf;
        private Timer timer;
        private Stopwatch stopwatch;
        public List<Vector> stableCrit;
        public List<Vector> unstableCrit;
        public List<Vector> humanDiagram;
        public List<Vector> humanDiagramBase;
        public List<Vector> puppyDiagonal;
        public CritData[,] critData;
        private int diagSign;
        private double diagMinY;
        private double diagMaxY;

        public struct CritData
        {
            public Vector? stable1;
            public Vector? stable2;
            public Vector? unstable1;
            public Vector? unstable2;
            public bool forward;
        }

        public double Human { get => human; set => human = value.Frac(); }
        public double Puppy { get => puppy; set => puppy = value.Frac(); }
        public double DrawHuman { get => drawHuman; set => drawHuman = value.Frac(); }
        public double DrawPuppy { get => drawPuppy; set => drawPuppy = value.Frac(); }

        private void CreateStar(int n = 20, double r1 = 1, double r2 = 0.55)
        {
            for (int i = 0; i < n; i++)
            {
                double a = 2 * Math.PI * i / n;
                double r = (i & 1) == 1 ? r1 : r2;
                p.Add(Vector.Multiply(r, new Vector(Math.Cos(a), Math.Sin(a))));
            }
            ComputeLengths();
            Human = ls[n / 2 - 1];
            Puppy = ls[n / 2 + 1];
        }

        private void CreateChamferedStar(int n = 26, double r1 = 1, double r2 = 0.6, double d = 0.05)
        {
            for (int i = 0; i < n; i++)
            {
                double a = 2 * Math.PI * i / n;
                double r = (i & 1) == 1 ? r1 : r2;
                p.Add(Vector.Multiply(r, new Vector(Math.Cos(a), Math.Sin(a))));
                p.Add(Vector.Multiply(r, new Vector(Math.Cos(a+d), Math.Sin(a+d))));
            }
            ComputeLengths();
            Human = ls[n / 2 - 1];
            Puppy = ls[n / 2 + 1];
        }

        private void CreateDoubleLoop(int n = 10, double r1 = 0.85, double r2 = 0.7)
        {
            for (int i = 0; i < n; i++)
            {
                double a = 2 * Math.PI * (i + 0.5) / n;
                p.Add(Vector.Multiply(r1, new Vector(Math.Cos(a), Math.Sin(a))));
            }
            for (int i = 0; i < n; i++)
            {
                double a = 2 * Math.PI * (i + 0.5) / n;
                p.Add(Vector.Multiply(r2, new Vector(Math.Cos(a), Math.Sin(a))));
            }
            ComputeLengths();
            Human = ls[n / 2];
            Puppy = ls[n + n / 2];
        }

        private void CreateLimacon(int n = 200, double a = 0.2, double b = 1) // r = a + b cos(theta)
        {
            for (int i = 0; i < n; i++)
            {
                double t = 2 * Math.PI * i / n;
                double r = a + b * Math.Cos(t);
                p.Add(Vector.Multiply(r, new Vector(-Math.Cos(t), Math.Sin(t))));
            }
            ComputeLengths();
            Human = 0;
            Puppy = 0.5;
        }

        private void CreateFigureEight(int n = 200, double a = 0.8, double b = 0.5)
        {
            for (int i = 0; i < n; i++)
            {
                double t = 2 * Math.PI * (i + 0.5) / n;
                p.Add(new Vector(a * Math.Cos(t), b * Math.Sin(2 * t)));
            }
            ComputeLengths();
            Human = (ls[n / 2] + ls[n / 2 - 1]) / 2;
            Puppy = (ls[n] + ls[n - 1]) / 2;
        }

        private void CreateDoubleFigureEight(int n = 150, double a = 0.8, double b = 0.5, double d = 0.1)
        {
            for (int i = 0; i < n; i++)
            {
                double t = 2 * Math.PI * i / n;
                p.Add(new Vector(-a * Math.Cos(t - Math.PI / 2) - d * Math.Sin(t / 2) / 2, b * Math.Sin(2 * t - Math.PI) + d * i / n));
            }
            for (int i = 0; i < n; i++)
            {
                double t = 2 * Math.PI * i / n;
                p.Add(new Vector(-a * Math.Cos(t - Math.PI / 2) + d * Math.Sin(t / 2) / 2, b * Math.Sin(2 * t - Math.PI) + d * (n - i) / n));
            }
            ComputeLengths();
            Human = 0.125;
            Puppy = 0.5;
        }

        private void CreateC(int n = 100, double a = 0.9, double b = 0.3, double c = 0.5)
        {
            int m = n / 2;
            for (int i = 0; i <= n; i++)
            {
                double t = Math.PI * i / n;
                p.Add(new Vector(-a * Math.Sin(t), a * Math.Cos(t)));
            }
            for (int i = 0; i <= m; i++)
            {
                double t = Math.PI * i / m;
                p.Add(new Vector(c + (a - b) * Math.Sin(t) / 2, -(a - b) * Math.Cos(t) / 2 - (a + b) / 2));
            }
            for (int i = 0; i <= n; i++)
            {
                double t = Math.PI * i / n;
                p.Add(new Vector(- b * Math.Sin(t), - b * Math.Cos(t)));
            }
            for (int i = 0; i <= m; i++)
            {
                double t = Math.PI * i / m;
                p.Add(new Vector(c + (a - b) * Math.Sin(t) / 2, -(a - b) * Math.Cos(t) / 2 + (a + b) / 2));
            }
            ComputeLengths();
            Human = 0.975;
            Puppy = 0.4;
        }

        private void CreateSpiral(int n = 11, int k = 2)
        {
            double d = 2 * k * Math.PI / n;
            for (int i = 0; i <= n; i++)
            {
                double t = i * d;
                p.Add(new Vector(t * Math.Cos(t), t * Math.Sin(t)));
            }
            for (int i = -n / (2 * k); i < n; i++)
            {
                double t = (n - i) * d;
                p.Add(new Vector(-t * Math.Cos(t), -t * Math.Sin(t)));
            }
            ComputeLengths();
            Human = (ls[n / 2] + ls[n / 2 - 1]) / 2;
            Puppy = (ls[n] + ls[n - 1]) / 2;
        }

        private void CreateDoubleSpiral(int n = 200, int k = 2)
        {
            double d = 2 * k * Math.PI / n;
            double m = (2 * k + 0.5) * Math.PI;
            for (int i = 0; i <= n; i++)
            {
                double t = i * d;
                p.Add(new Vector(t * Math.Cos(t) - m, t * Math.Sin(t)));
                if (i < n / 2) i++;
                if (i < n / 3) i++;
                if (i < n / 4) i++;
            }
            for (int i = n + n / (2 * k) - 1; i >= 1; i--)
            {
                double t = i * d;
                p.Add(new Vector(t * Math.Cos(t) + m, t * Math.Sin(t)));
                if (i < n / 2) i--;
                if (i < n / 3) i--;
                if (i < n / 4) i--;
            }
            for (int i = 0; i <= n; i++)
            {
                double t = i * d;
                p.Add(new Vector(-t * Math.Cos(t) + m, -t * Math.Sin(t)));
                if (i < n / 2) i++;
                if (i < n / 3) i++;
                if (i < n / 4) i++;
            }
            for (int i = n + n / (2 * k) - 1; i >= 1; i--)
            {
                double t = i * d;
                p.Add(new Vector(-t * Math.Cos(t) - m, -t * Math.Sin(t)));
                if (i < n / 2) i--;
                if (i < n / 3) i--;
                if (i < n / 4) i--;
            }
            ComputeLengths();
            Human = (ls[n / 2] + ls[n / 2 - 1]) / 2;
            Puppy = (ls[n] + ls[n - 1]) / 2;
        }

        private void CreateOrtho()
        {
            p.Add(new Vector(-0, 0)); p.Add(new Vector(-0, 2)); p.Add(new Vector(1, 2)); p.Add(new Vector(1, 4)); p.Add(new Vector(-2, 4)); p.Add(new Vector(-2, 8));
            p.Add(new Vector(-0, 8)); p.Add(new Vector(-0, 6)); p.Add(new Vector(2, 6)); p.Add(new Vector(2, 9)); p.Add(new Vector(-1, 9)); p.Add(new Vector(-1, 11));
            p.Add(new Vector(-21, 11)); p.Add(new Vector(-21, 7)); p.Add(new Vector(-20, 7)); p.Add(new Vector(-20, 3)); p.Add(new Vector(-18, 3)); p.Add(new Vector(-18, 0));
            p.Add(new Vector(-13, 0)); p.Add(new Vector(-13, 2)); p.Add(new Vector(-15, 2)); p.Add(new Vector(-15, 6)); p.Add(new Vector(-16, 6)); p.Add(new Vector(-16, 9));
            p.Add(new Vector(-3, 9)); p.Add(new Vector(-3, 7)); p.Add(new Vector(-8, 7)); p.Add(new Vector(-8, 2)); p.Add(new Vector(-11, 2)); p.Add(new Vector(-11, 0));
            p.Add(new Vector(-6, 0)); p.Add(new Vector(-6, 5)); p.Add(new Vector(-4, 5)); p.Add(new Vector(-4, 3)); p.Add(new Vector(-2, 3)); p.Add(new Vector(-2, 0));
            ComputeLengths();
            Human = ls[17];
            Puppy = ls[29];
        }

        /*        private void CreateTrefoil(int n = 100, double a = 0.5, double b = 1)
                {
                    for (int i = 0; i < n; i++)
                    {
                        double t = 2 * Math.PI * i / n;
                        double r = a + b * Math.Cos(t);
                        p.Add(Vector.Multiply(r, new Vector(Math.Cos(t) - (a + b) / 2, Math.Sin(t))));
                    }
                    ComputeLengths();
                    Human = ls[0];
                    Puppy = ls[n / 2];
                }*/

        public Track(TrackForm tf, int type, bool extended)
        {
            trackType = type;
            switch (type)
            {
                case 1: CreateOrtho(); break;
                case 2: CreateStar(24, 1, 0.65); break; // CreateStar(30, 0.8, 0.5);32, 0.6, 0.45
                case 3: CreateSpiral(); break;
                case 4: CreateDoubleSpiral(); break;
                case 5: CreateLimacon(); break;
                case 6: CreateFigureEight(); break;
                case 7: CreateDoubleFigureEight(); break;
                case 8: CreateChamferedStar(); break;
                default: CreateC(); break;
            }
            ComputeCritical(extended);
            this.tf = tf;
            stopwatch = new Stopwatch();
            timer = new Timer { Interval = Program.TIMER_INTERVAL_MILLISECONDS };
            timer.Tick += new EventHandler(Animate);
            DrawHuman = Human;
            DrawPuppy = Puppy;
            MovePuppy(Human, 0, true);
        }

        public Track(TrackForm tf, int type, List<Vector> p, bool extended, double human = 0, double puppy = 0.5)
        {
            trackType = type;
            this.tf = tf;
            this.p = p;
            DrawHuman = Human = human;
            DrawPuppy = Puppy = puppy;
            ComputeLengths();
            ComputeCritical(extended);
            stopwatch = new Stopwatch();
            timer = new Timer { Interval = Program.TIMER_INTERVAL_MILLISECONDS };
            timer.Tick += new EventHandler(Animate);
            MovePuppy(Human, 0, true);
        }

        public Vector GetP(int n) => p[n.Mod(p.Count)];

        public Vector GetN(int n) => nor[n.Mod(nor.Length)];

        public double GetH(int n) => hd[n.Mod(hd.Length)];

        private void ComputeLengths()
        {
            double minX, maxX, minY, maxY;
            minX = maxX = p[0].X;
            minY = maxY = p[0].Y;
            for (int i = 1; i < p.Count; i++)
            {
                minX = Math.Min(p[i].X, minX);
                maxX = Math.Max(p[i].X, maxX);
                minY = Math.Min(p[i].Y, minY);
                maxY = Math.Max(p[i].Y, maxY);
            }
            Vector d = new Vector((minX + maxX) / 2, (minY + maxY) / 2);
            for (int i = 0; i < p.Count; i++)
                p[i] = Vector.Subtract(p[i], d);
            double w = maxX - minX;
            double h = maxY - minY;
            double s;
            if (w > h)
            {
                s = 1 / w;
                size = new Vector(1, h * s);
            }
            else
            {
                s = 1 / h;
                size = new Vector(w * s, 1);
            }
            for (int i = 0; i < p.Count; i++)
                p[i] = Vector.Multiply(p[i], s);
            l = new double[p.Count];
            ls = new double[p.Count + 1];
            nor = new Vector[p.Count];
            ls[0] = 0;
            for (int i = 0; i < l.Length; i++)
            {
                Vector dif = Vector.Subtract(GetP(i + 1), GetP(i));
                l[i] = dif.Length;
                ls[i + 1] = ls[i] + l[i];
                nor[i] = new Vector(-dif.Y / l[i], dif.X / l[i]);
            }
            tl = ls[ls.Length - 1];
            ls[ls.Length - 1] = 1;
            if (tl == 0) return;
            for (int i = 0; i < p.Count; i++) ls[i] /= tl;
        }

        private int BinaryLocate(double w, double[] a)
        {
            w = w.Frac();
            int left = 0;
            int right = a.Length - 2;
            while (left <= right)
            {
                int mid = (left + right) / 2;
                if (w < a[mid]) right = mid - 1;
                else if (w >= a[mid + 1]) left = mid + 1;
                else return mid;
            }
            return 0;
        }

        private int BinaryLocateL(double w) => BinaryLocate(w, ls);

        private (int, bool, Vector, double) BinaryLocateA(double w)
        {
            int i = BinaryLocate(w, hd);
            int j = i / 2;
            bool b = i == 2 * j;
            double u = b ? ls[j] + (w - hd[i]) / edgeFactor : an[j] * (w - hd[i]) / (hd[i + 1] - hd[i]);
            if (b) return (j, b, Locate(u, j).Item1, u);
            return (j, b, GetP(j + 1), u);
        }

        private (Vector, int) Locate(double w)
        {
            w = w.Frac();
            int i = BinaryLocateL(w);
            return (Locate(w, i).Item1, i);
        }

        private (Vector, double) Locate(double w, int i, bool frac = true)
        {
            if (frac) w = w.Frac();
            double l1 = w - ls[i];
            double l2 = ls[i + 1] - ls[i];
            if (l2 != 0) l1 /= l2;
            return (GetP(i).Interpolate(GetP(i + 1), l1), l1);
        }

        private (Vector, double, int) ClosestOnSeg(int i, Vector q, bool angles = false)
        {
            i = i.Mod(p.Count);
            Vector a = GetP(i);
            Vector b = GetP(i + 1);
            Vector ab = Vector.Subtract(b, a);
            Vector aq = Vector.Subtract(q, a);
            double x = Vector.Multiply(ab, aq) / ab.LengthSquared;
            if (x <= 0) return (a, angles ? hd[2 * i] : ls[i], -1);
            if (x >= 1) return (b, angles ? hd[2 * i + 1] : ls[i + 1], 1);
            Vector v = a.Interpolate(b, x);
            double d = angles? hd[2 * i] * (1 - x) + hd[2 * i + 1] * x : ls[i] * (1 - x) + ls[i + 1] * x;
            return (v, d, 0);
        }

        public (double, double) ClosestOnTrack(Vector q, bool angle)
        {
            double d = -1;
            double w = 0;
            int ii = 0;
            for(int i = 0; i < p.Count; i++)
            {
                var (v, u, _) = ClosestOnSeg(i, q);
                double dist = q.DistanceSquared(v);
                if (d == -1 || dist < d)
                {
                    d = dist;
                    w = u;
                    ii = i;
                }
            }
            if (!angle) return (w.Frac(), 0);
            var (_, ww, dir) = ClosestOnSeg(ii, q, true);
            if (dir == 0) return (w.Frac(), ww);
            if (dir == -1)
            {
                ii--;
                if (ii < 0) ii += p.Count;
            }
            double t = hd[2 * ii + 1];
            double tota = an[ii];
            double r = (hd[2 * ii + 2] - t) / tota;
            Vector norm = GetN(ii);
            if (tota > 0) norm.Negate();
            double a = norm.Angle(Vector.Subtract(q, GetP(ii + 1)));

            return (w.Frac(), t + a * r);
        }

        public void PlotColumn(DirectBitmap b, int x, double ofs = 0.5)
        {
            int res = b.Height;
            Vector q = Locate((double)(x + ofs) / res).Item1;
            int y1 = res - (int)(ls[0] * res + ofs);
            for (int i = 0; i < p.Count; i++)
            {
                int y2 = y1;
                y1 = res - (int)(ls[i + 1] * res + ofs);
                var (_, w, j) = ClosestOnSeg(i, q);
                if (j == 1) b.VertLine(x, y1, y2 - 1, Program.FORWARD_CONFIGURATION_COLOR);
                else if (j == -1) b.VertLine(x, y1, y2 - 1, Program.BACKWARD_CONFIGURATION_COLOR);
                else
                {
                    int y3 = res - (int)(w * res + ofs);
                    b.VertLine(x, y1, y3 - 1, Program.BACKWARD_CONFIGURATION_COLOR);
                    b.VertLine(x, y3, y2 - 1, Program.FORWARD_CONFIGURATION_COLOR);
                }
            }
        }

        public void PlotColumnExtended(DirectBitmap b, int x, double ofs = 0.5)
        {
            int res = b.Height;
            double xx = (double)(x + ofs) / res;
            var (q, jj) = Locate(xx);
            for (int i = 0; i < p.Count; i++)
            {
                int y1 = res - (int)(hd[2 * i + 1] * res + ofs);
                int y2 = res - (int)(hd[2 * i] * res + ofs);
                var (_, w, j) = ClosestOnSeg(i, q, true);
                if (j == 1) b.VertLine(x, y1, y2 - 1, Program.FORWARD_CONFIGURATION_COLOR);
                else if (j == -1) b.VertLine(x, y1, y2 - 1, Program.BACKWARD_CONFIGURATION_COLOR);
                else
                {
                    int y3 = res - (int)(w * res + ofs);
                    b.VertLine(x, y1, y3 - 1, Program.BACKWARD_CONFIGURATION_COLOR);
                    b.VertLine(x, y3, y2 - 1, Program.FORWARD_CONFIGURATION_COLOR);
                }
            }
            for (int i = 0; i < p.Count; i++)
            {
                double h1 = hd[2 * i + 1];
                double h2 = hd[2 * i + 2];

                int y1 = res - (int)(h2 * res + ofs);
                int y2 = res - (int)(h1 * res + ofs);

                CritData c = critData[jj, i];
                if (c.stable1.HasValue && c.stable1.Value.X <= xx && xx <= c.stable2.Value.X)
                {
                    double d = c.stable2.Value.X - c.stable1.Value.X;
                    double w = d == 0 ? (c.stable1.Value.Y + c.stable2.Value.Y) / 2 : c.stable1.Value.Y + (xx - c.stable1.Value.X) * (c.stable2.Value.Y - c.stable1.Value.Y) / d;
                    int y3 = res - (int)(w * res + ofs);
                    b.VertLine(x, y1, y3 - 1, Program.BACKWARD_CONFIGURATION_COLOR);
                    b.VertLine(x, y3, y2 - 1, Program.FORWARD_CONFIGURATION_COLOR);
                }
                else if (c.unstable1.HasValue && c.unstable1.Value.X <= xx && xx <= c.unstable2.Value.X)
                {
                    double d = c.unstable2.Value.X - c.unstable1.Value.X;
                    double w = d == 0 ? (c.unstable1.Value.Y + c.unstable2.Value.Y) / 2 : c.unstable1.Value.Y + (xx - c.unstable1.Value.X) * (c.unstable2.Value.Y - c.unstable1.Value.Y) / d;
                    int y3 = res - (int)(w * res + ofs);
                    b.VertLine(x, y1, y3 - 1, Program.FORWARD_CONFIGURATION_COLOR);
                    b.VertLine(x, y3, y2 - 1, Program.BACKWARD_CONFIGURATION_COLOR);
                }
                else if (c.stable1.HasValue)
                {
                    double mid = (h1 + h2) / 2;
                    if (c.stable2.Value.X < xx) b.VertLine(x, y1, y2 - 1, c.stable2.Value.Y > mid ? Program.FORWARD_CONFIGURATION_COLOR : Program.BACKWARD_CONFIGURATION_COLOR);
                    else b.VertLine(x, y1, y2 - 1, c.stable1.Value.Y > mid ? Program.FORWARD_CONFIGURATION_COLOR : Program.BACKWARD_CONFIGURATION_COLOR);
                }
                else if (c.unstable1.HasValue)
                {
                    double mid = (h1 + h2) / 2;
                    if (c.unstable2.Value.X < xx) b.VertLine(x, y1, y2 - 1, c.unstable2.Value.Y < mid ? Program.FORWARD_CONFIGURATION_COLOR : Program.BACKWARD_CONFIGURATION_COLOR);
                    else b.VertLine(x, y1, y2 - 1, c.unstable1.Value.Y < mid ? Program.FORWARD_CONFIGURATION_COLOR : Program.BACKWARD_CONFIGURATION_COLOR);
                }
                else b.VertLine(x, y1, y2 - 1, c.forward ? Program.FORWARD_CONFIGURATION_COLOR : Program.BACKWARD_CONFIGURATION_COLOR);
            }
        }

        public int LeftRightTurn(Vector a, Vector b)
        {
            double c = Vector.CrossProduct(a, b);
            if (c < 0) return -1;
            if (c > 0) return 1;
            return 0;
        }

        public int LeftRightTurn(Vector p1, Vector p2, Vector p3) => LeftRightTurn(Vector.Subtract(p2, p1), Vector.Subtract(p3, p1));

        public void HumanDiagramAdd(double x1, double y1, double x2, double y2)
        {
            humanDiagram.Add(new Vector(x1, y1));
            humanDiagram.Add(new Vector(x2, y2));
            if (x1 < margin || x2 < margin)
            {
                humanDiagram.Add(new Vector(x1 + 1, y1));
                humanDiagram.Add(new Vector(x2 + 1, y2));
            }
            if (x1 > 1 - margin || x2 > 1 - margin)
            {
                humanDiagram.Add(new Vector(x1 - 1, y1));
                humanDiagram.Add(new Vector(x2 - 1, y2));
            }
        }

        public void HumanDiagramInterpAdd(Vector norm, Vector u1, Vector u2, double t, double r, bool invert, int n)
        {
            double x1, y1, x2 = 0, y2 = 0;
            for (int i = 0; i <= n; i++)
            {
                x1 = x2;
                y1 = y2;
                Vector u = u1.Interpolate(u2, (double)i / n);
                x2 = t + norm.Angle(u) * r;
                y2 = invert ? -u.Length : u.Length;
                if (i > 0) HumanDiagramAdd(x1, y1, x2, y2);
            }
        }

        public void CriticalSegSeg(int i, int j, bool extended)
        {
            double p1 = ls[i];
            double p2 = ls[i + 1];
            double q1 = ls[j];
            double q2 = ls[j + 1];
            double h1 = hd[2 * j];
            double h2 = hd[2 * j + 1];
            Vector a = Vector.Subtract(GetP(j + 1), GetP(j));
            Vector b1 = Vector.Subtract(GetP(i), GetP(j));
            Vector b2 = Vector.Subtract(GetP(i + 1), GetP(j));
            double s = 1 / (l[j] * tl);
            double w1 = q1 + Vector.Multiply(a, b1) * s;
            double w2 = q1 + Vector.Multiply(a, b2) * s;
            bool inw1 = (q1 <= w1 && w1 <= q2);
            bool inw2 = (q1 <= w2 && w2 <= q2);
            bool added = false;
            double px1 = 0, py1 = 0, px2 = 0, py2 = 0, hx1 = 0, hy1 = 0, hx2 = 0, hy2 = 0;
            if (inw1)
            {
                var (k1, l1) = Locate(w1, j, false);
                double x = h1 + l1 * (h2 - h1);
                px1 = p1;
                py1 = extended ? x : w1;
                hx1 = x;
                hy1 = Vector.Multiply(GetN(j), Vector.Subtract(GetP(i), k1));
                added = true;
            }
            if (inw2) {
                var (k2, l2) = Locate(w2, j, false);
                double x = h1 + l2 * (h2 - h1);
                px2 = p2;
                py2 = extended ? x : w2;
                hx2 = x;
                hy2 = Vector.Multiply(GetN(j), Vector.Subtract(GetP(i + 1), k2));
                added = true;
            }
            if (!inw1 || !inw2)
            {
                if (inw1 || inw2)
                {
                    double u, x;
                    int jj;
                    if ((w1 > w2) == inw1)
                    {
                        u = q1;
                        x = h1;
                        jj = j;
                    }
                    else
                    {
                        u = q2;
                        x = h2;
                        jj = j + 1;
                    }
                    double r = (p2 - p1) / (w2 - w1);
                    double v = inw1 ? p1 + r * (u - w1) : p2 + r * (u - w2);
                    Vector k = Locate(v, i, false).Item1;
                    if (inw1)
                    {
                        px2 = v;
                        py2 = extended ? x : u;
                        hx2 = x;
                        hy2 = Vector.Multiply(GetN(j), Vector.Subtract(k, GetP(jj)));
                    }
                    else
                    {
                        px1 = v;
                        py1 = extended ? x : u;
                        hx1 = x;
                        hy1 = Vector.Multiply(GetN(j), Vector.Subtract(k, GetP(jj)));
                    }
                }
                else if (w1 < q1 && w2 > q2 || w2 < q1 && w1 > q2)
                {
                    double r = (p2 - p1) / (w2 - w1);
                    double v1 = p1 + r * (q1 - w1);
                    double v2 = p1 + r * (q2 - w1);
                    Vector k1 = Locate(v1, i, false).Item1;
                    Vector k2 = Locate(v2, i, false).Item1;
                    px1 = v1;
                    py1 = extended ? h1 : q1;
                    hx1 = h1;
                    hy1 = Vector.Multiply(GetN(j), Vector.Subtract(k1, GetP(j)));
                    px2 = v2;
                    py2 = extended ? h2 : q2;
                    hx2 = h2;
                    hy2 = Vector.Multiply(GetN(j), Vector.Subtract(k2, GetP(j + 1)));
                    added = true;
                }
            }
            if (added)
            {
                stableCrit.Add(new Vector(px1, py1));
                stableCrit.Add(new Vector(px2, py2));
                if (px1 < margin || px2 < margin)
                {
                    stableCrit.Add(new Vector(px1 + 1, py1));
                    stableCrit.Add(new Vector(px2 + 1, py2));
                }
                if (px1 > 1 - margin || px2 > 1 - margin)
                {
                    stableCrit.Add(new Vector(px1 - 1, py1));
                    stableCrit.Add(new Vector(px2 - 1, py2));
                }
                if (py1 < margin || py2 < margin)
                {
                    stableCrit.Add(new Vector(px1, py1 + 1));
                    stableCrit.Add(new Vector(px2, py2 + 1));
                }
                if (py1 > 1 - margin || py2 > 1 - margin)
                {
                    stableCrit.Add(new Vector(px1, py1 - 1));
                    stableCrit.Add(new Vector(px2, py2 - 1));
                }
                humanDiagram.Add(new Vector(hx1, hy1));
                humanDiagram.Add(new Vector(hx2, hy2));
                if (hx1 < margin || hx2 < margin)
                {
                    humanDiagram.Add(new Vector(hx1 + 1, hy1));
                    humanDiagram.Add(new Vector(hx2 + 1, hy2));
                }
                if (hx1 > 1 - margin || hx2 > 1 - margin)
                {
                    humanDiagram.Add(new Vector(hx1 - 1, hy1));
                    humanDiagram.Add(new Vector(hx2 - 1, hy2));
                }
            }
        }

        public (Vector?, double, Vector?, double, Vector?, double) ClipSeg(Vector? p1, double w1, Vector? p2, double w2, Vector q) // splits segment p1p2 through line orthogonal to unit vector q
        {
            if (!p1.HasValue || !p2.HasValue) return (null, 0, null, 0, null, 0);
            double u1 = Vector.Multiply(q, p1.Value);
            double u2 = Vector.Multiply(q, p2.Value);
            if (u1 >= 0 && u2 >= 0) return (null, 0, p1, w1, p2, w2); 
            if (u1 <= 0 && u2 <= 0) return (p1, w1, p2, w2, null, 0);
            double d = u1 / (u1 - u2);
            double v = w1 + d * (w2 - w1);
            Vector p3 = p1.Value.Interpolate(p2.Value, d);
            if (u1 < 0) return (p1, w1, p3, v, p2, w2);
            return (p2, w2, p3, v, p1, w1);
        }

        public void CriticalSegVert(int i, int j, bool extended)
        {
            bool added = !extended;
            int k = j == 0 ? p.Count : j;
            Vector q = Vector.Divide(Vector.Subtract(GetP(j), GetP(j - 1)), l[k - 1]);
            Vector? p1 = Vector.Subtract(GetP(i), GetP(j));
            Vector? p2 = Vector.Subtract(GetP(i + 1), GetP(j));
            double w1 = ls[i];
            double w2 = ls[i + 1];
            var (v1, a1, v2, a2, v3, a3) = ClipSeg(p1, w1, p2, w2, q);
            q = Vector.Divide(Vector.Subtract(GetP(j), GetP(j + 1)), l[j]);
            var (u1, b1, u2, b2, u3, b3) = ClipSeg(v2, a2, v3, a3, q);
            if (u3.HasValue) {
                if (!extended)
                {
                    stableCrit.Add(new Vector(b2, ls[j]));
                    stableCrit.Add(new Vector(b3, ls[j]));
                    if (j == 0)
                    {
                        stableCrit.Add(new Vector(b2, 1));
                        stableCrit.Add(new Vector(b3, 1));
                    }
                }
                if (i != j && i != k - 1)
                {
                    double t = hd[2 * k - 1];
                    double tota = an[k - 1];
                    double r = (hd[2 * k] - t) / tota;
                    Vector norm = GetN(k - 1);
                    if (tota > 0) norm.Negate();
                    double an1 = norm.Angle(u2.Value);
                    double an2 = norm.Angle(u3.Value);
                    double x1 = t + an1 * r;
                    double x2 = t + an2 * r;
                    added = true;
                    Vector z1, z2;
                    if (b2 <= b3)
                    {
                        z1 = new Vector(b2, x1);
                        z2 = new Vector(b3, x2);
                    }
                    else
                    {
                        z2 = new Vector(b2, x1);
                        z1 = new Vector(b3, x2);
                    }
                    critData[i, k - 1].stable1 = z1;
                    critData[i, k - 1].stable2 = z2;
                    if (extended)
                    {
                        stableCrit.Add(z1);
                        stableCrit.Add(z2);
                        if (x1 < margin || x2 < margin)
                        {
                            stableCrit.Add(new Vector(b2, x1 + 1));
                            stableCrit.Add(new Vector(b3, x2 + 1));
                        }
                        if (x1 > 1 - margin || x2 > 1 - margin)
                        {
                            stableCrit.Add(new Vector(b2, x1 - 1));
                            stableCrit.Add(new Vector(b3, x2 - 1));
                        }
                        if (b2 < margin || b3 < margin)
                        {
                            stableCrit.Add(new Vector(b2 + 1, x1));
                            stableCrit.Add(new Vector(b3 + 1, x2));
                        }
                        if (b2 > 1 - margin || b3 > 1 - margin)
                        {
                            stableCrit.Add(new Vector(b2 - 1, x1));
                            stableCrit.Add(new Vector(b3 - 1, x2));
                        }
                    }
                    int n = (int)(Math.Abs((norm.Angle(u3.Value) - norm.Angle(u2.Value)) * r) / Program.HUMAN_DIAGRAM_ANGLE_GRAIN) + 1;
                    if (n <= 1) HumanDiagramAdd(x1, tota > 0 ? -u2.Value.Length : u2.Value.Length, x2, tota > 0 ? -u3.Value.Length : u3.Value.Length);
                    else HumanDiagramInterpAdd(norm, u2.Value, u3.Value, t, r, tota > 0, n);
                }
            }
            (u1, b1, u2, b2, u3, b3) = ClipSeg(v1, a1, v2, a2, q);
            if (u1.HasValue)
            {
                if (!extended)
                {
                    unstableCrit.Add(new Vector(b1, ls[j]));
                    unstableCrit.Add(new Vector(b2, ls[j]));
                    if (j == 0)
                    {
                        unstableCrit.Add(new Vector(b1, 1));
                        unstableCrit.Add(new Vector(b2, 1));
                    }
                }
                if (i != j && i != k - 1)
                {
                    double t = hd[2 * k - 1];
                    double tota = an[k - 1];
                    double r = (hd[2 * k] - t) / tota;
                    Vector norm = GetN(k - 1);
                    if (tota < 0) norm.Negate();
                    double an1 = norm.Angle(u1.Value);
                    double an2 = norm.Angle(u2.Value);
                    double x1 = t + an1 * r;
                    double x2 = t + an2 * r;
                    added = true;
                    Vector z1, z2;
                    if (b1 <= b2)
                    {
                        z1 = new Vector(b1, x1);
                        z2 = new Vector(b2, x2);
                    }
                    else
                    {
                        z2 = new Vector(b1, x1);
                        z1 = new Vector(b2, x2);
                    }
                    critData[i, k - 1].unstable1 = z1;
                    critData[i, k - 1].unstable2 = z2;
                    if (extended)
                    {
                        unstableCrit.Add(z1);
                        unstableCrit.Add(z2);
                        if (x1 < margin || x2 < margin)
                        {
                            unstableCrit.Add(new Vector(b1, x1 + 1));
                            unstableCrit.Add(new Vector(b2, x2 + 1));
                        }
                        if (x1 > 1 - margin || x2 > 1 - margin)
                        {
                            unstableCrit.Add(new Vector(b1, x1 - 1));
                            unstableCrit.Add(new Vector(b2, x2 - 1));
                        }
                        if (b1 < margin || b2 < margin)
                        {
                            unstableCrit.Add(new Vector(b1 + 1, x1));
                            unstableCrit.Add(new Vector(b2 + 1, x2));
                        }
                        if (b1 > 1 - margin || b2 > 1 - margin)
                        {
                            unstableCrit.Add(new Vector(b1 - 1, x1));
                            unstableCrit.Add(new Vector(b2 - 1, x2));
                        }
                    }

                    int n = (int)(Math.Abs((norm.Angle(u2.Value) - norm.Angle(u1.Value)) * r) / Program.HUMAN_DIAGRAM_ANGLE_GRAIN) + 1;
                    if (n <= 1) HumanDiagramAdd(x1, tota < 0 ? -u1.Value.Length : u1.Value.Length, x2, tota < 0 ? -u2.Value.Length : u2.Value.Length);
                    else HumanDiagramInterpAdd(norm, u1.Value, u2.Value, t, r, tota < 0, n);
                }
            }
            if ((Math.Abs(an[k - 1]) >= Math.PI * 0.5) && ((i == j) || (i == k - 1)))
            {
                double t = hd[2 * k - 1];
                double tota = an[k - 1];
                double r = (hd[2 * k] - t) / tota;
                Vector norm = GetN(k - 1);
                if (tota < 0) norm.Negate();
                Vector v = (i == j) ? p2.Value : p1.Value;
                double an1 = norm.Angle(v);
                double x = t + an1 * r;
                double y = tota > 0 ? v.Length : -v.Length;
                added = true;
                Vector z1, z2;
                if (w1 <= w2)
                {
                    z1 = new Vector(w1, x);
                    z2 = new Vector(w2, x);
                }
                else
                {
                    z2 = new Vector(w1, x);
                    z1 = new Vector(w2, x);
                }
                critData[i, k - 1].unstable1 = z1;
                critData[i, k - 1].unstable2 = z2;
                if (extended)
                {
                    unstableCrit.Add(z1);
                    unstableCrit.Add(z2);
                    if (w1 < margin)
                    {
                        unstableCrit.Add(new Vector(w1 + 1, x));
                        unstableCrit.Add(new Vector(w2 + 1, x));
                    }
                    if (w2 > 1 - margin)
                    {
                        unstableCrit.Add(new Vector(w1 - 1, x));
                        unstableCrit.Add(new Vector(w2 - 1, x));
                    }
                    if (x < margin)
                    {
                        unstableCrit.Add(new Vector(w1, x + 1));
                        unstableCrit.Add(new Vector(w2, x + 1));
                    }
                    if (x > 1 - margin)
                    {
                        unstableCrit.Add(new Vector(w1, x - 1));
                        unstableCrit.Add(new Vector(w2, x - 1));
                    }
                }
                HumanDiagramAdd(x, 0, x, y);
            }
            if (!added) critData[i, k - 1].forward = v3.HasValue;
        }

        private void ComputeAngles()
        {
            an = new double[p.Count];
            ans = new double[p.Count + 1];
            ans[0] = 0;
            for (int i = 0; i < p.Count; i++)
            {
                an[i] = GetN(i).Angle(GetN(i + 1));
                ans[i + 1] = ans[i] + Math.Abs(an[i]);
            }
            ta = ans[ans.Length - 1];
            ans[ans.Length - 1] = 1;
            if (ta != 0) for (int i = 0; i < p.Count; i++) ans[i] /= ta;
            edgeFactor = Program.HUMAN_DIAGRAM_EDGE_FACTOR * tl;
            angleFactor = Program.HUMAN_DIAGRAM_ANGLE_FACTOR * ta;
            double totFactor = edgeFactor + angleFactor;
            edgeFactor /= totFactor;
            angleFactor /= totFactor;
            hd = new double[2 * p.Count + 1];
            hd[0] = 0;
            for (int i = 0; i < p.Count; i += 1)
            {
                hd[2 * i + 1] = hd[2 * i] + (ls[i + 1] - ls[i]) * edgeFactor;
                hd[2 * i + 2] = hd[2 * i + 1] + (ans[i + 1] - ans[i]) * angleFactor;
            }
            hd[hd.Length - 1] = 1;
        }

        public void ComputeCritical(bool extended)
        {
            ComputeAngles();
            if (stableCrit != null) stableCrit.Clear();
            stableCrit = new List<Vector>();
            if (unstableCrit != null) unstableCrit.Clear();
            unstableCrit = new List<Vector>();
            if (puppyDiagonal != null) puppyDiagonal.Clear();
            puppyDiagonal = new List<Vector>();
            if (humanDiagram != null) humanDiagram.Clear();
            humanDiagram = new List<Vector>();
            if (humanDiagramBase != null) humanDiagramBase.Clear();
            humanDiagramBase = new List<Vector>();
            humanDiagramBase.Add(new Vector(0, 0));
            humanDiagramBase.Add(new Vector(1, 0));
            critData = new CritData[p.Count, p.Count];
            for (int i = 0; i < p.Count; i++)
            {
                for (int j = 0; j < p.Count; j++)
                {
                    if (i != j) CriticalSegSeg(i, j, extended);
                    CriticalSegVert(i, j, extended);
                }
                if (extended)
                {
                    puppyDiagonal.Add(new Vector(ls[i], hd[2 * i]));
                    puppyDiagonal.Add(new Vector(ls[i + 1], hd[2 * i + 1]));
                    puppyDiagonal.Add(new Vector(ls[i + 1], hd[2 * i + 1]));
                    puppyDiagonal.Add(new Vector(ls[i + 1], hd[2 * i + 2]));
                }
            }
            if (!extended)
            {
                puppyDiagonal.Add(new Vector(0, 0));
                puppyDiagonal.Add(new Vector(1, 1));
            }
            diagMinY = 0;
            diagMaxY = 0;
            for (int i = 0; i < humanDiagram.Count; i++)
            {
                diagMinY = Math.Min(humanDiagram[i].Y, diagMinY);
                diagMaxY = Math.Max(humanDiagram[i].Y, diagMaxY);
            }
            diagSign = 1;
            if (diagMaxY + diagMinY < 0)
            {
                diagSign = -1;
                double t = diagMinY;
                diagMinY = -diagMaxY;
                diagMaxY = -t;
            }
            if (diagMaxY == diagMinY) diagMaxY = 1;
            for (int i = 0; i < humanDiagram.Count; i++)
                humanDiagram[i] = new Vector(humanDiagram[i].X, HumanDiagramY(humanDiagram[i].Y));
            for (int i = 0; i < humanDiagramBase.Count; i++)
                humanDiagramBase[i] = new Vector(humanDiagramBase[i].X, HumanDiagramY(humanDiagramBase[i].Y));
        }

        public double HumanDiagramY(double y) => (diagSign * y - diagMinY) / (diagMaxY - diagMinY) - 0.5;

        public (Vector, Vector) HumanDiagramCoordinates(double puppy, double human)
        {
            double x, y;
            var (pv, j) = Locate(puppy);
            var dv = Vector.Subtract(Locate(human).Item1, pv);
            int k = j == 0 ? p.Count : j;
            x = ExtendedConversion(puppy, human);
            if (puppy == ls[j]) y = an[k - 1] < 0 ? dv.Length : -dv.Length;
            else y = Vector.Multiply(nor[j], dv);
            return (new Vector(x, HumanDiagramY(0)), new Vector(x, HumanDiagramY(y)));
        }

        public double ExtendedConversion(double puppy, double human)
        {
            puppy = puppy.Frac();
            human = human.Frac();

            int i = BinaryLocateL(puppy);
            if (puppy != ls[i]) return hd[2 * i] + (hd[2 * i + 1] - hd[2 * i]) * (puppy - ls[i]) / (ls[i + 1] - ls[i]);

            int k = i == 0 ? p.Count : i;
            CritData c = critData[BinaryLocateL(human), k - 1];

            double h1 = hd[2 * k - 1];
            double h2 = hd[2 * k];
            double mid = (h1 + h2) / 2;

            if (c.stable1.HasValue && c.stable1.Value.X <= human && human <= c.stable2.Value.X)
            {
                double d = c.stable2.Value.X - c.stable1.Value.X;
                return d == 0 ? (c.stable1.Value.Y + c.stable2.Value.Y) / 2 : c.stable1.Value.Y + (human - c.stable1.Value.X) * (c.stable2.Value.Y - c.stable1.Value.Y) / d;
            }
            else if (c.unstable1.HasValue && c.unstable1.Value.X <= human && human <= c.unstable2.Value.X)
            {
                double d = c.unstable2.Value.X - c.unstable1.Value.X;
                double w = d == 0 ? (c.unstable1.Value.Y + c.unstable2.Value.Y) / 2 : c.unstable1.Value.Y + (human - c.unstable1.Value.X) * (c.unstable2.Value.Y - c.unstable1.Value.Y) / d;
                return mid < w ? h1 : h2;
            }
            else if (c.stable1.HasValue)
            {
                if (c.stable2.Value.X < human) return c.stable2.Value.Y > mid ? h2 : h1;
                else return c.stable1.Value.Y > mid ? h2 : h1;
            }
            else if (c.unstable1.HasValue)
            {
                if (c.unstable2.Value.X < human) return c.unstable2.Value.Y < mid ? h2 : h1;
                else return c.unstable1.Value.Y < mid ? h2 : h1;
            }
            else return c.forward ? h2 : h1;
        }

        public (double, int) ExtendedConversion2(double puppy, double human)
        {
            puppy = puppy.Frac();
            human = human.Frac();
            var (i, seg, _, dd) = BinaryLocateA(puppy);
            if (seg) return (dd, 0);

            double f;

            int k = i == 0 ? p.Count : i;
            CritData c = critData[BinaryLocateL(human), i];

            double h1 = hd[2 * i + 1];
            double h2 = hd[2 * i + 2];
            double mid = (h1 + h2) / 2;

            if (c.stable1.HasValue && c.stable1.Value.X <= human && human <= c.stable2.Value.X) return (ls[i + 1], 0);
            else if (c.unstable1.HasValue && c.unstable1.Value.X <= human && human <= c.unstable2.Value.X)
            {
                double d = c.unstable2.Value.X - c.unstable1.Value.X;
                f = d == 0 ? (c.unstable1.Value.Y + c.unstable2.Value.Y) / 2 : c.unstable1.Value.Y + (human - c.unstable1.Value.X) * (c.unstable2.Value.Y - c.unstable1.Value.Y) / d;
            }
            else if (c.stable1.HasValue)
            {
                if (c.stable2.Value.X < human) f = c.stable2.Value.Y < mid ? 2 : -2;
                else f = c.stable1.Value.Y < mid ? 2 : -2;
            }
            else if (c.unstable1.HasValue)
            {
                if (c.unstable2.Value.X < human) f = c.unstable2.Value.Y > mid ? 2 : -2;
                else f = c.unstable1.Value.Y > mid ? 2 : -2;
            }
            else f = c.forward ? 2 : -2;

            return (ls[i + 1], puppy < f ? -1 : (puppy > f ? 1 : 0));
        }

        public (Vector, double, bool) Attract(double a, double w, int dir)
        {
            var (p, s) = Locate(a);
            int i = BinaryLocateL(w);
            int j, pj = 0;
            Vector q;
            double d = w;
            while (true)
            {
                if (s == i || (s == (i - 1).Mod(this.p.Count) && ls[i] == d))
                {
                    if (ls[i] != d || dir == 0 || (s == i && dir == 1) || (s == (i - 1).Mod(this.p.Count) && dir == -1)) return (p, a, true);
                    if (dir == -1)
                    {
                        i = (i - 1).Mod(this.p.Count);
                        dir = 1;
                    }
                }
                (q, d, j) = ClosestOnSeg(i, p);
                if (pj == 0 && ls[i] == w && dir != 1)
                {
                    var (q1, d1, j1) = ClosestOnSeg(i - 1, p);
                    if (dir == -1 || p.DistanceSquared(q1) < p.DistanceSquared(q)) {
                        i = (i - 1).Mod(this.p.Count);
                        q = q1;
                        d = d1;
                        j = j1;
                    }
                }
                if (j == 0 || j == -pj) return (q, d, false);
                i = (i + j).Mod(this.p.Count);
                pj = j;
                dir = 0;
            }
        }

        private void CheckStartTimer()
        {
            if (timer.Enabled) return;
            if (DrawHuman == Human && DrawPuppy == Puppy) return;
            stopwatch.Start();
            timer.Start();
        }

        private void CheckStopTimer()
        {
            if (DrawHuman != Human || DrawPuppy != Puppy) return;
            timer.Stop();
            stopwatch.Reset();
        }

        public void RelocateHuman(double w, bool instant = false)
        {
            Human = w;
            if (instant)
            {
                DrawHuman = Human;
                captured = false;
                reached = false;
                stopwatch.Start();
                timer.Start();
            }
            else
            {
                CheckStartTimer();
                Refresh();
            }
        }

        public void RelocatePuppy(double w, int dir, bool attract = false, bool startTimer = false)
        {
            captured = false;
            reached = false;
            if (startTimer)
            {
                stopwatch.Start();
                timer.Start();
            }
            this.attract = attract;
            Puppy = w;
            DrawPuppy = Puppy;
            if (attract) MovePuppy(DrawHuman, dir);
            else Refresh();
        }

        public void MovePuppy(double w, int dir, bool instant = false)
        {
            w = w.Frac();
            if (captured) Puppy = Human;
            else (_, Puppy, captured) = Attract(w, Puppy, dir);
            if (instant) DrawPuppy = Puppy;
            else CheckStartTimer();
            Refresh();
        }

        public void SpeedUp() => animationSpeed *= Program.SPEED_SCALE_FACTOR;

        public void SlowDown() => animationSpeed /= Program.SPEED_SCALE_FACTOR;

        public void Refresh()
        {
            if (!timer.Enabled && tf.gd != null) tf.gd.Refresh(true, true, true);
        }

        private (double, double) Approach(double a, double b, double delta, bool stopAtVertex = false)
        {
            a = a.Frac();
            b = b.Frac();
            double d = b - a;
            if (d > 0.5) d -= 1;
            if (d < -0.5) d += 1;
            double s = d >= 0 ? 1 : -1;
            double dest = (s * d <= delta ? b : a + s * delta).Frac();
            if (stopAtVertex)
            {
                double dd = (dest - a).Frac();
                int i = BinaryLocateL(a);
                if (s > 0)
                {
                    if (dd > ls[i + 1] - a) return (ls[i + 1].Frac(), delta - ls[i + 1] + a);
                }
                else
                {
                    dd = dd - 1;
                    if (a == ls[i])
                    {
                        i--;
                        if (i == -1)
                        {
                            i = p.Count - 1;
                            a = 1;
                        }
                    }
                    if (dd < ls[i] - a) return (ls[i].Frac(), delta - a + ls[i]);
                }
            }
            return (dest, 0);
        }

        public void Animate(object sender, EventArgs e)
        {
            double t = animationSpeed * stopwatch.ElapsedMilliseconds;
            stopwatch.Restart();
            double d = Program.HUMAN_SPEED * t / tl;
            while (d > 0)
            {
                (DrawHuman, d) = Approach(DrawHuman, Human, d, true);
                if (attract) MovePuppy(DrawHuman, 0);
            }

            //DrawPuppy = reached ? DrawHuman : Approach(DrawPuppy, Puppy, Program.PUPPY_SPEED * t / tl).Item1;
            DrawPuppy = reached ? Approach(DrawPuppy, DrawHuman, Program.PUPPY_SPEED * t / tl).Item1 : Approach(DrawPuppy, Puppy, Program.PUPPY_SPEED * t / tl).Item1;

            if (captured)
            {
                double dh = Human - DrawHuman;
                if (dh > 0.5) dh -= 1;
                if (dh < -0.5) dh += 1;
                double dp = Puppy - DrawPuppy;
                if (dp > 0.5) dp -= 1;
                if (dp < -0.5) dp += 1;
                if ((dh <= dp && dp <= 0) || (0 <= dp && dp <= dh)) reached = true;
            }
            CheckStopTimer();
            tf.gd.Refresh(true, true, true);
        }

        public void Draw(Graphics g)
        {
            if (tf.gd == null) return;
            tf.gd.DrawPolygon(g, p);
            Vector q1 = Locate(DrawPuppy).Item1;
            Vector q2 = Locate(DrawHuman).Item1;
            tf.gd.DrawArrowFrame(g, q1, q2, Program.TANGENT_COLOR);
            if (tf.gd.drawNormal)
            {
                var (i, seg, v, d) = BinaryLocateA((tf.gd.normalX - tf.hf.xOffset).Frac());
                if (seg) tf.gd.DrawTNormal(g, v, GetN(i), Program.NORMAL_COLOR);
                else tf.gd.DrawTNormal(g, v, GetN(i).Rotate(d), Program.NORMAL_COLOR);
            }
            tf.gd.DrawPuppy(g, q1);
            tf.gd.DrawHuman(g, q2);
        }

        public List<Vector> Chamfer()
        {
            double epsilon = 1000;
            for(int i= 0; i < p.Count; i++)
            {
                for (int j = 2; j < p.Count; j++)
                {
                    Vector v1 = GetP(i + j);
                    Vector v2 = ClosestOnSeg(i, v1).Item1;
                    epsilon = Math.Min(epsilon, v1.DistanceSquared(v2));
                }
            }
            epsilon = Math.Sqrt(epsilon) * Program.CHAMFERING_FACTOR;

            List<Vector> c = new List<Vector>(2 * p.Count);
            for(int i = 0; i < p.Count; i++)
            {
                Vector p1 = GetP(i);
                Vector p2 = GetP(i + 1);
                double lambda = epsilon / l[i];
                c.Add(p1.Interpolate(p2, lambda));
                c.Add(p2.Interpolate(p1, lambda));
            }
            return c;
        }

        public void Dispose() => timer.Dispose();
    }
}
