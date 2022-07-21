using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace stanworld
{
    namespace Utils
    {
        using System.Text;
        public class PicCacheManager : Singleton<PicCacheManager>
        {
            public struct StarPhotoInfo
            {
                public StarPhotoInfo(string in_photoURL, DateTime in_dateTime, Texture2D in_texture)
                {
                    photoURL = in_photoURL;
                    lastReflushDate = in_dateTime;
                    cacheTexture = in_texture;
                }

                public string photoURL;
                public DateTime lastReflushDate;
                public Texture2D cacheTexture;
            }

            protected override void Initialized()
            {
                
            }
            Dictionary<string, Texture2D> PicCacheList;
            /// <summary>
            /// Key : StarName, Value : PhotoInfo
            /// </summary>
            Dictionary<string, StarPhotoInfo> starPhotoCacheList;
            int starPhotoCoolTimeMin = 5;
            public void Init()
            {
                PicCacheList = new Dictionary<string, Texture2D>();
                starPhotoCacheList = new Dictionary<string, StarPhotoInfo>();
            }

            public bool ExistCacheImage(string InUrl)
            {
                bool bFind = false;
                if (InUrl == null)
                    return bFind;
                if (!PicCacheList.ContainsKey(InUrl))
                    bFind = IsExistCacheImageFile(InUrl);
                else
                    bFind = true;
                return bFind;
                //bFind = PicCacheList.ContainsKey(InUrl) || IsExistCacheImageFile(InUrl);
                //return bFind;
            }

            public Texture2D GetCacheImage(string InURL)
            {
                if(PicCacheList.ContainsKey(InURL))
                    return PicCacheList[InURL];
                string convertName = EncodingBase64(InURL);
                string imagePath = System.IO.Path.Combine(Application.persistentDataPath, convertName);
                byte[] fileData = System.IO.File.ReadAllBytes(imagePath);
                Texture2D tex = new Texture2D(2, 2);
                tex.LoadImage(fileData);
                PicCacheList.Add(InURL, tex);
                return PicCacheList[InURL];
            }

            public void RemoveCacheImage(string InUrl)
            {
                PicCacheList.Remove(InUrl);
            }

            public bool AddCacheImage(string InUrl, Texture2D inImage, bool bReflush = false)
            {
                if (ExistCacheImage(InUrl))
                {
                    if( bReflush == true)
                    {
                        RemoveCacheImage(InUrl);
                    }
                    else
                    {
                        return false;
                    }
                }
                SaveTexture(InUrl, inImage);
                PicCacheList.Add(InUrl, inImage);
                return true;
            }

            public void ClearCachePic()
            {
                foreach(var info in PicCacheList)
                {
                    UnityEngine.Object.Destroy(info.Value);
                }
                PicCacheList.Clear();
            }

            private bool IsExistCacheImageFile(string InURL)
            {
                string convertName = EncodingBase64(InURL);
                string imagePath = System.IO.Path.Combine(Application.persistentDataPath, convertName);
                //Debug.Log(imagePath);
                //Utils.Logger.Log(imagePath, Utils.LogLevel.INFO);
                return System.IO.File.Exists(imagePath);
            }

            private void SaveTexture(string InURL, Texture2D texture)
            {
                byte[] bytes = texture.EncodeToPNG();
                string convertName = EncodingBase64(InURL);
                string imagePath = System.IO.Path.Combine(Application.persistentDataPath, convertName);
                System.IO.File.WriteAllBytes(imagePath, bytes);
#if UNITY_EDITOR
                UnityEditor.AssetDatabase.Refresh();
#endif
            }

            public static string EncodingBase64(string plainText)
            {
                byte[] strByte = Encoding.UTF8.GetBytes(plainText);
                return System.Convert.ToBase64String(strByte);
            }

            //public static string DecodingBase64(string base64PlainText)
            //{
            //    Byte[] strByte = Convert.FromBase64String(base64PlainText);
            //    return Encoding.UTF8.GetString(strByte);
            //}

            public void UpdateStarCacheImage(string in_starName, string in_photoURL, DateTime in_dateTime, Texture2D in_Texture)
            {
                StarPhotoInfo starPhotoInfo = new StarPhotoInfo(in_photoURL, in_dateTime, in_Texture);

                if (starPhotoCacheList.ContainsKey(in_starName))
                    starPhotoCacheList.Remove(in_starName);

                starPhotoCacheList.Add(in_starName, starPhotoInfo);
            }

            public bool HasStarPhotoKey(string in_starName)
            {
                if (starPhotoCacheList.ContainsKey(in_starName)) return true;
                else return false;
            }

            public bool IsStarPhotoOverCoolTime(string in_starName)
            {
                DateTime currentTime = DateTime.Now;
                TimeSpan resultTime = currentTime.Subtract(starPhotoCacheList[in_starName].lastReflushDate);

                if (resultTime.Minutes > starPhotoCoolTimeMin)
                    return true;
                else
                    return false;
            }

            public Texture2D GetStarCacheImage(string in_starName)
            {
                if(starPhotoCacheList.ContainsKey(in_starName))
                {
                    return starPhotoCacheList[in_starName].cacheTexture;
                }
                else
                {
                    return null;
                }
            }
        }
    }    
}
