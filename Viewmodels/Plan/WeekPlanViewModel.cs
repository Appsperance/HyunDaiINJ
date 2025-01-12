using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Text;             // Encoding
using MyApp.Models.Plan;       // WeekRow
using HyunDaiINJ.Services;     // HandleTcpSocket, AsyncRelayCommand

namespace MyApp.ViewModels.Plan
{
    public class WeekPlanViewModel : INotifyPropertyChanged
    {
        // ---------- (1) 사용자 입력 PartId (헤더에 표시) ----------
        private string _partIdFirst;
        public string PartIdFirst
        {
            get => _partIdFirst;
            set
            {
                if (_partIdFirst != value)
                {
                    _partIdFirst = value;
                    OnPropertyChanged(nameof(PartIdFirst));
                }
            }
        }

        private string _partIdSecond;
        public string PartIdSecond
        {
            get => _partIdSecond;
            set
            {
                if (_partIdSecond != value)
                {
                    _partIdSecond = value;
                    OnPropertyChanged(nameof(PartIdSecond));
                }
            }
        }

        private string _partIdThird;
        public string PartIdThird
        {
            get => _partIdThird;
            set
            {
                if (_partIdThird != value)
                {
                    _partIdThird = value;
                    OnPropertyChanged(nameof(PartIdThird));
                }
            }
        }

        private string _partIdFourth;
        public string PartIdFourth
        {
            get => _partIdFourth;
            set
            {
                if (_partIdFourth != value)
                {
                    _partIdFourth = value;
                    OnPropertyChanged(nameof(PartIdFourth));
                }
            }
        }

        // ---------- (2) 52행 (Week=1..52, 각 행마다 Part1~4 수량) ----------
        public ObservableCollection<WeekRow> WeekPlanRows { get; }

        // ---------- (3) 합계를 표시할 속성: Part1/2/3/4 전체 합 ----------
        public int SumPart1 => WeekPlanRows.Sum(r => r.QuanPart1);
        public int SumPart2 => WeekPlanRows.Sum(r => r.QuanPart2);
        public int SumPart3 => WeekPlanRows.Sum(r => r.QuanPart3);
        public int SumPart4 => WeekPlanRows.Sum(r => r.QuanPart4);

        // ---------- (4) 저장 커맨드 (TCP 전송) ----------
        public ICommand SaveAllInsertCommand { get; }

        public WeekPlanViewModel()
        {
            // 초기 PartId
            _partIdFirst = "Part-A";
            _partIdSecond = "Part-B";
            _partIdThird = "Part-C";
            _partIdFourth = "Part-D";

            // 52행 생성
            WeekPlanRows = new ObservableCollection<WeekRow>();
            for (int w = 1; w <= 52; w++)
            {
                var row = new WeekRow(w);

                // 수량 바뀔 때 -> SumPartX 다시 계산
                row.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(WeekRow.QuanPart1) ||
                        e.PropertyName == nameof(WeekRow.QuanPart2) ||
                        e.PropertyName == nameof(WeekRow.QuanPart3) ||
                        e.PropertyName == nameof(WeekRow.QuanPart4))
                    {
                        OnPropertyChanged(nameof(SumPart1));
                        OnPropertyChanged(nameof(SumPart2));
                        OnPropertyChanged(nameof(SumPart3));
                        OnPropertyChanged(nameof(SumPart4));
                    }
                };
                WeekPlanRows.Add(row);
            }

