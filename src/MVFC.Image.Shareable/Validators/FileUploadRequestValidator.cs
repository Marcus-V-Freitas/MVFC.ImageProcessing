
namespace MVFC.Image.Shareable.Validators;

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
