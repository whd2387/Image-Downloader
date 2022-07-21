# Image-Downloader

## 작성 이유
> 이미지를 다운로드하여 처리하는 곳이 많고, 기존의 이미지 다운로드 및 캐싱처리 방식으로는 부족한 부분이 있었습니다.   
> 부족했던 부분인 이미지 캐싱 처리, 이미지 다운로드 취소 처리, 갱신 쿨타임 처리, 캐싱 데이터 관리 등을 추가하여 보완하였습니다.

## 작동 과정
1. ImageDownloadType에 따라 Caching 데이터 체크, CoolTime 시간 체크 후 조건이 맞으면 Caching 진행.
2. Caching 조건이 아니라면 다운로드 대기중인 목록에 추가.
3. 해당 URL이 다운로드 중이 아니면 다운로드 시작.
4. 다운로드 중 취소 검사.
5. 다운로드 완료 후, 대기중인 목록 중 해당 url이 같으면 CallBack 전송 후 대기중인 목록에서 삭제, 캐싱 데이터에 추가.

## Code 설명
* GameInstance.cs에 ImageDownloadController를 인스턴스화 시켰습니다.    
``` C#
  //GameInstance.cs
  ...
  public ImageDownloadController imageDownloadController { get; } = new ImageDownloadController();
  ...

```
<br><br />
* URL, RawImage, Type(Optional), CallBack(Optional) 를 넘겨주며 이미지 다운로드를 시작하는 메서드 입니다.    
``` C#
  //ImageDownloadController.cs
  public void DownloadImage(string in_URL, RawImage in_rawImage, ImageDownloadType in_type = ImageDownloadType.Caching, ImageDownloadCallback in_callback = null)
  {
  ...
  }


  //Usage Example
  GameInstance.Instance.imageDownloadController.DownloadImage(URL, RawImage);
  GameInstance.Instance.imageDownloadController.DownloadImage(URL, RawImage, ImageDownloadType.CoolTime);
  GameInstance.Instance.imageDownloadController.DownloadImage(URL, RawImage, ImageDownloadType.Caching, Callback);        
  public void Callback(bool isSuccess, string key, Texture2D resultTexture = null)
  {
      if (isSuccess)
      {
          Debug.Log("Image Download Success");
      }
      else
      {
          Debug.Log("Image Download Failed");
      }
  }

```
<br><br />
* 다운로드 취소 메서드 입니다.
```C#
// 진행중인 특정 URL의 다운로드를 취소하는 함수
public void CancelImageDownload(string url){ ... }

// 진행중인 특정 RawImage의 다운로드를 취소하는 함수
public void CancelImageDownload(RawImage in_rawImage){ ... }

// 진행중인 전체 다운로드를 취소하는 함수
public void CancelAllImageDownload(){ ... }
```

<br><br />
* 캐싱된 데이터 관리 메서드 입니다.
```C#
// 해당 URL의 캐싱 데이터를 삭제
public void RemoveImageData(string url){ ... }

// 캐싱 데이터 모두 삭제
public void RemoveAllImageData(){ ... }
```
