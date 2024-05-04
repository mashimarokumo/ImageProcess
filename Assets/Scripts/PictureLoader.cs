using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.ImgcodecsModule;
using OpenCVForUnity.ImgprocModule;

public class PictureLoader : SingletonForMonoBehaviour<PictureLoader>
{
    public RawImage image;
    public Mat curMat
    {
        get
        {
            if (_curMat == null)
            {
                Debug.Log("null");
            }
            return _curMat;
        }
        set {
            Core.flip(value, value, 1);
            _curMat = value.clone();
            Texture2D t2d = new Texture2D(value.cols(), value.rows(), TextureFormat.RGBA32, false);
            Utils.matToTexture2D(_curMat, t2d);
            image.texture = t2d;
            image.SetNativeSize();
        }
    }
    private Mat _curMat;
    public Mat savedMat;

    public Size matSize;
    public Mat oriMat;

    public void LoadAndShow()
    {
        Show(LoadPic());
    }

    public Mat LoadPic()
    {
        OpenFileName ofn = new OpenFileName();
        ofn.structSize = Marshal.SizeOf(ofn);
        ofn.filter = "ͼƬ�ļ�(*.jpg*.png)\0*.jpg;*.png";
        ofn.file = new string(new char[256]);
        ofn.maxFile = ofn.file.Length;
        ofn.fileTitle = new string(new char[64]);
        ofn.maxFileTitle = ofn.fileTitle.Length;
        //Ĭ��·��
        string path = Application.streamingAssetsPath;
        path = path.Replace('/', '\\');
        //Ĭ��·��
        //ofn.initialDir = "G:\\wenshuxin\\test\\HuntingGame_Test\\Assets\\StreamingAssets";
        ofn.initialDir = path;
        ofn.title = "Open Project";
        ofn.defExt = "JPG";//��ʾ�ļ�������
                           //ע�� һ����Ŀ��һ��Ҫȫѡ ����0x00000008�Ҫȱ��
        ofn.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000200 | 0x00000008;//OFN_EXPLORER|OFN_FILEMUSTEXIST|OFN_PATHMUSTEXIST| OFN_ALLOWMULTISELECT|OFN_NOCHANGEDIR
                                                                                   //���Windows����ʱ��ʼ����ѡ�е�ͼƬ
        if (WindowDll.GetOpenFileName(ofn))
        {
            Debug.Log("Selected file with full path: " + ofn.file);
            Utils.setDebugMode(true);

            Mat read = Imgcodecs.imread(Utils.getFilePath(ofn.file), Imgcodecs.IMREAD_UNCHANGED);
            Core.flip(read, read, 0);
            Core.flip(read, read, 1);
            Imgproc.cvtColor(read, read, Imgproc.COLOR_RGBA2BGRA);

            return read;
        }
        return null;
    }

    public void SavePic()
    {
        OpenFileName ofn = new OpenFileName();
        ofn.structSize = Marshal.SizeOf(ofn);
        ofn.filter = "ͼƬ�ļ�(*.jpg*.png)\0*.jpg;*.png";
        ofn.file = new string(new char[256]);
        ofn.maxFile = ofn.file.Length;
        ofn.fileTitle = new string(new char[64]);
        ofn.maxFileTitle = ofn.fileTitle.Length;
        //Ĭ��·��
        string path = Application.streamingAssetsPath;
        path = path.Replace('/', '\\');
        //Ĭ��·��
        //ofn.initialDir = "G:\\wenshuxin\\test\\HuntingGame_Test\\Assets\\StreamingAssets";
        ofn.initialDir = path;
        ofn.title = "Open Project";
        ofn.defExt = "JPG";//��ʾ�ļ�������
                           //ע�� һ����Ŀ��һ��Ҫȫѡ ����0x00000008�Ҫȱ��
        ofn.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000200 | 0x00000008;//OFN_EXPLORER|OFN_FILEMUSTEXIST|OFN_PATHMUSTEXIST| OFN_ALLOWMULTISELECT|OFN_NOCHANGEDIR
                                                                                   //���Windows����ʱ��ʼ����ѡ�е�ͼƬ
        if (WindowDll.GetSaveFileName(ofn))
        {
            CancelOption();
            
            Mat toSave = savedMat.clone();
            Core.flip(toSave, toSave, 0);
            Imgproc.cvtColor(toSave, toSave, Imgproc.COLOR_BGRA2RGBA);

            Imgcodecs.imwrite(ofn.file, toSave);
        }
    }

    void Show(Mat toShow)
    {
        if (null == toShow || toShow.empty()) return;
        _curMat = toShow;
        Texture2D t2d = new Texture2D(_curMat.cols(), _curMat.rows(), TextureFormat.RGBA32, false);

        Utils.matToTexture2D(_curMat, t2d);
        image.texture = t2d;
        image.SetNativeSize();
        matSize = _curMat.size();
        oriMat = _curMat.clone();
        savedMat = _curMat.clone();

        Utils.setDebugMode(false);
    }

    public void SaveOption()
    {
        savedMat = curMat.clone();
        matSize = _curMat.size();
    }

    public void CancelOption()
    {
        curMat = savedMat.clone();
    }
}

/// <summary>
/// ����ϵͳ�Ĵ���,���ݽ�����
/// </summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
public class OpenFileName
{
    public int structSize = 0;
    public IntPtr dlgOwner = IntPtr.Zero;
    public IntPtr instance = IntPtr.Zero;
    public String filter = null;
    public String customFilter = null;
    public int maxCustFilter = 0;
    public int filterIndex = 0;
    public String file = null;
    public int maxFile = 0;
    public String fileTitle = null;
    public int maxFileTitle = 0;
    public String initialDir = null;
    public String title = null;
    public int flags = 0;
    public short fileOffset = 0;
    public short fileExtension = 0;
    public String defExt = null;
    public IntPtr custData = IntPtr.Zero;
    public IntPtr hook = IntPtr.Zero;
    public String templateName = null;
    public IntPtr reservedPtr = IntPtr.Zero;
    public int reservedInt = 0;
    public int flagsEx = 0;
}

public class WindowDll
{
    [DllImport("Comdlg32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
    public static extern bool GetOpenFileName([In, Out] OpenFileName ofn);
    [DllImport("Comdlg32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
    public static extern bool GetSaveFileName([In, Out] OpenFileName ofd);
}
