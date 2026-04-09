namespace SolidarityConnection.Donations.Domain.Entities
{
    public class DonorReference
    {
        public Guid Id { get; set; }
        public int Code { get; set; }
        public string Email { get; set; } = string.Empty;
        public DateTimeOffset UpdatedAt { get; set; }
        public bool IsActive { get; set; }

        public DonorReference() { }

        public DonorReference(int code, string email, DateTimeOffset updatedAt, bool isActive)
        {
            Code = code;
            Email = email;
            UpdatedAt = updatedAt;
            IsActive = isActive;
        }
    }
}

