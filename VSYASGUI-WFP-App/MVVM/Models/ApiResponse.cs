using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSYASGUI_CommonLib.ResponseObjects;

namespace VSYASGUI_WFP_App.MVVM.Models
{
    /// <summary>
    /// Represents a response from the API.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class ApiResponse<T> where T : ResponseBase
    {
        public Error ErrorResult;
        public T? Response;

        public ApiResponse(Error error, T? response)
        {
            ErrorResult = error;
            Response = response;
        }
    }
}
