using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSYASGUI_CommonLib.ResponseObjects;

namespace VSYASGUI_CommonLib
{
    public static class ResponseFactory
    {
        public static ErrorResponse MakeErrorUnauthorised() 
        { 
            return new ErrorResponse() { error = "unauthorised" }; 
        }

        public static ErrorResponse MakeErrorBadRequest()
        {
            return new ErrorResponse() { error = "bad-request" };
        }

        public static ConsoleEntriesResponse MakeConsoleEntriesResponse(string[] lines)
        {
            return new ConsoleEntriesResponse() { NewLines = lines.ToArray() };
        }
    }
}
