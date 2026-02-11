namespace TfgApi.Dtos;

public record EmpresaCreateRequest(
    string NombreEmpresa,
    string Direccion,
    string? DescripcionEmpresa,
    string Email,
    string PasswordHash
);
