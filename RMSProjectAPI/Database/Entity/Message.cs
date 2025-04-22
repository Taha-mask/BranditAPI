using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RMSProjectAPI.Database.Entity
{
    public class Message
    {
        public Guid MessageID { get; set; }

        [Required]
        [ForeignKey(nameof(Chat))]
        public Guid ChatID { get; set; }

        [Required]
        public Guid SenderID { get; set; }

        [Required]
        public string MessageText { get; set; }

        public DateTime SentAt { get; set; } = DateTime.Now;

        public bool IsRead { get; set; } = false;

        // Navigation property
        public Chat Chat { get; set; }
    }
}
