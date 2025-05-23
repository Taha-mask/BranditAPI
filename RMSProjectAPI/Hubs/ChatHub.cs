using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using RMSProjectAPI.Database;
using RMSProjectAPI.Database.Entity;
using System.Security.Claims;

namespace RMSProjectAPI.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly AppDbContext _context;

        public ChatHub(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Sends a message to a specific user
        /// </summary>
        /// <param name="receiverId">The ID of the user who will receive the message</param>
        /// <param name="text">The message text</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task SendMessage(string receiverId, string text)
        {
            if (string.IsNullOrEmpty(text.Trim()))
                return;

            var senderId = Context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(senderId))
                throw new HubException("User is not authenticated");

            var senderGuid = Guid.Parse(senderId);
            var receiverGuid = Guid.Parse(receiverId);

            // Find existing chat or create a new one
            var chat = await FindOrCreateChat(senderGuid, receiverGuid);

            // Create and save the message
            var message = new Message
            {
                MessageID = Guid.NewGuid(),
                ChatID = chat.ChatID,
                SenderID = senderGuid,
                MessageText = text,
                SentAt = DateTime.UtcNow,
                IsRead = false
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            // Send the message to the sender (for confirmation)
            await Clients.Caller.SendAsync("ReceiveMessage", message);

            // Send the message to the receiver if they're connected
            await Clients.User(receiverId).SendAsync("ReceiveMessage", message);
        }

        /// <summary>
        /// Finds an existing chat between two users or creates a new one
        /// </summary>
        private async Task<Chat> FindOrCreateChat(Guid user1Id, Guid user2Id)
        {
            // Try to find existing chat (checking both user order possibilities)
            var chat = await _context.Chats
                .Where(c => (c.User1ID == user1Id && c.User2ID == user2Id) ||
                           (c.User1ID == user2Id && c.User2ID == user1Id))
                .FirstOrDefaultAsync();

            // If no chat exists, create a new one
            if (chat == null)
            {
                chat = new Chat
                {
                    ChatID = Guid.NewGuid(),
                    User1ID = user1Id,
                    User2ID = user2Id,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Chats.Add(chat);
                await _context.SaveChangesAsync();
            }

            return chat;
        }

        /// <summary>
        /// Marks messages in a conversation as read
        /// </summary>
        public async Task MarkMessagesAsRead(string chatId)
        {
            var userId = Context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                throw new HubException("User is not authenticated");

            var userGuid = Guid.Parse(userId);
            var chatGuid = Guid.Parse(chatId);

            // Verify the user is part of this chat
            var chat = await _context.Chats
                .FirstOrDefaultAsync(c => c.ChatID == chatGuid && 
                                         (c.User1ID == userGuid || c.User2ID == userGuid));

            if (chat == null)
                throw new HubException("Chat not found or you don't have access");

            // Mark all unread messages sent by the other user as read
            var unreadMessages = await _context.Messages
                .Where(m => m.ChatID == chatGuid && 
                           m.SenderID != userGuid && 
                           !m.IsRead)
                .ToListAsync();

            if (unreadMessages.Any())
            {
                foreach (var message in unreadMessages)
                {
                    message.IsRead = true;
                }

                await _context.SaveChangesAsync();

                // Notify the sender that their messages have been read
                var otherUserId = chat.User1ID == userGuid ? chat.User2ID : chat.User1ID;
                await Clients.User(otherUserId.ToString()).SendAsync("MessagesRead", chatId);
            }
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                await Clients.User(userId).SendAsync("UserConnected");
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userId = Context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                await Clients.User(userId).SendAsync("UserDisconnected");
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}