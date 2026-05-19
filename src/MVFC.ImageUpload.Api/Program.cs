var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors();

await builder.Services.RegisterUploadServicesAsync(builder.Configuration);

var app = builder.Build();

app.UseCors(p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

app.MapPost("/upload", async (IFormFile file, IMediator mediator, CancellationToken ct) =>
{
    var request = new FileUploadRequest(file.FileName, file.ContentType, file.Length, await file.ToByteArrayAsync());
    var result = await mediator.Send<FileUploadRequest, Result<string>>(request, ct);

    return result.IsSuccess
        ? Results.Accepted(result.Value)
        : Results.UnprocessableEntity(result.Errors);

}).DisableAntiforgery();

app.MapGet("/", () => "mvfc-image-upload-api ok");

await app.RunAsync();