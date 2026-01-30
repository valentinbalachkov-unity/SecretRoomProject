using System;
using Firebase.Extensions;
using Firebase.Storage;
using UnityEngine;

namespace _LocalMemeProj.ImageUploadService
{
    public class ImageUploadService : IImageUploadService
    {
        private FirebaseManager _firebaseManager;
        
        public ImageUploadService(FirebaseManager firebaseManager)
        {
            _firebaseManager = firebaseManager;
        }
        
        public void UploadImage(byte[] imageBytes, string lobbyId, string imageId, Action<string> onSuccess)
        {
            // Генерируем уникальное имя файла
            string fileName = $"img_{imageId}.jpg";
            // Путь: images/ID_лобби/имя_файла
            StorageReference uploadRef = _firebaseManager.Storage.GetReference($"images/{lobbyId}/{fileName}");

            // Метаданные (полезно для фильтрации)
            var newMetadata = new MetadataChange { ContentType = "image/jpeg" };

            // Загрузка
            uploadRef.PutBytesAsync(imageBytes, newMetadata).ContinueWithOnMainThread(task => {
                if (task.IsFaulted || task.IsCanceled) {
                    Debug.LogError("Upload failed: " + task.Exception);
                    return;
                }

                // После успешной загрузки получаем публичную ссылку (Download URL)
                uploadRef.GetDownloadUrlAsync().ContinueWithOnMainThread(urlTask => {
                    if (!urlTask.IsFaulted && !urlTask.IsCanceled) {
                        string downloadUrl = urlTask.Result.ToString();
                        Debug.Log("Upload complete! URL: " + downloadUrl);
                
                        // ВОЗВРАЩАЕМ ССЫЛКУ в Callback
                        onSuccess?.Invoke(downloadUrl); 
                    }
                });
            });
        }
    }
}