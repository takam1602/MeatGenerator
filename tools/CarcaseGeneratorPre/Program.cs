using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using MathNet.Numerics.Statistics;

Mat? _srcMat;
var rnd = new Random();
var b = rnd.Next(0, 255);
string imgSrc = "images/" + b + ".bmp";
List<Mat> CarcaseMat = new List<Mat>();

//y座標当たりの横幅のリスト，ここからばらつかせて疑似画像を生成する．
List<(List<Point> Left, List<Point> Right)> statList = new List<(List<Point>, List<Point>)>();
//List<Point> Distances = new List<Point>();
//List<(int key,double x,double y)> Distances = new List<(int ,double,double)>();
List<KmtPoint> Distances = new List<KmtPoint>();

int count = 0;
for (int kj = 1; kj < 11; kj++)
{
    //枝肉画像読み込み リサイズ
    _srcMat = Cv2.ImRead(@"../../../img/" + kj + ".jpg");
    Cv2.Resize(_srcMat, _srcMat, new Size(640, 480));
    var mat = _srcMat.Clone();

    //二値化ゴマ塩除去jk
    Cv2.CvtColor(mat, mat, ColorConversionCodes.BGRA2GRAY);
    Cv2.BitwiseNot(mat, mat);
    var th = Cv2.Threshold(mat, mat, 0, 255, ThresholdTypes.Binary | ThresholdTypes.Otsu);

    Cv2.MedianBlur(mat, mat, 17);

    //輪郭抽出
    Point[][] contours;
    HierarchyIndex[] hierarchyIndexes;
    mat.FindContours(out contours, out hierarchyIndexes, RetrievalModes.List, ContourApproximationModes.ApproxNone);
    
    //小さいor大きい輪郭除去
    var reContours = contours.Where(x => x.Count() > 100 && x.Count() < 1500).ToArray();

    //それぞれのクラスでx座標の平均でソート
    reContours = reContours.OrderBy(k => k.Average(p => p.X)).ToArray();

    //4つの枝肉画像，左側を基準にずらすための中心x座標造り
    //4頭映ってることが前提
    var CenterList = reContours.Select(d => d.Average(p => p.X)).OrderBy(p => p).ToList();

    for (int i = 0; i < reContours.Count(); i++)
    {
        List<KmtPoint> tempList = new List<KmtPoint>();

        for (int j = 0; j < reContours[i].Count() - 1; j++)
        {
            //4つの枝肉画像，左側を基準にずらす
            var x = reContours[i][j].X - CenterList[i] + CenterList[0];
            var y = reContours[i][j].Y;
            tempList.Add(new KmtPoint(count, x, y));
            count++;
        }
        Distances.AddRange(tempList);
    }
}

//json file に出力
//出力は中心から左右にどれだけ離れているかを個体ごとに表現したリスト
var json_str = JsonSerializer.Serialize(Distances);
File.WriteAllText("carcaseData.json", json_str);

//jsonを読み込む   
var desiriarise = File.ReadAllText("carcaseData.json");
var jsonData = JsonSerializer.Deserialize<List<KmtPoint>>(desiriarise);
var orderedList = jsonData?.Select(p => new Point(p.X, p.Y)).OrderBy(p => p.Y).ToList();

List<List<Point>> xClass = new List<List<Point>>();

//それぞれのx座標当たりの平均取得したいから，クラスわけ．
for (int k = 0; k < orderedList.Count - 2; k++)
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

var statAverageList = statList.Select(lists =>
{
    var avgLx = lists.Left.Average(d => (double)d.X);
    var avgLy = lists.Left.Average(d => (double)d.Y);
    var avgRx = lists.Right.Average(d => (double)d.X);
    var avgRy = lists.Right.Average(d => (double)d.Y);
    return (new Point(avgLx, avgLy), new Point(avgRx, avgRy));
}).ToList();

var statStdDevList = statList.Select(lists =>
{
    var lx = lists.Left.Select(d => (double)d.X).StandardDeviation();
    var ly = lists.Left.Select(d => (double)d.Y).StandardDeviation();
    var rx = lists.Right.Select(d => (double)d.X).StandardDeviation();
    var ry = lists.Right.Select(d => (double)d.Y).StandardDeviation();

    return (new Point(lx, ly), new Point(rx, ry));
}).ToList();

Console.WriteLine("statAverageList" + statStdDevList.Count);


for (int j = 0; j < 4; j++)
{
    List<List<Point>> pracList = new List<List<Point>>();
    List<Point> psudoList = new List<Point>();
    int par = rnd.Next(0, 301);
    int par2 = rnd.Next(0, 7);

    for (int i = 0; i < statList.Count; i++)
    {
        var sd = statStdDevList[i];
        var avg = statAverageList[i];

        var lowX = avg.Item1.X -(sd.Item1.X * par / 100)-par2;
        var highX = avg.Item2.X + (sd.Item2.X * par / 100)-par2;

        var y = avg.Item1.Y;

        psudoList.Add(new Point(lowX, y));
        psudoList.Add(new Point(highX, y));
    }

    pracList.Add(psudoList);
    var m = new Mat(new Size(160, 480), MatType.CV_8UC3, Scalar.White);
    m.DrawContours(pracList, -1, Scalar.Pink, 1);
    Cv2.MedianBlur(m,m, 17);
    Cv2.ImShow("draw", m);
    Cv2.WaitKey();
}


//結果をjsonで保存？
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
