
var builder = WebApplication.CreateBuilder(args);

const string DemoPolicy = "DemoLocalPolicy";

builder.Services.AddCors(options =>
{
    options.AddPolicy(DemoPolicy, policy =>
        policy
            .WithOrigins("http://localhost:3000") // Dashboard UI — porta do docker-compose
            .AllowAnyMethod()
            .AllowAnyHeader());
});

builder.Services.AddSingleton<IValidator<FileUploadRequest>, FileUploadRequestValidator>();
await builder.Services.RegisterUploadServicesAsync(builder.Configuration);
builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseCors(DemoPolicy);
// ⚠️ Demo policy — use AllowedOrigins por environment em produção

app.MapPost("/upload", async (IFormFile file, IMediator mediator, IValidator<FileUploadRequest> validator, CancellationToken ct) =>
{
    var request = new FileUploadRequest(file.FileName, file.ContentType, file.Length, await file.ToByteArrayAsync());

    var validation = await validator.ValidateAsync(request, ct);
    if (!validation.IsValid)
        return Results.ValidationProblem(validation.ToDictionary());

    var result = await mediator.Send<FileUploadRequest, Result<string>>(request, ct);

    return result.IsSuccess
        ? Results.Accepted(result.Value)
        : Results.UnprocessableEntity(result.Errors);

}).DisableAntiforgery();

app.MapHealthChecks("/health");
app.MapGet("/", () => "mvfc-image-upload-api ok");

await app.RunAsync();