using ExpenseTracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace ExpenseTracker.Controllers
{
    public class DashboardController : Controller
    {
        public readonly ApplicationDbContext _context;
        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<ActionResult> Index(DateTime? startDate, DateTime? endDate)
        {
            //Last 7 days transactions
            DateTime StartDate = startDate ?? DateTime.Today.AddDays(-7);
            DateTime EndDate = endDate ?? DateTime.Today;

            List<Transaction> SelectedTransactions = await GetTransactions(StartDate, EndDate);

            //Total Income
            long TotalIncome = SelectedTransactions
                .Where(i => i.Category.Type == "Income")
                .Sum(j => j.Amount);
            ViewBag.TotalIncome = TotalIncome.ToString("C0");

            //Total Expense
            long TotalExpense = SelectedTransactions
                .Where(i => i.Category.Type == "Expense")
                .Sum(j => j.Amount);
            ViewBag.TotalExpense = TotalExpense.ToString("C0");

            //Balance 
            long Balance = TotalIncome - TotalExpense;
            CultureInfo culture = CultureInfo.CreateSpecificCulture("en-IN");
            culture.NumberFormat.CurrencyNegativePattern = 1;
            ViewBag.Balance = String.Format(culture, "{0:C0}", Balance);

            //Doughnut Chart
            ViewBag.DoughnutChartData = SelectedTransactions
                .Where(i => i.Category.Type == "Expense")
                .GroupBy(j => j.Category.CategoryId)
                .Select(k => new
                {
                    categoryTitleWithIcon = k.First().Category.Icon + " " + k.First().Category.Title,
                    amount = k.Sum(j => j.Amount),
                    formattedAmount = k.Sum(j => j.Amount).ToString("C0"),
                })
                .OrderByDescending(l => l.amount)
                .ToList();


            //Recent Transactions
            ViewBag.RecentTransactions = await _context.Transactions
                .Include(i => i.Category)
                .OrderByDescending(j => j.TDate)
                .Take(5)
                .ToListAsync();


            ViewBag.StartDate = StartDate.ToString("yyyy-MM-dd");
            ViewBag.EndDate = EndDate.ToString("yyyy-MM-dd");
            return View();
        }

        public async Task<IActionResult> Reports(DateTime? startDate, DateTime? endDate)
        {
            DateTime StartDate = startDate ?? DateTime.Today.AddDays(-15);
            DateTime EndDate = endDate ?? DateTime.Today;

            List<Transaction> SelectedTransactions = await GetTransactions(StartDate, EndDate);

            //Doughnut Chart - Expense By Category
            ViewBag.DoughnutChartData = SelectedTransactions
                .Where(i => i.Category.Type == "Expense")
                .GroupBy(j => j.Category.CategoryId)
                .Select(k => new
                {
                    categoryTitleWithIcon = k.First().Category.Icon + " " + k.First().Category.Title,
                    amount = k.Sum(j => j.Amount),
                    formattedAmount = k.Sum(j => j.Amount).ToString("C0"),
                })
                .OrderByDescending(l => l.amount)
                .ToList();

            ViewBag.TransactionsList = SelectedTransactions;

            ViewBag.StartDate = StartDate.ToString("yyyy-MM-dd");
            ViewBag.EndDate = EndDate.ToString("yyyy-MM-dd");
            return View();
        }
        private async Task<List<Transaction>> GetTransactions(DateTime startDate, DateTime endDate)
        {
            return await _context.Transactions
                .Include(x => x.Category)
                .Where(y => y.TDate.Date >= startDate.Date && y.TDate.Date <= endDate.Date)
                .ToListAsync();
        }
        public class SplineChartData
        {
            public string day;
            public long income;
            public long expense;

        }
    }
}
