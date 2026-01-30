using System;
using _LocalMemeProj.ImageUploadService;
using Dreamers.UI.UIService.Interfaces;
using Fusion;
using UnityEngine;

public class SessionCreator : NetworkBehaviour
{
    private IImageUploadService _imageUploadService;
    private NetworkRunner _networkRunner;
    
    public SessionCreator(IImageUploadService imageUploadService)
    {
        _imageUploadService = imageUploadService;
    }
    
    public override void Spawned()
    {
        Debug.Log("Я появился в сети!");
    }

    public void CreateSession(LevelPackData levelPackData)
    {
        foreach (var imaData in levelPackData.Images)
        {
            imaData.id =  Guid.NewGuid().ToString();
            _imageUploadService.UploadImage(imaData.sprite.texture.EncodeToJPG(75), _networkRunner.SessionInfo.Name, imaData.id, OnGetURL);
        }
    }

    public void OnGetURL(string url)
    {
        
    }
    
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void Rpc_SendImageMessage(string downloadUrl, string senderName)
    {
        // Этот код сработает у всех игроков в лобби
        Debug.Log($"User {senderName} sent an image: {downloadUrl}");
        
        // Тут вызываем UI менеджер, который создаст "пузырь" сообщения
        // и начнет скачивание картинки
    }
}
