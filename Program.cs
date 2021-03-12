using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Forms;

namespace Puppy
{
    static class Program
    {
        private const string CONFIG_FILE = "config.txt";
        private const string TRACK_FILE = "CustomTrack.txt";

        public static int TRACK_WINDOW_SIZE = 600;
        public static double PUPPY_WINDOW_SIZE_FACTOR = 1;
        public static double HUMAN_WINDOW_W_SIZE_FACTOR = 2;
        public static double HUMAN_WINDOW_H_SIZE_FACTOR = 0.5;
        public static int PUPPY_WINDOW_SIZE_1 = (int)(TRACK_WINDOW_SIZE * PUPPY_WINDOW_SIZE_FACTOR);
        public static int PUPPY_WINDOW_SIZE_2 = PUPPY_WINDOW_SIZE_1 * 3 / 2;
        public static int PUPPY_WINDOW_SIZE_3 = PUPPY_WINDOW_SIZE_1 * 2;
        public static int HUMAN_WINDOW_WIDTH = (int)(TRACK_WINDOW_SIZE * HUMAN_WINDOW_W_SIZE_FACTOR);
        public static int HUMAN_WINDOW_HEIGHT = (int)(TRACK_WINDOW_SIZE * HUMAN_WINDOW_H_SIZE_FACTOR);

        public static double INITIAL_TRACK_SCALE = 0.85;
        public static double TRACK_RESIZE_FACTOR = 1.05;
        public static double PUPPY_DIAGRAM_RESIZE_FACTOR = 1.05;
        public static double HUMAN_DIAGRAM_H_SCALE = 0.9;
        public static double HUMAN_DIAGRAM_RESIZE_FACTOR = 1.05;
        public static double INITIAL_FEATURE_SCALE = 0.002;
        public static double FEATURE_RESIZE_FACTOR = 1.05;

        public static double PUPPY_DIAGRAM_BACKGROUND_RESOLUTION = 0.5;
        public static double HUMAN_DIAGRAM_ANGLE_GRAIN = 0.001;

        public static double TRACK_PORTRAIT_SIZE = 60;
        public static double DIAGRAM_PORTRAIT_SIZE = 60;
        public static double PUPPY_PORTRAIT_OFFSET = 0.04;
        public static int PUPPY_BITMAP_BASE_RESOLUTION = (int)(PUPPY_WINDOW_SIZE_1 * PUPPY_DIAGRAM_BACKGROUND_RESOLUTION);

        public static double HUMAN_SPEED = 0.001;
        public static double PUPPY_SPEED = 0.002;
        public static double SPEED_SCALE_FACTOR = 1.05;
        public static int TIMER_INTERVAL_MILLISECONDS = 5;

        public static double HUMAN_DIAGRAM_EDGE_FACTOR = 0.75;
        public static double HUMAN_DIAGRAM_ANGLE_FACTOR = 1 - HUMAN_DIAGRAM_EDGE_FACTOR;

        public static double TRACK_LINE_THICKNESS = 4;
        public static double ARROW_LINE_THICKNESS = 5;
        public static double ARROW_W_SIZE = 25;
        public static double ARROW_H_SIZE = 40;
        public static double TANGENT_LENGTH = 0.5;
        public static double NORMAL_LINE_THICKNESS = 5;
        public static double NORMAL_LENGTH = 2;
        public static double TRACK_CIRCLE_SIZE = 10;
        public static double DIAGRAM_LINE_THICKNESS = 3;
        public static double DIAGRAM_NORMAL_THICKNESS = 4;
        public static double DIAGRAM_CIRCLE_SIZE = 10;

        public static double CHAMFERING_FACTOR = 0.49;

        public static Color FORWARD_CONFIGURATION_COLOR = Color.White;
        public static Color BACKWARD_CONFIGURATION_COLOR = Color.LightGray;
        public static Color TRACK_COLOR = Color.Black;
        public static Color TANGENT_COLOR = Color.Green;
        public static Color NORMAL_COLOR = Color.Red;
        public static Color NORMAL_CIRCLE_COLOR = Color.Blue;
        public static Color PUPPY_CIRCLE_COLOR = Color.Blue;
        public static Color STABLE_COLOR = Color.Green;
        public static Color UNSTABLE_COLOR = Color.Red;
        public static Color HUMAN_BACKGROUND_COLOR = Color.White;
        public static Color HUMAN_LINE_COLOR = Color.Black;
        public static Color HUMAN_BASELINE_COLOR = Color.Blue;

