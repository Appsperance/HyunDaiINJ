using System;
using System.Collections.Generic;
using HyunDaiINJ.DATA.DAO;
using HyunDaiINJ.DATA.DTO;

namespace HyunDaiINJ.Models.Monitoring.ThirdSection
{
    public class VisionCumModel
    {
        private readonly VisionCumDAO visionCumDAO;

        public VisionCumModel()
        {
            try
            {
                visionCumDAO = new VisionCumDAO();
                Console.WriteLine("VisionCumDAO successfully initialized.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing VisionCumDAO: {ex.Message}");
                throw;
            }
        }

        public List<VisionCumDTO> GetVisionCumData()
        {
            try
            {
                var data = visionCumDAO.GetVisionCumData();
                Console.WriteLine($"Retrieved {data.Count} records from VisionCumDAO.");
                return data;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving data from VisionCumDAO: {ex.Message}");
                throw;
            }
        }
    }
}
