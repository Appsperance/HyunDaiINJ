using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyunDaiINJ.DATA.DTO
{
    public class VisionNgImgPathRspDto
    {
        public string lineId { get; set; }
        public List<VisionNgDTO> visionNgImages { get; set; }
    }

}
