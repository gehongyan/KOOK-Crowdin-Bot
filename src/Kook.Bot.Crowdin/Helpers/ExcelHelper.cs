using Kook.Bot.Crowdin.Data.Models;
using OfficeOpenXml;

namespace Kook.Bot.Crowdin.Helpers;

public static class ExcelHelper
{
    public static async Task<Stream> ToExcelAsync(this List<TermEntity> terms)
    {
        int totalDataRows = terms.Count;
        int targetLanguageCount = terms.Select(x => x.Translations.Count).Max();
        string sourceLanguage = terms.Select(x => x.LanguageId).Distinct().Single();
        List<string> targetLanguageIds = terms.SelectMany(x => x.Translations.Select(y => y.LanguageId)).Distinct().ToList();
        int targetLanguageColumnOffset = 3;

        ExcelPackage excelPackage = new();
        ExcelWorksheet workSheet = excelPackage.Workbook.Worksheets.Add("Terms");
        
        // fill header
        workSheet.Cells[1, 1].Value = sourceLanguage;
        workSheet.Cells[1, 2].Value = "part";
        workSheet.Cells[1, 3].Value = "description";
        for (int columnOffset = targetLanguageColumnOffset + 1; columnOffset <= targetLanguageCount + targetLanguageColumnOffset; columnOffset++)
            workSheet.Cells[1, columnOffset].Value = targetLanguageIds[columnOffset - 1 - targetLanguageColumnOffset];
        // bold header
        workSheet.Cells[1, 1, 1, targetLanguageCount + targetLanguageColumnOffset].Style.Font.Bold = true;
        
        // fill data
        for (int rowOffset = 2; rowOffset <= totalDataRows + 1; rowOffset++)
        {
            workSheet.Cells[rowOffset, 1].Value = terms[rowOffset - 2].Text;
            workSheet.Cells[rowOffset, 2].Value = terms[rowOffset - 2].PartOfSpeech;
            workSheet.Cells[rowOffset, 3].Value = terms[rowOffset - 2].Description;
            for (int columnOffset = targetLanguageColumnOffset + 1; columnOffset <= targetLanguageCount + targetLanguageColumnOffset; columnOffset++)
            {
                workSheet.Cells[rowOffset, columnOffset].Value = terms[rowOffset - 2]
                    .Translations
                    .SingleOrDefault(x => x.LanguageId == targetLanguageIds[columnOffset - 1 - targetLanguageColumnOffset])?
                    .Text;
            }
        }
        
        // auto fit columns
        workSheet.Column(1).AutoFit(15, 40);
        workSheet.Column(2).AutoFit(15, 40);
        workSheet.Column(3).AutoFit(15, 40);
        for (int columnOffset = targetLanguageColumnOffset + 1; columnOffset <= targetLanguageCount + targetLanguageColumnOffset; columnOffset++)
            workSheet.Column(columnOffset).AutoFit(15, 40);
        for (int columnOffset = 1; columnOffset <= workSheet.Dimension.End.Column; columnOffset++)
            workSheet.Column(columnOffset).Width = workSheet.Column(columnOffset).Width + 1;

        Stream outputStream = new MemoryStream();
        await excelPackage.SaveAsAsync(outputStream);
        outputStream.Position = 0;
        return outputStream;
    }
}