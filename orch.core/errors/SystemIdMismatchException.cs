namespace orch.core.errors
{
    public class SystemIdMismatchException : Exception
    {
        private SystemIdMismatchException(Guid expected, Guid? actual, string message)
            : base(message)
        {
            Expected = expected;
            Actual = actual;
        }

        public Guid Expected { get; }
        public Guid? Actual { get; }

        public static void ThrowIfNotEqual(Guid expectedSystemId, Guid? actualSystemId)
        {
            if (expectedSystemId == Guid.Empty)
                throw new SystemIdMismatchException(expectedSystemId, actualSystemId, "System information is not set.");
            
            if (actualSystemId is null || actualSystemId == Guid.Empty)
                throw new SystemIdMismatchException(expectedSystemId, actualSystemId, "The action does not specify a system ID.");
            
            if (actualSystemId != expectedSystemId)
            {
                throw new SystemIdMismatchException(
                    expectedSystemId,
                    actualSystemId,
                    "The system ID of the action does not match the current system ID.");
            }
        }
    }
}