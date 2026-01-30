
using UnityEngine;
using UnityEngine.UI;
using RoseDev.tools.byteTools;
using RoseDev.tools.CryptoAES256;
using RoseDev.tools.ImageTools;
using RoseDev.tools.textTools;
using RoseDev.tools.zipper;


public class demo : MonoBehaviour
{

    #region byte tools
    [SerializeField] byte[] bigByteArray;
    [SerializeField] byte[][] buf;
    // for split big array you can use this method
    public void byteToolExample_splitByte()
    {
        buf = byteTools.BufferSplit(bigByteArray, 512);
        Debug.Log(buf.Length);
    }
    public void byteToolExample_MergeTowToOne()
    {
        bigByteArray = byteTools.MergeTowToOne(buf);
        Debug.Log(bigByteArray.Length);
    }

    #endregion
    #region crypto
    // for encrypt and decrypt any string use this methods 
    public void cryptoExmaple_Encrypt(string str)
    {
        string EncryptStr = AEScrypto.EncryptText(str, "123abc");
        Debug.Log(EncryptStr);
    }
    public void cryptoExmaple_Decrypt(string str)
    {
        string DecryptStr = AEScrypto.DecryptText(str, "123abc");
        Debug.Log(DecryptStr);
    }
    #endregion
    #region Image Serialize
    string SerializedImage;
    [SerializeField] Image img;
    public void ImageToolsExample_Serialize()
    {
        string url_ = "file path";
        Vector2 imageSize = new Vector2(256, 256);
        SerializedImage = imageConverter.Serialize(url_, imageSize);
    }
    public void ImageToolsExample_Deserialize()
    {
        Vector2 imageSize = new Vector2(256, 256);
        img.sprite = imageConverter.DeSerialize(SerializedImage, imageSize);
    }
    #endregion
    #region text tool
    public void convertBigNumbersTo_K_M()
    {
        string str = textTool.numberFormat(10000000);
        Debug.Log(str); // print : 1M 
    }
    #endregion
    #region zipper
 
    public void CompressStringToByte_DecompressByteToString()
    {
        string str = "test mess";

        byte[] b = zipper.CompressStringToByte(str);
        Debug.Log( b.Length);
   
        str = zipper.DecompressByteToString (b);
        Debug.Log(str);
    }
    #endregion
}
