using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using System.Web;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CompanyManager.WebApi.Controllers
{
    using TModel = Models.Customer;
    using TEntity = Logic.Entities.Customer;
    using TContract = Common.Contracts.ICustomer;

    /// <summary>
    /// Controller for managing customers.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private const int MaxCount = 500;

        /// <summary>
        /// Gets the context for database operations.
        /// </summary>
        /// <returns>The database context.</returns>
        protected Logic.Contracts.IContext GetContext()
        {
            return Logic.DataContext.Factory.CreateContext();
        }

        /// <summary>
        /// Gets the DbSet for customer entities.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <returns>The DbSet for customer entities.</returns>
        protected DbSet<TEntity> GetDbSet(Logic.Contracts.IContext context)
        {
            return context.CustomerSet;
        }

        /// <summary>
        /// Converts a customer entity to a customer model.
        /// </summary>
        /// <param name="entity">The customer entity.</param>
        /// <returns>The customer model.</returns>
        protected virtual TModel ToModel(TEntity entity)
        {
            var result = new TModel();

            (result as TContract).CopyProperties(entity);
            return result;
        }

        /// <summary>
        /// Converts a customer model to a customer entity.
        /// </summary>
        /// <param name="model">The customer model.</param>
        /// <param name="entity">The customer entity.</param>
        /// <returns>The customer entity.</returns>
        protected virtual TEntity ToEntity(TModel model, TEntity? entity)
        {
            var result = entity ?? new TEntity();

            (result as TContract).CopyProperties(model);
            return result;
        }

        /// <summary>
        /// Gets a list of customers.
        /// </summary>
        /// <returns>A list of customer models.</returns>
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
        /// Queries customers based on a predicate.
        /// </summary>
        /// <param name="predicate">The query predicate.</param>
        /// <returns>A list of customer models.</returns>
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
        /// Gets a customer by ID.
        /// </summary>
        /// <param name="id">The customer ID.</param>
        /// <returns>The customer model.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<TModel?> Get(int id)
        {
            using var context = GetContext();
            var dbSet = GetDbSet(context);
            var result = dbSet.FirstOrDefault(e => e.Id == id);
            return result == null ? NotFound() : Ok(ToModel(result));
        }

        /// <summary>
        /// Creates a new customer.
        /// </summary>
        /// <param name="model">The customer model.</param>
        /// <returns>The created customer model.</returns>
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
                return CreatedAtAction("Get", new { id = entity.Id }, ToModel(entity));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Updates an existing customer.
        /// </summary>
        /// <param name="id">The customer ID.</param>
        /// <param name="model">The customer model.</param>
        /// <returns>The updated customer model.</returns>
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
        /// Partially updates an existing customer.
        /// </summary>
        /// <param name="id">The customer ID.</param>
        /// <param name="patchModel">The JSON patch document.</param>
        /// <returns>The updated customer model.</returns>
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
        /// Deletes a customer by ID.
        /// </summary>
        /// <param name="id">The customer ID.</param>
        /// <returns>No content if successful.</returns>
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
