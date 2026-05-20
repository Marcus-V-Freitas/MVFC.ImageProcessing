# MVFC.ImageProcessing — Walkthrough Completo para o GDG

> Visão de um arquiteto que vai subir no palco. Cada item tem: **o problema real**, **onde está no código**, **criticidade** e **exemplo de solução pronto para commitar**.

---

## Arquitetura Atual — O que você tem

Antes de criticar, é importante ser honesto sobre o que está correto. O fluxo do sistema é:

```
[Upload API] → PubSub → [Converter Worker]
                      → [Thumbnail Worker]
                      → [Analysis Worker]
                      → [Delete Worker]
[Dashboard UI] ← GCS ←────────────────────
```

Camadas identificadas:
- `MVFC.Image.Domain` — handlers + contratos de domínio
- `MVFC.Image.Infra` — implementações (GCS + PubSub)
- `MVFC.Image.IoC` — composição de dependências por worker
- `MVFC.Image.Shareable` — requests, responses, configs
- Workers e APIs como entry points isolados

A direção está certa. O problema está na execução. Vamos item a item.

---

## 🔴 #1 — `Console.WriteLine` dentro do Domínio

**Criticidade: Bloqueante para apresentação**

`ImageAnalysisHandler` e `ImageConverterHandler` usam `Console.WriteLine` para logar erros dentro do domínio. Isso é a violação mais visível de Clean Architecture que existe — o domínio adquire dependência implícita de I/O, quebrando o princípio central que você quer demonstrar na palestra.

**Antes:**
```csharp
catch (Exception ex)
{
    Console.WriteLine($"Erro crítico ao tentar converter {request.FileName}. Erro: {ex.Message}");
    return Result.Fail(ex.Message);
}
```

**Depois — correto:**
```csharp
public sealed class ImageConverterHandler(
    IStorageService storage,
    IPublishService publisher,
    AppConfigConverter appConfig,
    ILogger<ImageConverterHandler> logger) : ICommandHandler<FileUploadedRequest, Result>
{
    public async ValueTask<Result> Handle(FileUploadedRequest request, CancellationToken cancellationToken = default)
    {
        var original = await storage.DownloadImageAsync(
            request.Bucket, request.FileName, cancellationToken);

        try
        {
            using var image = new MagickImage(original);
            image.Format = MagickFormat.Png;
            var bytes = image.ToByteArray();

            await storage.UploadImageAsync(
                request.Bucket, request.FileName, "image/png", bytes, cancellationToken);

            var newEvt = new FileUploadedRequest(
                request.FileName, "image/png", bytes.Length,
                request.Bucket, DateTime.UtcNow);

            var attributes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "event-type", "file.png.converted" }
            };

            await publisher.PublishAsync(
                newEvt, appConfig.PubSubConfig.Topics["FileConvertTopic"], attributes);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Falha ao converter imagem {FileName}", request.FileName);
            return Result.Fail(ex.Message);
        }
    }
}
```

> Aplica o mesmo padrão no `ImageAnalysisHandler`.

---

## 🔴 #2 — Bucket names como magic strings espalhadas por handlers

**Criticidade: Bloqueante — contradição interna visível**

`"uploads"`, `"thumbnails"` e `"analysis-results"` estão hardcoded em `ImageUploadHandler`, `ImageAnalysisHandler` e `ImageDeleteHandler`. O próprio projeto já resolve esse problema corretamente para PubSub Topics via `AppConfigUpload.PubSubConfig.Topics["ImageUploadTopic"]`. A inconsistência é gritante.

**Solução — adicionar `StorageConfig` no Shareable:**

```csharp
// src/MVFC.Image.Shareable/Configs/StorageConfig.cs
namespace MVFC.Image.Shareable.Configs;

public sealed record StorageConfig(
    string UploadBucket,
    string ThumbnailBucket,
    string AnalysisBucket);
```

