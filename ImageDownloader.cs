using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace stanworld
{
    private Action<bool> callback = null;
    private ImageCachingData downloadData = null;
    private string url;
    private bool isCancel = false;

    public ImageDownloader(string in_url, Action<bool> in_callback, ImageCachingData in_downloadData)
    {
        url = in_url;
        callback = in_callback;
        downloadData = in_downloadData;
    }

    public async void DoDownloadImage()
    {
        UnityWebRequest req = UnityWebRequestTexture.GetTexture(url);
        UnityWebRequestAsyncOperation operation = req.SendWebRequest();

        while (!operation.isDone)
        {
            if (isCancel)
            {
                req.Abort();
                DoDownloadFail();
                return;
            }
            await Task.Delay(10);
        }

        if (true == isCancel)
        {
            DoDownloadFail();
            return;
        }

        switch (req.result)
        {
            case UnityWebRequest.Result.Success:
                DoDownloadSuccess(req);
                break;
            default:
                Utils.Logger.Log(req.error);
                DoDownloadFail();
                break;
        }
    }

    private void DoDownloadSuccess(UnityWebRequest request)
    {
        Texture2D texture = DownloadHandlerTexture.GetContent(request);
        if (null != texture)
        {
            // 성공
            downloadData.SetTexture(texture);
            callback?.Invoke(true);
        }
        else
        {
            callback?.Invoke(false);
        }
    }

    private void DoDownloadFail()
    {
        callback?.Invoke(false);
    }

    public void CancelDownload()
    {
        isCancel = true;
    }
}