        private static void ReadInt(StreamReader reader, ref int v)
        {
            int t;
            try
            {
                t = Int32.Parse(reader.ReadLine().Split('=')[1], NumberStyles.Integer);
            }
            catch
            {
                return;
            }
            v = t;
        }

        private static void ReadDouble(StreamReader reader, ref double v)
        {
            double t;
            try
            {
                t =Double.Parse(reader.ReadLine().Split('=')[1], NumberStyles.Float);
            }
            catch
            {
                return;
            }
            v = t;
        }

        private static void WriteConfigFile()
        {
            try
            {
                using (StreamWriter writer = File.CreateText(CONFIG_FILE))
                {
                    writer.WriteLine("Track window size = " + TRACK_WINDOW_SIZE.ToString());
                    writer.WriteLine("Puppy window size factor = " + PUPPY_WINDOW_SIZE_FACTOR.ToString());
                    writer.WriteLine("Human window size factor = " + HUMAN_WINDOW_W_SIZE_FACTOR.ToString());
                    writer.WriteLine("Human window x-size factor = " + HUMAN_WINDOW_H_SIZE_FACTOR.ToString());
                    writer.WriteLine("Initial track scale = " + INITIAL_TRACK_SCALE.ToString());
                    writer.WriteLine("Track resize factor = " + TRACK_RESIZE_FACTOR.ToString());
                    writer.WriteLine("Puppy diagram resize factor = " + PUPPY_DIAGRAM_RESIZE_FACTOR.ToString());
                    writer.WriteLine("Human diagram x-scale = " + HUMAN_DIAGRAM_H_SCALE.ToString());
                    writer.WriteLine("Human diagram resize factor = " + HUMAN_DIAGRAM_RESIZE_FACTOR.ToString());
                    writer.WriteLine("Initial feature scale = " + INITIAL_FEATURE_SCALE.ToString());
                    writer.WriteLine("Feature resize factor = " + FEATURE_RESIZE_FACTOR.ToString());
                    writer.WriteLine("Puppy diagram background resolution = " + PUPPY_DIAGRAM_BACKGROUND_RESOLUTION.ToString());
                    writer.WriteLine("Human diagram angle grain = " + HUMAN_DIAGRAM_ANGLE_GRAIN.ToString());
                    writer.WriteLine("Track portrait size = " + TRACK_PORTRAIT_SIZE.ToString());
                    writer.WriteLine("Diagram portrait size = " + DIAGRAM_PORTRAIT_SIZE.ToString());
                    writer.WriteLine("Puppy portrait offset = " + PUPPY_PORTRAIT_OFFSET.ToString());
                    writer.WriteLine("Human animation speed = " + HUMAN_SPEED.ToString());
                    writer.WriteLine("Puppy animation speed = " + PUPPY_SPEED.ToString());
                    writer.WriteLine("Speed scale factor = " + SPEED_SCALE_FACTOR.ToString());
                    writer.WriteLine("Timer interval (milliseconds) = " + TIMER_INTERVAL_MILLISECONDS.ToString());
                    writer.WriteLine("Edge-to-angle ratio in diagrams = " + HUMAN_DIAGRAM_EDGE_FACTOR.ToString());
                    writer.WriteLine("Track line thickness = " + TRACK_LINE_THICKNESS.ToString());
                    writer.WriteLine("Arrow line thickness = " + ARROW_LINE_THICKNESS.ToString());
                    writer.WriteLine("Arrowhead width = " + ARROW_W_SIZE.ToString());
                    writer.WriteLine("Arrowhead length = " + ARROW_H_SIZE.ToString());
                    writer.WriteLine("Tangent length = " + TANGENT_LENGTH.ToString());
                    writer.WriteLine("Normal line thickness = " + NORMAL_LINE_THICKNESS.ToString());
                    writer.WriteLine("Normal line length = " + NORMAL_LENGTH.ToString());
                    writer.WriteLine("Track circle size = " + TRACK_CIRCLE_SIZE.ToString());
                    writer.WriteLine("Diagram line thckness = " + DIAGRAM_LINE_THICKNESS.ToString());
                    writer.WriteLine("Diagram normal thickness = " + DIAGRAM_NORMAL_THICKNESS.ToString());
                    writer.WriteLine("Diagram circle size = " + DIAGRAM_CIRCLE_SIZE.ToString());
                    writer.WriteLine("Chamfering factor = " + CHAMFERING_FACTOR.ToString());
                }
            }
            catch { }
        }

