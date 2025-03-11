using Microsoft.EntityFrameworkCore;

namespace CompanyManager.Logic.Contracts
{
    public interface IContext : IDisposable
    {
        DbSet<Entities.Company> CompanySet { get; }
        DbSet<Entities.Customer> CustomerSet { get; }
        DbSet<Entities.Employee> EmployeeSet { get; }

        int SaveChanges();
    }
}