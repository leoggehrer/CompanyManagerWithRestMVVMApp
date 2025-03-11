using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using System.Web;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CompanyManager.WebApi.Controllers
{
    using TModel = Models.Company;
    using TEntity = Logic.Entities.Company;
    using TContract = Common.Contracts.ICompany;

    /// <summary>
    /// Controller for managing companies.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class CompaniesController : ControllerBase
    {
        private const int MaxCount = 500;

        /// <summary>
        /// Gets the context for the database operations.
        /// </summary>
        /// <returns>The database context.</returns>
        protected Logic.Contracts.IContext GetContext()
        {
            return Logic.DataContext.Factory.CreateContext();
        }

        /// <summary>
        /// Gets the DbSet for the company entity.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <returns>The DbSet for the company entity.</returns>
        protected DbSet<TEntity> GetDbSet(Logic.Contracts.IContext context)
        {
            return context.CompanySet;
        }

        /// <summary>
        /// Converts a company entity to a company model.
        /// </summary>
        /// <param name="entity">The company entity.</param>
        /// <returns>The company model.</returns>
        protected virtual TModel ToModel(TEntity entity)
        {
            var result = new TModel();

            (result as TContract).CopyProperties(entity);
            if (entity.Customers != null)
            {
                result.Customers = [.. entity.Customers.Select(e => 
                {
                    var result = new Models.Customer();

                    (result as Common.Contracts.ICustomer).CopyProperties(e);
                    return result;
                })];
            }
            return result;
        }

        /// <summary>
        /// Converts a company model to a company entity.
        /// </summary>
        /// <param name="model">The company model.</param>
        /// <param name="entity">The existing company entity, or null to create a new entity.</param>
        /// <returns>The company entity.</returns>
        protected virtual TEntity ToEntity(TModel model, TEntity? entity)
        {
            TEntity result = entity ?? new TEntity();

            (result as TContract).CopyProperties(model);
            return result;
        }

        /// <summary>
        /// Gets a list of companies.
        /// </summary>
        /// <returns>A list of company models.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<TModel>> Get()
        {
            using var context = GetContext();
            var dbSet = GetDbSet(context);
            var querySet = dbSet.AsQueryable().AsNoTracking();
            var query = querySet.Take(MaxCount).ToArray();
            var result = query.Select(e => ToModel(e));

            return Ok(result);
        }

        /// <summary>
        /// Queries companies based on a predicate.
        /// </summary>
        /// <param name="predicate">The query predicate.</param>
        /// <returns>A list of company models that match the predicate.</returns>
        [HttpGet("/api/[controller]/query/{predicate}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<TModel>> Query(string predicate)
        {
            using var context = GetContext();
            var dbSet = GetDbSet(context);
            var querySet = dbSet.AsQueryable().AsNoTracking();
            var query = querySet.Where(HttpUtility.UrlDecode(predicate)).Take(MaxCount).ToArray();
            var result = query.Select(e => ToModel(e));

            return Ok(result);
        }

        /// <summary>
        /// Gets a company by its ID.
        /// </summary>
        /// <param name="id">The company ID.</param>
        /// <returns>The company model.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<TModel?> Get(int id)
        {
            using var context = GetContext();
            var dbSet = GetDbSet(context);
            var result = dbSet.Include(e => e.Customers).FirstOrDefault(e => e.Id == id);

            return result == null ? NotFound() : Ok(ToModel(result));
        }

        /// <summary>
        /// Creates a new company.
        /// </summary>
        /// <param name="model">The company model.</param>
        /// <returns>The created company model.</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<TModel> Post([FromBody] TModel model)
        {
            try
            {
                using var context = GetContext();
                var dbSet = GetDbSet(context);
                var entity = ToEntity(model, null);

                (entity as TContract).CopyProperties(model);
                dbSet.Add(entity);
                context.SaveChanges();

                return CreatedAtAction("Get", new { id = entity.Id }, entity);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Updates an existing company.
        /// </summary>
        /// <param name="id">The company ID.</param>
        /// <param name="model">The company model.</param>
        /// <returns>The updated company model.</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<TModel> Put(int id, [FromBody] TModel model)
        {
            try
            {
                using var context = GetContext();
                var dbSet = GetDbSet(context);
                var entity = dbSet.FirstOrDefault(e => e.Id == id);

                if (entity != null)
                {
                    model.Id = id;
                    entity = ToEntity(model, entity);
                    context.SaveChanges();
                }
                return entity == null ? NotFound() : Ok(ToModel(entity));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Partially updates an existing company.
        /// </summary>
        /// <param name="id">The company ID.</param>
        /// <param name="patchModel">The JSON patch document.</param>
        /// <returns>The updated company model.</returns>
        [HttpPatch("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<TModel> Patch(int id, [FromBody] JsonPatchDocument<TModel> patchModel)
        {
            try
            {
                using var context = GetContext();
                var dbSet = GetDbSet(context);
                var entity = dbSet.FirstOrDefault(e => e.Id == id);

                if (entity != null)
                {
                    var model = ToModel(entity);

                    patchModel.ApplyTo(model);

                    (entity as TContract).CopyProperties(model);
                    context.SaveChanges();
                }
                return entity == null ? NotFound() : Ok(ToModel(entity));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Deletes a company by its ID.
        /// </summary>
        /// <param name="id">The company ID.</param>
        /// <returns>No content if the deletion was successful.</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult Delete(int id)
        {
            try
            {
                using var context = GetContext();
                var dbSet = GetDbSet(context);
                var entity = dbSet.FirstOrDefault(e => e.Id == id);

                if (entity != null)
                {
                    dbSet.Remove(entity);
                    context.SaveChanges();
                }
                return entity == null ? NotFound() : NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
