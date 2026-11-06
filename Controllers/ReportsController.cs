using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace ContosoPizza.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    [HttpPost("generate-sales-summary")]
    public IActionResult GenerateSalesSummary()
    {
        try
        {
            // 1. Establish project directory structures 
            string currentDirectory = Directory.GetCurrentDirectory();
            string storesDirectory = Path.Combine(currentDirectory, "stores");
            string salesDir = Path.Combine(currentDirectory, "salesTotalDir");

            // Safeguard: Create output directory if it doesn't exist
            if (!Directory.Exists(salesDir))
            {
                Directory.CreateDirectory(salesDir);
            }

            // Safeguard: Verify if input data exists
            if (!Directory.Exists(storesDirectory))
            {
                return NotFound(new { message = "The 'stores' directory was not found in the project root." });
            }

            // 2. Discover all sales.json targets recursively
            var salesFiles = Directory.EnumerateFiles(storesDirectory, "sales.json", SearchOption.AllDirectories);

            if (!salesFiles.Any())
            {
                return BadRequest(new { message = "No 'sales.json' files were found inside the stores directory." });
            }

            // 3. Process individual values and aggregate balances
            double totalSales = 0;
            StringBuilder detailsBuilder = new StringBuilder();
            string reportFilePath = Path.Combine(salesDir, "salesSummary.txt");

            foreach (var file in salesFiles)
            {
                string fileName = Path.GetFileName(file);
                string fileContent = System.IO.File.ReadAllText(file);
                
                if (double.TryParse(fileContent, out double fileTotal))
                {
                    totalSales += fileTotal;
                    
                    // Uses :C format specifier for clean regional currency values
                    detailsBuilder.AppendLine($"  {fileName}: {fileTotal:C}");
                }
            }

            // 4. Construct the structural layout of the text report
            StringBuilder reportBuilder = new StringBuilder();
            reportBuilder.AppendLine("Sales Summary");
            reportBuilder.AppendLine("----------------------------");
            reportBuilder.AppendLine($" Total Sales: {totalSales:C}");
            reportBuilder.AppendLine();
            reportBuilder.AppendLine(" Details:");
            reportBuilder.Append(detailsBuilder.ToString());

            // 5. Commit to hard drive storage
            System.IO.File.WriteAllText(reportFilePath, reportBuilder.ToString());

            return Ok(new { 
                message = "Sales summary report successfully generated!", 
                outputPath = reportFilePath,
                grandTotal = totalSales.ToString("C")
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred during compilation.", error = ex.Message });
        }
    }
}
