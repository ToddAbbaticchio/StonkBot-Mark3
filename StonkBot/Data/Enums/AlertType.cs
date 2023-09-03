namespace StonkBot.Data.Enums;

public enum AlertType
{
    Unknown = 0,
    
    ErLowDate = 1,
    ErHighDate = 2,
    StartAlert1 = 3,
    StartAlert2 = 4,
    NegStartAlert1 = 5,
    NegStartAlert2 = 6,

    SureErLowDate = 11,
    SureErHighDate = 12,
    SureStartAlert1 = 13,
    SureStartAlert2 = 14,
    SureNegStartAlert1 = 15,
    SureNegStartAlert2 = 16,

    LowHalfAlert = 21,
    LowThirdAlert = 22,
    HighHalfAlert = 23,
    HighThirdAlert = 24,

    JumpHighAlert = 30,
    JumpHighReverseAlert = 31,
    JumpLowAlert = 32,
    JumpLowReverseAlert = 33,

    FourHand = 40,
    UpperShadow = 41,
    Volume = 42,
    Volume2 = 43,
    AllDailyAlerts = 49,
}