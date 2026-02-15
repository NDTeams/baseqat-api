using System;
using System.Collections.Generic;
using System.Text;

namespace Baseqat.CORE.Response
{
    public class ApiBaseResponse<T>
    {
        public bool Succeeded { get; set; }
        public string Message { get; set; }
        public string[] Errors { get; set; }
        public T Data { get; set; }

        protected ApiBaseResponse() { }

        protected ApiBaseResponse(T data, string message = null)
        {
            Succeeded = true;
            Message = message;
            Errors = null;
            Data = data;
        }

        protected ApiBaseResponse(string message)
        {
            Succeeded = false;
            Message = message;
            Errors = new string[] { message };
        }

        public static ApiBaseResponse<T> Success(T data, string message = null) =>
            new ApiBaseResponse<T>(data, message);

        public static ApiBaseResponse<T> Fail(string message, string[] errors = null) =>
            new ApiBaseResponse<T>
            {
                Succeeded = false,
                Message = message,
                Errors = errors ?? new[] { message }
            };
    }
}
