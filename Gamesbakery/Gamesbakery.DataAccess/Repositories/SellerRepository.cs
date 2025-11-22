using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gamesbakery.Core;
using Gamesbakery.Core.DTOs;
using Gamesbakery.Core.Entities;
using Gamesbakery.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Gamesbakery.DataAccess.Repositories
{
    public class SellerRepository : ISellerRepository
    {
        private readonly GamesbakeryDbContext _context;

        public SellerRepository(GamesbakeryDbContext context)
        {
            _context = context;
        }

        public async Task<SellerDTO> AddAsync(SellerDTO dto, UserRole role)
        {
            if (role != UserRole.Admin)
                throw new UnauthorizedAccessException("Only admins can add sellers.");
            var entity = new Seller(dto.Id, dto.SellerName, dto.RegistrationDate, dto.AvgRating, dto.Password);
            _context.Sellers.Add(entity);
            await _context.SaveChangesAsync();
            return MapToDTO(entity);
        }

        public async Task DeleteAsync(Guid id, UserRole role)
        {
            if (role != UserRole.Admin)
                throw new UnauthorizedAccessException("Only admins can delete sellers.");
            var entity = await _context.Sellers.FindAsync(id);
            if (entity != null)
            {
                _context.Sellers.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<SellerDTO>> GetAllAsync(UserRole role)
        {
            if (role != UserRole.Admin)
                throw new UnauthorizedAccessException("Only admins can access all sellers.");
            var sellers = await _context.Sellers.ToListAsync();
            return sellers.Select(MapToDTO);
        }

        public async Task<SellerDTO?> GetByIdAsync(Guid id, UserRole role)
        {
            //if (role == UserRole.Seller)
            //    throw new UnauthorizedAccessException("Sellers can only access their own profile.");
            var entity = await _context.Sellers.FindAsync(id);
            return entity != null ? MapToDTO(entity) : null;
        }

        public async Task<SellerDTO> UpdateAsync(SellerDTO dto, UserRole role)
        {
            if (role != UserRole.Admin)
                throw new UnauthorizedAccessException("Only admins can update sellers.");
            var entity = await _context.Sellers.FindAsync(dto.Id);
            if (entity == null)
                throw new KeyNotFoundException($"Seller {dto.Id} not found");
            entity.SetSellerName(dto.SellerName);
            entity.SetAvgRating(dto.AvgRating);
            _context.Sellers.Update(entity);
            await _context.SaveChangesAsync();
            return MapToDTO(entity);
        }

        public async Task<SellerDTO?> GetProfileAsync(Guid id, UserRole role)
        {
            if (role == UserRole.Seller)
                throw new UnauthorizedAccessException("Sellers can only access their own profile.");
            var entity = await _context.Sellers.FirstOrDefaultAsync(s => s.Id == id);
            return entity != null ? MapToDTO(entity) : null;
        }

        public async Task<int> GetCountAsync(UserRole role)
        {
            if (role != UserRole.Admin)
                throw new UnauthorizedAccessException("Only admins can access seller count.");
            return await _context.Sellers.CountAsync();
        }

        private SellerDTO MapToDTO(Seller entity)
        {
            return new SellerDTO
            {
                Id = entity.Id,
                SellerName = entity.SellerName,
                RegistrationDate = entity.RegistrationDate,
                AvgRating = entity.AvgRating,
                Password = entity.Password
            };
        }
    }
}