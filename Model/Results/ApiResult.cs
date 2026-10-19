namespace S1.Model.Results
{
    public class ApiResults<T>
    {
        public int StatusCode { get; set; }
        public string RequestMethod { get; set; } = string.Empty;
        public T Data { get; set; }
    }
}