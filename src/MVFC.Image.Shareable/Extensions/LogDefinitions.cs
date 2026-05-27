namespace MVFC.Image.Shareable.Extensions;

public static partial class LogDefinitions
{
    [LoggerMessage(Level = LogLevel.Error, Message = "Erro ao analisar: {Message}")]
    public static partial void LogErrorAnalyze(this ILogger logger, Exception ex, string message);

    [LoggerMessage(Level = LogLevel.Error, Message = "Falha ao converter imagem {FileName}")]
    public static partial void LogErrorConvert(this ILogger logger, Exception ex, string fileName);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Payload inválido recebido. Data: {Data}")]
    public static partial void LogWarningInvalidPayload(this ILogger logger, string data);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to generate thumbnail for {FileName}")]
    public static partial void LogErrorThumbnail(this ILogger logger, Exception ex, string fileName);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to delete artifacts for {FileName}")]
    public static partial void LogErrorDelete(this ILogger logger, Exception ex, string fileName);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to list gallery objects: {Message}")]
    public static partial void LogErrorGallery(this ILogger logger, Exception ex, string message);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to publish delete event for {FileName}")]
    public static partial void LogErrorDeletePublish(this ILogger logger, Exception ex, string fileName);
}
