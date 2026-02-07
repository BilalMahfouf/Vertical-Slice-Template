using VeterinaryApi.Domain.Common;

namespace VeterinaryApi.Domain.Notifications;

public sealed class Notification : Entity
{
    public string Title { get; private set; } = null!;
    public string Body { get; private set; } = null!;
    public bool IsRead { get; private set; }

    public static Notification Create(string title, string body)
    {
        var notficaiton = new Notification();
        notficaiton.Title = title;
        notficaiton.Body = body;
        notficaiton.IsRead = false; 
        return notficaiton;
    }
    public void MarkAsRead()
    {
        this.IsRead = true;
    }

}
