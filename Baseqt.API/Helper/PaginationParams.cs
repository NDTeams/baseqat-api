using Baseqat.EF.Consts;

namespace Baseqt.API.Helper
{
    public class PaginationParams
    {
        public int PageNumber { get; set; } = PaginationConstants.PageNumber;
        public int PageSize { get; set; } = PaginationConstants.PageSize;
    }
}
