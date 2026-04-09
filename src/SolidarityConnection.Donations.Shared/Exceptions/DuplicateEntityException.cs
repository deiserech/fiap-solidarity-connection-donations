using System;

namespace SolidarityConnection.Donations.Shared.Exceptions
{
    public class DuplicateEntityException : Exception
    {
        public DuplicateEntityException()
        {
        }

        public DuplicateEntityException(string message)
            : base(message)
        {
        }

        public DuplicateEntityException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
