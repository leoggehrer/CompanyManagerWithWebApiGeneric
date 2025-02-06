# CompanyManager With WebApi and Generic

**Lernziele:**

- Wie eine generische Klasse für die standard (GET, POST, PUT, PATCH und DELETE) REST-API-Operationen erstellt wird. mit ASP.NET Core Web API erstellt, konfiguriert und mit einer SQLite-Datenbank verbunden wird.


**Hinweis:** Als Startpunkt wird die Vorlage [CompanyManagerWithWebApi](https://github.com/leoggehrer/CompanyManagerWithWebApi) verwendet.

## Vorbereitung

Bevor mit der Umsetzung begonnen wird, sollte die Vorlage heruntergeladen und die Funktionalität verstanden werden.

### Analyse der Kontroller `CompaniesController`, `CustomersController` und `EmployeesController`

Wenn Sie die genannten Kontroller gegenüberstellen, dann werden Sie nur einen kleinen Teil 

- Erstellen Sie ein neues Projekt vom Typ **ASP.NET Core Web API** und vergeben Sie den Namen **CompanyManager.WebApi**.
- Verbinden Sie das Projekt **CompanyManager.WebApi** mit dem Projekt **CompanyManager.Logic**.

### Packages installieren

- Fügen Sie das Package `System.Linq.Dynamic.Core` hinzu, um Zeichenfolgen (strings) in LINQ-Abfragen zu verwenden. 
- Fügen Sie das Package `Microsoft.AspNetCore.JsonPatch` 
- und das Package `Microsoft.AspNetCore.Mvc.NewtonsoftJson`dem Projekt hinzu.
  
Das Hinzufügen des Packages erfolgt im Konsole-Programm und die Anleitung dazu finden Sie [hier](https://github.com/leoggehrer/Slides/tree/main/NutgetInstall).

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

    /// <summary>
    /// Copies the properties from another company instance.
    /// </summary>
    /// <param name="other">The company instance to copy properties from.</param>
    public virtual void CopyProperties(Logic.Contracts.ICompany other)
    {
        base.CopyProperties(other);

        Name = other.Name;
        Address = other.Address;
        Description = other.Description;
    }

    /// <summary>
    /// Creates a new company instance from an existing company.
    /// </summary>
    /// <param name="company">The company instance to copy properties from.</param>
    /// <returns>A new company instance.</returns>
    public static Company Create(Logic.Contracts.ICompany company)
    {
        var result = new Company();

        result.CopyProperties(company);
        return result;
    }
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

    /// <summary>
    /// Copies the properties from another identifiable object.
    /// </summary>
    /// <param name="other">The other identifiable object to copy properties from.</param>
    /// <exception cref="ArgumentNullException">Thrown when the other object is null.</exception>
    public virtual void CopyProperties(Logic.Contracts.IIdentifiable other)
    {
        if (other == null) throw new ArgumentNullException(nameof(other));

        Id = other.Id;
    }
}
```

### Erstellen der Kontroller-Klassen

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
    protected TModel ToModel(TEntity entity)
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

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<TModel> Post([FromBody] TModel model)
    {
        try
        {
            using var context = GetContext();
            var dbSet = GetDbSet(context);
            var entity = new TEntity();

            entity.CopyProperties(model);
            dbSet.Add(entity);
            context.SaveChanges();

            return CreatedAtAction("Get", new { id = entity.Id }, ToModel(entity));
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

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
                entity.CopyProperties(model);
                context.SaveChanges();
            }
            return entity == null ? NotFound() : Ok(ToModel(entity));
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

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

                entity.CopyProperties(model);
                context.SaveChanges();
            }
            return entity == null ? NotFound() : Ok(ToModel(entity));
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

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

Diese Anfrage listed alle `Company`-Einträge im json-Format auf.

## Hilfsmitteln

- keine

## Abgabe

- Termin: 1 Woche nach der Ausgabe
- Klasse:
- Name:

## Quellen

- keine Angabe

> **Viel Erfolg!**
