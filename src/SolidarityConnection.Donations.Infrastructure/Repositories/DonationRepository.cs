using SolidarityConnection.Donations.Application.Interfaces.Repositories;
using SolidarityConnection.Donations.Domain.Entities;
using SolidarityConnection.Donations.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace SolidarityConnection.Donations.Infrastructure.Repositories
{
    public class DonationRepository : IDonationRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<DonationRepository> _logger;

        public DonationRepository(AppDbContext context, ILogger<DonationRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Donation?> GetByCampaignAndDonorCodeAsync(int campaignCode, int donorCode)
        {
            _logger.LogDebug("Fetching donation by CampaignCode: {CampaignCode} and DonorCode: {DonorCode}", campaignCode, donorCode);
            return await _context.Donations
                .Include(d => d.Campaign)
                .Include(d => d.Donor)
                .FirstOrDefaultAsync(d => d.Campaign != null
                    && d.Campaign.Code == campaignCode
                    && d.Donor != null
                    && d.Donor.Code == donorCode);
        }

        public async Task<Donation> AddAsync(Donation donation)
        {
            _logger.LogDebug("Adding donation for CampaignId: {CampaignId}, DonorId: {DonorId}", donation.CampaignId, donation.DonorId);
            _context.Donations.Add(donation);
            await _context.SaveChangesAsync();
            return await _context.Donations
                .Include(d => d.Campaign)
                .Include(d => d.Donor)
                .FirstOrDefaultAsync(d => d.Id == donation.Id) ?? donation;
        }

        public async Task<IEnumerable<Donation>> GetAllAsync()
        {
            _logger.LogDebug("Listing all donations");
            return await _context.Donations
                .Include(d => d.Campaign)
                .Include(d => d.Donor)
                .ToListAsync();
        }

        public async Task<Donation?> GetByIdAsync(Guid id)
        {
            _logger.LogDebug("Fetching donation by Id: {Id}", id);
            return await _context.Donations
                .Include(d => d.Campaign)
                .Include(d => d.Donor)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<Donation> UpdateAsync(Donation donation)
        {
            _logger.LogDebug("Updating donation Id: {Id}", donation.Id);
            await _context.SaveChangesAsync();
            return await GetByIdAsync(donation.Id) ?? donation;
        }

        public async Task DeleteAsync(Guid id)
        {
            _logger.LogDebug("Deleting donation by Id: {Id}", id);
            var donation = await _context.Donations.FindAsync(id);
            if (donation != null)
            {
                _context.Donations.Remove(donation);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.Donations.AnyAsync(d => d.Id == id);
        }
    }
}
