using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace HyunDaiINJ.Services
{
    /// <summary>
    /// Base64 → BitmapImage 변환을 담당하는 헬퍼 메서드 모음.
    /// </summary>
    public static class Base64ImageHelper
    {
        /// <summary>
        /// Base64 문자열을 받아 BitmapImage 객체로 만들어 반환합니다.
        /// </summary>
        public static BitmapImage ConvertBase64ToBitmapImage(string base64String)
        {
            // Base64가 비어있다면 null 반환
            if (string.IsNullOrEmpty(base64String))
                return null;

            try
            {
                // 1) Base64 → byte[]
                byte[] imageBytes = Convert.FromBase64String(base64String);

                // 2) MemoryStream을 통해 BitmapImage 만들기
                using (var ms = new MemoryStream(imageBytes))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    // OnLoad로 해야 스트림이 닫혀도 이미지를 사용할 수 있음
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = ms;
                    bitmap.EndInit();
                    // Freeze()로 UI 스레드 밖에서도 안전하게 사용 가능
                    bitmap.Freeze();
                    return bitmap;
                }
            }
            catch
            {
                // 디코딩 실패 시 null
                return null;
            }
        }
    }
}
