using Baseqat.EF.Consts;
using System;
using System.Collections.Generic;
using System.Text;

namespace Baseqat.CORE.Response.Pagination
{
    public class PagedResponse<T> : ApiBaseResponse<List<T>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }

        public PagedResponse() { }

        private PagedResponse(List<T> data, int pageNumber, int pageSize, int totalCount, string message = null)
            : base(data, message)
        {
            PageNumber = pageNumber;
            PageSize = pageSize;
            TotalCount = totalCount;
        }

        public static PagedResponse<T> Success(List<T> data, int pageSize, int pageNumber, string message = null)
        {
            var totalCount = data.Count;
            data = data.Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            if (pageSize > 100)
            {
                pageSize = PaginationConstants.MaxPageSize;
            }
            return new PagedResponse<T>(data, pageNumber, pageSize, totalCount, message);
        }

        public new static PagedResponse<T> Fail(string message, string[] errors = null)
        {
            return new PagedResponse<T>
            {
                Succeeded = false,
                Message = message,
                Errors = errors ?? new[] { message }
            };
        }
    }
}
