using System.Collections;
using SFB;
using UniRx;
using UnityEngine;

public class FileOpener : MonoBehaviour
{
    public ReactiveProperty<Texture2D> OnTextureLoad = new();
    
    private ExtensionFilter[] _extensionFilters = {
        new ExtensionFilter("Image Files", "png", "jpg", "jpeg" )
    };
    public void OpenFile()
    {
        var paths = StandaloneFileBrowser.OpenFilePanel("Title", "", _extensionFilters, false);
        if (paths.Length > 0) {
            StartCoroutine(OutputRoutine(new System.Uri(paths[0]).AbsoluteUri));
        }
    }
    
    private IEnumerator OutputRoutine(string url) {
        var loader = new WWW(url);
        yield return loader;
        
        var t = loader.texture;

        if (t.width > 2000 || t.height > 2000)
        {
            t = CompressTexture(loader.texture, 0.3f);
        }
        else if(t.width > 1000 || t.height > 1000)
        {
            t = CompressTexture(loader.texture, 0.6f);
        }
        else if(t.width > 680 || t.height > 680)
        {
            t = CompressTexture(loader.texture, 0.8f);
        }
        
        OnTextureLoad.Value = t;
    }
    
    private Texture2D CompressTexture(Texture2D texture, float scale)
    {
        int newWidth = Mathf.Max(1, Mathf.RoundToInt(texture.width * scale));
        int newHeight = Mathf.Max(1, Mathf.RoundToInt(texture.height * scale));
        
        Texture2D resizedTexture = new Texture2D(newWidth, newHeight, texture.format, texture.mipmapCount > 0);
        Color[] pixels = texture.GetPixels();
        Color[] resizedPixels = new Color[newWidth * newHeight];

        for (int y = 0; y < newHeight; y++)
        {
            for (int x = 0; x < newWidth; x++)
            {
                int originalX = Mathf.FloorToInt(x / scale);
                int originalY = Mathf.FloorToInt(y / scale);
                resizedPixels[y * newWidth + x] = pixels[originalY * texture.width + originalX];
            }
        }

        resizedTexture.SetPixels(resizedPixels);
        resizedTexture.Apply();

        return resizedTexture;
    }
}
