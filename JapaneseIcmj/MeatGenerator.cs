using System;
using OpenCvSharp;

namespace JapaneseIcmj
{
    public class Meat
    {
        public int Key { get; private set; } = 0;
        public double BMS { get; private set; } = 0;
        public double MeatColor { get; private set; } = 0;
        public double Area { get; private set; } = 0;

        public Meat()
        { }

        public void SetKey(int k)
        { 
            this.Key = k;
        }

        public void SetBms(List<(Point p,int s)>list)
        {
            var MarblingArea = 0.0;
            foreach (var d in list)
            { 
                if(d.s%2==0)
                    MarblingArea += 3*d.s * d.s * Math.PI/4;
                else
                    MarblingArea += d.s * d.s * Math.PI;
            }

            //this.BMS = -1.26 + 100 * 0.462*MarblingArea / this.Area;
            this.BMS = (7.184+100*(MarblingArea / this.Area))/4.418;
        }

        public void SetArea(double a,double b)
        { 
            this.Area = Math.PI*a*b;
        }

        public void SetMeatColor(Scalar mc)
        { 
            var bcs = 19.858019 - 0.095460824*mc.Val2 - 0.008808958*mc.Val1 + 0.17291258*mc.Val0;
            this.MeatColor = bcs;
        }
    }

    public class MeatGenerator
    {
        public Meat MeatData { get; private set; } = new();
        
        private int squaresize = 256;

        public Mat MeatMat(int key)
        {
            MeatData = new();
            MeatData.SetKey(key);
            var mat = this.GenerateMeat();
            return mat;
        }

        private Mat GenerateMeat()
        {
            Mat meat = new Mat(new Size(squaresize,squaresize),MatType.CV_8UC3,new Scalar(0,0,0));
            Mat dstMat = new Mat(new Size(squaresize,squaresize),MatType.CV_8UC3,new Scalar(0,0,0));
            var shaft = this.EllipseShaftsSize();
            var mc = this.MeatColor();
            var fc = this.FatColor();
            var result = this.MarblingPositoinAndSize((int)shaft.a,(int)shaft.b);
            Cv2.Ellipse(meat,new RotatedRect(new Point2f(squaresize/2,squaresize/2),new Size2f(2*shaft.a,2*shaft.b),0),mc,-1);
            
            foreach (var r in result)
            {
                if(r.size%2 ==0)
                    Cv2.Ellipse(meat,new RotatedRect(new Point2f(r.position.X,r.position.Y),new Size2f(3*r.size,1*r.size),0),fc,-1);
                else 
                    Cv2.Circle(meat,r.position,r.size,fc,-1,LineTypes.AntiAlias);
            }
            MeatData.SetMeatColor(mc);
            MeatData.SetArea(shaft.a,shaft.b);
            MeatData.SetBms(result);

            using var mama = new Mat();
            Cv2.MedianBlur(meat,mama,7);
            mama.CopyTo(dstMat);
            return dstMat; 
        }

        // random value, value range is analyzed w/ jmga meat color sampling
        private Scalar MeatColor()
        {
            var rnd = new Random();
            var b = 28 + rnd.Next(0, 4);
            var g = 49 + rnd.Next(0, 19);
            var r = 205 + rnd.Next(0, 34);

            var color = new Scalar(b, g, r);

            return color;
        }

        // random value, value range is analyzed w/ jmga meat color sampling
        private Scalar FatColor()
        {
            var rnd = new Random();
            var b = 255 - rnd.Next(0, 2);
            var g = 255 - rnd.Next(0, 26);
            var r = 255 - rnd.Next(0, 78);

            var color = new Scalar(b, g, r);

            return color;
        }

        //random value
        private List<(Point position,int size)> MarblingPositoinAndSize(int a,int b)
        {
            var rnd = new Random();
            var numOfMarbling = rnd.Next(499,799);
            List<(Point position,int size)> points = new List<(Point,int)>();

            for (var i = 0; i < numOfMarbling; i++)
            { 
                var x = rnd.Next(squaresize/2 - a, squaresize/2+ a);
                var y = rnd.Next(squaresize/2 - b, squaresize/2+ b);
                var size = rnd.Next(2, 5);

                var dx = x - squaresize / 2;
                var dy = y - squaresize / 2;

                var insidePar = b*b * dx * dx + a*a * dy * dy - a * a * b * b;

                if (insidePar > 0)
                    continue;
                    
                points.Add((new Point(x, y),size));   
            }
            return points;  
        }
        // random value, value range is 
        private (double a,double b) EllipseShaftsSize()
        {
            var rnd = new Random();
            var rad = rnd.Next((int)(0.8*squaresize/2), (int)squaresize/2);
            var shaftX = rad;
            rad = rnd.Next((int)(0.8*squaresize/2), (int)squaresize/2);
            var shaftY = rad;
            
            return (shaftX, shaftY);
        }
    }
}
