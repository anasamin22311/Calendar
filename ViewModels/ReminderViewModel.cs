namespace Calendar.ModelViews
{
    public class ReminderViewModel
    {
        public int ReminderId { get; set; }
        public string Method { get; set; }
        public int? Minutes { get; set; }
        public int EventId { get; set; }
    }

}