using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using MyWay.Models;

namespace MyWay.Services
{
    public class PdfExportService
    {
        public async Task<string> ExportStatsAsync(List<DailyRecord> records, string period)
        {
            return await Task.Run(() =>
            {
                var doc = new PdfDocument();
                doc.Info.Title = $"MyWay – Statystyki {period}";
                var page = doc.AddPage();
                var gfx = XGraphics.FromPdfPage(page);

                var fontBold = new XFont("Arial", 20, XFontStyleEx.Bold);
                var fontTitle = new XFont("Arial", 14, XFontStyleEx.Bold);
                var fontNormal = new XFont("Arial", 11, XFontStyleEx.Regular);
                var fontSmall = new XFont("Arial", 9, XFontStyleEx.Regular);

                var accent = XBrushes.DarkSlateBlue;
                var dark = XBrushes.Black;
                var gray = new XSolidBrush(XColor.FromArgb(100, 100, 100));

                double y = 40;

                // Header
                gfx.DrawString("MyWay", fontBold, accent, new XRect(40, y, 500, 30), XStringFormats.TopLeft);
                y += 30;
                gfx.DrawString($"Raport statystyk — {period}", fontNormal, gray,
                    new XRect(40, y, 500, 20), XStringFormats.TopLeft);
                y += 20;
                gfx.DrawString($"Wygenerowano: {DateTime.Now:dd.MM.yyyy HH:mm}", fontSmall, gray,
                    new XRect(40, y, 500, 16), XStringFormats.TopLeft);
                y += 30;

                // Separator
                gfx.DrawLine(XPens.LightGray, 40, y, page.Width - 40, y);
                y += 20;

                if (records.Count == 0)
                {
                    gfx.DrawString("Brak danych dla wybranego okresu.", fontNormal, dark,
                        new XRect(40, y, 500, 30), XStringFormats.TopLeft);
                }
                else
                {
                    // Summary
                    int totalPoints = 0, totalExtra = 0;
                    double avgMood = 0;
                    foreach (var r in records)
                    {
                        totalPoints += r.TaskPoints;
                        totalExtra += r.ExtraPoints;
                        avgMood += r.MoodScore;
                    }
                    avgMood /= records.Count;

                    gfx.DrawString("Podsumowanie okresu", fontTitle, accent,
                        new XRect(40, y, 500, 20), XStringFormats.TopLeft);
                    y += 28;

                    void DrawStat(string label, string value)
                    {
                        gfx.DrawString($"• {label}", fontNormal, dark,
                            new XRect(50, y, 300, 18), XStringFormats.TopLeft);
                        gfx.DrawString(value, fontNormal, accent,
                            new XRect(350, y, 160, 18), XStringFormats.TopLeft);
                        y += 20;
                    }

                    DrawStat("Liczba dni z danymi:", $"{records.Count}");
                    DrawStat("Łączne punkty z zadań:", $"{totalPoints} pkt");
                    DrawStat("Łączne punkty ekstra:", $"{totalExtra} pkt");
                    DrawStat("Suma punktów:", $"{totalPoints + totalExtra} pkt");
                    DrawStat("Średni nastrój:", $"{avgMood:F1} / 5");

                    y += 10;
                    gfx.DrawLine(XPens.LightGray, 40, y, page.Width - 40, y);
                    y += 20;

                    // Daily breakdown
                    gfx.DrawString("Szczegóły dzienne", fontTitle, accent,
                        new XRect(40, y, 500, 20), XStringFormats.TopLeft);
                    y += 28;

                    // Table header
                    gfx.DrawString("Data", fontSmall, gray, new XRect(50, y, 80, 16), XStringFormats.TopLeft);
                    gfx.DrawString("Nastrój", fontSmall, gray, new XRect(140, y, 60, 16), XStringFormats.TopLeft);
                    gfx.DrawString("Pkt. zadania", fontSmall, gray, new XRect(210, y, 90, 16), XStringFormats.TopLeft);
                    gfx.DrawString("Pkt. ekstra", fontSmall, gray, new XRect(310, y, 80, 16), XStringFormats.TopLeft);
                    gfx.DrawString("Suma", fontSmall, gray, new XRect(400, y, 60, 16), XStringFormats.TopLeft);
                    y += 18;
                    gfx.DrawLine(XPens.LightGray, 40, y, page.Width - 40, y);
                    y += 6;

                    foreach (var r in records)
                    {
                        if (y > page.Height - 60) break;
                        gfx.DrawString(r.Date.ToString("dd.MM.yyyy"), fontSmall, dark, new XRect(50, y, 80, 14), XStringFormats.TopLeft);
                        gfx.DrawString($"{r.MoodScore}/5", fontSmall, dark, new XRect(140, y, 60, 14), XStringFormats.TopLeft);
                        gfx.DrawString($"{r.TaskPoints}", fontSmall, dark, new XRect(210, y, 90, 14), XStringFormats.TopLeft);
                        gfx.DrawString($"{r.ExtraPoints}", fontSmall, dark, new XRect(310, y, 80, 14), XStringFormats.TopLeft);
                        gfx.DrawString($"{r.TotalPoints}", fontSmall, accent, new XRect(400, y, 60, 14), XStringFormats.TopLeft);
                        y += 16;
                    }
                }

                // Footer
                y = page.Height - 30;
                gfx.DrawString("MyWay — Twój menedżer produktywności", fontSmall, gray,
                    new XRect(40, y, 500, 16), XStringFormats.TopLeft);

                var desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                var filename = Path.Combine(desktop, $"MyWay_Stats_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
                doc.Save(filename);
                return filename;
            });
        }
    }
}
