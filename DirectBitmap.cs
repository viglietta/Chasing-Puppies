using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Puppy
{
    public class DirectBitmap : IDisposable
    {
        public Bitmap Bitmap { get; private set; }
        public Int32[] Bits { get; private set; }
        public bool Disposed { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        protected GCHandle BitsHandle { get; private set; }

        public DirectBitmap(int width, int height)
        {
            Width = width;
            Height = height;
            Bits = new Int32[width * height];
            BitsHandle = GCHandle.Alloc(Bits, GCHandleType.Pinned);
            Bitmap = new Bitmap(width, height, width * 4, PixelFormat.Format32bppPArgb, BitsHandle.AddrOfPinnedObject());
        }

        public void Clear(Color color) {
            int t = Width * Height;
            Int32 c = color.ToArgb();
            for (int i = 0; i < t; i++) Bits[i] = c;
        }

        public void SetPixel(int x, int y, Color color)
        {
            int index = x + (y * Width);
            Bits[index] = color.ToArgb();
        }

        public Color GetPixel(int x, int y)
        {
            int index = x + (y * Width);
            return Color.FromArgb(Bits[index]);
        }

        public void VertLine(int x, int y1, int y2, Color color)
        {
            int index = x + (y1 * Width);
            Int32 c = color.ToArgb();
            for (int y = y1; y <= y2; y++)
            {
                Bits[index] = c;
                index += Width;
            }
        }

        public void VertLineDiagram(int x, int y1, int y2, int baseRes, Color color)
        {
            int index = x + (y1 * Width);
            int ofs = baseRes * Width;
            Int32 c = color.ToArgb();
            for (int y = y1; y <= y2; y++)
            {
                if (y > baseRes - x) Bits[index] = c;
                else if (y < x) Bits[index + ofs] = c;
                else Bits[index + baseRes] = c;
                index += Width;
            }
        }

        public void DiagramRearrange(int baseRes)
        {
            int index = 0;
            int ofs = baseRes * Width;
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    if (y >= baseRes - x)
                    {
                        if (y >= x + baseRes) Bits[index] = 0;
                        else if (y > 2 * baseRes - x) Bits[index] = 0;
                        else if (y < x - baseRes) Bits[index] = 0;
                    }
                    else if (y < x)
                    {
                        Bits[index + ofs] = Bits[index];
                        Bits[index] = 0;
                    }
                    else
                    {
                        Bits[index + baseRes] = Bits[index];
                        Bits[index] = 0;
                    }
                    index++;
                }
            }
        }

        public void Dispose()
        {
            if (Disposed) return;
            Disposed = true;
            Bitmap.Dispose();
            BitsHandle.Free();
        }
    }
}
