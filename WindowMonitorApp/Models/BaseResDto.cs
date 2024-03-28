namespace WindowMonitorApp.Models
{
    public class BaseResDto<T>
    {
        public int Code { get; set; }

        public string Message { get; set; }


        public T Data { get; set; } 
    }
}