        public static List<Vector> ReadTrack()
        {
            try
            {
                if (!File.Exists(TRACK_FILE)) return null;
                List<Vector> t = new List<Vector>();
                using (StreamReader reader = new StreamReader(TRACK_FILE))
                {
                    while (!reader.EndOfStream)
                    {
                        string[] s = reader.ReadLine().Split(',');
                        if (s.Length != 2) return null;
                        double x = Double.Parse(s[0], NumberStyles.Float);
                        double y = Double.Parse(s[1], NumberStyles.Float);
                        t.Add(new Vector(x, y));
                    }
                }
                return t;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]

        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (!File.Exists(CONFIG_FILE)) WriteConfigFile();
            else
            {
                using (StreamReader reader = new StreamReader(CONFIG_FILE))
                {
                    ReadInt(reader, ref TRACK_WINDOW_SIZE);
                    ReadDouble(reader, ref PUPPY_WINDOW_SIZE_FACTOR);
                    ReadDouble(reader, ref HUMAN_WINDOW_W_SIZE_FACTOR);
                    ReadDouble(reader, ref HUMAN_WINDOW_H_SIZE_FACTOR);
                    ReadDouble(reader, ref INITIAL_TRACK_SCALE);
                    ReadDouble(reader, ref TRACK_RESIZE_FACTOR);
                    ReadDouble(reader, ref PUPPY_DIAGRAM_RESIZE_FACTOR);
                    ReadDouble(reader, ref HUMAN_DIAGRAM_H_SCALE);
                    ReadDouble(reader, ref HUMAN_DIAGRAM_RESIZE_FACTOR);
                    ReadDouble(reader, ref INITIAL_FEATURE_SCALE);
                    ReadDouble(reader, ref FEATURE_RESIZE_FACTOR);
                    ReadDouble(reader, ref PUPPY_DIAGRAM_BACKGROUND_RESOLUTION);
                    ReadDouble(reader, ref HUMAN_DIAGRAM_ANGLE_GRAIN);
                    ReadDouble(reader, ref TRACK_PORTRAIT_SIZE);
                    ReadDouble(reader, ref DIAGRAM_PORTRAIT_SIZE);
                    ReadDouble(reader, ref PUPPY_PORTRAIT_OFFSET);
                    ReadDouble(reader, ref HUMAN_SPEED);
                    ReadDouble(reader, ref PUPPY_SPEED);
                    ReadDouble(reader, ref SPEED_SCALE_FACTOR);
                    ReadInt(reader, ref TIMER_INTERVAL_MILLISECONDS);
                    ReadDouble(reader, ref HUMAN_DIAGRAM_EDGE_FACTOR);
                    ReadDouble(reader, ref TRACK_LINE_THICKNESS);
                    ReadDouble(reader, ref ARROW_LINE_THICKNESS);
                    ReadDouble(reader, ref ARROW_W_SIZE);
                    ReadDouble(reader, ref ARROW_H_SIZE);
                    ReadDouble(reader, ref TANGENT_LENGTH);
                    ReadDouble(reader, ref NORMAL_LINE_THICKNESS);
                    ReadDouble(reader, ref NORMAL_LENGTH);
                    ReadDouble(reader, ref TRACK_CIRCLE_SIZE);
                    ReadDouble(reader, ref DIAGRAM_LINE_THICKNESS);
                    ReadDouble(reader, ref DIAGRAM_NORMAL_THICKNESS);
                    ReadDouble(reader, ref DIAGRAM_CIRCLE_SIZE);
                    ReadDouble(reader, ref CHAMFERING_FACTOR);
                }
                PUPPY_WINDOW_SIZE_1 = (int)(TRACK_WINDOW_SIZE * PUPPY_WINDOW_SIZE_FACTOR);
                PUPPY_WINDOW_SIZE_2 = PUPPY_WINDOW_SIZE_1 * 3 / 2;
                PUPPY_WINDOW_SIZE_3 = PUPPY_WINDOW_SIZE_1 * 2;
                HUMAN_WINDOW_WIDTH = (int)(TRACK_WINDOW_SIZE * HUMAN_WINDOW_W_SIZE_FACTOR);
                HUMAN_WINDOW_HEIGHT = (int)(TRACK_WINDOW_SIZE * HUMAN_WINDOW_H_SIZE_FACTOR);
                PUPPY_BITMAP_BASE_RESOLUTION = (int)(PUPPY_WINDOW_SIZE_1 * PUPPY_DIAGRAM_BACKGROUND_RESOLUTION);
                HUMAN_DIAGRAM_ANGLE_FACTOR = 1 - HUMAN_DIAGRAM_EDGE_FACTOR;
                WriteConfigFile();
            }

            Application.Run(new TrackForm());
        }
    }
}
