using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SpotMate.Application.Context;
using SpotMate.Application.DTOs.Base;
using SpotMate.Application.DTOs.Requests;
using SpotMate.Application.DTOs.Responses;
using SpotMate.Application.Exceptions;
using SpotMate.Application.OperationResult;
using SpotMate.Application.Options;
using SpotMate.Domain.Entities;

namespace SpotMate.Application.Services.Impl;

public sealed class ChatService: IChatService
{
    private readonly BaseUrlOptions _baseUrlOptions;
    private readonly IApplicationDbContext _context;

    public ChatService(IApplicationDbContext context, IOptions<BaseUrlOptions> baseUrlOptions)
    {
        _context = context;
        _baseUrlOptions = baseUrlOptions.Value;
    }

    public async Task<Result<IEnumerable<ChatFullDto>>> GetChatsAsync(Guid userId)
    {
        var chats = await _context.ChatUsers
            .AsNoTracking()
            .Where(cu => cu.UserId == userId)
            .Select(cu => new ChatFullDto
            {
                Chat = new ChatShortDto
                {
                    Id = cu.ChatId,
                    Avatar = cu.Friend.AvatarFileName != null ? $"{_baseUrlOptions.Url}{cu.Friend.AvatarFileName}" : null,
                    LastOnline = cu.Friend.LastOnline,
                    Title = cu.Friend.FullName,
                    UserStatus = cu.Friend.UserStatus
                },
                UnreadMessagesCount = cu.Chat.Messages.Count(m => m.IsUnread && m.UserId != userId),
                IsBlocked = cu.User.Friends.All(f => f.FriendId != cu.FriendId),
                LastMessage = cu.Chat.Messages.OrderByDescending(m => m.CreateTime).Select(m => new MessageDto
                {
                    Id = m.Id,
                    CreateTime = m.CreateTime,
                    IsMine = m.UserId == userId,
                    Text = m.Text,
                    User = new UserMessageModel
                    {
                        Id = m.User.Id,
                        Avatar = m.User.AvatarFileName != null ? $"{_baseUrlOptions.Url}{m.User.AvatarFileName}" : null,
                        UserName = m.User.UserName
                    },
                    ChatId = cu.ChatId,
                    IsUnread = m.IsUnread
                }).FirstOrDefault()
            }).ToListAsync();

        return chats;
    }

    public async Task<Result<ChatShortDto>> CreateChat(CreateChatDto createChatDto, Guid userId)
    {
        if (!await _context.Users.AnyAsync(u => u.Id == createChatDto.UserId) ||
            !await _context.Users.AnyAsync(u => u.Id == userId))
        {
            return new NotFoundException(nameof(SpotMateUser));
        }

        if (!await _context.UserFriends.AnyAsync(uf => uf.UserId == userId && uf.FriendId == createChatDto.UserId))
        {
            return new ForbiddenException("User is not your friend");
        }
        
        if (await _context.ChatUsers.AnyAsync(cu => cu.UserId == userId && cu.FriendId == createChatDto.UserId))
        {
            return new BadRequestException("Chat already exists");
        }

        var chatId = Guid.NewGuid();
        await _context.Chats.AddAsync(new Chat
        {
            Id = chatId
        });

        await _context.ChatUsers.AddAsync(new ChatUser
        {
            UserId = userId,
            FriendId = createChatDto.UserId,
            ChatId = chatId
        });
        
        await _context.ChatUsers.AddAsync(new ChatUser
        {
            UserId = createChatDto.UserId,
            FriendId = userId,
            ChatId = chatId
        });

        await _context.SaveChangesAsync();

        var friend = await _context.Users.FirstAsync(u => u.Id == createChatDto.UserId);
        return new ChatShortDto
        {
            Id = chatId,
            Avatar = friend.AvatarFileName != null ? $"{_baseUrlOptions.Url}{friend.AvatarFileName}" : null,
            LastOnline = friend.LastOnline,
            Title = friend.FullName,
            UserStatus = friend.UserStatus
        };
    }

    public async Task<Result<IEnumerable<MessageDto>>> GetMessages(Guid chatId, BaseSearchParameters searchParameters, Guid userId)
    {
        if (!await _context.ChatUsers.AnyAsync(cu => cu.ChatId == chatId && cu.UserId == userId))
        {
            return new NotFoundException(nameof(Chat));
        }

        var messages = await _context.Messages
            .Include(m => m.User)
            .Where(m => m.ChatId == chatId)
            .OrderByDescending(m => m.CreateTime)
            .Skip(searchParameters.Offset)
            .Take(searchParameters.Limit)
            .ToListAsync();

        var linkedList = new LinkedList<MessageDto>();

        foreach (var m in messages)
        {
            if (m.IsUnread && m.UserId != userId)
            {
                m.IsUnread = false;
            }
            
            linkedList.AddFirst(new MessageDto
            {
                Id = m.Id,
                CreateTime = m.CreateTime,
                IsMine = m.UserId == userId,
                Text = m.Text,
                User = new UserMessageModel
                {
                    Id = m.User.Id,
                    Avatar = m.User.AvatarFileName != null ? $"{_baseUrlOptions.Url}{m.User.AvatarFileName}" : null,
                    UserName = m.User.UserName,
                },
                IsUnread = m.IsUnread,
                ChatId = chatId
            });
        }

        await _context.SaveChangesAsync();
        return linkedList;
    }
}