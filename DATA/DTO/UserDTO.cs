using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyunDaiINJ.DATA.DTO
{
    public class UserDTO
    {
        public string? Id { get; set; }  
        public int? EmployeeNumber { get; set; }
        public string? LoginId { get; set; }
        public string? LoginPw {  get; set; }
        public string? Salt {  get; set; }
        public string? Name { get; set; }
        public string? Roles { get; set; }

    }
}
