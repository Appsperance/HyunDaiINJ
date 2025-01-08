using System;
using System.Collections.Generic;
using Npgsql; // PostgreSQL용 Npgsql 라이브러리
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace HyunDaiINJ.Views.Monitoring.Controls.ThirdSection.Vision
{
    public partial class NgCard : UserControl
    {
        public NgCard()
        {
            InitializeComponent();
            LoadImagesFromDatabase();
        }

        // DB 연결 및 이미지 경로 불러오기
        private void LoadImagesFromDatabase()
        {
            // PostgreSQL 연결 문자열
            string connectionString = "Host=localhost;Port=5432;Username=root;Password=vaporcloud;Database=msd_db";

            // DB에서 이미지 경로를 저장할 리스트
            List<string> imagePaths = new List<string>();

            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    // SQL 쿼리
                    string query = "SELECT ng_img_path FROM vision_ng";

                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string imagePath = reader["ng_img_path"].ToString();
                            imagePaths.Add(imagePath);
                        }
                    }
                }

                // ListView에 바인딩
                ImageListView.ItemsSource = imagePaths;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"DB에서 이미지를 불러오는 중 오류 발생: {ex.Message}");
            }
        }

        // 선택된 이미지 표시
        private void ImageListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ImageListView.SelectedItem is string selectedImagePath)
            {
                try
                {
                    // 선택된 이미지를 상단 영역에 표시
                    SelectedImage.Source = new BitmapImage(new Uri(selectedImagePath));
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"이미지를 불러오는 중 오류 발생: {ex.Message}");
                }
            }
        }
    }
}
