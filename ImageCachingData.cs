using System;
using UnityEngine;
using UnityEngine.UI;

public enum ImageDownloadType
{
    ForcedReflush,
    Caching,
    CoolTime,
}

public enum CachingType
{
    Star,
    Profile,
    ChatRoom,
    DM,
    StreamingList,
}

public delegate void ImageDownloadCallback(bool isSuccess, string key, Texture2D resultTexture = null);

public class ImageCachingData
{
    public Texture2D texture { get; private set; } = null;
    public DateTime? lastReflushTime { get; private set; } = null;
    public string URL { get; private set; } = string.Empty;
    public int reflushCoolTime { get; private set; } = 5;

    public ImageCachingData(string in_url)
    {
        URL = in_url;
    }

    public void SetTexture(Texture2D in_texture)
    {
        texture = in_texture;
        lastReflushTime = DateTime.Now;
    }

    public void SetCooltime(int in_minute)
    {
        reflushCoolTime = in_minute;
    }
}

public class ImageDownloadData
{
    public ImageDownloadCallback callback { get; private set; } = null;
    public ImageDownloadType downloadType { get; private set; } = ImageDownloadType.Caching;
    public RawImage rawImage { get; private set; } = null;
    public string URL { get; private set; } = string.Empty;
    public bool isCancel = false;

    public ImageDownloadData(string in_URL, RawImage in_rawImage, ImageDownloadType in_type = ImageDownloadType.Caching, ImageDownloadCallback in_callback = null)
    {
        URL = in_URL;
        rawImage = in_rawImage;
        downloadType = in_type;
        callback = in_callback;
    }
}
