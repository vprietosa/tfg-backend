using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Microsoft.EntityFrameworkCore;
using TfgApi.Data;
using TfgApi.Models;

var builder = WebApplication.CreateBuilder(args);


// Entity Framework + Azure SQL (con reintentos)
builder.Services.AddDbContext<TfgDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sql => sql.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null
        )
    )
);

// OpenAPI (sin UI tipo Swagger)
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); // /openapi/v1.json
}

app.UseHttpsRedirection();


// =========================
//   DTOs mínimos (solo foto)
// =========================



// =========================
//          ALUMNOS
// =========================

// GET todos los alumnos (sin password)
app.MapGet("/api/alumnos", async (TfgDbContext context) =>
{
    var alumnos = await context.Alumnos
        .AsNoTracking()
        .Select(a => new
        {
            a.Id,
            a.Nombre,
            a.Apellidos,
            a.Email,
            a.Edad,
            a.Ciudad,
            a.InstitucionEducativa,
            a.FechaRegistro,
            a.FotoPerfilUrl
        })
        .ToListAsync();

    return Results.Ok(alumnos);
});

// GET alumno por id (sin password)
app.MapGet("/api/alumnos/{id:int}", async (int id, TfgDbContext context) =>
{
    var a = await context.Alumnos
        .AsNoTracking()
        .Where(x => x.Id == id)
        .Select(alumno => new
        {
            alumno.Id,
            alumno.Nombre,
            alumno.Apellidos,
            alumno.Email,
            alumno.Edad,
            alumno.Ciudad,
            alumno.InstitucionEducativa,
            alumno.FechaRegistro,
            alumno.FotoPerfilUrl
        })
        .FirstOrDefaultAsync();

    return a is null ? Results.NotFound() : Results.Ok(a);
});

// POST crear alumno (hashea password)
app.MapPost("/api/alumnos", async (Alumno alumno, TfgDbContext context) =>
{
    if (await context.Alumnos.AnyAsync(a => a.Email == alumno.Email))
        return Results.Conflict("Ese email ya existe");

    alumno.PasswordHash = BCrypt.Net.BCrypt.HashPassword(alumno.PasswordHash);
    alumno.FechaRegistro = DateTime.UtcNow;

    context.Alumnos.Add(alumno);
    await context.SaveChangesAsync();

    return Results.Created($"/api/alumnos/{alumno.Id}", new
    {
        alumno.Id,
        alumno.Nombre,
        alumno.Apellidos,
        alumno.Email
    });
});

// PUT actualizar alumno (sin tocar password)
app.MapPut("/api/alumnos/{id:int}", async (int id, Alumno alumno, TfgDbContext context) =>
{
    var existing = await context.Alumnos.FindAsync(id);
    if (existing is null) return Results.NotFound();

    existing.Nombre = alumno.Nombre;
    existing.Apellidos = alumno.Apellidos;
    existing.Edad = alumno.Edad;
    existing.Ciudad = alumno.Ciudad;
    existing.InstitucionEducativa = alumno.InstitucionEducativa;
    // no tocamos existing.Email ni existing.PasswordHash aquí
    // no tocamos FotoPerfilUrl aquí

    await context.SaveChangesAsync();
    return Results.NoContent();
});

// DELETE alumno
app.MapDelete("/api/alumnos/{id:int}", async (int id, TfgDbContext context) =>
{
    var alumno = await context.Alumnos.FindAsync(id);
    if (alumno is null) return Results.NotFound();

    context.Alumnos.Remove(alumno);
    await context.SaveChangesAsync();
    return Results.NoContent();
});


// =========================
//         AUTONOMOS
// =========================

app.MapGet("/api/autonomos", async (TfgDbContext context) =>
    Results.Ok(await context.Autonomos.AsNoTracking().ToListAsync())
);

app.MapGet("/api/autonomos/{id:int}", async (int id, TfgDbContext context) =>
{
    var item = await context.Autonomos.FindAsync(id);
    return item is null ? Results.NotFound() : Results.Ok(item);
});

app.MapPost("/api/autonomos", async (Autonomo autonomo, TfgDbContext context) =>
{
    context.Autonomos.Add(autonomo);
    await context.SaveChangesAsync();
    return Results.Created($"/api/autonomos/{autonomo.Id}", autonomo);
});

app.MapPut("/api/autonomos/{id:int}", async (int id, Autonomo autonomo, TfgDbContext context) =>
{
    var existing = await context.Autonomos.FindAsync(id);
    if (existing is null) return Results.NotFound();

    context.Entry(existing).CurrentValues.SetValues(autonomo);
    existing.Id = id;

    await context.SaveChangesAsync();
    return Results.NoContent();
});

