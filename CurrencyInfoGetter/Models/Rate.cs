using System.ComponentModel.DataAnnotations;

namespace CurrencyInfoGetter.Models
{
    public class Rate
    {
        [Key]
        public int Cur_ID { get; set; }
        public DateTime Date { get; set; }
        public required string Cur_Abbreviation { get; set; }
        public int Cur_Scale { get; set; }
        public required string Cur_Name { get; set; }
        public decimal? Cur_OfficialRate { get; set; }
    }
}
