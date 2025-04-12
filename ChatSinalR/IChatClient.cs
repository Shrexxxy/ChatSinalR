namespace ChatSinalR;

public interface IChatClient
{
    public Task ReceiveMessage(string userName ,string message);
}