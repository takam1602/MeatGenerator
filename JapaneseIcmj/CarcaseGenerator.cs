using OpenCvSharp;
using System.Text.Json;
using System.Text.Json.Serialization; 

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
            CarcaseData.SetKey(key);

            var rnd = new Random();
            List<(List<Point> low, List<Point> high)> statList = new List<(List<Point>, List<Point>)>();
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
                List<Point> low = new List<Point>();
                List<Point> high = new List<Point>();
                foreach (var s in l)
                {
                    if (s.X <= avg)
                        low.Add(s);
                    else
                        high.Add(s);
                }
                statList.Add((low, high));
            }

            List<List<Point>> pracList = new List<List<Point>>();

            for (int j = 0; j < 4; j++)
            {
                List<Point> psudoList = new List<Point>();
                for (int i = 0; i < statList.Count; i++)
                {
                    var l = statList[i];

                    int lowC = rnd.Next(0, l.low.Count() - 1);
                    int highC = rnd.Next(0, l.high.Count() - 1);
                    
                    var lowX = l.low[lowC].X;
                    var highX = l.high[highC].X;

                    var y = l.high.Min(p => p.Y);

                    psudoList.Add(new Point(150 * j + lowX, y));
                    psudoList.Add(new Point(150 * j + highX, y));
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