**Plugar nos AppConfigs que precisam:**
```csharp
// AppConfigUpload.cs
public sealed record AppConfigUpload(
    PubSubConfig PubSubConfig,
    StorageConfig StorageConfig);

// AppConfigAnalysis.cs
public sealed record AppConfigAnalysis(
    string VisualApiUrl,
    StorageConfig StorageConfig);
```

**Uso no handler:**
```csharp
// ImageUploadHandler — ao invés de "uploads" hardcoded
await storage.UploadImageAsync(
    appConfig.StorageConfig.UploadBucket, fileName, ...);
```

**appsettings.json correspondente:**
```json
{
  "StorageConfig": {
    "UploadBucket": "uploads",
    "ThumbnailBucket": "thumbnails",
    "AnalysisBucket": "analysis-results"
  }
}
```

---

## 🔴 #3 — Projeto de testes vazio

**Criticidade: Bloqueante — destrói o argumento principal da palestra**

O `MVFC.ImageProcessing.Tests.csproj` tem apenas uma referência ao `MVFC.Pack.Testing` e zero arquivos de teste. A Clean Architecture existe **exatamente** para viabilizar testes unitários sem subir infraestrutura. Sem testes, você não tem como demonstrar o valor real do que foi construído.

**Estrutura de pastas recomendada:**
```
tests/MVFC.ImageProcessing.Tests/
  Handlers/
    ImageUploadHandlerTests.cs
    ImageAnalysisHandlerTests.cs
    ImageConverterHandlerTests.cs
    ImageDeleteHandlerTests.cs
  Validators/
    FileUploadRequestValidatorTests.cs
  Builders/
    FileUploadRequestBuilder.cs
```

**Teste completo do `ImageUploadHandler`:**
```csharp
namespace MVFC.ImageProcessing.Tests.Handlers;

public class ImageUploadHandlerTests
{
    private readonly IStorageService _storage = Substitute.For<IStorageService>();
    private readonly IPublishService _publisher = Substitute.For<IPublishService>();
    private readonly AppConfigUpload _config = new(
        new PubSubConfig("proj-test",
            new Dictionary<string, string> { ["ImageUploadTopic"] = "image-upload" }),
        new StorageConfig("uploads", "thumbnails", "analysis-results"));

    [Fact]
    public async Task Handle_ValidRequest_ShouldUploadToStorageAndPublishEvent()
    {
        // Arrange
        var handler = new ImageUploadHandler(_storage, _config, _publisher);
        var request = new FileUploadRequest("foto.jpg", "image/jpeg", 1024, [0xFF, 0xD8]);

        _storage.UploadImageAsync(Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<byte[]>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult("uploads/guid-foto.jpg"));

        // Act
        var result = await handler.Handle(request);

        // Assert
        result.IsSuccess.Should().BeTrue();

        await _storage.Received(1).UploadImageAsync(
            "uploads",
            Arg.Is<string>(name => name.EndsWith("-foto.jpg")),
            "image/jpeg",
            Arg.Any<byte[]>(),
            Arg.Any<CancellationToken>());

        await _publisher.Received(1).PublishAsync(
            Arg.Any<FileUploadedRequest>(),
            "image-upload",
            Arg.Is<IReadOnlyDictionary<string, string>>(
                d => d["event-type"] == "file.uploaded"));
    }

    [Fact]
    public async Task Handle_WhenStorageThrows_ShouldReturnFailResult()
    {
        // Arrange
        var handler = new ImageUploadHandler(_storage, _config, _publisher);
        var request = new FileUploadRequest("foto.jpg", "image/jpeg", 1024, [0xFF]);

        _storage.UploadImageAsync(Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<byte[]>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("GCS indisponível"));

        // Act
        var result = await handler.Handle(request);

        // Assert — publisher NÃO deve ser chamado se o storage falhou
        result.IsSuccess.Should().BeFalse();
        await _publisher.DidNotReceive().PublishAsync(
            Arg.Any<FileUploadedRequest>(), Arg.Any<string>(),
            Arg.Any<IReadOnlyDictionary<string, string>>());
    }
}
```

