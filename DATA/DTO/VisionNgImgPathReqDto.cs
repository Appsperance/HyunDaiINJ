using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyunDaiINJ.DATA.DTO
{
    public class VisionNgImgPathReqDto
    {
        public List<string> lineIds { get; set; }
        public int offset { get; set; }
        public int count { get; set; }
    }
}
