using RMSProjectAPI.Database.Entity;

namespace RMSProjectAPI.DTOs
{
    /// <summary>
    /// DTO for returning chat information
    /// </summary>
    public class ChatDTO
    {
        public Guid ChatID { get; set; }
        public Guid User1ID { get; set; }
        public Guid User2ID { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<MessageDTO> Messages { get; set; } = new List<MessageDTO>();
        public UserDTO OtherUser { get; set; }
    }

    /// <summary>
    /// DTO for returning message information
    /// </summary>
    public class MessageDTO
    {
        public Guid MessageID { get; set; }
        public Guid ChatID { get; set; }
        public Guid SenderID { get; set; }
        public string MessageText { get; set; }
        public DateTime SentAt { get; set; }
        public bool IsRead { get; set; }
    }

    /// <summary>
    /// DTO for creating a new chat
    /// </summary>
    public class CreateChatDTO
    {
        public string UserId { get; set; }
    }

    /// <summary>
    /// DTO for sending a message
    /// </summary>
    public class SendMessageDTO
    {
        public Guid ChatID { get; set; }
        public string Text { get; set; }
    }

    /// <summary>
    /// DTO for user information
    /// </summary>
    public class UserDTO
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ProfilePictureUrl { get; set; }
        public string Status { get; set; } = "offline";
    }
}
