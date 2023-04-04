namespace RPM_exercise
{
    public class JsonModel
    {
        public Response response { get; set; }
    }

    public class Response
    {
        public List<Data> data { get; set; }
    }

    public class Data
    {
        public string period { get; set; }
        public double value { get; set; }
    }

}
