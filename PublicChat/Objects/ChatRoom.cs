using System.Text.Json;

namespace PublicChat.Objects;

public class ChatRoom
{
    private long ConnectionIndex;
    private readonly HashSet<Member> Members = new();
    private int CurrentUsers => Members.Count;

    public Task Broadcast(Message message, Member? exclude = null)
    {
        var serialized = JsonSerializer.Serialize(message);
        
        lock (Members)
        {
            var members = Members.Where(m => m != exclude);

            members.AsParallel().ForAll(MessageMember);
        }

        return Task.CompletedTask;
        
        async void MessageMember(Member m)
        {
            await m.WriteString(serialized);
        }
    }

    public long GetConnectionID() => ConnectionIndex++;

    public void AddMember(Member member)
    {
        Members.Add(member);
    }

    public void RemoveMember(Member member)
    {
        Members.Remove(member);
    }
}