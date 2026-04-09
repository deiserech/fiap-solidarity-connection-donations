namespace SolidarityConnection.Donations.Domain.Entities
{
    public class CampaignReference
    {
        public Guid Id { get; set; }
        public int Code { get; set; }
        public string? Title { get; set; }
        public decimal GoalAmount { get; set; }
        public string Status { get; set; } = "active";
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
        public bool IsActive { get; set; }

        public CampaignReference() { }

        public CampaignReference(int code, string? title, decimal goalAmount, string status, DateTimeOffset updatedAt, bool isActive)
        {
            Code = code;
            Title = title;
            GoalAmount = goalAmount;
            Status = status;
            UpdatedAt = updatedAt;
            IsActive = isActive;
        }
    }
}
