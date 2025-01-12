using System;
using System.Collections.Generic;
using Npgsql;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace HyunDaiINJ.Views.Monitoring.Controls.Vision
{
    public partial class NgCard : UserControl
    {
        private const int pageSize = 2;   // 한 번에 가져올 개수
        private int currentOffset = 0;     // 현재까지 불러온 개수(=OFFSET)
        private bool hasMoreData = true;   // 더 가져올 데이터가 있는지 여부

        private List<string> imagePaths = new List<string>();

        public NgCard()
        {
            InitializeComponent();
            // 초기에 첫 페이지 로드
            LoadNextPage();
        }

        // (1) 다음 페이지 로드
        private void LoadNextPage()
        {
            if (!hasMoreData)
            {
                MessageBox.Show("더 이상 데이터가 없습니다.");
                return;
            }

            string connectionString = "Host=localhost;Port=5432;Username=root;Password=vaporcloud;Database=msd_db";
            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"
                        SELECT ng_img_path 
                          FROM vision_ng
                         ORDER BY id
                         LIMIT @Limit OFFSET @Offset;
                    ";

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("Limit", pageSize);
                        command.Parameters.AddWithValue("Offset", currentOffset);

                        using (var reader = command.ExecuteReader())
                        {
                            int rowCount = 0;
                            while (reader.Read())
                            {
                                string path = reader["ng_img_path"].ToString();
                                imagePaths.Add(path);
                                rowCount++;
                            }
                            // 만약 rowCount < pageSize면 더 이상 불러올 데이터가 없음
                            if (rowCount < pageSize)
                            {
                                hasMoreData = false;
                            }
                            // 다음번 OFFSET 업데이트
                            currentOffset += rowCount;
                        }
                    }
                }

                // ListView에 바인딩 새로고침
                ImageListView.ItemsSource = null; // or CollectionView.Refresh()
                ImageListView.ItemsSource = imagePaths;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"DB에서 이미지를 불러오는 중 오류: {ex.Message}");
            }
        }

        // (2) ListView 항목 선택 시 -> 상단 영역에 이미지 표시
        private void ImageListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ImageListView.SelectedItem is string selectedImagePath)
            {
                try
                {
                    SelectedImage.Source = new BitmapImage(new Uri(selectedImagePath));
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"이미지 로드 오류: {ex.Message}");
                }
            }
        }

        // (3) "더보기" 버튼 클릭 시 -> 다음 페이지 로드
        private void BtnLoadMore_Click(object sender, RoutedEventArgs e)
        {
            LoadNextPage();
        }
    }
}
