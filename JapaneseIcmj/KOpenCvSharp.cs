using OpenCvSharp;

namespace JapaneseIcmj
{
    public class KOpenCvSharp
    {
        public Mat ResizeNearest(Mat src, int h, int w)
        {
            //出力画像用の配列生成（要素は全て空）
            Mat dst = new Mat(new OpenCvSharp.Size(w, h), MatType.CV_8UC3, new Scalar(0, 0, 0));

            //# 元画像のサイズを取得
            //hi, wi = src.shape[0], src.shape[1]
            var hi = src.Height;
            var wi = src.Width;

            //# 拡大率を計算
            var ax = w / (float)wi;
            var ay = h / (float)hi;

            //# 最近傍補間
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    var xi = (int)(Math.Round(x / ax));
                    var yi = (int)(Math.Round(y / ay));
                    //# 存在しない座標の処理
                    if (xi > wi - 1)
                        xi = wi - 1;
                    if (yi > hi - 1)
                        yi = hi - 1;

                    dst.At<Vec3b>(y, x) = src.At<Vec3b>(yi, xi);
                }
            }
            return dst;
        }

        public Mat Black2White(Mat res,int th,int th2)
        {
            Mat dst = res.Clone();
            Mat hsv = new();
            Cv2.CvtColor(res,hsv,ColorConversionCodes.BGR2HSV);

            for (int y = 2; y < res.Height-2; ++y)
            {
                for (int x = 2; x < res.Width-2; ++x)
                {
                    var a = hsv.At<Vec3b>(y, x);

                    if (a.Item2<(int)th*2.55)
                    { 
                        dst.At<Vec3b>(y-1, x-1) = new Vec3b(255,255,255);
                        dst.At<Vec3b>(y, x-1) = new Vec3b(255,255,255);
                        dst.At<Vec3b>(y+1, x-1) = new Vec3b(255,255,255);
                        dst.At<Vec3b>(y-1, x) = new Vec3b(255,255,255);
                        dst.At<Vec3b>(y, x) = new Vec3b(255,255,255);
                        dst.At<Vec3b>(y+1, x) = new Vec3b(255,255,255);
                        dst.At<Vec3b>(y-1, x+1) = new Vec3b(255,255,255);
                        dst.At<Vec3b>(y, x+1) = new Vec3b(255,255,255);
                        dst.At<Vec3b>(y+1, x+1) = new Vec3b(255,255,255);
                    }
                }
            }

            return dst;
        }
        
        private bool isBlack(List<int> li,int threash,int threash2)
        {
            if (li.Count>0&&(li.Max()-li.Average())<threash&&li.Average()<threash2)
                return true;
            else
                return false;
        }
        
        public Mat OhtsuBinary(Mat src)
        {
            Mat dst = new Mat(new Size(src.Width, src.Height),MatType.CV_8UC1);

            var hist = Enumerable.Repeat<int>(0, 256).ToArray();
            /* ヒストグラム作成 */

            for (int y = 0; y < src.Height; ++y)
            {
                for (int x = 0; x < src.Width; ++x)
                {
                    hist[(int)(src.At<byte>(y, x))]++;  // 輝度値を集計
                }
            }

            /* 判別分析法 */
            int t = 0;  // 閾値
            double max = 0.0;  // w1 * w2 * (m1 - m2)^2 の最大値

            for (int i = 0; i < 256; ++i)
            {
                int w1 = 0;  // クラス１の画素数
                int w2 = 0;  // クラス２の画素数
                long sum1 = 0;  // クラス１の平均を出すための合計値
                long sum2 = 0;  // クラス２の平均を出すための合計値
                double m1 = 0.0;  // クラス１の平均
                double m2 = 0.0;  // クラス２の平均

                for (int j = 0; j <= i; ++j)
                {
                    w1 += hist[j];
                    sum1 += j * hist[j];
                }

                for (int j = i + 1; j < 256; ++j)
                {
                    w2 += hist[j];
                    sum2 += j * hist[j];
                }

                if (w1!=0)
                    m1 = (double)(sum1 / w1);

                if (w2!=0)
                    m2 = (double)(sum2 / w2);

                double tmp = (double)(w1 * w2 * (m1 - m2) * (m1 - m2));

                if (tmp > max)
                {
                    max = tmp;
                    t = i;
                }
            }

            /* tの値を使って２値化 */
            for (int y = 0; y < src.Height; ++y)
            {
                for (int x = 0; x < src.Width; ++x)
                {
                    if (src.At<byte>(y, x) < (byte)t)
                        dst.At<byte>(y, x) = (byte)255;
                    else
                        dst.At<byte>(y, x) = (byte)0;
                }
            }

            return dst;
        }

        public Mat BGR2Gray(Mat src)
        { 
            Mat dst = new Mat(new Size(src.Width, src.Height),MatType.CV_8UC1);
            
            /* tの値を使って２値化 */
            for (int y = 0; y < src.Height; ++y)
            {
                for (int x = 0; x < src.Width; ++x)
                {
                    var r = src.At<Vec3b>(y, x);
                    var k = 0.299 * r.Item2 + 0.587 * r.Item1 + 0.114 * r.Item0;
                    dst.At<byte>(y, x) = (byte)(k);
                }
            }

            return dst;


        }
    }
}
