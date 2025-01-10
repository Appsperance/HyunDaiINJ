using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HyunDaiINJ.DATA.DAO;
using HyunDaiINJ.DATA.DTO;

namespace HyunDaiINJ.Models.Monitoring.ThirdSection
{
    public class VisionNgModel
    {
        private readonly VisionNgDAO visionNgDAO;
        public VisionNgModel()
        {
            try
            {
                visionNgDAO = new VisionNgDAO();
                Console.WriteLine("VisionNgModel : 연결 성공");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"VisionNgModel {ex.Message}");
                throw;
            }
        }

        public List<VisionNgDTO> GetVisionNgData()
        {
            try
            {
                var data = visionNgDAO.GetVisionNgData();
                Console.WriteLine($"VisionNgModel : GetVisionNgData {data.Count}");
                return data;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"VisionNgModel GetVisionNgData: {ex.Message}");
                throw;
            }
        }

        public List<VisionNgDTO> GetVisionNgDataAll()
        {
            try
            {
                var data = visionNgDAO.GetVisionNgDataAll();
                Console.WriteLine($"VisionNgModel : GetVisionNgDataAll {data.Count}");
                return data;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"VisionNgModel GetVisionNgDataAll: {ex.Message}");
                throw;
            }
        }

        public List<VisionNgDTO> GetVisionNgDataWeek()
        {
            try
            {
                var data = visionNgDAO.GetVisionNgDataWeek();
                Console.WriteLine($"VisionNgModel : GetVisionNgDataWeek {data.Count}");
                return data;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving data from VisionNgDAO: {ex.Message}");
                throw;
            }
        }

    }
}
