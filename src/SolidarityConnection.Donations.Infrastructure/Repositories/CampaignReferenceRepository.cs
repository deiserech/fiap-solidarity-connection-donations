using SolidarityConnection.Donations.Domain.Entities;
using SolidarityConnection.Donations.Domain.Interfaces.Repositories;
using SolidarityConnection.Donations.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace SolidarityConnection.Donations.Infrastructure.Repositories
{
    public class CampaignReferenceRepository : ICampaignReferenceRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CampaignReferenceRepository> _logger;

        public CampaignReferenceRepository(AppDbContext context, ILogger<CampaignReferenceRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<CampaignReference?> GetByIdAsync(Guid id)
        {
            _logger.LogDebug("Fetching campaign by Id: {Id}", id);
            return await _context.CampaignReferences
                .FirstOrDefaultAsync(g => g.Id == id);
        }

        public async Task<CampaignReference?> GetByCodeAsync(int code)
        {
            _logger.LogDebug("Fetching campaign by Code: {Code}", code);
            return await _context.CampaignReferences
                .FirstOrDefaultAsync(g => g.Code == code);
        }

        public async Task<CampaignReference> CreateAsync(CampaignReference campaign)
        {
            _logger.LogDebug("Creating campaign: {Title}", campaign.Title);
            _context.CampaignReferences.Add(campaign);
            await _context.SaveChangesAsync();
            return await GetByIdAsync(campaign.Id) ?? campaign;
        }

        public async Task<CampaignReference> UpdateAsync(CampaignReference campaign)
        {
            _logger.LogDebug("Updating campaign: {Title}", campaign.Title);
            await _context.SaveChangesAsync();
            return await GetByIdAsync(campaign.Id) ?? campaign;
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.CampaignReferences.AnyAsync(g => g.Id == id);
        }
    }
}