            // 저장(Command)
            SaveAllInsertCommand = new AsyncRelayCommand(SaveAllAsync);
        }

        /// <summary>
        /// 저장(전송) 메서드:
        ///  PartId별, Week별, Quan값을 바탕으로 "Header(4)+Payload(28×N)" 바이너리 생성 후 TCP 전송
        /// </summary>
        private async Task SaveAllAsync()
        {
            try
            {
                // ---------- (A) Payload(28바이트×N) 만들기 ----------
                var recordList = new System.Collections.Generic.List<byte>();

                // WeekRow X 52개 순회
                //  Part1..4 가 각각 28바이트 레코드 1개씩
                foreach (var row in WeekPlanRows)
                {
                    // row.QuanPart1 => PartIdFirst
                    if (!string.IsNullOrEmpty(PartIdFirst))
                    {
                        byte[] oneRec = BuildRecord28Bytes(PartIdFirst, row.Week, row.QuanPart1);
                        recordList.AddRange(oneRec);
                    }

                    // row.QuanPart2 => PartIdSecond
                    if (!string.IsNullOrEmpty(PartIdSecond))
                    {
                        byte[] oneRec = BuildRecord28Bytes(PartIdSecond, row.Week, row.QuanPart2);
                        recordList.AddRange(oneRec);
                    }

                    // row.QuanPart3 => PartIdThird
                    if (!string.IsNullOrEmpty(PartIdThird))
                    {
                        byte[] oneRec = BuildRecord28Bytes(PartIdThird, row.Week, row.QuanPart3);
                        recordList.AddRange(oneRec);
                    }

                    // row.QuanPart4 => PartIdFourth
                    if (!string.IsNullOrEmpty(PartIdFourth))
                    {
                        byte[] oneRec = BuildRecord28Bytes(PartIdFourth, row.Week, row.QuanPart4);
                        recordList.AddRange(oneRec);
                    }
                }

                int payloadSize = recordList.Count;  // (28×N)

                // 여기선 byte(1)로 처리, 255 초과시 예외
                if (payloadSize > 255)
                {
                    throw new Exception($"Payload({payloadSize} bytes)가 255를 초과합니다. 실제 코드에서는 2바이트 이상 사용 권장.");
                }

                // ---------- (B) Header(4바이트) ----------
                //    [0]=MessageLength, [1]=Version, [2]=Role, [3]=Reserved
                //    예: Length=payloadSize, Version=1, Role=0(생산)
                byte[] header = new byte[4];
                header[0] = (byte)payloadSize; // 최대 255
                header[1] = 1;                 // Version=1
                header[2] = 0;                 // Role=0
                header[3] = 0;                 // Reserved=0

                // ---------- (C) 최종 패킷 ----------
                byte[] finalPacket = new byte[4 + payloadSize];
                Array.Copy(header, 0, finalPacket, 0, 4);
                Array.Copy(recordList.ToArray(), 0, finalPacket, 4, payloadSize);

                // ---------- (D) TCP 전송 ----------
                var socket = new HandleTcpSocket();
                // 새로 만든 "바이너리 전송" 메서드를 사용(예시)
                // 여기서는 SendMultipleAsync(byte[] rawPacket) 식으로 만든다고 가정
                await socket.SendMultipleAsync(finalPacket);

                int recordCount = payloadSize / 28; // 레코드 개수
                MessageBox.Show($"총 {recordCount}건 전송 완료 (총 {payloadSize} bytes).");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"전송 오류: {ex.Message}");
            }
        }

        /// <summary>
        /// 하나의 28바이트 레코드를 생성 
        /// [PartId(4) + Time(8) + Week(4) + DailyQuan(4) + Days(4) + WeekQuan(4)]
        /// </summary>
        private byte[] BuildRecord28Bytes(string partId, int week, int weekQuan)
        {
            byte[] record = new byte[28];

            // (1) PartId(4) - space padding
            string p = (partId ?? "").PadRight(4).Substring(0, 4);
            Array.Copy(Encoding.ASCII.GetBytes(p), 0, record, 0, 4);

            // (2) Time(8) - PG Epoch(2000-01-01) 기준 microsec
            long microSec = ToPgEpochMicroseconds(DateTime.Now);
            Array.Copy(BitConverter.GetBytes(microSec), 0, record, 4, 8);

            // (3) Week(4)
            Array.Copy(BitConverter.GetBytes(week), 0, record, 12, 4);

            // (4) DailyQuan(4) → 여기선 0
            int dailyQ = 0;
            Array.Copy(BitConverter.GetBytes(dailyQ), 0, record, 16, 4);

            // (5) Days(4) → 공백
            Array.Copy(Encoding.ASCII.GetBytes("    "), 0, record, 20, 4);

            // (6) WeekQuan(4) → row.Quan
            Array.Copy(BitConverter.GetBytes(weekQuan), 0, record, 24, 4);

            return record;
        }

        /// <summary>
        /// PostgreSQL Epoch(2000-01-01) 기준 microseconds 변환
        /// </summary>
        private long ToPgEpochMicroseconds(DateTime dt)
        {
            // PG epoch = 2000-01-01 UTC
            DateTime pgEpoch = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime dtUtc = dt.ToUniversalTime();
            long ticks = dtUtc.Ticks - pgEpoch.Ticks; // 1 tick=100ns
            long microSec = ticks / 10;               // 1 microsec=10 ticks
            return microSec;
        }

        // INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
