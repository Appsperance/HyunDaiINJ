﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HyunDaiINJ.DATA.DTO;

namespace HyunDaiINJ.DATA.DAO
{
    public interface IVisionCumDAO
    {
        List<VisionCumDTO> GetVisionCumData();
    }
}