app.MapDelete("/api/autonomos/{id:int}", async (int id, TfgDbContext context) =>
{
    var item = await context.Autonomos.FindAsync(id);
    if (item is null) return Results.NotFound();

    context.Autonomos.Remove(item);
    await context.SaveChangesAsync();
    return Results.NoContent();
});


// =========================
//          EMPRESAS
// =========================

app.MapGet("/api/empresas", async (TfgDbContext context) =>
    Results.Ok(await context.Empresas.AsNoTracking().ToListAsync())
);

app.MapGet("/api/empresas/{id:int}", async (int id, TfgDbContext context) =>
{
    var item = await context.Empresas.FindAsync(id);
    return item is null ? Results.NotFound() : Results.Ok(item);
});

app.MapPost("/api/empresas", async (Empresa empresa, TfgDbContext context) =>
{
    context.Empresas.Add(empresa);
    await context.SaveChangesAsync();
    return Results.Created($"/api/empresas/{empresa.Id}", empresa);
});

app.MapPut("/api/empresas/{id:int}", async (int id, Empresa empresa, TfgDbContext context) =>
{
    var existing = await context.Empresas.FindAsync(id);
    if (existing is null) return Results.NotFound();

    context.Entry(existing).CurrentValues.SetValues(empresa);
    existing.Id = id;

    await context.SaveChangesAsync();
    return Results.NoContent();
});

app.MapDelete("/api/empresas/{id:int}", async (int id, TfgDbContext context) =>
{
    var item = await context.Empresas.FindAsync(id);
    if (item is null) return Results.NotFound();

    context.Empresas.Remove(item);
    await context.SaveChangesAsync();
    return Results.NoContent();
});


// =========================
//          PRACTICAS
// =========================

app.MapGet("/api/practicas", async (TfgDbContext context) =>
    Results.Ok(await context.Practicas.AsNoTracking().ToListAsync())
);

app.MapGet("/api/practicas/{id:int}", async (int id, TfgDbContext context) =>
{
    var item = await context.Practicas.FindAsync(id);
    return item is null ? Results.NotFound() : Results.Ok(item);
});

app.MapPost("/api/practicas", async (Practica practica, TfgDbContext context) =>
{
    context.Practicas.Add(practica);
    await context.SaveChangesAsync();
    return Results.Created($"/api/practicas/{practica.Id}", practica);
});

app.MapPut("/api/practicas/{id:int}", async (int id, Practica practica, TfgDbContext context) =>
{
    var existing = await context.Practicas.FindAsync(id);
    if (existing is null) return Results.NotFound();

    context.Entry(existing).CurrentValues.SetValues(practica);
    existing.Id = id;

    await context.SaveChangesAsync();
    return Results.NoContent();
});

app.MapDelete("/api/practicas/{id:int}", async (int id, TfgDbContext context) =>
{
    var item = await context.Practicas.FindAsync(id);
    if (item is null) return Results.NotFound();

    context.Practicas.Remove(item);
    await context.SaveChangesAsync();
    return Results.NoContent();
});


// =========================
//     PRACTICAS REALIZADAS
// =========================

app.MapGet("/api/practicasrealizadas", async (TfgDbContext context) =>
    Results.Ok(await context.PracticasRealizadas.AsNoTracking().ToListAsync())
);

app.MapGet("/api/practicasrealizadas/{id:int}", async (int id, TfgDbContext context) =>
{
    var item = await context.PracticasRealizadas.FindAsync(id);
    return item is null ? Results.NotFound() : Results.Ok(item);
});

app.MapPost("/api/practicasrealizadas", async (PracticaRealizada pr, TfgDbContext context) =>
{
    context.PracticasRealizadas.Add(pr);
    await context.SaveChangesAsync();
    return Results.Created($"/api/practicasrealizadas/{pr.Id}", pr);
});

app.MapPut("/api/practicasrealizadas/{id:int}", async (int id, PracticaRealizada pr, TfgDbContext context) =>
{
    var existing = await context.PracticasRealizadas.FindAsync(id);
    if (existing is null) return Results.NotFound();

    context.Entry(existing).CurrentValues.SetValues(pr);
    existing.Id = id;

    await context.SaveChangesAsync();
    return Results.NoContent();
});

app.MapDelete("/api/practicasrealizadas/{id:int}", async (int id, TfgDbContext context) =>
{
    var item = await context.PracticasRealizadas.FindAsync(id);
    if (item is null) return Results.NotFound();

    context.PracticasRealizadas.Remove(item);
    await context.SaveChangesAsync();
    return Results.NoContent();
});


// =========================
//           IMÁGENES
// =========================

