using AutoMapper;
using StonkBotChartoMatic.Services.FileUtilService.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using StonkBot.Data.Entities;

namespace StonkBotChartoMatic.Services.MapperService;

public interface IMapperService
{
    List<TCandle> ConvertToTCandles(List<EsCandle> esCandles, CancellationToken cToken);
    List<TCandle> ConvertToTCandles(List<NqCandle> esCandles, CancellationToken cToken);
}

public class MapperService : IMapperService
{
    private readonly IMapper _mapper;

    public MapperService(IMapper mapper)
    {
        _mapper = mapper;
    }

    public List<TCandle> ConvertToTCandles(List<EsCandle> esCandles, CancellationToken cToken)
    {
        return esCandles
            .Select(esCandle => _mapper.Map<TCandle>(esCandle))
            .ToList();
    }

    public List<TCandle> ConvertToTCandles(List<NqCandle> nqCandles, CancellationToken cToken)
    {
        return nqCandles
            .Select(nqCandle => _mapper.Map<TCandle>(nqCandle))
            .ToList();
    }
}