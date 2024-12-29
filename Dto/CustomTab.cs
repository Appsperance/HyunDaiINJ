using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace HyunDaiINJ.DTO
{
    public class CustomTab
    {
        /// <summary>
        /// 탭의 헤더 제목
        /// </summary>
        public string? CustomHeader { get; set; }

        /// <summary>
        /// 탭의 콘텐츠
        /// </summary>
        public Object? CustomContent { get; set; }

        /// <summary>
        /// 탭 닫기 명령
        /// </summary>
        public ICommand? CloseCommand { get; set; }

        /// <summary>
        /// 탭의 닫기 동작을 수행하기 위한 파라미터
        /// </summary>

    }
}
