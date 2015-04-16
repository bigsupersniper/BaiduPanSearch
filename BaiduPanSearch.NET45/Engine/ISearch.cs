using BaiduPanSearch.NET45.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaiduPanSearch.NET45.Engine
{
    public interface ISearch
    {
        /// <summary>
        /// 每页显示的条数
        /// </summary>
        int PageSize { get; }

        /// <summary>
        /// 总页数
        /// </summary>
        int TotalPage { get; }

        /// <summary>
        /// 当前页码
        /// </summary>
        int CurrentPage { get; }

        /// <summary>
        /// 搜索关键字
        /// </summary>
        string Keyword { get; }

        /// <summary>
        /// 搜索结果状态描述
        /// </summary>
        string ResultStatus { get; }

        /// <summary>
        /// 搜索缓存
        /// </summary>
        Dictionary<int, List<GridRowItem>> Cached { get; }

        /// <summary>
        /// 搜索
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        Task<List<GridRowItem>> Search(string keyword);

        /// <summary>
        /// 上一页
        /// </summary>
        /// <returns></returns>
        Task<List<GridRowItem>> PageUp();

        /// <summary>
        /// 下一页
        /// </summary>
        /// <returns></returns>
        Task<List<GridRowItem>> PageDown();
    }
}
