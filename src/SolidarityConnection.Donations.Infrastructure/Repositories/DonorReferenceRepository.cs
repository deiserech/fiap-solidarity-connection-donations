using SolidarityConnection.Donations.Domain.Entities;
using SolidarityConnection.Donations.Domain.Interfaces.Repositories;
using SolidarityConnection.Donations.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace SolidarityConnection.Donations.Infrastructure.Repositories
{
    public class DonorReferenceRepository : IDonorReferenceRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<DonorReferenceRepository> _logger;

        public DonorReferenceRepository(AppDbContext context, ILogger<DonorReferenceRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<DonorReference?> GetByIdAsync(Guid id)
        {
            _logger.LogDebug("Fetching donor by Id: {Id}", id);
            return await _context.DonorReferences
                .FirstOrDefaultAsync(u => u.Id == id);
        }
        
        public async Task<DonorReference?> GetByCodeAsync(int code)
        {
            _logger.LogDebug("Fetching donor by Code: {Code}", code);
            return await _context.DonorReferences
                .FirstOrDefaultAsync(u => u.Code == code);
        }

        public async Task<DonorReference?> GetByEmailAsync(string email)
        {
            _logger.LogDebug("Fetching donor by Email: {Email}", email);
            return await _context.DonorReferences
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<IEnumerable<DonorReference>> GetAllAsync()
        {
            _logger.LogDebug("Listing all donors");
            return await _context.DonorReferences
                .ToListAsync();
        }

        public async Task<DonorReference> CreateAsync(DonorReference donor)
        {
            _logger.LogDebug("Creating donor: {Email}", donor.Email);
            _context.DonorReferences.Add(donor);
            await _context.SaveChangesAsync();
            return await GetByIdAsync(donor.Id) ?? donor;
        }

        public async Task<DonorReference> UpdateAsync(DonorReference donor)
        {
            _logger.LogDebug("Updating donor: {Email}", donor.Email);
            await _context.SaveChangesAsync();
            return await GetByIdAsync(donor.Id) ?? donor;
        }

        public async Task DeleteAsync(Guid id)
        {
            _logger.LogDebug("Deleting donor by Id: {Id}", id);
            var donor = await _context.DonorReferences.FindAsync(id);
            if (donor != null)
            {
                _context.DonorReferences.Remove(donor);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.DonorReferences.AnyAsync(u => u.Id == id);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.DonorReferences.AnyAsync(u => u.Email == email);
        }
    }
}
