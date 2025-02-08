# CompanyManager With WebApi and Generic

**Lernziele:**

- Wie eine generische Klasse für die Standard (GET, POST, PUT, PATCH und DELETE) REST-API-Operationen erstellt wird.

**Hinweis:** Als Startpunkt wird die Vorlage [CompanyManagerWithWebApi](https://github.com/leoggehrer/CompanyManagerWithWebApi) verwendet.

## Vorbereitung

Bevor mit der Umsetzung begonnen wird, sollte die Vorlage heruntergeladen und die Funktionalität verstanden werden.

### Analyse der Controller `CompaniesController`, `CustomersController` und `EmployeesController`

Wenn Sie die genannten Controller gegenüberstellen, dann werden Sie feststellen, dass nur geringe Programm-Teile unterschiedlich sind. Dies ist ein Hinweis darauf, dass wir einen **generischen-Controller** entwickeln können. Betrachten wir dazu die folgenden Programm-Ausschnitte:

```csharp
namespace CompanyManager.WebApi.Controllers
{
    using TModel = Models.Company;
    using TEntity = Logic.Entities.Company;

    [Route("api/[controller]")]
    [ApiController]
    public class CompaniesController : ControllerBase
    {
        private const int MaxCount = 500;

        protected Logic.Contracts.IContext GetContext()
        {
            return Logic.DataContext.Factory.CreateContext();
        }
        protected DbSet<TEntity> GetDbSet(Logic.Contracts.IContext context)
        {
            return context.CompanySet;
        }
        protected virtual TModel ToModel(TEntity entity)
        {
            var result = new TModel();

            result.CopyProperties(entity);
            if (entity.Customers != null)
            {
                result.Customers = entity.Customers.Select(e => Models.Customer.Create(e)).ToArray();
            }
            return result;
        }

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
    ...
    }
}

namespace CompanyManager.WebApi.Controllers
{
    using TModel = Models.Customer;
    using TEntity = Logic.Entities.Customer;

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
        protected virtual TModel ToModel(TEntity entity)
        {
            var result = new TModel();

            result.CopyProperties(entity);
            return result;
        }

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
    ...
    }
}

namespace CompanyManager.WebApi.Controllers
{
    using TModel = Models.Employee;
    using TEntity = Logic.Entities.Employee;

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
    ...
    }
}
```

#### Entwicklung der generischen Klasse `GenericController<TModel, TEntity>`

Wir sehen, dass die Controller `CompaniesController`, `CustomersController` und `EmployeesController` sehr ähnlich sind. Die Operation `Get` ist in allen Controllern identisch. Die Operationen `Post`, `Put`, `Patch` und `Delete` sind ebenfalls sehr ähnlich. Wir können also eine generische Klasse erstellen, die Standard-REST-API-Operationen implementiert. Dazu arbeiten wir zuerst die Unterschiede heraus. Im Wesentlichen sind die Unterschiede in den folgenden Punkten zu finden:

- Die Typen `TModel` und `TEntity` sind unterschiedlich. 
- Die Methode `GetDbSet` gibt das entsprechende `DbSet<TEntity>` zurück. 
- Die Methode `ToModel` konvertiert ein `TEntity`-Objekt in ein `TModel`-Objekt.

Zuerst erstellen wir eine Klasse `ContextAccessor` welche den Zugriff auf den `DbContext` und den entsprechenden `DbSet<TEntity>` ermöglicht. Diese Klasse wird in den Container der 'Dependency Injection (DI)' registriert und der Klasse `GenericController<TModel, TEntity>` referenziert. Der Aufbau der Klasse `ContextAccessor` sieht wie folgt aus:

```csharp
namespace CompanyManager.WebApi.Contracts
{
    public interface IContextAccessor : IDisposable
    {
        IContext GetContext();
        DbSet<TEntity>? GetDbSet<TEntity>() where TEntity : class;
    }
}

namespace CompanyManager.WebApi.Controllers
{
    /// <summary>
    /// Provides access to the database context and its DbSets.
    /// </summary>
    public sealed class ContextAccessor : IContextAccessor
    {
        #region fields
        private Logic.Contracts.IContext? context = null;
        #endregion fields

        /// <summary>
        /// Gets the current context or creates a new one if it doesn't exist.
        /// </summary>
        /// <returns>The current context.</returns>
        public Logic.Contracts.IContext GetContext() => context ??= Logic.DataContext.Factory.CreateContext();

        /// <summary>
        /// Gets the DbSet for the specified entity type.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <returns>The DbSet for the specified entity type, or null if the entity type is not recognized.</returns>
        public DbSet<TEntity>? GetDbSet<TEntity>() where TEntity : class
        {
            DbSet<TEntity>? result = default;

            if (typeof(TEntity) == typeof(Logic.Entities.Company))
            {
                result = GetContext().CompanySet as DbSet<TEntity>;
            }
            else if (typeof(TEntity) == typeof(Logic.Entities.Customer))
            {
                result = GetContext().CustomerSet as DbSet<TEntity>;
            }
            else if (typeof(TEntity) == typeof(Logic.Entities.Employee))
            {
                result = GetContext().EmployeeSet as DbSet<TEntity>;
            }
            return result;
        }

        /// <summary>
        /// Disposes the current context.
        /// </summary>
        public void Dispose()
        {
            context?.Dispose();
            context = null;
        }
    }
}
```

Beachten Sie, dass diese Klasse ein `IContext`-Objekt verwaltet. Das `IContext`-Objekt wird in der Methode `GetContext` erstellt, wenn es nicht bereits existiert. Das `IContext`-Objekt ist vom Typ **'Resource-Object'** und muss in der Methode `Dispose` freigegeben werden. 

Die Methode `GetDbSet` gibt das entsprechende `DbSet<TEntity>` in Abhängigkeit des generischen Parameters `TEntity` zurück. Damit ist die Voraussetzung für die Entwicklung der generischen Klasse `GenericController<TModel, TEntity>` geschaffen.

Diese Klasse wird in der **'Dependency Injection (DI)'** mit der Schnittstelle `IContextAccessor` registriert. Die Registrierung erfolgt in der Methode `Main(...)` der Klasse `Program`. Der Aufruf sieht wie folgt aus:

```csharp
...
// Add ContextAccessor to the services.
builder.Services.AddScoped<Contracts.IContextAccessor, Controllers.ContextAccessor>();
...
```

Die generische Klasse `GenericController<TModel, TEntity>` wird wie folgt implementiert:

```csharp
namespace CompanyManager.WebApi.Controllers
{
    /// <summary>
    /// A generic controller for handling CRUD operations.
    /// </summary>
    /// <typeparam name="TModel">The type of the model.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    [Route("api/[controller]")]
    [ApiController]
    public abstract class GenericController<TModel, TEntity> : ControllerBase
        where TModel : Models.ModelObject, new()
        where TEntity : Logic.Entities.EntityObject, new()
    {
        #region properties
        /// <summary>
        /// Gets the max count.
        /// </summary>
        protected virtual int MaxCount { get; } = 500;
        /// <summary>
        /// Gets the context accessor.
        /// </summary>
        protected IContextAccessor ContextAccessor { get; }
        /// <summary>
        /// Gets the context.
        /// </summary>
        protected virtual IContext Context => ContextAccessor.GetContext();
        /// <summary>
        /// Gets the DbSet.
        /// </summary>
        protected virtual DbSet<TEntity> EntitySet => ContextAccessor.GetDbSet<TEntity>() ?? throw new Exception($"Invalid DbSet<{typeof(TEntity)}>");
        #endregion properties

        protected GenericController(IContextAccessor contextAccessor) 
        {
            ContextAccessor = contextAccessor;
        }

        /// <summary>
        /// Converts an entity to a model.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The model.</returns>
        protected abstract TModel ToModel(TEntity entity);

        /// <summary>
        /// Converts an model to a entity.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="entity">The entity.</param>
        /// <returns>The entity.</returns>
        protected abstract TEntity ToEntity(TModel model, TEntity? entity);

        /// <summary>
        /// Gets all models.
        /// </summary>
        /// <returns>A list of models.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual ActionResult<IEnumerable<TModel>> Get()
        {
            var dbSet = EntitySet;
            var querySet = dbSet.AsQueryable().AsNoTracking();
            var query = querySet.Take(MaxCount).ToArray();
            var result = query.Select(e => ToModel(e));

            return Ok(result);
        }

        /// <summary>
        /// Queries models based on a predicate.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <returns>A list of models.</returns>
        [HttpGet("/api/[controller]/query/{predicate}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual ActionResult<IEnumerable<TModel>> Query(string predicate)
        {
            var dbSet = EntitySet;
            var querySet = dbSet.AsQueryable().AsNoTracking();
            var query = querySet.Where(HttpUtility.UrlDecode(predicate)).Take(MaxCount).ToArray();
            var result = query.Select(e => ToModel(e)).ToArray();

            return Ok(result);
        }

        /// <summary>
        /// Gets a model by ID.
        /// </summary>
        /// <param name="id">The ID.</param>
        /// <returns>The model.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public virtual ActionResult<TModel?> Get(int id)
        {
            var dbSet = EntitySet;
            var result = dbSet.FirstOrDefault(e => e.Id == id);

            return result == null ? NotFound() : Ok(ToModel(result));
        }

        /// <summary>
        /// Creates a new model.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>The created model.</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual ActionResult<TModel> Post([FromBody] TModel model)
        {
            try
            {
                var dbSet = EntitySet;
                var entity = ToEntity(model, null);

                dbSet.Add(entity);
                Context.SaveChanges();

                return CreatedAtAction("Get", new { id = entity.Id }, ToModel(entity));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Updates a model by ID.
        /// </summary>
        /// <param name="id">The ID.</param>
        /// <param name="model">The model.</param>
        /// <returns>The updated model.</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual ActionResult<TModel> Put(int id, [FromBody] TModel model)
        {
            try
            {
                var dbSet = EntitySet;
                var entity = dbSet.FirstOrDefault(e => e.Id == id);

                if (entity != null)
                {
                    model.Id = id;
                    entity = ToEntity(model, entity);
                    Context.SaveChanges();
                }
                return entity == null ? NotFound() : Ok(ToModel(entity));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Partially updates a model by ID.
        /// </summary>
        /// <param name="id">The ID.</param>
        /// <param name="patchModel">The patch document.</param>
        /// <returns>The updated model.</returns>
        [HttpPatch("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual ActionResult<TModel> Patch(int id, [FromBody] JsonPatchDocument<TModel> patchModel)
        {
            try
            {
                var dbSet = EntitySet;
                var entity = dbSet.FirstOrDefault(e => e.Id == id);

                if (entity != null)
                {
                    var model = ToModel(entity);

                    patchModel.ApplyTo(model);

                    entity.CopyProperties(model);
                    Context.SaveChanges();
                }
                return entity == null ? NotFound() : Ok(ToModel(entity));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Deletes a model by ID.
        /// </summary>
        /// <param name="id">The ID.</param>
        /// <returns>No content.</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual ActionResult Delete(int id)
        {
            try
            {
                var dbSet = EntitySet;
                var entity = dbSet.FirstOrDefault(e => e.Id == id);

                if (entity != null)
                {
                    dbSet.Remove(entity);
                    Context.SaveChanges();
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

Die Klasse `GenericController<TModel, TEntity>` ist eine abstrakte Klasse, die die Standard-REST-API-Operationen implementiert. Die Klasse ist generisch und erwartet die Typen `TModel` und `TEntity`. Die Klasse `TModel` muss von der Klasse `ModelObject` abgeleitet sein und die Klasse `TEntity` muss von der Klasse `EntityObject` abgeleitet sein. Beide generische Parameter müssen einen parameterlosen Konstruktor (`new()`) bereitstellen. Die Klasse `ModelObject` und `EntityObject` sind Basisklassen, die die Methode `CopyProperties` implementieren. Die Methode `CopyProperties` kopiert die Eigenschaften von einem Objekt in ein anderes Objekt. Die Methode `CopyProperties` ist in der Klasse `EntityObject` implementiert und wird von der Klasse `ModelObject` geerbt. Die Methode `CopyProperties` ist in der Klasse `ModelObject` implementiert und wird von der Klasse `TModel` geerbt.

> **Tipp:** Wenn eine generische Klasse konzipiert wird, dann sollten die Klassen-Members als `virtual` definiert werden. Damit können die Members in der abgeleiteten Klassen angepasst werden (`override`). 

Der Konstruktor `protected GenericController(IContextAccessor contextAccessor)` übernimmt die Instanz der Klasse `ContextAccessor` aus der Unterklasse. Die Klasse `ContextAccessor` wird in der **'Dependency Injection (DI)'** registriert und der Unterklasse von `GenericController<TModel, TEntity>` übergeben.

#### Verwendung der Klasse `GenericController<TModel, TEntity>`

Nun kann die generische Klasse angewendet werden und die konkreten Klassen `CompaniesController`, `CustomersController` und `EmployeesController` erstellt werden.

Die Klasse `CompaniesController` sieht wie folgt aus:

```csharp
namespace CompanyManager.WebApi.Controllers
{
    using TModel = Models.Company;
    using TEntity = Logic.Entities.Company;

    /// <summary>
    /// Controller for managing companies.
    /// </summary>
    public class CompaniesController : GenericController<TModel, TEntity>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CompaniesController"/> class.
        /// </summary>
        /// <param name="contextAccessor">The context accessor.</param>
        public CompaniesController(Contracts.IContextAccessor contextAccessor)
            : base(contextAccessor)
        {
        }

        /// <summary>
        /// Converts an entity to a model.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The model.</returns>
        protected override TModel ToModel(TEntity entity)
        {
            var result = new TModel();

            result.CopyProperties(entity);
            return result;
        }

        /// <summary>
        /// Converts a model to an entity.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="entity">The entity.</param>
        /// <returns>The entity.</returns>
        protected override TEntity ToEntity(TModel model, TEntity? entity)
        {
            var result = entity ??= new TEntity();

            result.CopyProperties(model);
            return result;
        }
    }
}
```

In der konkreten Klasse müssen nur noch die abstrakten Members implementiert werden. Die Methode `ToModel` konvertiert ein `TEntity`-Objekt in ein `TModel`-Objekt. Die Methode `ToEntity` konvertiert ein `TModel`-Objekt in ein `TEntity`-Objekt. Die Methode `ToEntity` wird in der Methode `Post` und `Put` verwendet. Die Methode `ToModel` wird in der Methode `Get` verwendet.

Diese Vorlage kann für die Klassen `CustomersController` und `EmployeesController` übernommen werden. Die Klassen `CustomersController` und `EmployeesController` sehen wie folgt aus:

```csharp
namespace CompanyManager.WebApi.Controllers
{
    using TModel = Models.Customer;
    using TEntity = Logic.Entities.Customer;