**Teste do `ImageDeleteHandler` demonstrando paralelismo:**
```csharp
public class ImageDeleteHandlerTests
{
    private readonly IStorageService _storage = Substitute.For<IStorageService>();

    [Fact]
    public async Task Handle_ShouldDeleteAllThreeArtifactsInParallel()
    {
        // Arrange
        var storageConfig = new StorageConfig("uploads", "thumbnails", "analysis-results");
        var handler = new ImageDeleteHandler(_storage, storageConfig);
        var request = new FileDeleteRequest("guid-foto.png");

        // Act
        var result = await handler.Handle(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _storage.Received(1).DeleteImageAsync("uploads", "guid-foto.png", Arg.Any<CancellationToken>());
        await _storage.Received(1).DeleteImageAsync("thumbnails", "thumb-guid-foto.png", Arg.Any<CancellationToken>());
        await _storage.Received(1).DeleteImageAsync("analysis-results", "analysis-guid-foto.png.json", Arg.Any<CancellationToken>());
    }
}
```

---

## 🟠 #4 — `catch` genérico engolindo silenciosamente exceções na Infra

**Criticidade: Alto — falha silenciosa em produção**

`StorageService.DeleteImageAsync` captura qualquer exceção e ignora. Se o GCS lançar `403 Forbidden` ou `429 QuotaExceeded`, o sistema continua como se tivesse deletado com sucesso.

**Antes:**
```csharp
catch
{
    // Ignore
}
```

**Depois — explícito sobre o que ignora e por quê:**
```csharp
public async Task DeleteImageAsync(
    string bucketName, string objectName, CancellationToken cancellationToken)
{
    try
    {
        await _storageClient.DeleteObjectAsync(
            bucketName, objectName, cancellationToken: cancellationToken);
    }
    catch (Google.GoogleApiException ex)
        when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
    {
        // 404 é esperado: objeto pode já ter sido deletado, idempotência intencional
    }
    // Qualquer outro erro (403, 429, 500) sobe normalmente
}
```

---

## 🟠 #5 — Workers sem validação do payload do PubSub

**Criticidade: Alto — crash não tratado em runtime**

Todos os workers (`Analysis`, `Thumbnail`, `Converter`, `Delete`) têm o mesmo padrão problemático:

```csharp
var evt = JsonSerializer.Deserialize<FileConvertedRequest>(json)!;  // ! suprime nullable
await mediator.Send<FileConvertedRequest, Result>(evt, ct);
return Results.Ok();  // retorna 200 mesmo se Send falhou
```

O `!` não previne `NullReferenceException` em runtime, e o resultado do `mediator.Send` é ignorado — o PubSub recebe `200 OK` e descarta a mensagem mesmo quando o handler falhou.

**Solução aplicável em todos os workers:**
```csharp
app.MapPost("/pubsub/push", async (
    PubSubRequest request,
    IMediator mediator,
    ILogger<Program> logger,
    CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(request.Message?.Data))
        return Results.BadRequest("Payload vazio.");

    var json = Encoding.UTF8.GetString(Convert.FromBase64String(request.Message.Data));
    var evt = JsonSerializer.Deserialize<FileThumbnailRequest>(json);

    if (evt is null)
    {
        logger.LogWarning("Payload inválido recebido. Data: {Data}", request.Message.Data);
        return Results.BadRequest("Deserialização falhou.");
    }

    var result = await mediator.Send<FileThumbnailRequest, Result>(evt, ct);

    return result.IsSuccess
        ? Results.Ok()
        : Results.UnprocessableEntity(result.Errors);
        // Retornar 4xx/5xx faz o PubSub retentar a mensagem (comportamento correto)
});
```

---

## 🟠 #6 — `PubSubMessageRequest` com mutabilidade acidental

**Criticidade: Alto — quebra a consistência do modelo**

