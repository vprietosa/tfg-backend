using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Microsoft.EntityFrameworkCore;
using TfgApi.Data;
using TfgApi.Models;
using TfgApi.Dtos;

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

// POST crear alumno (hashea password + fecha)
app.MapPost("/api/alumnos", async (AlumnoCreateRequest req, TfgDbContext context) =>
{
    if (await context.Alumnos.AnyAsync(a => a.Email == req.Email))
        return Results.Conflict("Ese email ya existe");

    var alumno = new Alumno
    {
        Nombre = req.Nombre,
        Apellidos = req.Apellidos,
        Email = req.Email,
        Edad = req.Edad,
        Ciudad = req.Ciudad,
        InstitucionEducativa = req.InstitucionEducativa,
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.PasswordHash),
        FechaRegistro = DateTime.UtcNow,
        FotoPerfilUrl = null
    };

    context.Alumnos.Add(alumno);
    await context.SaveChangesAsync();

    return Results.Created($"/api/alumnos/{alumno.Id}", new
    {
        alumno.Id,
        alumno.Nombre,
        alumno.Apellidos,
        alumno.Email,
        alumno.FechaRegistro
    });
});


// PUT actualizar alumno (sin tocar email/password/fecha/foto)
app.MapPut("/api/alumnos/{id:int}", async (int id, Alumno alumno, TfgDbContext context) =>
{
    var existing = await context.Alumnos.FindAsync(id);
    if (existing is null) return Results.NotFound();

    existing.Nombre = alumno.Nombre;
    existing.Apellidos = alumno.Apellidos;
    existing.Edad = alumno.Edad;
    existing.Ciudad = alumno.Ciudad;
    existing.InstitucionEducativa = alumno.InstitucionEducativa;

    // no tocamos existing.Email
    // no tocamos existing.PasswordHash
    // no tocamos existing.FechaRegistro
    // no tocamos existing.FotoPerfilUrl

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

// GET todos (sin password)
app.MapGet("/api/autonomos", async (TfgDbContext context) =>
{
    var autonomos = await context.Autonomos
        .AsNoTracking()
        .Select(a => new
        {
            a.Id,
            a.Nombre,
            a.Oficio,
            a.Ciudad,
            a.Email,
            a.FechaRegistro
        })
        .ToListAsync();

    return Results.Ok(autonomos);
});

// GET por id (sin password)
app.MapGet("/api/autonomos/{id:int}", async (int id, TfgDbContext context) =>
{
    var a = await context.Autonomos
        .AsNoTracking()
        .Where(x => x.Id == id)
        .Select(x => new
        {
            x.Id,
            x.Nombre,
            x.Oficio,
            x.Ciudad,
            x.Email,
            x.FechaRegistro
        })
        .FirstOrDefaultAsync();

    return a is null ? Results.NotFound() : Results.Ok(a);
});

// POST crear (hashea password + fecha)
app.MapPost("/api/autonomos", async (AutonomoCreateRequest req, TfgDbContext context) =>
{
    if (await context.Autonomos.AnyAsync(a => a.Email == req.Email))
        return Results.Conflict("Ese email ya existe");

    var autonomo = new Autonomo
    {
        Nombre = req.Nombre,
        Oficio = req.Oficio,
        Ciudad = req.Ciudad,
        Email = req.Email,
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.PasswordHash),
        FechaRegistro = DateTime.UtcNow
    };

    context.Autonomos.Add(autonomo);
    await context.SaveChangesAsync();

    return Results.Created($"/api/autonomos/{autonomo.Id}", new
    {
        autonomo.Id,
        autonomo.Nombre,
        autonomo.Oficio,
        autonomo.Ciudad,
        autonomo.Email,
        autonomo.FechaRegistro
    });
});


// PUT actualizar (sin tocar email/password/fecha)
app.MapPut("/api/autonomos/{id:int}", async (int id, Autonomo autonomo, TfgDbContext context) =>
{
    var existing = await context.Autonomos.FindAsync(id);
    if (existing is null) return Results.NotFound();

    existing.Nombre = autonomo.Nombre;
    existing.Oficio = autonomo.Oficio;
    existing.Ciudad = autonomo.Ciudad;

    // no tocamos existing.Email
    // no tocamos existing.PasswordHash
    // no tocamos existing.FechaRegistro

    await context.SaveChangesAsync();
    return Results.NoContent();
});

// DELETE
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

// GET todas (sin password)
app.MapGet("/api/empresas", async (TfgDbContext context) =>
{
    var empresas = await context.Empresas
        .AsNoTracking()
        .Select(e => new
        {
            e.Id,
            e.NombreEmpresa,
            e.Direccion,
            e.DescripcionEmpresa,
            e.Email,
            e.FechaRegistro
        })
        .ToListAsync();

    return Results.Ok(empresas);
});

// GET por id (sin password)
app.MapGet("/api/empresas/{id:int}", async (int id, TfgDbContext context) =>
{
    var e = await context.Empresas
        .AsNoTracking()
        .Where(x => x.Id == id)
        .Select(x => new
        {
            x.Id,
            x.NombreEmpresa,
            x.Direccion,
            x.DescripcionEmpresa,
            x.Email,
            x.FechaRegistro
        })
        .FirstOrDefaultAsync();

    return e is null ? Results.NotFound() : Results.Ok(e);
});

// POST crear (hashea password + fecha)
app.MapPost("/api/empresas", async (EmpresaCreateRequest req, TfgDbContext context) =>
{
    if (await context.Empresas.AnyAsync(e => e.Email == req.Email))
        return Results.Conflict("Ese email ya existe");

    var empresa = new Empresa
    {
        NombreEmpresa = req.NombreEmpresa,
        Direccion = req.Direccion,
        DescripcionEmpresa = req.DescripcionEmpresa,
        Email = req.Email,
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.PasswordHash),
        FechaRegistro = DateTime.UtcNow
    };

    context.Empresas.Add(empresa);
    await context.SaveChangesAsync();

    return Results.Created($"/api/empresas/{empresa.Id}", new
    {
        empresa.Id,
        empresa.NombreEmpresa,
        empresa.Direccion,
        empresa.DescripcionEmpresa,
        empresa.Email,
        empresa.FechaRegistro
    });
});


// PUT actualizar (sin tocar email/password/fecha)
app.MapPut("/api/empresas/{id:int}", async (int id, Empresa empresa, TfgDbContext context) =>
{
    var existing = await context.Empresas.FindAsync(id);
    if (existing is null) return Results.NotFound();

    existing.NombreEmpresa = empresa.NombreEmpresa;
    existing.Direccion = empresa.Direccion;
    existing.DescripcionEmpresa = empresa.DescripcionEmpresa;

    // no tocamos existing.Email
    // no tocamos existing.PasswordHash
    // no tocamos existing.FechaRegistro

    await context.SaveChangesAsync();
    return Results.NoContent();
});

// DELETE
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