    /// <summary>
    /// Controller for handling customer-related operations.
    /// </summary>
    public class CustomersController : GenericController<TModel, TEntity>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomersController"/> class.
        /// </summary>
        /// <param name="contextAccessor">The context accessor.</param>
        public CustomersController(Contracts.IContextAccessor contextAccessor)
            : base(contextAccessor)
        {
        }

        /// <summary>
        /// Converts an entity to a model.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The model.</returns>
        protected override TModel ToModel(TEntity entity)
        {
            var result = new TModel();

            result.CopyProperties(entity);
            return result;
        }

        /// <summary>
        /// Converts a model to an entity.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="entity">The entity.</param>
        /// <returns>The entity.</returns>
        protected override TEntity ToEntity(TModel model, TEntity? entity)
        {
            var result = entity ??= new TEntity();

            result.CopyProperties(model);
            return result;
        }
    }
}

namespace CompanyManager.WebApi.Controllers
{
    using TModel = Models.Employee;
    using TEntity = Logic.Entities.Employee;

    /// <summary>
    /// Controller for handling Employee related operations.
    /// </summary>
    public class EmployeesController : GenericController<TModel, TEntity>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EmployeesController"/> class.
        /// </summary>
        /// <param name="contextAccessor">The context accessor.</param>
        public EmployeesController(Contracts.IContextAccessor contextAccessor)
            : base(contextAccessor)
        {
        }

