namespace FPS.Application.Common.Interfaces;

public interface IPdfService
{
    Task<byte[]> GeneratePdf<T>(string title, List<string> columns, List<T> items) where T : class;
    Task<byte[]> GenerateEmployeeCardPdf(
        string fullName,
        string fullNameEn,
        string jobTitle,
        string jobTitleEn,
        string number,
        DateOnly startDate,
        DateOnly endDate);
}