// SAS SUBIDA: perfil (privado)
app.MapPost("/api/storage/sas/profile/{tipo}/{id:int}", (string tipo, int id, IConfiguration config) =>
{
    tipo = tipo.ToLowerInvariant();
    if (tipo is not ("alumnos" or "empresas" or "autonomos"))
        return Results.BadRequest("tipo debe ser: alumnos | empresas | autonomos");

    var cs = config["AzureStorage:ConnectionString"];
    var containerName = config["AzureStorage:ProfileContainer"];

    if (string.IsNullOrWhiteSpace(cs) || string.IsNullOrWhiteSpace(containerName))
        return Results.Problem("Falta configurar AzureStorage en appsettings.json");

    var blobName = $"{tipo}/{id}/profile.jpg";

    var service = new BlobServiceClient(cs);
    var container = service.GetBlobContainerClient(containerName);
    var blob = container.GetBlobClient(blobName);

    var sas = new BlobSasBuilder
    {
        BlobContainerName = containerName,
        BlobName = blobName,
        Resource = "b",
        ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(15)
    };
    sas.SetPermissions(BlobSasPermissions.Create | BlobSasPermissions.Write);

    return Results.Ok(new
    {
        blobName,
        blobUrl = blob.Uri.ToString(),              // URL base sin SAS (para BD)
        uploadUrl = blob.GenerateSasUri(sas).ToString() // URL SAS para subir
    });
});

// SAS SUBIDA: galería establishments (privado)
app.MapPost("/api/storage/sas/establishments/{tipo}/{id:int}", (string tipo, int id, IConfiguration config) =>
{
    tipo = tipo.ToLowerInvariant();
    if (tipo is not ("empresas" or "autonomos"))
        return Results.BadRequest("tipo debe ser: empresas | autonomos");

    var cs = config["AzureStorage:ConnectionString"];
    var containerName = config["AzureStorage:EstablishmentsContainer"];

    if (string.IsNullOrWhiteSpace(cs) || string.IsNullOrWhiteSpace(containerName))
        return Results.Problem("Falta configurar AzureStorage en appsettings.json");

    var fileName = $"{Guid.NewGuid():N}.jpg";
    var blobName = $"{tipo}/{id}/gallery/{fileName}";

    var service = new BlobServiceClient(cs);
    var container = service.GetBlobContainerClient(containerName);
    var blob = container.GetBlobClient(blobName);

    var sas = new BlobSasBuilder
    {
        BlobContainerName = containerName,
        BlobName = blobName,
        Resource = "b",
        ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(15)
    };
    sas.SetPermissions(BlobSasPermissions.Create | BlobSasPermissions.Write);

    return Results.Ok(new
    {
        blobName,
        blobUrl = blob.Uri.ToString(),
        uploadUrl = blob.GenerateSasUri(sas).ToString()
    });
});

// Guardar en BD la URL base (sin SAS) de la foto de perfil del alumno
app.MapPut("/api/alumnos/{id:int}/foto", async (int id, SetFotoRequest body, TfgDbContext context) =>
{
    var alumno = await context.Alumnos.FindAsync(id);
    if (alumno is null) return Results.NotFound("Alumno no encontrado");

    alumno.FotoPerfilUrl = body.FotoUrl;
    await context.SaveChangesAsync();

    return Results.Ok(new { alumno.Id, alumno.FotoPerfilUrl });
});

// SAS LECTURA: ver perfil (privado)
app.MapGet("/api/storage/sas/read/profile/{tipo}/{id:int}", async (string tipo, int id, IConfiguration config) =>
{
    tipo = tipo.ToLowerInvariant();
    if (tipo is not ("alumnos" or "empresas" or "autonomos"))
        return Results.BadRequest("tipo debe ser: alumnos | empresas | autonomos");

    var cs = config["AzureStorage:ConnectionString"];
    var containerName = config["AzureStorage:ProfileContainer"];

    if (string.IsNullOrWhiteSpace(cs) || string.IsNullOrWhiteSpace(containerName))
        return Results.Problem("Falta configurar AzureStorage en appsettings.json");

    var blobName = $"{tipo}/{id}/profile.jpg";

    var service = new BlobServiceClient(cs);
    var container = service.GetBlobContainerClient(containerName);
    var blob = container.GetBlobClient(blobName);

    if (!await blob.ExistsAsync())
        return Results.NotFound("No existe foto de perfil para ese usuario");

    var sas = new BlobSasBuilder
    {
        BlobContainerName = containerName,
        BlobName = blobName,
        Resource = "b",
        ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(30)
    };
    sas.SetPermissions(BlobSasPermissions.Read);

    return Results.Ok(new
    {
        blobName,
        blobUrl = blob.Uri.ToString(),
        readUrl = blob.GenerateSasUri(sas).ToString()
    });
});

app.Run();
public record SetFotoRequest(string FotoUrl);