        /// <summary>
        /// Converts an entity to a model.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The model.</returns>
        protected override TModel ToModel(TEntity entity)
        {
            var result = new TModel();

            result.CopyProperties(entity);
            return result;
        }

        /// <summary>
        /// Converts a model to an entity.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="entity">The entity.</param>
        /// <returns>The entity.</returns>
        protected override TEntity ToEntity(TModel model, TEntity? entity)
        {
            var result = entity ??= new TEntity();

            result.CopyProperties(model);
            return result;
        }
    }
}
```

#### Anpassung einer Controller-Klasse

In diesen Abschnitt soll die Anpassung der Operation `Get(int id)` in der Klasse `CompaniesController` gezeigt werden. Die Operation `Get(int id)` wird in der Klasse `GenericController<TModel, TEntity>` implementiert. Die Methode `Get(int id)` wird in der Klasse `CompaniesController` überschrieben, um die Navigationseigenschaften `Customers` zu laden. Die geänderte Klasse `CompaniesController` sieht wie folgt aus:

```csharp
namespace CompanyManager.WebApi.Controllers
{
    using TModel = Models.Company;
    using TEntity = Logic.Entities.Company;

    /// <summary>
    /// Controller for managing companies.
    /// </summary>
    public class CompaniesController : GenericController<TModel, TEntity>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CompaniesController"/> class.
        /// </summary>
        /// <param name="contextAccessor">The context accessor.</param>
        public CompaniesController(Contracts.IContextAccessor contextAccessor)
            : base(contextAccessor)
        {
        }

