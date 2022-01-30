using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

Mat? _srcMat;
var rnd = new Random();
var b = rnd.Next(0, 255);
string imgSrc = "images/" + b + ".bmp";
List<Mat> CarcaseMat = new List<Mat>();

//y座標当たりの横幅のリスト，ここからばらつかせて疑似画像を生成する．
List<(List<Point> low, List<Point> high)> statList = new List<(List<Point>, List<Point>)>();
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

    //4つの枝肉画像，左側を基準にずらす
    //4頭映ってることが前提
    var CenterList = reContours.Select(d => d.Average(p => p.X)).OrderBy(p => p).ToList();

    for (int i = 0; i < reContours.Count(); i++)
    {
        //List<Point> tempList = new List<Point>();
        //List<(int,double,double)> tempList = new List<(int,double,double)>();
        List<KmtPoint> tempList = new List<KmtPoint>();

        for (int j = 0; j < reContours[i].Count() - 1; j++)
        {
            var x = reContours[i][j].X - CenterList[i] + CenterList[0];
            var y = reContours[i][j].Y;
            tempList.Add(new KmtPoint(count,x, y));
            count++;
        }
        Distances.AddRange(tempList);
    }
}
   
//json file に出力
var json_str = JsonSerializer.Serialize(Distances);
File.WriteAllText("carcaseData.json", json_str);
   
var desiriarise = File.ReadAllText("carcaseData.json");

var jsonData = JsonSerializer.Deserialize<List<KmtPoint>>(desiriarise);
//var orderedList = Distances.Select(p=>new Point(p.X,p.Y)).OrderBy(p => p.Y).ToList();
var orderedList = jsonData?.Select(p=>new Point(p.X,p.Y)).OrderBy(p => p.Y).ToList();

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

List<List<Point>> pracList = new List<List<Point>>();

for (int j = 0; j < 4; j++)
{
    List<Point> psudoList = new List<Point>();
    for (int i = 0; i < statList.Count; i++)
    {
        var l = statList[i];
        
        int lowC  = rnd.Next(0,l.low.Count()-1);
        int highC  = rnd.Next(0,l.high.Count()-1);

        //一様分布から取ると差がでにくい
        //var lowX = rnd.Next(l.low.Min(p => p.X), l.low.Max(p => p.X));
        //var highX = rnd.Next(l.high.Min(p => p.X), l.high.Max(p => p.X));

        //ので頻度を考慮した実際の値から取得
        var lowX = l.low[lowC].X;
        var highX = l.high[highC].X;

        var y = l.high.Min(p => p.Y);

        psudoList.Add(new Point(150 * j + lowX, y));
        psudoList.Add(new Point(150 * j + highX, y));
    }
    pracList.Add(psudoList);
}

Mat? newMat = new Mat(new Size(640, 480), MatType.CV_8UC3, Scalar.Black);
newMat.DrawContours(pracList, -1, Scalar.Pink, 1);
Cv2.MedianBlur(newMat, newMat, 17);
Cv2.ImShow("draw", newMat);
Cv2.WaitKey();

//結果をjsonで保存？
public class KmtPoint
{
	public int Id { get;private  set; }
	public double X { get; private set; }
    public double Y { get; private set; }

    public KmtPoint(int id, double x, double y)
    {
        this.Id = id;
        this.X = x;
        this.Y = y;
    }
}
