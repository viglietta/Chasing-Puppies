using System;
using System.Drawing;
using System.Windows;

namespace Puppy
{
    static class ExtensionMethods
    {
        public static double Frac(this double d)
        {
            d -= Math.Floor(d);
            if (d < 0) d += 1;
            if (d >= 1) d -= 1;
            return d;
        }

        public static int Mod(this int a, int b)
        {
            a %= b;
            return a < 0 ? a + b : a;
        }

        public static double Angle(this Vector operand1, Vector operand2)
        {
            double d = Vector.Multiply(operand1, operand2);
            double c = operand1.X * operand2.Y - operand1.Y * operand2.X;
            return Math.Atan2(c, d);
        }

        public static Vector Rotate(this Vector operand, double angle)
        {
            double cos = Math.Cos(angle);
            double sin = Math.Sin(angle);
            return new Vector(operand.X * cos - operand.Y * sin, operand.X * sin + operand.Y * cos);
        }

        public static double Distance(this Vector operand1, Vector operand2)
        {
            return Vector.Subtract(operand1, operand2).Length;
        }

        public static double DistanceSquared(this Vector operand1, Vector operand2)
        {
            return Vector.Subtract(operand1, operand2).LengthSquared;
        }

        public static Vector Interpolate(this Vector operand1, Vector operand2, double lambda)
        {
            return Vector.Add(operand1, Vector.Multiply(lambda, Vector.Subtract(operand2, operand1)));
        }

        public static Vector ToVector(this PointF operand)
        {
            return new Vector(operand.X, operand.Y);
        }

        public static PointF ToPointF(this Vector operand)
        {
            return new PointF((float)operand.X, (float)operand.Y);
        }

        public static PointF Scale(this PointF operand, float scale)
        {
            return new PointF(operand.X * scale, operand.Y * scale);
        }

        /*        public static Point Sub(this Point operand1, Point operand2)
                {
                    return new Point(operand1.X - operand2.X, operand1.Y - operand2.Y);
                }*/
    }
}