O `PubSubMessageRequest` mistura primary constructor imutável com propriedade mutável:

```csharp
public sealed record PubSubMessageRequest(
    string Data,
    string MessageId,
    string PublishTime)
{
    // Propriedade mutável num record — inconsistente
    public IDictionary<string, string>? Attributes { get; set; } = new Dictionary<string, string>(...);
}
```

**Solução — tudo imutável e no construtor:**
```csharp
public sealed record PubSubMessageRequest(
    string Data,
    string MessageId,
    string PublishTime,
    IReadOnlyDictionary<string, string>? Attributes = null);
```

---

## 🟠 #7 — CORS sem policy nomeada

**Criticidade: Alto para uma demo pública**

```csharp
app.UseCors(p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
```

Num GDG, alguém **sempre** pergunta sobre segurança em demos com APIs expostas.

**Solução:**
```csharp
const string DemoPolicy = "DemoLocalPolicy";

builder.Services.AddCors(options =>
{
    options.AddPolicy(DemoPolicy, policy =>
        policy
            .WithOrigins("http://localhost:3000") // Dashboard UI — porta do docker-compose
            .AllowAnyMethod()
            .AllowAnyHeader());
});

app.UseCors(DemoPolicy);
// ⚠️ Demo policy — use AllowedOrigins por environment em produção
```

---

## 🟡 #8 — Sem validação de entrada na Upload API

**Criticidade: Médio — oportunidade de demo do stack FluentValidation**

A API de upload aceita qualquer arquivo sem validação de tipo, tamanho ou nome.

**Validator:**
```csharp
// Shareable/Validators/FileUploadRequestValidator.cs
public sealed class FileUploadRequestValidator : AbstractValidator<FileUploadRequest>
{
    private static readonly HashSet<string> _allowedTypes =
        ["image/jpeg", "image/png", "image/webp", "image/gif"];

    public FileUploadRequestValidator()
    {
        RuleFor(x => x.FileName)
            .NotEmpty()
            .MaximumLength(255)
            .Matches(@"^[\w\-. ]+$").WithMessage("Nome de arquivo inválido.");

        RuleFor(x => x.ContentType)
            .NotEmpty()
            .Must(ct => _allowedTypes.Contains(ct))
            .WithMessage($"Content-Type não suportado. Aceitos: {string.Join(", ", _allowedTypes)}");

        RuleFor(x => x.Length)
            .GreaterThan(0).WithMessage("Arquivo vazio.")
            .LessThanOrEqualTo(10 * 1024 * 1024).WithMessage("Arquivo excede 10MB.");

        RuleFor(x => x.Data)
            .NotEmpty().WithMessage("Dados do arquivo ausentes.");
    }
}
```

**Plugar no endpoint:**
```csharp
app.MapPost("/upload", async (
    IFormFile file,
    IMediator mediator,
    IValidator<FileUploadRequest> validator,
    CancellationToken ct) =>
{
    var request = new FileUploadRequest(
        file.FileName, file.ContentType, file.Length,
        await file.ToByteArrayAsync());

    var validation = await validator.ValidateAsync(request, ct);
    if (!validation.IsValid)
        return Results.ValidationProblem(validation.ToDictionary());

    var result = await mediator.Send<FileUploadRequest, Result<string>>(request, ct);

    return result.IsSuccess
        ? Results.Accepted(result.Value)
        : Results.UnprocessableEntity(result.Errors);
}).DisableAntiforgery();
```

**Testes do validator:**
```csharp
public class FileUploadRequestValidatorTests
{
    private readonly FileUploadRequestValidator _validator = new();

    [Fact]
    public async Task Validate_ValidJpeg_ShouldPass()
    {
        var request = new FileUploadRequest("foto.jpg", "image/jpeg", 1024, [0xFF, 0xD8]);
        var result = await _validator.ValidateAsync(request);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ExeFile_ShouldFail()
    {
        var request = new FileUploadRequest("virus.exe", "application/octet-stream", 1024, [0x4D, 0x5A]);
        var result = await _validator.ValidateAsync(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(FileUploadRequest.ContentType));
    }

    [Fact]
    public async Task Validate_FileTooLarge_ShouldFail()
    {
        var request = new FileUploadRequest("big.jpg", "image/jpeg", 11 * 1024 * 1024, [0xFF]);
        var result = await _validator.ValidateAsync(request);
        result.Errors.Should().Contain(e => e.PropertyName == nameof(FileUploadRequest.Length));
    }
}
```

