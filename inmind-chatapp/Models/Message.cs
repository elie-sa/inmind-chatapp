namespace inmind_chatapp_client.Models;

public class Message
{
    public int Id { get; set; }
    public string FromUser { get; set; }
    public string ToUser { get; set; }
    public string MessageText { get; set; }
    public long Timestamp { get; set; }
}