using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using SpotMate.Application.Context;
using SpotMate.Application.DTOs.HubModels;
using SpotMate.Application.DTOs.Responses;
using SpotMate.Application.Options;
using SpotMate.Domain.Entities;

namespace SpotMate.Application.Hubs.Impl;

[Authorize]
public sealed class ChatHub: Hub<IChatHub>
{
   private const string StringToAdd = "chat";
   private readonly BaseUrlOptions _baseUrlOptions;
   private readonly IDistributedCache _cache;
   private readonly IApplicationDbContext _context;
   
   public ChatHub(IDistributedCache cache, IApplicationDbContext context, IOptions<BaseUrlOptions> baseUrlOptions)
   {
      _cache = cache;
      _context = context;
      _baseUrlOptions = baseUrlOptions.Value;
   }
   
   public override async Task OnConnectedAsync()
   {
      await _cache.SetStringAsync(UserId + StringToAdd, Context.ConnectionId);
      await base.OnConnectedAsync();
   }

   public async Task SendMessage(SendMessageModel messageModel)
   {
      var userId = UserId;
      var chatUser = await _context.ChatUsers.Where(cu => cu.ChatId == messageModel.ChatId).ToListAsync();
      if (chatUser.Count != 2 || (chatUser[0].UserId != userId && chatUser[1].UserId != userId)) return;
      if (!await _context.UserFriends.AnyAsync(uf =>
             uf.UserId == chatUser[0].UserId && uf.FriendId == chatUser[1].UserId)) return;
         
      var message = new Message
      {
         ChatId = messageModel.ChatId,
         Text = messageModel.Text,
         UserId = userId,
      };

      await _context.Messages.AddAsync(message);
      await _context.SaveChangesAsync();
      
      var user = await _context.Users.FirstAsync(u => u.Id == userId);

      var messageDto = new MessageDto
      {
         Id = message.Id,
         ChatId = messageModel.ChatId,
         CreateTime = message.CreateTime,
         Text = message.Text,
         IsUnread = message.IsUnread,
         IsMine = true,
         User = new UserMessageModel()
         {
            Id = user.Id,
            Avatar = user.AvatarFileName != null ? $"{_baseUrlOptions.Url}{user.AvatarFileName}" : null,
            UserName = user.UserName,
         }
      };

      await Clients.Client(Context.ConnectionId).ReceiveMessage(messageDto);
      var secondUserId = chatUser[0].UserId == userId ? chatUser[0].FriendId : chatUser[0].UserId;
      var secondConnectionId = await _cache.GetStringAsync(secondUserId + StringToAdd);
      if (secondConnectionId != null)
      {
         messageDto.IsMine = false;
         await Clients.Client(secondConnectionId).ReceiveMessage(messageDto);
      }
   }

   public async Task ReadMessage(Guid messageId)
   {
      var userId = UserId;
      
      var message = await _context.Messages
         .Include(m => m.Chat)
         .ThenInclude(c => c.ChatUsers)
         .FirstOrDefaultAsync(m => m.Id == messageId);
      
      if (message == null || message.UserId == UserId || message.Chat.ChatUsers.All(cu => cu.UserId != UserId)) return;
      message.IsUnread = false;
      await _context.SaveChangesAsync();
   }

   public async Task StartTyping(Guid chatId)
   {
      var userId = UserId;
      var chatUser = await _context.ChatUsers.FirstOrDefaultAsync(cu => cu.ChatId == chatId);
      if (chatUser == null) return;

      var friendId = chatUser.UserId == userId ? chatUser.FriendId : chatUser.UserId;
      var friendConnectionId = await _cache.GetStringAsync(friendId + StringToAdd);
      if (friendConnectionId != null)
      {
         await Clients.Client(friendConnectionId).ReceiveTyping(chatId);
      }
   }

   public override async Task OnDisconnectedAsync(Exception? exception)
   {
      await _cache.RemoveAsync(UserId + StringToAdd);
      await base.OnDisconnectedAsync(exception);
   }

   private Guid UserId
   {
      get
      {
         var value = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
         return Context.User?.Identity?.IsAuthenticated == null || value == null
            ? Guid.Empty
            : Guid.Parse(value);
      }
   }
}