        /// <summary>
        /// Converts an entity to a model.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The model.</returns>
        protected override TModel ToModel(TEntity entity)
        {
            var result = new TModel();

            result.CopyProperties(entity);
            if (entity.Customers != null)
            {
                result.Customers = entity.Customers.Select(e => Models.Customer.Create(e)).ToArray();
            }
            return result;
        }

        /// <summary>
        /// Converts a model to an entity.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="entity">The entity.</param>
        /// <returns>The entity.</returns>
        protected override TEntity ToEntity(TModel model, TEntity? entity)
        {
            var result = entity ??= new TEntity();

            result.CopyProperties(model);
            return result;
        }

        /// <summary>
        /// Gets a company by ID.
        /// </summary>
        /// <param name="id">The ID.</param>
        /// <returns>The company model.</returns>
        public override ActionResult<TModel?> Get(int id)
        {
            var dbSet = EntitySet.Include(e => e.Customers);
            var result = dbSet.FirstOrDefault(e => e.Id == id);

            return result == null ? NotFound() : Ok(ToModel(result));
        }
    }
}
```

### Testen des Systems

- Testen Sie die REST-API mit dem Programm **Postman**. Ein `GET`-Request sieht wie folgt aus:

```bash
GET: https://localhost:7074/api/companies
```

Diese Anfrage listet alle `Company`-Einträge im json-Format auf.

## Hilfsmittel

- keine

## Abgabe

- Termin: 1 Woche nach der Ausgabe
- Klasse:
- Name:

## Quellen

- keine Angabe

> **Viel Erfolg!**
