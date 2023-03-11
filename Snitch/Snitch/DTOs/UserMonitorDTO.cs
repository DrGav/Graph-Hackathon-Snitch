namespace Snitch.DTOs
{
    public class UserMonitorDTO
    {
        public string UserId { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public bool Monitoring { get; set; }
        public int AvailableMinutes { get; set; }
        public int BusyMinutes { get; set; }
        public int AwayMinutes { get; set; }
        public int DoNotDisturbMinutes { get; set; }
    }
}
