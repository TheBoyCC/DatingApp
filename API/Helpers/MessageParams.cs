namespace API.Helpers
{
    public class MessageParams : PaginationParams
    {
        public string Usrname { get; set; }
        public string Container { get; set; } = "Unread";
    }
}