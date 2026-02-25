using POS.Data;
using POS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace POS.Services
{
    public class BuyerService : IBuyerService
    {
        private readonly AppDbContext _context;

        public BuyerService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Buyer> AddBuyerAsync(string name)
        {
            var buyer = new Buyer
            {
                Name = name
            };

            _context.Buyers.Add(buyer);
            await _context.SaveChangesAsync();
            return buyer;
        }

        public async Task<Buyer> GetOrCreateBuyerAsync(string name)
        {
            // Try to find existing buyer by name
            var buyer = await _context.Buyers
                .FirstOrDefaultAsync(b => b.Name.ToLower() == name.ToLower());

            if (buyer != null)
            {
                return buyer;
            }

            // Create new buyer
            buyer = new Buyer
            {
                Name = name
            };

            _context.Buyers.Add(buyer);
            await _context.SaveChangesAsync();
            return buyer;
        }

        public async Task<Buyer?> GetBuyerByNameAsync(string name)
        {
            return await _context.Buyers
                .FirstOrDefaultAsync(b => b.Name.ToLower() == name.ToLower());
        }

        public async Task<IEnumerable<Buyer>> GetAllBuyersAsync()
        {
            return await _context.Buyers.ToListAsync();
        }
    }
}