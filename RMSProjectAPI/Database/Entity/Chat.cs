using System.ComponentModel.DataAnnotations;

namespace RMSProjectAPI.Database.Entity
{
    public class Chat
    {
        [Key]
        public Guid ChatID { get; set; }

        [Required]
        public Guid User1ID { get; set; } // First participant

        [Required]
        public Guid User2ID { get; set; } // Second participant

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation property for messages
        public ICollection<Message> Messages { get; set; }
    }
}
