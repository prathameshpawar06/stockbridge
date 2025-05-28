namespace stockbridge_api.Helper
{
    public class GenericResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        public int? TotalItems { get; set; }

        public GenericResponse(bool success, string message, T data, int? totalItems = null)
        {
            Success = success;
            Message = message;
            Data = data;
            TotalItems = totalItems;
        }
    }
}
