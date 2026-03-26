namespace CafeteraService.Models;

/// <summary>
/// Interfaz para el calentador
/// </summary>
public interface ICalentador
{
    // Enciende el calentador
    void Encender();
    // Apaga el calentador
    void Apagar();
    // Devuelve true si está caliente
    bool EstaCaliente();
}
