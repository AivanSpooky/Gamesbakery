using Gamesbakery.Core.DTOs.GiftDTO;
using Gamesbakery.Core;
using Gamesbakery.Core.Entities;
using Gamesbakery.Core.Repositories;
using Gamesbakery.DataAccess;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace Gamesbakery.DataAccess.Repositories
{
    public class GiftRepository : IGiftRepository
    {
        private readonly GamesbakeryDbContext _context;

        public GiftRepository(GamesbakeryDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Gift> AddAsync(Gift gift)
        {
            try
            {
                await _context.Database.ExecuteSqlRawAsync(
                    "INSERT INTO Gifts (GiftID, SenderID, RecipientID, OrderItemID, GiftDate) " +
                    "VALUES (@GiftID, @SenderID, @RecipientID, @OrderItemID, @GiftDate)",
                    new SqlParameter("@GiftID", gift.Id),
                    new SqlParameter("@SenderID", gift.SenderId),
                    new SqlParameter("@RecipientID", gift.RecipientId),
                    new SqlParameter("@OrderItemID", gift.OrderItemId),
                    new SqlParameter("@GiftDate", gift.GiftDate));
                return gift;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to add gift to the database.", ex);
            }
        }

        public async Task<Gift> GetByIdAsync(Guid giftId, UserRole role, GiftSource source, Guid? currentUserId)
        {
            try
            {
                string query;
                SqlParameter[] parameters;

                if (role == UserRole.Admin)
                {
                    query = source == GiftSource.Sent
                        ? "SELECT GiftID, SenderID, RecipientID, OrderItemID, GiftDate " +
                          "FROM UserSentGifts WHERE GiftID = @GiftID"
                        : "SELECT GiftID, SenderID, RecipientID, OrderItemID, GiftDate " +
                          "FROM UserReceivedGifts WHERE GiftID = @GiftID";
                    parameters = new[] { new SqlParameter("@GiftID", giftId) };
                }
                else if (source == GiftSource.Sent)
                {
                    if (currentUserId == null)
                        throw new ArgumentNullException(nameof(currentUserId), "CurrentUserId is required for non-admin roles.");
                    query = "SELECT GiftID, SenderID, RecipientID, OrderItemID, GiftDate " +
                            "FROM UserSentGifts WHERE GiftID = @GiftID AND SenderID = @SenderID";
                    parameters = new[]
                    {
                    new SqlParameter("@GiftID", giftId),
                    new SqlParameter("@SenderID", currentUserId.Value)
                };
                }
                else // GiftSource.Received
                {
                    if (currentUserId == null)
                        throw new ArgumentNullException(nameof(currentUserId), "CurrentUserId is required for non-admin roles.");
                    query = "SELECT GiftID, SenderID, RecipientID, OrderItemID, GiftDate " +
                            "FROM UserReceivedGifts WHERE GiftID = @GiftID AND RecipientID = @RecipientID";
                    parameters = new[]
                    {
                    new SqlParameter("@GiftID", giftId),
                    new SqlParameter("@RecipientID", currentUserId.Value)
                };
                }

                Gift gift;
                if (source == GiftSource.Sent)
                {
                    var sentGift = await _context.UserSentGifts
                        .FromSqlRaw(query, parameters)
                        .Select(g => new Gift(g.Id, g.SenderId, g.RecipientId, g.OrderItemId, g.GiftDate))
                        .FirstOrDefaultAsync();
                    gift = sentGift;
                }
                else
                {
                    var receivedGift = await _context.UserReceivedGifts
                        .FromSqlRaw(query, parameters)
                        .Select(g => new Gift(g.Id, g.SenderId, g.RecipientId, g.OrderItemId, g.GiftDate))
                        .FirstOrDefaultAsync();
                    gift = receivedGift;
                }

                return gift ?? throw new KeyNotFoundException($"Gift with ID {giftId} not found.");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to retrieve gift with ID {giftId}.", ex);
            }
        }

        public async Task<IEnumerable<SentGift>> GetBySenderIdAsync(Guid senderId, UserRole role)
        {
            try
            {
                return await _context.UserSentGifts
                    .FromSqlRaw("SELECT GiftID, SenderID, RecipientID, OrderItemID, GiftDate " +
                                "FROM UserSentGifts WHERE SenderID = @SenderID " +
                                "ORDER BY GiftDate DESC",
                        new SqlParameter("@SenderID", senderId))
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to retrieve gifts for sender {senderId}.", ex);
            }
        }

        public async Task<IEnumerable<ReceivedGift>> GetByRecipientIdAsync(Guid recipientId, UserRole role)
        {
            try
            {
                return await _context.UserReceivedGifts
                    .FromSqlRaw("SELECT GiftID, SenderID, RecipientID, OrderItemID, GiftDate " +
                                "FROM UserReceivedGifts WHERE RecipientID = @RecipientID " +
                                "ORDER BY GiftDate DESC",
                        new SqlParameter("@RecipientID", recipientId))
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to retrieve gifts for recipient {recipientId}.", ex);
            }
        }

        public async Task DeleteAsync(Guid giftId)
        {
            try
            {
                var rowsAffected = await _context.Database.ExecuteSqlRawAsync(
                    "DELETE FROM Gifts WHERE GiftID = @GiftID",
                    new SqlParameter("@GiftID", giftId));

                if (rowsAffected == 0)
                    throw new KeyNotFoundException($"Gift with ID {giftId} not found.");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to delete gift with ID {giftId}.", ex);
            }
        }
    }
}