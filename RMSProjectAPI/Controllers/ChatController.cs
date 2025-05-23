using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RMSProjectAPI.Database;
using RMSProjectAPI.Database.Entity;
using RMSProjectAPI.DTOs;
using System.Security.Claims;

namespace RMSProjectAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ChatController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets all chats for the current user
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ChatDTO>>> GetChats()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var userGuid = Guid.Parse(userId);

            // Get all chats where the current user is a participant
            var chats = await _context.Chats
                .Include(c => c.Messages)
                .Where(c => c.User1ID == userGuid || c.User2ID == userGuid)
                .ToListAsync();

            // Transform to DTOs with user information
            var chatDTOs = new List<ChatDTO>();
            foreach (var chat in chats)
            {
                // Get the other user's ID
                var otherUserId = chat.User1ID == userGuid ? chat.User2ID : chat.User1ID;

                // Get other user details
                var otherUser = await _context.Users
    .Where(u => u.Id == otherUserId)
    .Select(u => new UserDTO
    {
        Id = u.Id.ToString(), // Convert Guid to string
        UserName = u.UserName,
        FirstName = u.FirstName,
        LastName = u.LastName,
        ProfilePictureUrl = u.ProfilePictureUrl
    })
    .FirstOrDefaultAsync();


                if (otherUser == null)
                    continue;

                // Map messages
                var messageDTOs = chat.Messages?.Select(m => new MessageDTO
                {
                    MessageID = m.MessageID,
                    ChatID = m.ChatID,
                    SenderID = m.SenderID,
                    MessageText = m.MessageText,
                    SentAt = m.SentAt,
                    IsRead = m.IsRead
                }).ToList() ?? new List<MessageDTO>();

                chatDTOs.Add(new ChatDTO
                {
                    ChatID = chat.ChatID,
                    User1ID = chat.User1ID,
                    User2ID = chat.User2ID,
                    CreatedAt = chat.CreatedAt,
                    Messages = messageDTOs,
                    OtherUser = otherUser
                });
            }

            return Ok(chatDTOs);
        }

        /// <summary>
        /// Gets a specific chat by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ChatDTO>> GetChat(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var userGuid = Guid.Parse(userId);

            // Get the chat and verify the user is a participant
            var chat = await _context.Chats
                .Include(c => c.Messages)
                .FirstOrDefaultAsync(c => c.ChatID == id && 
                                         (c.User1ID == userGuid || c.User2ID == userGuid));

            if (chat == null)
                return NotFound();

            // Get the other user's ID
            var otherUserId = chat.User1ID == userGuid ? chat.User2ID : chat.User1ID;

            // Get other user details
            var otherUser = await _context.Users
     .Where(u => u.Id == otherUserId)
     .Select(u => new UserDTO
     {
         Id = u.Id.ToString(), // Convert Guid to string
         UserName = u.UserName,
         FirstName = u.FirstName,
         LastName = u.LastName,
         ProfilePictureUrl = u.ProfilePictureUrl
     })
     .FirstOrDefaultAsync();


            if (otherUser == null)
                return NotFound("Other user not found");

            // Map messages
            var messageDTOs = chat.Messages?.Select(m => new MessageDTO
            {
                MessageID = m.MessageID,
                ChatID = m.ChatID,
                SenderID = m.SenderID,
                MessageText = m.MessageText,
                SentAt = m.SentAt,
                IsRead = m.IsRead
            }).ToList() ?? new List<MessageDTO>();

            // Mark messages as read
            await MarkChatMessagesAsRead(chat.ChatID, userGuid);

            var chatDTO = new ChatDTO
            {
                ChatID = chat.ChatID,
                User1ID = chat.User1ID,
                User2ID = chat.User2ID,
                CreatedAt = chat.CreatedAt,
                Messages = messageDTOs,
                OtherUser = otherUser
            };

            return Ok(chatDTO);
        }

        /// <summary>
        /// Creates a new chat with another user
        /// </summary>
        [HttpPost("create")]
        public async Task<ActionResult<ChatDTO>> CreateChat([FromBody] CreateChatDTO createChatDTO)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var userGuid = Guid.Parse(userId);
            var otherUserGuid = Guid.Parse(createChatDTO.UserId);

            // Check if chat already exists
            var existingChat = await _context.Chats
                .FirstOrDefaultAsync(c => (c.User1ID == userGuid && c.User2ID == otherUserGuid) ||
                                         (c.User1ID == otherUserGuid && c.User2ID == userGuid));

            if (existingChat != null)
            {
                return BadRequest("Chat already exists");
            }

            // Create new chat
            var newChat = new Chat
            {
                ChatID = Guid.NewGuid(),
                User1ID = userGuid,
                User2ID = otherUserGuid,
                CreatedAt = DateTime.UtcNow,
                Messages = new List<Message>()
            };

            _context.Chats.Add(newChat);
            await _context.SaveChangesAsync();

            // Get other user details
            // Updated line to define 'otherUserId' correctly in the CreateChat method
            var otherUserId = otherUserGuid;

            // Get other user details
            var otherUser = await _context.Users
                .Where(u => u.Id == otherUserId)
                .Select(u => new UserDTO
                {
                    Id = u.Id.ToString(), // Convert Guid to string
                    UserName = u.UserName,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    ProfilePictureUrl = u.ProfilePictureUrl
                })
                .FirstOrDefaultAsync();



            if (otherUser == null)
                return NotFound("Other user not found");

            var chatDTO = new ChatDTO
            {
                ChatID = newChat.ChatID,
                User1ID = newChat.User1ID,
                User2ID = newChat.User2ID,
                CreatedAt = newChat.CreatedAt,
                Messages = new List<MessageDTO>(),
                OtherUser = otherUser
            };

            return Ok(chatDTO);
        }

        /// <summary>
        /// Sends a message in a chat
        /// </summary>
        [HttpPost("message")]
        public async Task<ActionResult<MessageDTO>> SendMessage([FromBody] SendMessageDTO messageDTO)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var userGuid = Guid.Parse(userId);

            // Verify the chat exists and user is a participant
            var chat = await _context.Chats
                .FirstOrDefaultAsync(c => c.ChatID == messageDTO.ChatID && 
                                         (c.User1ID == userGuid || c.User2ID == userGuid));

            if (chat == null)
                return NotFound("Chat not found or you don't have access");

            // Create and save the message
            var message = new Message
            {
                MessageID = Guid.NewGuid(),
                ChatID = messageDTO.ChatID,
                SenderID = userGuid,
                MessageText = messageDTO.Text,
                SentAt = DateTime.UtcNow,
                IsRead = false
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            // Return the created message
            var messageResponse = new MessageDTO
            {
                MessageID = message.MessageID,
                ChatID = message.ChatID,
                SenderID = message.SenderID,
                MessageText = message.MessageText,
                SentAt = message.SentAt,
                IsRead = message.IsRead
            };

            return Ok(messageResponse);
        }

        /// <summary>
        /// Marks all messages in a chat as read for the current user
        /// </summary>
        [HttpPost("read/{chatId}")]
        public async Task<ActionResult> MarkMessagesAsRead(Guid chatId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var userGuid = Guid.Parse(userId);

            // Verify the chat exists and user is a participant
            var chat = await _context.Chats
                .FirstOrDefaultAsync(c => c.ChatID == chatId && 
                                         (c.User1ID == userGuid || c.User2ID == userGuid));

            if (chat == null)
                return NotFound("Chat not found or you don't have access");

            await MarkChatMessagesAsRead(chatId, userGuid);

            return Ok();
        }

        /// <summary>
        /// Helper method to mark messages as read
        /// </summary>
        private async Task MarkChatMessagesAsRead(Guid chatId, Guid userId)
        {
            // Mark all unread messages sent by the other user as read
            var unreadMessages = await _context.Messages
                .Where(m => m.ChatID == chatId && 
                           m.SenderID != userId && 
                           !m.IsRead)
                .ToListAsync();

            if (unreadMessages.Any())
            {
                foreach (var message in unreadMessages)
                {
                    message.IsRead = true;
                }

                await _context.SaveChangesAsync();
            }
        }
    }
}
