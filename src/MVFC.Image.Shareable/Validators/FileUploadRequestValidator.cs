
namespace MVFC.Image.Shareable.Validators;

public sealed class FileUploadRequestValidator : AbstractValidator<FileUploadRequest>
{
    public FileUploadRequestValidator()
    {
        RuleFor(x => x.FileName)
            .NotEmpty()
            .MaximumLength(255)
            .Matches(@"^[^<>:""/\\|?*]+$").WithMessage("Nome de arquivo inválido, contém caracteres não permitidos.");

        RuleFor(x => x.ContentType)
            .NotEmpty()
            .Must(ct => ct != null && ct.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            .WithMessage("O arquivo enviado não é um formato de imagem válido.");

        RuleFor(x => x.Length)
            .GreaterThan(0).WithMessage("Arquivo vazio.")
            .LessThanOrEqualTo(10 * 1024 * 1024).WithMessage("Arquivo excede 10MB.");

        RuleFor(x => x.Data)
            .NotEmpty().WithMessage("Dados do arquivo ausentes.");
    }
}
