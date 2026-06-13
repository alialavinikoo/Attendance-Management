namespace AttendanceManagement.Models
{
    internal class CardLog
    {
        public long LogID { get; set; }
        public int PersonelID {  get; set; }
        public DateTime CardDate { get; set; }
        public TimeSpan CardTime {  get; set; }
        public byte CardStatus {  get; set; }
        public byte GateNumber {  get; set; }
        
        public CardLog() { }
    }
}
