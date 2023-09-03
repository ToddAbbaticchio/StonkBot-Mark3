using System.ComponentModel.DataAnnotations;

namespace StonkBot.Data.Entities;
public class IpoListing
{
    [Key]public string Symbol { get; set; } = null!;
    public string? Name { get; set; }
    public string OfferingPrice { get; set; } = null!;
    public string? OfferAmmount { get; set; }
    public DateTime? OfferingEndDate { get; set; }
    public DateTime? ExpectedListingDate { get; set; }
    public DateTime? ScrapeDate { get; set; }
    public decimal? Open { get; set; }
    public decimal? Close { get; set; }
    public decimal? Low { get; set; }
    public decimal? High { get; set; }
    public decimal? Volume { get; set; }
    public DateTime? FirstPassDate { get; set; }
    public DateTime? LastSecondPassDate { get; set; }
    public List<IpoHData>? HData { get; set; }
}