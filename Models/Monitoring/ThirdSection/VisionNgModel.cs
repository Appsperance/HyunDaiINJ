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
                Console.WriteLine("VisionCumDAO successfully initialized.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing VisionNgDAO: {ex.Message}");
                throw;
            }
        }

        public List<VisionNgDTO> GetVisionNgData()
        {
            try
            {
                var data = visionNgDAO.GetVisionNgData();
                Console.WriteLine($"Retrieved {data.Count} records from VisionNgDAO.");
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
