# CompanyManager With Rest Console

**Lernziele:**

- Wie mit dem **HttpClient** REST-Schnittstellen abgefragt werden.
- Wie mit einer Konsolen-Programm über die REST-API abgefragt und manipuliert werden.

**Hinweis:** Als Startpunkt wird die Vorlage [CompanyManagerWithWebApi](https://github.com/leoggehrer/CompanyManagerWithWebApi) verwendet.

## Vorbereitung

Bevor mit der Umsetzung begonnen wird, sollte die Vorlage heruntergeladen und die Funktionalität verstanden werden.

### RestConApp-Projekt erstellen

- Erstellen Sie ein neues Projekt vom Typ **Core-Console** und vergeben Sie den Namen **CompanyManager.RestConApp**.
- Verbinden Sie das Projekt **CompanyManager.RestConApp** mit dem Projekt **CompanyManager.Common**.

### Packages installieren

- Fügen Sie das Package `System.Linq.Dynamic.Core` hinzu, um Zeichenfolgen (strings) in LINQ-Abfragen zu verwenden. 
- Fügen Sie das Package `Microsoft.AspNetCore.JsonPatch` 
- und das Package `Microsoft.AspNetCore.Mvc.NewtonsoftJson`dem Projekt hinzu.
  
Das Hinzufügen des Packages erfolgt im Konsolen-Programm und die Anleitung dazu finden Sie [hier](https://github.com/leoggehrer/Slides/tree/main/NugetInstall).

Initialisieren Sie die `NewtonsoftJson`-Bibliothek mit der folgenden Zeile in der Klasse `Program`.

```csharp
...
builder.Services.AddControllers()
                .AddNewtonsoftJson();   // Add this to the controllers for PATCH-operation.
...
```

### Erstellen der Models

Erstellen Sie im Projekt **CompanyManager.WebApi** einen Ordner **Models** und fügen Sie die Klassen **Company**, **Customer** und **Employee** hinzu.

Nachfolgend ein Beispiel für das **Company**-Model:

```csharp
/// <summary>
/// Represents a company entity.
/// </summary>
public class Company : ModelObject, Logic.Contracts.ICompany
{
    /// <summary>
    /// Gets or sets the name of the company.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the address of the company.
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Gets or sets the description of the company.
    /// </summary>
    public string? Description { get; set; }
}
```

Diese Implementierung kann als Vorlage für alle anderen Models verwendet werden.

**Erläuterung:**

Die abstrakte Klasse `ModelObject` ist die Basisklasse für alle Models. Es beinhaltet die Eigenschaft `Id` (diese Eigenschaft stellen alle Models bereitstellen) und eine Methode `public virtual void CopyProperties(IIdentifiable other)`.
Die Klasse **Company** erbt die Eigenschaften und Methoden der Klasse `ModelObject` und ergänzt diese um weitere Eigenschaften und Methoden. Die Methoden `public virtual void CopyProperties(ICompany company)`und das Überschreiben der Methode `public override ToString()` ist für die Entität nicht erforderlich, sind aber im Verlauf für die weitere Entwicklung hilfreich.

```csharp
/// <summary>
/// Represents an abstract base class for model objects that are identifiable.
/// </summary>
public abstract class ModelObject : Logic.Contracts.IIdentifiable
{
    /// <summary>
    /// Gets or sets the unique identifier for the model object.
    /// </summary>
    public int Id { get; set; }
}
```

### Erstellen der Controller-Klassen

Die Kontroller-Klassen nehmen eine zentrale Rolle innerhalb des **MVC-(Model-View-Controller)** Musters ein. Sie sind für die Verarbeitung von HTTP-Anfragen verantwortlich und steuern die Interaktion zwischen dem Client und der Geschäftslogik der Anwendung.

**Aufgaben der Kontroller-Klassen:**

1. **Annahme und Verarbeitung von HTTP-Anfragen**

- Ein Controller empfängt HTTP-Anfragen (z. B. GET, POST, PUT, DELETE).
- Er analysiert die übermittelten Parameter und leitet sie an die entsprechenden Methoden weiter.

2. **Auswahl und Aufruf der Geschäftslogik**

- Er ruft Services oder Repositories auf, um Daten zu verarbeiten oder aus der Datenbank abzurufen.
- Die Trennung zwischen Controller und Geschäftslogik wird durch Dependency Injection (DI) ermöglicht.

3. **Verarbeitung und Validierung von Eingaben**

- Ein Controller validiert die eingehenden Daten mithilfe von Modellvalidierung ([Required], [Range], [StringLength] usw.).
- Falls die Validierung fehlschlägt, gibt er eine entsprechende Fehlermeldung zurück (400 Bad Request).

4. **Erstellung von HTTP-Antworten**

- Er generiert und sendet Antworten an den Client in Form von JSON oder XML.
- Er setzt den passenden HTTP-Statuscode (200 OK, 201 Created, 404 Not Found, 500 Internal Server Error etc.).

5. **Routing und Endpunktverwaltung**

- Durch Attribute wie [Route] oder [HttpGet] werden Endpunkte definiert, die die Client-Anfragen steuern.

**Wichtige Aspekte eines Controllers:**

| Aspekt | Beschreibung |
|--------|--------------|
| `ApiController` | Markiert die Klasse als Web-API-Controller. |
| `ProductsController` | Der Name der konkrete Klasse muss mit dem Postfix `Controller` enden. | 
| Route("api/products")	| Definiert die Basis-URL für die API. |
| HttpGet, HttpPost	| Spezifiziert, welche HTTP-Methoden unterstützt werden. |
| Ok(), NotFound(), BadRequest() | Erzeugt standardisierte HTTP-Antworten. |
| CreatedAtAction()	| Gibt eine 201 Created-Antwort mit einer neuen Ressource zurück. |

Im folgenden wird die Kontroller-Klasse `CompaniesController` beispielhaft für alle anderen Entitäten implementiert:

```csharp
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
```

Hier ist eine Tabelle, die die wichtigsten HTTP-Methoden (GET, POST, PUT, DELETE, PATCH) in Bezug auf ihre Verwendung in einer Web-API beschreibt:

| HTTP-Methode | Beschreibung | Verwendetes Attribut | Statuscodes (Erfolgsfälle) | Beispiel |
|--------------|--------------|----------------------|----------------------------|----------|
| **GET** |	Fordert Daten vom Server an (idempotent).| [HttpGet] | 200 OK, 404 Not Found | GET /api/products |
| **POST** | Erstellt eine neue Ressource auf dem Server. | [HttpPost] | 201 Created, 400 Bad Request |	POST /api/products mit JSON-Body |
| **PUT** | Aktualisiert eine gesamte Ressource (idempotent). | [HttpPut] |	200 OK, 204 No Content, 400 Bad Request, 404 Not Found | PUT /api/products/1 mit JSON-Body |
| **PATCH**	| Aktualisiert eine Ressource teilweise. | [HttpPatch] | 200 OK, 204 No Content, 400 Bad Request, 404 Not Found | PATCH /api/products/1 mit JSON-Body |
| **DELETE** | Löscht eine Ressource vom Server. | [HttpDelete]	| 200 OK, 204 No Content, 404 Not Found	| DELETE /api/products/1 |

Diese Tabelle gibt einen strukturierten Überblick über die verschiedenen Methoden und deren typische Verwendung in einer Web-API.

#### Die Kontroller `CustomersController` und `EmployeesController` können analog zur `CompaniesController` implementiert werden.

Vorgehensweise:

Kopieren Sie die Klasse `CompaniesController` und benennen Sie sie in `CustomersController` um. Ändern Sie die Typen `Company` in `Customer`. Nachfolgend finden Sie die Änderungen, die Sie vornehmen müssen:

```csharp
namespace CompanyManager.WebApi.Controllers
{
    using TModel = Models.Customer;
    using TEntity = Logic.Entities.Customer;
    using TContract = Common.Contracts.ICustomer;

    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private const int MaxCount = 500;

        protected Logic.Contracts.IContext GetContext()
        {
            return Logic.DataContext.Factory.CreateContext();
        }
        protected DbSet<TEntity> GetDbSet(Logic.Contracts.IContext context)
        {
            return context.CustomerSet;
        }
        ...
    }
}
```

Das gleiche Vorgehen gilt für die Klasse `EmployeesController`. Nachfolgend finden Sie die Änderungen, die Sie vornehmen müssen:

```csharp
namespace CompanyManager.WebApi.Controllers
{
    using TModel = Models.Employee;
    using TEntity = Logic.Entities.Employee;
    using TContract = Common.Contracts.IEmployee;

    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private const int MaxCount = 500;

        protected Logic.Contracts.IContext GetContext()
        {
            return Logic.DataContext.Factory.CreateContext();
        }
        protected DbSet<TEntity> GetDbSet(Logic.Contracts.IContext context)
        {
            return context.EmployeeSet;
        }
        protected virtual TModel ToModel(TEntity entity)
        {
            var result = new TModel();

            result.CopyProperties(entity);
            return result;
        }
        ...
    }
}
```

### Testen des Systems

- Testen Sie die REST-API mit dem Programm **Postman**. Ein `GET`-Request sieht wie folgt aus:

```bash
GET: https://localhost:7074/api/companies
```

Diese Anfrage listet alle `Company`-Einträge im json-Format auf.

> **ACHTUNG:** `CompanyManager.WebApi` ist eine ausführbares Projekt und benötigt eigene `AppSettings`-Dateien. Kopieren Sie die beiden `appsettings`-Dateien aus dem Projekt `CompanyManager.ConApp` in das Projekt `CompanyManager.WebApi` und passen Sie die Verbindungszeichenfolge an.

## Hilfsmittel

- keine

## Abgabe

- Termin: 1 Woche nach der Ausgabe
- Klasse:
- Name:

## Quellen

- keine Angabe

> **Viel Erfolg!**
