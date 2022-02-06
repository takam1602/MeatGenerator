using OpenCvSharp;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using MathNet.Numerics.Statistics;

namespace JapaneseIcmj
{
    public class KmtPoint
    {
        public int Id { get; private set; }
        public double X { get; private set; }
        public double Y { get; private set; }

        public KmtPoint(int id, double x, double y)
        {
            this.Id = id;
            this.X = x;
            this.Y = y;
        }
    }

    public class Carcase
    {
        public int Key { get; private set; } = 0;
        public double Butt { get; private set; } = 0;
        public double Loin { get; private set; } = 0;
        public double Chuck { get; private set; } = 0;

        public void SetKey(int k)
        {
            this.Key = k;
        }

        public void SetButt(double bu)
        {
            Butt = bu;
        }
        public void SetLoin(double lo)
        {
            Loin = lo;
        }
        public void SetChuck(double cu)
        {
            Chuck = cu;
        }
    }

    public class CarcaseGenerator
    {
        public Carcase CarcaseData { get; private set; } = new();

        public CarcaseGenerator()
        {
        }

        private List<(List<Point> Left, List<Point> Right)> statList = new List<(List<Point>, List<Point>)>();
        private List<(Point Left,Point Right )>statAverageList = new List<(Point,Point)>();
        private List<(Point Left,Point Right )>statStdDevList = new List<(Point,Point)>();

        public CarcaseGenerator(List<KmtPoint>? list)
        {
            var orderedList = list?.Select(p => new Point(p.X, p.Y)).OrderBy(p => p.Y).ToList();

            List<List<Point>> xClass = new List<List<Point>>();
            for (int k = 0; k < orderedList?.Count - 2; k++)
            {
                List<Point> temp = new List<Point>();

                while (true)
                {
                    temp.Add(orderedList[k]);
                    k++;

                    if (k == orderedList.Count() - 1 || orderedList[k].Y != orderedList[k + 1].Y)
                    {
                        temp.Add(orderedList[k]);
                        break;
                    }
                }
                xClass.Add(temp);
            }


            for (int i = 0; i < xClass.Count; i++)
            {
                var l = xClass[i];
                var avg = l.Average(p => p.X);
                List<Point> left = new List<Point>();
                List<Point> right = new List<Point>();
                foreach (var s in l)
                {
                    if (s.X <= avg)
                        left.Add(s);
                    else
                        right.Add(s);
                }
                statList.Add((left, right));
            }


            statAverageList = statList.Select(lists =>
            {
                var avgLx = lists.Left.Average(d => (double)d.X);
                var avgLy = lists.Left.Average(d => (double)d.Y);
                var avgRx = lists.Right.Average(d => (double)d.X);
                var avgRy = lists.Right.Average(d => (double)d.Y);
                return (new Point(avgLx, avgLy), new Point(avgRx, avgRy));
            }).ToList();

            statStdDevList = statList.Select(lists =>
            {
                var lx = lists.Left.Select(d => (double)d.X).StandardDeviation();
                var ly = lists.Left.Select(d => (double)d.Y).StandardDeviation();
                var rx = lists.Right.Select(d => (double)d.X).StandardDeviation();
                var ry = lists.Right.Select(d => (double)d.Y).StandardDeviation();

                return (new Point(lx, ly), new Point(rx, ry));
            }).ToList();
        }
        
        public (Mat mat, Carcase result, Mat resultmat) GenerateCarcaseSingle(int key)
        {
            List<List<Point>> pracList = new List<List<Point>>();
            List<Point> psudoList = new List<Point>();

            Carcase carcase = new Carcase();
            
            Random rnd2 = new Random();
            
            int par = rnd2.Next(0, 301);
            int par2 = rnd2.Next(0, 10);
            
            var height = statList.Count;

            for (int i = 0; i < statList.Count; i++)
            {
                var sd = statStdDevList[i];
                var avg = statAverageList[i];

                var lowX = avg.Item1.X - (sd.Item1.X * par / 100) - par2;
                var highX = avg.Item2.X + (sd.Item2.X * par / 100) - par2;

                var y = avg.Item1.Y;

                if (y == (int)(height / 2))
                    carcase.SetLoin((double)Math.Abs(lowX - highX));

                psudoList.Add(new Point(lowX, y));
                psudoList.Add(new Point(highX, y));
            }

            pracList.Add(psudoList);

            var butR = psudoList.Where(d => d.Y < height / 2).OrderByDescending(d => d.X).FirstOrDefault();
            var butL = psudoList[psudoList.IndexOf(butR)+1];

            var chuckL = psudoList.Where(d => d.Y > height / 2).OrderBy(d => d.X).FirstOrDefault();
            var chuckR = psudoList[psudoList.IndexOf(chuckL)+1];

            carcase.SetButt((double)Math.Abs(butR.X - butL.X));
            carcase.SetChuck((double)Math.Abs(chuckL.X - chuckR.X));
            carcase.SetKey(key);

            Mat? newMat = new Mat(new Size(160, 480), MatType.CV_8UC3, Scalar.Black);
            newMat.DrawContours(pracList, -1, Scalar.Red, 1);
            Cv2.MedianBlur(newMat, newMat, 17);

            var ansMat = newMat.Clone();
            Cv2.Line(ansMat, new Point(0, (int)chuckL.Y), new Point(newMat.Width, (int)chuckL.Y), Scalar.Green, 1);
            Cv2.Line(ansMat, new Point(0, (int)butL.Y), new Point(newMat.Width, (int)butL.Y), Scalar.Green, 1);
            Cv2.Line(ansMat, new Point(0, (int)height / 2), new Point(newMat.Width, (int)height / 2), Scalar.Green, 1);

            return (newMat, carcase, ansMat);
        }
    }
}
