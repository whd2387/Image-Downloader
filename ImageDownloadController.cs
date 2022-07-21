using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageDownloadController
{
    //---------------------------------------------------------------------------
    // 다운로드 대기, 다운로드 중, 완료의 3가지 상태로 데이터를 관리합니다.
    // 1. 다운로드 대기 -> 캐싱 및 쿨타임 확인
    // 2. 다운로드 중   -> 다운로드 진행 및 취소 체크
    // 3. 완료          -> 완료된 데이터 보관
    //---------------------------------------------------------------------------

    /// <summary>
    /// 다운받은 이미지들을 저장하고 있는 데이터
    ///_ Key : Value = URL : Data
    /// </summary>
    private Dictionary<string, ImageCachingData> ImageCachingDatas = new Dictionary<string, ImageCachingData>();
    /// <summary>
    /// 다운로드 대기중인 데이터 목록
    /// </summary>
    private List<ImageDownloadData> downloadDatas = new List<ImageDownloadData>();
    /// <summary>
    /// 다운로드 중인 ImageDownloader Dictionary
    /// </summary>
    private Dictionary<string, ImageDownloader> imageDownloaders = new Dictionary<string, ImageDownloader>();

    public void DownloadImage(string in_URL, RawImage in_rawImage, ImageDownloadType in_type = ImageDownloadType.Caching, ImageDownloadCallback in_callback = null)
    {
        if (string.IsNullOrEmpty(in_URL))
        {
            Utils.Logger.Log("Download URL is Null !", Utils.LogLevel.ERROR);
            return;
        }

        if (null == in_rawImage)
        {
            Utils.Logger.Log("RawImage URL is Null !", Utils.LogLevel.ERROR);
            return;
        }

        CancelImageDownload(in_rawImage);

        ImageDownloadData downloadData = new ImageDownloadData(in_URL, in_rawImage, in_type, in_callback);
        // 캐싱 및 쿨타임 확인
        if (true == DoCheckCachingOrCoolTime(downloadData))
            return;

        // Download 데이터 추가
        DoAddDownloadData(downloadData);
        // 이미 다운로드 중이면 Download 데이터만 추가한 후 리턴
        if (imageDownloaders.ContainsKey(downloadData.URL))
            return;
        // 해당 url 기본 데이터 추가
        DoAddDownloadData(downloadData.URL);
        // 새로 다운로드를 시작
        DoStartDownload(downloadData);
    }

    /// ImageDownloader에서 비동기로 이미지 다운로드 시작 메서드
    private void DoStartDownload(ImageDownloadData downloadData)
    {
        ImageDownloader imageDownloader = new ImageDownloader(downloadData.URL, (result) => { DownloadCompleteCallback(result, downloadData.URL); }, ImageCachingDatas[downloadData.URL]);
        imageDownloaders.Add(downloadData.URL, imageDownloader);
        imageDownloader.DoDownloadImage();
    }

    #region Public 데이터 처리 메서드
    /// <summary>
    /// 해당 URL의 캐싱 데이터를 삭제
    /// </summary>
    public void RemoveImageData(string url)
    {
        if (ImageCachingDatas.ContainsKey(url))
        {
            ImageCachingDatas.Remove(url);
            Utils.Logger.Log($"Remove Caching Data :: {url}");
        }
    }

    /// <summary>
    /// 캐싱 데이터 모두 삭제
    /// </summary>
    public void RemoveAllImageData()
    {
        ImageCachingDatas.Clear();
        Utils.Logger.Log("Remove All Caching Data");
    }

    /// <summary>
    /// 진행중인 특정 URL의 다운로드를 취소하는 함수
    /// </summary>
    public void CancelImageDownload(string url)
    {
        imageDownloaders[url].CancelDownload();
        Utils.Logger.Log($"Cancel Download:: {url}");
    }

    /// <summary>
    /// 진행중인 특정 RawImage의 다운로드를 취소하는 함수
    /// </summary>
    public void CancelImageDownload(RawImage in_rawImage)
    {
        if (in_rawImage == null)
        {
            Utils.Logger.Log($"RawImage is null!", Utils.LogLevel.ERROR);
            return;
        }
        foreach (var data in downloadDatas)
        {
            if (data.rawImage.Equals(in_rawImage))
            {
                if (imageDownloaders.ContainsKey(data.URL))
                {
                    data.isCancel = true;
                    Utils.Logger.Log($"Cancel Download:: {in_rawImage.name}");
                }
                else
                {
                    break;
                }
            }
        }
    }

    /// <summary>
    /// 진행중인 전체 다운로드를 취소하는 함수
    /// </summary>
    public void CancelAllImageDownload()
    {
        foreach (var pair in imageDownloaders)
        {
            pair.Value.CancelDownload();
        }
        Utils.Logger.Log($"Cancel All Download");
    }
    #endregion

    #region Private 데이터 처리 메서드
    /// <summary>
    /// 캐싱, 쿨타임 체크
    /// </summary>
    private bool DoCheckCachingOrCoolTime(ImageDownloadData downloadData)
    {
        switch (downloadData.downloadType)
        {
            case ImageDownloadType.Caching:
                if (true == DoReturnCachingData(downloadData))
                    return true;
                break;
            case ImageDownloadType.CoolTime:
                if (HasDatetime(downloadData.URL))
                {
                    DateTime? lastTime = ImageCachingDatas[downloadData.URL].lastReflushTime;
                    if (null != lastTime)
                    {
                        TimeSpan subTime = DateTime.Now.Subtract((DateTime)lastTime);
                        // 쿨타임을 넘지 않았으면 캐싱 후 리턴
                        if (subTime.TotalMinutes < ImageCachingDatas[downloadData.URL].reflushCoolTime)
                        {
                            if (true == DoReturnCachingData(downloadData))
                                return true;
                        }
                    }
                }
                break;
        }
        return false;
    }

    /// <summary>
    /// 다운로드 대기중인 목록에 데이터 추가
    /// </summary>
    private void DoAddDownloadData(ImageDownloadData downloadData)
    {
        foreach (ImageDownloadData data in downloadDatas)
        {
            if (data.rawImage.Equals(downloadData.rawImage))
            {
                downloadDatas.Remove(data);
                break;
            }
        }
        downloadDatas.Add(downloadData);
    }

    /// <summary>
    /// 다운로드 대기중인 목록에서 데이터 삭제
    /// </summary>
    private void DoRemoveDownloadData(ImageDownloadData DownloadData)
    {
        if (downloadDatas.Contains(DownloadData))
        {
            downloadDatas.Remove(DownloadData);
        }
    }

    /// <summary>
    /// 다운로드 대기중 목록에 해당 URL을 가진 데이터 모두 삭제
    /// </summary>
    private void DoRemoveDownloadData(string in_url)
    {
        List<int> indexs = new List<int>();
        for (int i = 0; i < downloadDatas.Count; i++)
        {
            if (downloadDatas[i].URL == in_url)
            {
                indexs.Add(i);
            }
        }

        for (int i = 1; i < indexs.Count + 1; i++)
        {
            downloadDatas.RemoveAt(indexs[indexs.Count - i]);
        }
    }

    /// <summary>
    /// 다운로드 데이터 목록에 데이터 추가
    /// </summary>
    private void DoAddDownloadData(string in_url)
    {
        if (ImageCachingDatas.ContainsKey(in_url))
        {
            return;
        }
        else
        {
            ImageCachingData imageDownloadData = new ImageCachingData(in_url);
            ImageCachingDatas.Add(in_url, imageDownloadData);
        }
    }
    #endregion

    /// <summary>
    /// 해당 데이터에 Datetime 유무 체크
    /// </summary>
    private bool HasDatetime(string in_url)
    {
        if (ImageCachingDatas.ContainsKey(in_url))
        {
            if (ImageCachingDatas[in_url].lastReflushTime != null) return true;
        }
        return false;
    }

    /// <summary>
    /// 캐싱 데이터 있을 시 캐싱처리하는 메서드
    /// </summary>
    private bool DoReturnCachingData(ImageDownloadData downloadData)
    {
        if (ImageCachingDatas.ContainsKey(downloadData.URL))
        {
            if (null != ImageCachingDatas[downloadData.URL].texture)
            {
                if (downloadData.callback != null)
                {
                    downloadData.callback(true, downloadData.URL, ImageCachingDatas[downloadData.URL].texture);
                }
                downloadData.rawImage.texture = ImageCachingDatas[downloadData.URL].texture;

                return true;
            }
        }
        return false;
    }

    // 콜백 함수
    private void DownloadCompleteCallback(bool result, string in_url)
    {
        if (imageDownloaders.ContainsKey(in_url))
            imageDownloaders.Remove(in_url);

        foreach (ImageDownloadData data in downloadDatas)
        {
            if (data.URL == in_url)
            {
                if (data.isCancel)
                {
                    if (null != data.callback)
                        data.callback(false, in_url);
                }
                else
                {
                    if (true == result)
                    {
                        if (data.rawImage != null)
                        {
                            data.rawImage.texture = ImageCachingDatas[in_url].texture;
                        }
                        if (null != data.callback)
                            data.callback(result, in_url, ImageCachingDatas[in_url].texture);
                    }
                    else
                    {
                        if (null != data.callback)
                            data.callback(result, in_url);
                    }
                }
                data.isCancel = false;
            }
        }
        DoRemoveDownloadData(in_url);
    }
}
