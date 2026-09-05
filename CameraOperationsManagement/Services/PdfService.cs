using CameraOperationsManagement.ViewModels.Cameras;
using CameraOperationsManagement.ViewModels.Sites;
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
        public byte[] GenerateSiteHistoryPdf(
    SiteHistoryViewModel model)
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


                    // HEADER
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
                                .Text("Site History Report")
                                .FontSize(12)
                                .SemiBold()
                                .FontColor("#365F63");

                            column.Item()
                                .Text(
                                    $"Generated: {DateTime.Now:dd/MM/yyyy HH:mm}")
                                .FontSize(8)
                                .FontColor(
                                    Colors.Grey.Darken1);

                            column.Item()
                                .PaddingTop(8)
                                .LineHorizontal(1)
                                .LineColor("#DDE5E5");
                        });


                    // CONTENT
                    page.Content()
                        .PaddingVertical(10)
                        .Column(column =>
                        {
                            column.Spacing(18);

                            column.Item()
                                .Element(container =>
                                    ComposeSiteInformation(
                                        container,
                                        model));

                            column.Item()
                                .Element(container =>
                                    ComposeUnifiedSiteVisits(
                                        container,
                                        model));
                        });


                    // FOOTER
                    page.Footer()
                        .PaddingTop(12)
                        .Row(row =>
                        {
                            row.RelativeItem()
                                .Text(
                                    $"Site: {model.SiteName}")
                                .FontSize(8)
                                .FontColor(
                                    Colors.Grey.Darken1);

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
        private static void ComposeSiteInformation(
    IContainer container,
    SiteHistoryViewModel model)
        {
            container.Column(column =>
            {
                column.Spacing(10);

                column.Item()
                    .Text("Site Information")
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
                            "Site Name",
                            model.SiteName);

                        AddInformationCell(
                            table,
                            "Site ID",
                            model.SiteId);

                        AddInformationCell(
                            table,
                            "Location",
                            DisplayValue(model.Location));

                        AddInformationCell(
                            table,
                            "Status",
                            model.IsActive
                                ? "Active"
                                : "Inactive");

                        AddInformationCell(
                            table,
                            "Notes",
                            DisplayValue(model.Notes));
                    });
            });
        }
        private static void ComposeSiteVisits(
    IContainer container,
    SiteHistoryViewModel model)
        {
            container.Column(column =>
            {
                column.Spacing(10);

                column.Item()
                    .Text("Site Visits")
                    .FontSize(13)
                    .Bold()
                    .FontColor("#172327");


                if (!model.SiteVisits.Any())
                {
                    column.Item()
                        .Border(1)
                        .BorderColor("#DDE5E5")
                        .CornerRadius(6)
                        .Padding(15)
                        .Text("No site visits have been recorded.")
                        .FontColor(Colors.Grey.Darken1);

                    return;
                }


                foreach (var visit in model.SiteVisits)
                {
                    column.Item()
                        .Border(1)
                        .BorderColor("#DDE5E5")
                        .CornerRadius(6)
                        .Padding(12)
                        .Column(details =>
                        {
                            details.Spacing(8);

                            details.Item()
                                .Text(
                                    visit.VisitDate.ToString(
                                        "dd/MM/yyyy HH:mm"))
                                .Bold()
                                .FontColor("#172327");

                            AddDetailBlock(
                                details,
                                "Purpose",
                                visit.Purpose);

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
                                "Notes",
                                DisplayValue(
                                    visit.Notes));
                        });
                }
            });
        }
        private static void ComposeSiteCameraVisits(
    IContainer container,
    SiteHistoryViewModel model)
        {
            container.Column(column =>
            {
                column.Spacing(10);

                column.Item()
                    .Text("Camera Activity")
                    .FontSize(13)
                    .Bold()
                    .FontColor("#172327");


                if (!model.CameraVisits.Any())
                {
                    column.Item()
                        .Border(1)
                        .BorderColor("#DDE5E5")
                        .CornerRadius(6)
                        .Padding(15)
                        .Text(
                            "No camera activity has been recorded.")
                        .FontColor(Colors.Grey.Darken1);

                    return;
                }


                foreach (var visit in model.CameraVisits)
                {
                    column.Item()
                        .Border(1)
                        .BorderColor("#DDE5E5")
                        .CornerRadius(6)
                        .Column(visitColumn =>
                        {
                            visitColumn.Item()
                                .Background("#F3F6F6")
                                .Padding(10)
                                .Column(header =>
                                {
                                    header.Spacing(2);

                                    header.Item()
                                        .Text(
                                            visit.CameraName)
                                        .Bold()
                                        .FontColor("#172327");

                                    header.Item()
                                        .Text(
                                            $"{visit.VisitDate:dd/MM/yyyy HH:mm} · {visit.Purpose}")
                                        .FontSize(9)
                                        .FontColor(
                                            Colors.Grey.Darken1);
                                });


                            visitColumn.Item()
                                .Padding(12)
                                .Column(details =>
                                {
                                    details.Spacing(9);

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
        public byte[] GenerateCameraListPdf(
            IEnumerable<CameraListItemViewModel> cameras)
        {
            var cameraList = cameras.ToList();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());

                    page.Margin(30);

                    page.PageColor(Colors.White);

                    page.DefaultTextStyle(
                        style => style
                            .FontSize(9)
                            .FontColor("#263238"));


                    // HEADER
                    page.Header()
                        .PaddingBottom(12)
                        .Column(column =>
                        {
                            column.Spacing(4);

                            column.Item()
                                .Text("Camera Operations Management")
                                .FontSize(17)
                                .Bold()
                                .FontColor("#172327");

                            column.Item()
                                .Text("Camera Report")
                                .FontSize(12)
                                .SemiBold()
                                .FontColor("#365F63");

                            column.Item()
                                .Text(
                                    $"Generated: {DateTime.Now:dd/MM/yyyy HH:mm}")
                                .FontSize(8)
                                .FontColor(
                                    Colors.Grey.Darken1);

                            column.Item()
                                .PaddingTop(8)
                                .LineHorizontal(1)
                                .LineColor("#DDE5E5");
                        });


                    // CONTENT
                    page.Content()
                        .Column(column =>
                        {
                            column.Spacing(12);


                            column.Item()
                                .Text(
                                    $"Total Cameras: {cameraList.Count}")
                                .SemiBold();


                            if (!cameraList.Any())
                            {
                                column.Item()
                                    .PaddingTop(20)
                                    .AlignCenter()
                                    .Text(
                                        "No cameras match the selected filters.")
                                    .FontColor(
                                        Colors.Grey.Darken1);

                                return;
                            }


                            column.Item()
                                .Table(table =>
                                {
                                    // COLUMNS
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(1.5f); // Camera
                                        columns.RelativeColumn(1.2f); // Site
                                        columns.RelativeColumn(1.2f); // Recorder
                                        columns.RelativeColumn(0.8f); // Type
                                        columns.RelativeColumn(1.1f); // IP
                                        columns.RelativeColumn(1.2f); // Location
                                        columns.RelativeColumn(1.0f); // Switch
                                        columns.RelativeColumn(0.8f); // Status
                                    });


                                    // TABLE HEADER
                                    table.Header(header =>
                                    {
                                        header.Cell()
                                            .Background("#F3F6F6")
                                            .BorderBottom(1)
                                            .BorderColor("#DDE5E5")
                                            .Padding(8)
                                            .Text("Camera")
                                            .SemiBold()
                                            .FontColor("#365F63");

                                        header.Cell()
                                            .Background("#F3F6F6")
                                            .BorderBottom(1)
                                            .BorderColor("#DDE5E5")
                                            .Padding(8)
                                            .Text("Site")
                                            .SemiBold()
                                            .FontColor("#365F63");

                                        header.Cell()
                                            .Background("#F3F6F6")
                                            .BorderBottom(1)
                                            .BorderColor("#DDE5E5")
                                            .Padding(8)
                                            .Text("Recorder")
                                            .SemiBold()
                                            .FontColor("#365F63");

                                        header.Cell()
                                            .Background("#F3F6F6")
                                            .BorderBottom(1)
                                            .BorderColor("#DDE5E5")
                                            .Padding(8)
                                            .Text("Type")
                                            .SemiBold()
                                            .FontColor("#365F63");

                                        header.Cell()
                                            .Background("#F3F6F6")
                                            .BorderBottom(1)
                                            .BorderColor("#DDE5E5")
                                            .Padding(8)
                                            .Text("IP Address")
                                            .SemiBold()
                                            .FontColor("#365F63");

                                        header.Cell()
                                            .Background("#F3F6F6")
                                            .BorderBottom(1)
                                            .BorderColor("#DDE5E5")
                                            .Padding(8)
                                            .Text("Location")
                                            .SemiBold()
                                            .FontColor("#365F63");

                                        header.Cell()
                                            .Background("#F3F6F6")
                                            .BorderBottom(1)
                                            .BorderColor("#DDE5E5")
                                            .Padding(8)
                                            .Text("Switch")
                                            .SemiBold()
                                            .FontColor("#365F63");

                                        header.Cell()
                                            .Background("#F3F6F6")
                                            .BorderBottom(1)
                                            .BorderColor("#DDE5E5")
                                            .Padding(8)
                                            .Text("Status")
                                            .SemiBold()
                                            .FontColor("#365F63");
                                    });


                                    // ROWS
                                    foreach (var camera in cameraList)
                                    {
                                        AddCameraBodyCell(
                                            table,
                                            BuildCameraName(camera));

                                        AddCameraBodyCell(
                                            table,
                                            $"{camera.SiteName}\n{camera.SiteId}");

                                        AddCameraBodyCell(
                                            table,
                                            camera.RecorderName);

                                        AddCameraBodyCell(
                                            table,
                                            DisplayValue(camera.Type));

                                        AddCameraBodyCell(
                                            table,
                                            DisplayValue(camera.IpAddress));

                                        AddCameraBodyCell(
                                            table,
                                            camera.InstallationLocation);

                                        AddCameraBodyCell(
                                            table,
                                            DisplayValue(
                                                camera.NetworkSwitchName));

                                        AddCameraBodyCell(
                                            table,
                                            camera.IsActive
                                                ? "Active"
                                                : "Inactive");
                                    }
                                });
                        });


                    // FOOTER
                    page.Footer()
                        .PaddingTop(10)
                        .Row(row =>
                        {
                            row.RelativeItem()
                                .Text("Camera Report")
                                .FontSize(8)
                                .FontColor(
                                    Colors.Grey.Darken1);


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
        private static void AddCameraHeaderCell(
            TableDescriptor table,
            string text)
        {
            table.Cell()
                .Background("#F3F6F6")
                .BorderBottom(1)
                .BorderColor("#DDE5E5")
                .Padding(7)
                .Text(text)
                .FontSize(8)
                .SemiBold()
                .FontColor("#365F63");
        }
        private static void AddCameraBodyCell(
    TableDescriptor table,
    string value)
        {
            table.Cell()
                .BorderBottom(1)
                .BorderColor("#E8EEEE")
                .Padding(7)
                .Text(value)
                .FontSize(8);
        }
        private static string BuildCameraName(
    CameraListItemViewModel camera)
        {
            var details = string.Join(
                " ",
                new[]
                {
            camera.Brand,
            camera.Model
                }
                .Where(x =>
                    !string.IsNullOrWhiteSpace(x)));

            return string.IsNullOrWhiteSpace(details)
                ? camera.Name
                : $"{camera.Name}\n{details}";
        }
        public byte[] GenerateSiteStructurePdf(
    SiteStructureViewModel model)
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
                            .FontSize(9)
                            .FontColor("#263238"));


                    // HEADER
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
                                .Text("Site Structure Report")
                                .FontSize(12)
                                .SemiBold()
                                .FontColor("#365F63");

                            column.Item()
                                .Text(
                                    $"Generated: {DateTime.Now:dd/MM/yyyy HH:mm}")
                                .FontSize(8)
                                .FontColor(
                                    Colors.Grey.Darken1);

                            column.Item()
                                .PaddingTop(8)
                                .LineHorizontal(1)
                                .LineColor("#DDE5E5");
                        });


                    // CONTENT
                    page.Content()
                        .PaddingVertical(10)
                        .Column(column =>
                        {
                            column.Spacing(18);


                            // SITE
                            column.Item()
                                .AlignCenter()
                                .Width(280)
                                .Border(2)
                                .BorderColor("#365F63")
                                .CornerRadius(8)
                                .Padding(14)
                                .Column(site =>
                                {
                                    site.Spacing(3);

                                    site.Item()
                                        .AlignCenter()
                                        .Text("SITE")
                                        .FontSize(8)
                                        .SemiBold()
                                        .FontColor("#607477");

                                    site.Item()
                                        .AlignCenter()
                                        .Text(model.SiteName)
                                        .FontSize(14)
                                        .Bold()
                                        .FontColor("#172327");

                                    site.Item()
                                        .AlignCenter()
                                        .Text(model.SiteId)
                                        .FontSize(9)
                                        .FontColor(
                                            Colors.Grey.Darken1);

                                    if (!string.IsNullOrWhiteSpace(
                                        model.Location))
                                    {
                                        site.Item()
                                            .AlignCenter()
                                            .Text(model.Location)
                                            .FontSize(8)
                                            .FontColor(
                                                Colors.Grey.Darken1);
                                    }
                                });


                            // RECORDERS
                            column.Item()
                                .Column(recorders =>
                                {
                                    recorders.Spacing(10);

                                    recorders.Item()
                                        .Text("RECORDERS")
                                        .FontSize(11)
                                        .Bold()
                                        .FontColor("#365F63");


                                    if (!model.Recorders.Any())
                                    {
                                        recorders.Item()
                                            .Border(1)
                                            .BorderColor("#DDE5E5")
                                            .Padding(12)
                                            .Text(
                                                "No recorders configured for this site.")
                                            .FontColor(
                                                Colors.Grey.Darken1);
                                    }
                                    else
                                    {
                                        foreach (var recorder in model.Recorders)
                                        {
                                            recorders.Item()
                                                .Border(1)
                                                .BorderColor("#DDE5E5")
                                                .CornerRadius(6)
                                                .Column(recorderColumn =>
                                                {
                                                    recorderColumn.Item()
                                                        .Background("#F3F6F6")
                                                        .Padding(10)
                                                        .Row(row =>
                                                        {
                                                            row.RelativeItem()
                                                                .Column(info =>
                                                                {
                                                                    info.Spacing(2);

                                                                    info.Item()
                                                                        .Text(
                                                                            recorder.Type)
                                                                        .FontSize(7)
                                                                        .SemiBold()
                                                                        .FontColor(
                                                                            "#607477");

                                                                    info.Item()
                                                                        .Text(
                                                                            recorder.Name)
                                                                        .FontSize(11)
                                                                        .Bold()
                                                                        .FontColor(
                                                                            "#172327");
                                                                });


                                                            row.ConstantItem(90)
                                                                .AlignRight()
                                                                .Text(
                                                                    recorder.IsActive
                                                                        ? "Active"
                                                                        : "Inactive")
                                                                .FontSize(8)
                                                                .SemiBold();
                                                        });


                                                    if (!string.IsNullOrWhiteSpace(
                                                        recorder.NetworkSwitchName))
                                                    {
                                                        recorderColumn.Item()
                                                            .PaddingHorizontal(10)
                                                            .PaddingTop(8)
                                                            .Text(
                                                                $"Connected Switch: {recorder.NetworkSwitchName}")
                                                            .FontSize(8)
                                                            .FontColor(
                                                                Colors.Grey.Darken1);
                                                    }


                                                    recorderColumn.Item()
                                                        .Padding(10)
                                                        .Column(cameras =>
                                                        {
                                                            cameras.Spacing(6);

                                                            cameras.Item()
                                                                .Text("Cameras")
                                                                .FontSize(8)
                                                                .SemiBold()
                                                                .FontColor(
                                                                    "#607477");


                                                            if (!recorder.Cameras.Any())
                                                            {
                                                                cameras.Item()
                                                                    .Text(
                                                                        "No cameras connected.")
                                                                    .FontSize(8)
                                                                    .FontColor(
                                                                        Colors.Grey.Darken1);
                                                            }
                                                            else
                                                            {
                                                                foreach (var camera
                                                                    in recorder.Cameras)
                                                                {
                                                                    cameras.Item()
                                                                        .BorderBottom(1)
                                                                        .BorderColor(
                                                                            "#E8EEEE")
                                                                        .PaddingVertical(6)
                                                                        .Row(row =>
                                                                        {
                                                                            row.RelativeItem(1.4f)
                                                                                .Column(info =>
                                                                                {
                                                                                    info.Item()
                                                                                        .Text(
                                                                                            camera.Name)
                                                                                        .SemiBold();

                                                                                    if (!string.IsNullOrWhiteSpace(
                                                                                        camera.Type))
                                                                                    {
                                                                                        info.Item()
                                                                                            .Text(
                                                                                                camera.Type)
                                                                                            .FontSize(7)
                                                                                            .FontColor(
                                                                                                Colors.Grey.Darken1);
                                                                                    }
                                                                                });


                                                                            row.RelativeItem()
                                                                                .Text(
                                                                                    string.IsNullOrWhiteSpace(
                                                                                        camera.IpAddress)
                                                                                        ? "-"
                                                                                        : camera.IpAddress)
                                                                                .FontSize(8);


                                                                            row.RelativeItem()
                                                                                .Text(
                                                                                    camera.InstallationLocation)
                                                                                .FontSize(8);


                                                                            row.ConstantItem(55)
                                                                                .AlignRight()
                                                                                .Text(
                                                                                    camera.IsActive
                                                                                        ? "Active"
                                                                                        : "Inactive")
                                                                                .FontSize(8);
                                                                        });
                                                                }
                                                            }
                                                        });
                                                });
                                        }
                                    }
                                });


                            // SWITCHES
                            column.Item()
                                .Column(switches =>
                                {
                                    switches.Spacing(10);

                                    switches.Item()
                                        .Text("SWITCHES")
                                        .FontSize(11)
                                        .Bold()
                                        .FontColor("#365F63");


                                    if (!model.Switches.Any())
                                    {
                                        switches.Item()
                                            .Border(1)
                                            .BorderColor("#DDE5E5")
                                            .Padding(12)
                                            .Text(
                                                "No switches configured for this site.")
                                            .FontColor(
                                                Colors.Grey.Darken1);
                                    }
                                    else
                                    {
                                        foreach (var networkSwitch
                                            in model.Switches)
                                        {
                                            switches.Item()
                                                .Border(1)
                                                .BorderColor("#DDE5E5")
                                                .CornerRadius(6)
                                                .Padding(10)
                                                .Row(row =>
                                                {
                                                    row.RelativeItem()
                                                        .Column(info =>
                                                        {
                                                            info.Item()
                                                                .Text("SWITCH")
                                                                .FontSize(7)
                                                                .SemiBold()
                                                                .FontColor(
                                                                    "#607477");

                                                            info.Item()
                                                                .Text(
                                                                    networkSwitch.Name)
                                                                .SemiBold();
                                                        });


                                                    row.ConstantItem(65)
                                                        .AlignRight()
                                                        .Text(
                                                            networkSwitch.IsActive
                                                                ? "Active"
                                                                : "Inactive")
                                                        .FontSize(8);
                                                });
                                        }
                                    }
                                });
                        });


                    // FOOTER
                    page.Footer()
                        .PaddingTop(10)
                        .Row(row =>
                        {
                            row.RelativeItem()
                                .Text(
                                    $"Site: {model.SiteName} ({model.SiteId})")
                                .FontSize(8)
                                .FontColor(
                                    Colors.Grey.Darken1);


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
        private static void ComposeUnifiedSiteVisits(
    IContainer container,
    SiteHistoryViewModel model)
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
                            "No visits have been recorded for this site.")
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
                            // =========================
                            // VISIT HEADER
                            // =========================

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
                                                    visit.ComponentName)
                                                .FontSize(11)
                                                .Bold()
                                                .FontColor(
                                                    "#172327");

                                            left.Item()
                                                .Text(
                                                    visit.ComponentType
                                                        .ToString())
                                                .FontSize(8)
                                                .SemiBold()
                                                .FontColor(
                                                    "#607477");

                                            left.Item()
                                                .Text(
                                                    visit.VisitDate
                                                        .ToString(
                                                            "dd/MM/yyyy HH:mm"))
                                                .FontSize(8)
                                                .FontColor(
                                                    Colors.Grey
                                                        .Darken1);
                                        });


                                    row.RelativeItem()
                                        .AlignRight()
                                        .Column(right =>
                                        {
                                            right.Spacing(2);

                                            right.Item()
                                                .AlignRight()
                                                .Text("Purpose")
                                                .FontSize(7)
                                                .SemiBold()
                                                .FontColor(
                                                    Colors.Grey
                                                        .Darken1);

                                            right.Item()
                                                .AlignRight()
                                                .Text(
                                                    visit.Purpose)
                                                .FontSize(9);
                                        });
                                });


                            // =========================
                            // VISIT DETAILS
                            // =========================

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
    }
}