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

        public static ConsoleEntriesResponse MakeConsoleEntriesResponse(List<string> lines)
        {
            return new ConsoleEntriesResponse() { NewLines = lines };
        }
    }
}
