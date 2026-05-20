
namespace MVFC.Image.Shareable.Extensions;

public static partial class LogDefinitions
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Error, Message = "Erro ao analisar: {Message}")]
    public static partial void LogErrorAnalyze(this ILogger logger, Exception ex, string message);

    [LoggerMessage(EventId = 2, Level = LogLevel.Error, Message = "Falha ao converter imagem {FileName}")]
    public static partial void LogErrorConvert(this ILogger logger, Exception ex, string fileName);

    [LoggerMessage(EventId = 3, Level = LogLevel.Warning, Message = "Payload inválido recebido. Data: {Data}")]
    public static partial void LogWarningInvalidPayload(this ILogger logger, string data);
}
