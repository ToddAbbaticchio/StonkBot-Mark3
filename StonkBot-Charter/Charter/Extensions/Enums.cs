namespace StonkBotChartoMatic.Charter.Extensions;

public enum RunMode
{
    Local,
    Remote
}

public enum SBChart
{
    ESCandle,
    ESHighLow,
    FluxValue,
    TargetZone
}

/*    public enum Market
    {
        Day,
        Night,
        Both
    }*/

public enum SBTable
{
    es_candles,
    history_es,
}

public enum DroppedFileMode
{
    TDAmeritrade,
    TradeStation
}