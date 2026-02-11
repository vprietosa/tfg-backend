namespace TfgApi.Dtos;

public record AutonomoCreateRequest(
    string Nombre,
    string Oficio,
    string Ciudad,
    string Email,
    string PasswordHash
);