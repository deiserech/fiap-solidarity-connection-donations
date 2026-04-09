using System.Diagnostics;

namespace SolidarityConnection.Donations.Shared.Tracing
{
    public static class Tracing
    {
        public static readonly ActivitySource ActivitySource = new("SolidarityConnection.Donations.Application");
    }
}
