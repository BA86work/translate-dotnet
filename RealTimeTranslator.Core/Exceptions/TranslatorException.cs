using System;

namespace RealTimeTranslator.Core.Exceptions;

public class TranslatorException : Exception
{
    public TranslatorErrorCode ErrorCode { get; }

    public TranslatorException(TranslatorErrorCode errorCode, string message) 
        : base(message)
    {
        ErrorCode = errorCode;
    }

    public TranslatorException(TranslatorErrorCode errorCode, string message, Exception innerException) 
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
}

public enum TranslatorErrorCode
{
    Unknown = 0,
    ApiKeyInvalid = 1,
    NetworkError = 2,
    TranslationFailed = 3,
    OcrFailed = 4,
    ScreenCaptureFailed = 5,
    DatabaseError = 6,
    UpdateFailed = 7
} 