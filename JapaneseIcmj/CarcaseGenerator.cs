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
    }

    public class CarcaseGenerator
    {
        public Carcase CarcaseData { get; private set; } = new();

        public Mat GenerateCarcase(int key,List<KmtPoint>? list)
        {
            //Getstatistic data from json, learned by Australian ICMJ
            //list is it

            CarcaseData.SetKey(key);

            var rnd = new Random();
            List<(List<Point> Left, List<Point> Right)> statList = new List<(List<Point>, List<Point>)>();
            var orderedList = list?.Select(p => new Point(p.X, p.Y)).OrderBy(p => p.Y).ToList();

            List<List<Point>> xClass = new List<List<Point>>();
            
            //x
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
            
            
            var statAverageList = statList.Select(lists =>
            {
                var avgLx = lists.Left.Average(d => (double)d.X);
                var avgLy = lists.Left.Average(d => (double)d.Y);
                var avgRx = lists.Right.Average(d => (double)d.X);
                var avgRy = lists.Right.Average(d => (double)d.Y);
                return (new Point(avgLx, avgLy),new Point(avgRx, avgRy));
            }).ToList();
            
            var statStdDevList = statList.Select(lists =>
            {
                var lx = lists.Left.Select(d => (double)d.X).StandardDeviation();
                var ly = lists.Left.Select(d => (double)d.Y).StandardDeviation();
                var rx = lists.Right.Select(d => (double)d.X).StandardDeviation();
                var ry = lists.Right.Select(d => (double)d.Y).StandardDeviation();

                return (new Point(lx, ly),new Point(rx, ry));
            }).ToList();

            Console.WriteLine("statAverageList"+statStdDevList.Count);

            List<List<Point>> pracList = new List<List<Point>>();

            for (int j = 0; j < 4; j++)
            {
                List<Point> psudoList = new List<Point>();
                int par = rnd.Next(0,31);

                for (int i = 0; i < statList.Count; i++)
                {
                    var sd = statStdDevList[i];
                    var avg = statAverageList[i];

                    //var l = statList[i];

                    //int lowC = rnd.Next(0, l.Left.Count() - 1);
                    //int highC = rnd.Next(0, l.Right.Count() - 1);

                    //var lowX = l.Left[lowC].X;
                    //var highX = l.Right[highC].X;
                    var lowX = avg.Item1.X - (sd.Item1.X* par/10);
                    var highX = avg.Item2.X +(sd.Item2.X* par/10);

                    var y = sd.Item1.Y;

                    psudoList.Add(new Point((150 * j) + lowX, y));
                    psudoList.Add(new Point((150 * j) + highX, y));
                }
                pracList.Add(psudoList);
            }

            Mat? newMat = new Mat(new Size(640, 480), MatType.CV_8UC3, Scalar.White);
            newMat.DrawContours(pracList, -1, Scalar.Red, 1);
            Cv2.MedianBlur(newMat, newMat, 17);
            return newMat;
        }
    }
}
