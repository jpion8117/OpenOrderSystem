using static OpenOrderSystem.Data.DataModels.Organization;

namespace OpenOrderSystem.Data.DataModels
{
    public struct OrganizationOptions
    {
        public OrganizationOptions()
        {
            Name = string.Empty;
            Description = null;
            UseInstorePickup = true;
            UseIndividualBarcode = false;
            Hours = new Dictionary<DayOfWeek, OrganizationOpenCloseTimes>
                {
                    { DayOfWeek.Sunday,     new OrganizationOpenCloseTimes() },
                    { DayOfWeek.Monday,     new OrganizationOpenCloseTimes() },
                    { DayOfWeek.Tuesday,    new OrganizationOpenCloseTimes() },
                    { DayOfWeek.Wednesday,  new OrganizationOpenCloseTimes() },
                    { DayOfWeek.Thursday,   new OrganizationOpenCloseTimes() },
                    { DayOfWeek.Friday,     new OrganizationOpenCloseTimes() },
                    { DayOfWeek.Saturday,   new OrganizationOpenCloseTimes() }
                };
            OrganizationTimeZone = TimeZoneInfo.Local;
        }

        public string Name { get; set; }
        public string? Description { get; set; }
        public bool UseInstorePickup { get; set; }
        public bool UseIndividualBarcode { get; set; }
        public TimeZoneInfo OrganizationTimeZone { get; set; }
        public Dictionary<DayOfWeek, OrganizationOpenCloseTimes> Hours { get; set; }
    }
}
