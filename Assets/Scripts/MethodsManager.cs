using OpenCVForUnity.CoreModule;
using OpenCVForUnity.Dnn_superresModule;
using OpenCVForUnity.DnnModule;
using OpenCVForUnity.ImgcodecsModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UI;
using Rect = OpenCVForUnity.CoreModule.Rect;

public class MethodsManager : MonoBehaviour
{
    Mat matBuffer;
    public Slider brightness;
    public Slider sat;
    public Slider alpha;
    public Slider size;

    public void AddNoise()
    {
        Mat curMat = PictureLoader.Instance.savedMat.clone();
        Mat noiseMat = new Mat(curMat.rows(), curMat.cols(), curMat.type(), new Scalar(0, 0, 0, 0));
        Core.randn(noiseMat, 0, 30);
        Mat dst = new Mat(curMat.size(), curMat.type());
        Core.addWeighted(curMat, 0.9, noiseMat, 0.1, 0, dst);
        Core.normalize(dst, dst, 0, 255, Core.NORM_MINMAX);
        //Imgproc.cvtColor(dst, dst, Imgproc.COLOR_BGR2RGB);
        PictureLoader.Instance.curMat = dst;
    }

    public void GuiYiFilter()
    {
        Mat curMat = PictureLoader.Instance.savedMat.clone();
        Imgproc.blur(curMat, curMat, new Size(10, 10));
        PictureLoader.Instance.curMat = curMat;
        curMat.Dispose();
    }

    public void CannyFilter()
    {
        Mat curMat = PictureLoader.Instance.savedMat.clone();
        Mat edge = new Mat();
        Imgproc.cvtColor(curMat, curMat, Imgproc.COLOR_BGRA2GRAY);

        //Imgproc.blur(curMat, edge, new Size(10, 10));

        Imgproc.Laplacian(curMat, edge, CvType.CV_16S, 3, 1, 0, Core.BORDER_DEFAULT);
        Core.convertScaleAbs(edge, edge);

        PictureLoader.Instance.curMat = edge;
    }

    public void RGBToGray()
    {
        Mat curMat = PictureLoader.Instance.savedMat.clone();
        Imgproc.cvtColor(curMat, curMat, Imgproc.COLOR_BGR2GRAY);
        PictureLoader.Instance.curMat = curMat;
        curMat.Dispose();
    }

    public void Rotate()
    {
        Mat curMat = PictureLoader.Instance.curMat.clone();
        Core.rotate(curMat, curMat, Core.ROTATE_90_CLOCKWISE);
        PictureLoader.Instance.curMat = curMat;
        curMat.Dispose();
    }
    public void Flip()
    {
        Mat curMat = PictureLoader.Instance.curMat.clone();
        Core.flip(curMat, curMat, 0); // 0 x, + y, - z
        PictureLoader.Instance.curMat = curMat;
        curMat.Dispose();
    }

    public void Resize() //todo: 具体缩放大小
    {
        Mat curMat = PictureLoader.Instance.savedMat.clone();
        Imgproc.resize(curMat, curMat, PictureLoader.Instance.matSize * (size.value + 0.5f));
        PictureLoader.Instance.curMat = curMat;
        curMat.Dispose();
    }

    public void EaqualizeHist()
    {
        Mat curMat = PictureLoader.Instance.savedMat.clone();
        if (curMat.type() != CvType.CV_8UC1) return;
        Imgproc.equalizeHist(curMat, curMat);
        PictureLoader.Instance.curMat = curMat;
        curMat.Dispose();
    }

    public void AdjustBrightness()
    {
        Mat curMat = PictureLoader.Instance.savedMat.clone();
        Mat newMat = new Mat(curMat.size(), curMat.type());

        float value = brightness.value;
        curMat.convertTo(newMat, curMat.type(), 1, value);

        PictureLoader.Instance.curMat = newMat;
        curMat.Dispose();
    }

    public void AdjustAlpha()
    {
        Mat curMat = PictureLoader.Instance.savedMat.clone();
        Mat newMat = new Mat(curMat.size(), curMat.type());

        float value = alpha.value;
        curMat.convertTo(newMat, curMat.type(), value, 0);

        PictureLoader.Instance.curMat = newMat;
        curMat.Dispose();
    }

    public void AdjustSaturability()
    {
        StartCoroutine(Sat());
    }