---

## 🟡 #9 — `IStorageService` com assinatura inconsistente

**Criticidade: Médio — incoerência de API interna**

`DownloadImageAsync` e `DeleteImageAsync` têm `CancellationToken` como parâmetro posicional obrigatório, mas `UploadImageAsync` o declara como nomeado com default. Inconsistência que confunde quem lê o contrato.

**Solução — padronizar:**
```csharp
public interface IStorageService
{
    Task<MemoryStream> DownloadImageAsync(
        string bucketName, string objectName,
        CancellationToken cancellationToken = default);

    Task<string> UploadImageAsync(
        string bucketName, string objectName,
        string contentType, byte[] bytes,
        CancellationToken cancellationToken = default);

    Task DeleteImageAsync(
        string bucketName, string objectName,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> ListObjectsAsync(
        string bucketName, string prefix,
        CancellationToken cancellationToken = default);
}
```

---

## 🟡 #10 — Infra sem organização interna por responsabilidade

**Criticidade: Médio — prejudica leitura do projeto na tela do projetor**

`PublishService`, `StorageService` e `StorageServiceDependencies` estão na raiz do `MVFC.Image.Infra`.

**Estrutura sugerida:**
```
src/MVFC.Image.Infra/
  Messaging/
    PublishService.cs
  Storage/
    StorageService.cs
    StorageServiceDependencies.cs
  Usings.cs
```

---

## 🟡 #11 — `docker-compose.yml` sem healthchecks

**Criticidade: Médio — risco alto numa demo ao vivo**

O `docker-compose.yml` usa `depends_on` mas sem `condition: service_healthy`. Numa demo ao vivo, o worker pode subir antes do emulador do PubSub estar pronto e crashar na frente da plateia.

**Solução:**
```yaml
pubsub:
  image: gcr.io/google.com/cloudsdktool/google-cloud-cli:emulators
  command: >
    gcloud beta emulators pubsub start --host-port=0.0.0.0:8085
  ports:
    - "8085:8085"
  healthcheck:
    test: ["CMD", "curl", "-f", "http://localhost:8085"]
    interval: 5s
    timeout: 3s
    retries: 10

mvfc-image-upload-api:
  depends_on:
    pubsub:
      condition: service_healthy
    gcs:
      condition: service_healthy
```

---

## Roadmap de Execução para a Palestra

Ordene assim para maximizar impacto no menor tempo:

| Prioridade | Item | Tempo estimado |
|---|---|---|
| 1 | Remover `Console.WriteLine`, injetar `ILogger<T>` | 15 min |
| 2 | Criar `StorageConfig` + plugar nos configs | 20 min |
| 3 | Implementar testes dos 4 handlers | 45 min |
| 4 | Corrigir `catch` genérico no StorageService | 5 min |
| 5 | Tratar resultado do `mediator.Send` nos workers | 15 min |
| 6 | Corrigir `PubSubMessageRequest` mutabilidade | 5 min |
| 7 | Adicionar `FileUploadRequestValidator` + testes | 30 min |
| 8 | CORS com policy nomeada | 5 min |
| 9 | Healthchecks no docker-compose | 10 min |
| 10 | Reorganizar subpastas da Infra | 10 min |

> **Total realista: ~2h40** para ter o projeto em nível de palestra de alto padrão.  
> Os itens **1, 2 e 3** sozinhos já mudam completamente a percepção da audiência — se o tempo apertar, resolva esses três primeiro.
