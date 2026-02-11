namespace TfgApi.Dtos;

public record AlumnoCreateRequest(
    string Nombre,
    string Apellidos,
    string Email,
    int Edad,
    string Ciudad,
    string InstitucionEducativa,
    string PasswordHash
);
