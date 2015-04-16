using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaiduPanSearch.NET45.Models
{
    public class DropdownItem
    {
        /// <summary>
        /// 搜索引擎名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 搜索引擎类型
        /// </summary>
        public Type EngineType { get; set; }
    }
}
