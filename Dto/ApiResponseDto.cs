namespace VMart.Dto
{
    public class ApiResponseDto<T>
    {
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public bool Success { get; set; }

        public ApiResponseDto()
        {
        }

        public ApiResponseDto(string message, T? data, bool success)
        {
            Message = message;
            Data = data;
            Success = success;
        }

        // Static factory methods for convenience
        public static ApiResponseDto<T> SuccessResponse(string message, T data)
        {
            return new ApiResponseDto<T>(message, data, true);
        }

        public static ApiResponseDto<T> ErrorResponse(string message)
        {
            return new ApiResponseDto<T>(message, default(T), false);
        }
    }
}
