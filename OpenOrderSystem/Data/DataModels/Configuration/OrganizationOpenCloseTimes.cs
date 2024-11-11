namespace OpenOrderSystem.Data.DataModels
{
    public struct OrganizationOpenCloseTimes
    {
        public OrganizationOpenCloseTimes(int openHour, int openMin, int closeHour, int closeMin)
        {
            Open = new TimeOnly(openHour, openMin);
            Close = new TimeOnly(closeHour, closeMin);
        }

        public OrganizationOpenCloseTimes(TimeOnly open, TimeOnly close)
        {
            Open = open;
            Close = close;
        }
        public OrganizationOpenCloseTimes()
        {
            Open = new TimeOnly(0, 0);
            Close = new TimeOnly(0, 0);
        }

        public TimeOnly Open { get; set; }
        public TimeOnly Close { get; set; }
    }
}
