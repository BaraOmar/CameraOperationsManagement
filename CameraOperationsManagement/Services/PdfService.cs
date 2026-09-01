using CameraOperationsManagement.ViewModels.Cameras;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CameraOperationsManagement.Services
{
    public class PdfService : IPdfService
    {
        public byte[] GenerateCameraHistoryPdf(
            CameraHistoryViewModel model)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);

                    page.Margin(35);

                    page.PageColor(Colors.White);

                    page.DefaultTextStyle(
                        style => style
                            .FontSize(10)
                            .FontColor("#263238"));


                    // =========================
                    // HEADER
                    // =========================

                    page.Header()
                        .PaddingBottom(15)
                        .Column(column =>
                        {
                            column.Spacing(4);

                            column.Item()
                                .Text("Camera Operations Management")
                                .FontSize(17)
                                .Bold()
                                .FontColor("#172327");

                            column.Item()
                                .Text("Camera History Report")
                                .FontSize(12)
                                .SemiBold()
                                .FontColor("#365F63");

                            column.Item()
                                .Text(
                                    $"Generated: {DateTime.Now:dd/MM/yyyy HH:mm}")
                                .FontSize(8)
                                .FontColor(Colors.Grey.Darken1);

                            column.Item()
                                .PaddingTop(8)
                                .LineHorizontal(1)
                                .LineColor("#DDE5E5");
                        });


                    // =========================
                    // CONTENT
                    // =========================

                    page.Content()
                        .PaddingVertical(10)
                        .Column(column =>
                        {
                            column.Spacing(18);


                            // CAMERA INFORMATION
                            column.Item()
                                .Element(container =>
                                    ComposeCameraInformation(
                                        container,
                                        model));


                            // VISIT HISTORY
                            column.Item()
                                .Element(container =>
                                    ComposeVisitHistory(
                                        container,
                                        model));
                        });


                    // =========================
                    // FOOTER
                    // =========================

                    page.Footer()
                        .PaddingTop(12)
                        .Row(row =>
                        {
                            row.RelativeItem()
                                .Text(
                                    $"Camera: {model.CameraName}")
                                .FontSize(8)
                                .FontColor(Colors.Grey.Darken1);


                            row.RelativeItem()
                                .AlignRight()
                                .Text(text =>
                                {
                                    text.DefaultTextStyle(
                                        style => style
                                            .FontSize(8)
                                            .FontColor(
                                                Colors.Grey.Darken1));

                                    text.Span("Page ");

                                    text.CurrentPageNumber();

                                    text.Span(" of ");

                                    text.TotalPages();
                                });
                        });
                });
            });


            return document.GeneratePdf();
        }


        private static void ComposeCameraInformation(
            IContainer container,
            CameraHistoryViewModel model)
        {
            container.Column(column =>
            {
                column.Spacing(10);


                column.Item()
                    .Text("Camera Information")
                    .FontSize(13)
                    .Bold()
                    .FontColor("#172327");


                column.Item()
                    .Border(1)
                    .BorderColor("#DDE5E5")
                    .CornerRadius(6)
                    .Padding(12)
                    .Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });


                        AddInformationCell(
                            table,
                            "Camera Name",
                            model.CameraName);

                        AddInformationCell(
                            table,
                            "Status",
                            model.IsActive
                                ? "Active"
                                : "Inactive");

                        AddInformationCell(
                            table,
                            "Site",
                            $"{model.SiteName} ({model.SiteId})");

                        AddInformationCell(
                            table,
                            "Recorder",
                            model.RecorderName);

                        AddInformationCell(
                            table,
                            "Brand",
                            DisplayValue(model.Brand));

                        AddInformationCell(
                            table,
                            "Model",
                            DisplayValue(model.Model));

                        AddInformationCell(
                            table,
                            "Serial Number",
                            DisplayValue(model.SerialNumber));

                        AddInformationCell(
                            table,
                            "Camera Type",
                            DisplayValue(model.Type));

                        AddInformationCell(
                            table,
                            "IP Address",
                            DisplayValue(model.IpAddress));

                        AddInformationCell(
                            table,
                            "Installation Location",
                            model.InstallationLocation);

                        AddInformationCell(
                            table,
                            "Installation Date",
                            model.InstallationDate.HasValue
                                ? model.InstallationDate.Value
                                    .ToString("dd/MM/yyyy")
                                : "-");
                    });
            });
        }


        private static void ComposeVisitHistory(
            IContainer container,
            CameraHistoryViewModel model)
        {
            container.Column(column =>
            {
                column.Spacing(10);


                column.Item()
                    .Text("Visit History")
                    .FontSize(13)
                    .Bold()
                    .FontColor("#172327");


                if (!model.Visits.Any())
                {
                    column.Item()
                        .Border(1)
                        .BorderColor("#DDE5E5")
                        .CornerRadius(6)
                        .Padding(18)
                        .AlignCenter()
                        .Text(
                            "No camera visits have been recorded.")
                        .FontColor(
                            Colors.Grey.Darken1);

                    return;
                }


                foreach (var visit in model.Visits)
                {
                    column.Item()
                        .Border(1)
                        .BorderColor("#DDE5E5")
                        .CornerRadius(6)
                        .Column(visitColumn =>
                        {
                            // VISIT HEADER
                            visitColumn.Item()
                                .Background("#F3F6F6")
                                .Padding(10)
                                .Row(row =>
                                {
                                    row.RelativeItem()
                                        .Column(left =>
                                        {
                                            left.Spacing(2);

                                            left.Item()
                                                .Text(
                                                    visit.VisitDate
                                                        .ToString(
                                                            "dd/MM/yyyy HH:mm"))
                                                .Bold()
                                                .FontColor(
                                                    "#172327");

                                            left.Item()
                                                .Text(
                                                    visit.Purpose)
                                                .FontSize(9)
                                                .FontColor(
                                                    Colors.Grey
                                                        .Darken1);
                                        });


                                    row.ConstantItem(95)
                                        .AlignRight()
                                        .Text(
                                            string.IsNullOrWhiteSpace(
                                                visit.MalfunctionType)
                                                ? "No Malfunction"
                                                : visit.MalfunctionType)
                                        .FontSize(8)
                                        .SemiBold()
                                        .FontColor(
                                            string.IsNullOrWhiteSpace(
                                                visit.MalfunctionType)
                                                ? "#39785C"
                                                : "#8A6420");
                                });


                            // VISIT BODY
                            visitColumn.Item()
                                .Padding(12)
                                .Column(details =>
                                {
                                    details.Spacing(10);


                                    AddDetailBlock(
                                        details,
                                        "Workers",
                                        visit.WorkerNames.Any()
                                            ? string.Join(
                                                ", ",
                                                visit.WorkerNames)
                                            : "-");


                                    AddDetailBlock(
                                        details,
                                        "Malfunction Type",
                                        DisplayValue(
                                            visit.MalfunctionType));


                                    AddDetailBlock(
                                        details,
                                        "Malfunction Description",
                                        DisplayValue(
                                            visit.MalfunctionDescription));


                                    AddDetailBlock(
                                        details,
                                        "Work Performed",
                                        DisplayValue(
                                            visit.RepairWorkPerformed));


                                    AddDetailBlock(
                                        details,
                                        "Repair Result",
                                        DisplayValue(
                                            visit.RepairResult));


                                    if (!string.IsNullOrWhiteSpace(
                                        visit.Notes))
                                    {
                                        AddDetailBlock(
                                            details,
                                            "Notes",
                                            visit.Notes);
                                    }
                                });
                        });
                }
            });
        }


        private static void AddInformationCell(
            TableDescriptor table,
            string label,
            string value)
        {
            table.Cell()
                .Padding(6)
                .Column(column =>
                {
                    column.Spacing(2);

                    column.Item()
                        .Text(label)
                        .FontSize(8)
                        .SemiBold()
                        .FontColor(
                            Colors.Grey.Darken1);

                    column.Item()
                        .Text(value)
                        .FontSize(10)
                        .FontColor("#172327");
                });
        }


        private static void AddDetailBlock(
            ColumnDescriptor column,
            string label,
            string value)
        {
            column.Item()
                .Column(inner =>
                {
                    inner.Spacing(3);

                    inner.Item()
                        .Text(label)
                        .FontSize(8)
                        .SemiBold()
                        .FontColor(
                            Colors.Grey.Darken1);

                    inner.Item()
                        .Text(value)
                        .FontSize(9)
                        .FontColor("#263238");
                });
        }


        private static string DisplayValue(
            string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? "-"
                : value.Trim();
        }
    }
}