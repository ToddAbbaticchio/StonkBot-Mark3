using StonkBot.Data.Entities;
using StonkBotChartoMatic.Services.FileUtilService.Models;

namespace StonkBotChartoMatic.Services.MapperService.Mappers;
public class TCandleMapper : SbCharterMapper
{
    public TCandleMapper()
    {
        CreateMap<EsCandle, TCandle>().ForMember(d => d.Transactions, opt => opt.NullSubstitute(null));
        CreateMap<NqCandle, TCandle>().ForMember(d => d.Transactions, opt => opt.NullSubstitute(null));
    }
}
