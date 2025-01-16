using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace HyunDaiINJ.Services
{
    public static class Base64ImageHelper
    {
        /// <summary>
        /// Base64 문자열을 받아 BitmapImage 객체를 반환합니다.
        /// </summary>
        public static BitmapImage ConvertBase64ToBitmapImage(string base64String)
        {
            if (string.IsNullOrEmpty(base64String))
                return null;

            try
            {
                byte[] imageBytes = Convert.FromBase64String(base64String);
                using (var ms = new MemoryStream(imageBytes))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = ms;
                    bitmap.EndInit();
                    bitmap.Freeze();   // UI 스레드 이슈 방지
                    return bitmap;
                }
            }
            catch
            {
                // Base64가 잘못되어 있거나, 디코딩 실패 시 등
                return null;
            }
        }
    }
}