    IEnumerator Sat()
    {
        Mat curMat = PictureLoader.Instance.savedMat.clone();
        Mat newMat = new Mat(curMat.size(), curMat.type());

        float stime = Time.time;
        float saturation = sat.value;
        Debug.Log("s");
        Debug.Log(curMat);
        float increment = (saturation - 80) * 1.0f / 200f;
        for (int col = 0; col < curMat.cols(); col++)
        {
            for (int row = 0; row < curMat.rows(); row++)
            {
                // R,G,B 分别对应数组中下标的 2,1,0
                float r = (float)curMat.get(row, col)[2];
                float g = (float)curMat.get(row, col)[1];
                float b = (float)curMat.get(row, col)[0];

                float maxn = Mathf.Max(r, Mathf.Max(g, b));
                float minn = Mathf.Max(r, Mathf.Max(g, b));

                float delta, value;
                delta = (maxn - minn) / 255;
                value = (maxn + minn) / 255;

                float new_r, new_g, new_b;

                float light, sat, alpha;
                light = value / 2;

                if (light < 0.5)
                    sat = delta / value;
                else
                    sat = delta / (2 - value);

                if (increment >= 0)
                {
                    if ((increment + sat) >= 1)
                        alpha = sat;
                    else
                    {
                        alpha = 1 - increment;
                    }
                    alpha = 1 / alpha - 1;
                    new_r = r + (r - light * 255) * alpha;
                    new_g = g + (g - light * 255) * alpha;
                    new_b = b + (b - light * 255) * alpha;
                }
                else
                {
                    alpha = increment;
                    new_r = light * 255 + (r - light * 255) * (1 + alpha);
                    new_g = light * 255 + (g - light * 255) * (1 + alpha);
                    new_b = light * 255 + (b - light * 255) * (1 + alpha);
                }
                newMat.put(row, col, new double[] { new_b, new_g, new_r, curMat.get(row, col)[3] });
                //Debug.Log(new double[] { new_b, new_g, new_r, curMat.get(row, col)[3] });
            }
            yield return null;
        }
        PictureLoader.Instance.curMat = newMat;
        Debug.Log("f" + (Time.time - stime));
    }

    public void MakeBorder()
    {
        Mat border = PictureLoader.Instance.LoadPic();
        Mat curMat = PictureLoader.Instance.savedMat.clone();

        Imgproc.resize(border, border, curMat.size());

        Mat mask = border.clone();
        Imgproc.cvtColor(mask, mask, Imgproc.COLOR_BGRA2GRAY);
        Imgproc.threshold(mask, mask, 10, 255, Imgproc.THRESH_BINARY);

        border.copyTo(curMat, mask);
        //Core.bitwise_and(curMat, border, curMat, mask);

        PictureLoader.Instance.curMat = curMat;
    }

    public void HorizontalCat()
    {
        Mat right = PictureLoader.Instance.LoadPic();
        Core.flip(right, right, 1);
        Mat curMat = PictureLoader.Instance.savedMat.clone();
        Imgproc.resize (right, right, new Size((float)curMat.height() / (float)right.height() * right.width(), curMat.height()));

        Core.hconcat(new List<Mat>() { curMat, right }, curMat);

        PictureLoader.Instance.curMat = curMat;
    }

    public void VerticalCat()
    {
        Mat right = PictureLoader.Instance.LoadPic();
        Core.flip(right, right, 1);
        Mat curMat = PictureLoader.Instance.savedMat.clone();
        Imgproc.resize(right, right, new Size(curMat.width(), (float)curMat.width() / (float)right.width() * right.height()));

        Core.vconcat(new List<Mat>() { curMat, right }, curMat);

        PictureLoader.Instance.curMat = curMat;
    }

    public void FuDiao()
    {
        Mat src = PictureLoader.Instance.savedMat.clone();
        if (src.type() == CvType.CV_8UC1) return;

        Mat dst = src.clone();
        int rowNumber = dst.rows();
        int colNumber = dst.cols();

        for (int i = 1; i < rowNumber - 1; ++i)
        {
            for (int j = 1; j < colNumber - 1; ++j)
            {
                dst.put(i, j, new double[] { (src.get(i + 1, j + 1)[0] - src.get(i - 1, j - 1)[0] + 128), (src.get(i + 1, j + 1)[1] - src.get(i - 1, j - 1)[1] + 128) , (src.get(i + 1, j + 1)[2] - src.get(i - 1, j - 1)[2] + 128), 255 });
            }

        }
        PictureLoader.Instance.curMat = dst;
    }

    public void Reflection()
    {
        Mat img = PictureLoader.Instance.savedMat.clone();
        Mat dstImg = new Mat(2 * img.rows(), img.cols(), img.type());
        img.copyTo(new Mat(dstImg, new Rect(0, img.rows(), img.cols(), img.rows())));

        int rowNumber = img.rows();
        int colNumber = img.cols();

        for (int i = 1; i < rowNumber - 1; ++i)
        {
            for (int j = 1; j < colNumber - 1; ++j)
            {
                int deltax = UnityEngine.Random.Range(0, 50);
                int deltay = UnityEngine.Random.Range(0, 50);

                while (j + deltax >= colNumber)
                {
                    deltax = UnityEngine.Random.Range(0, 50);
                }
                while (i + deltay >= rowNumber)
                {
                    deltay = UnityEngine.Random.Range(0, 50);
                }

                img.put(i, j, new double[] { img.get(i + deltay, j + deltax)[0], img.get(i + deltay, j + deltax)[1], img.get(i + deltay, j + deltax)[2], 255 });
            }

        }

        Core.flip(img, img, 0);
        img.copyTo(new Mat(dstImg, new Rect(0, 0, img.cols(), img.rows())));

        PictureLoader.Instance.curMat = dstImg;
    }
}