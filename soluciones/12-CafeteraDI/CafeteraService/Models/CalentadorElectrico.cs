namespace CafeteraService.Models;

/// <summary>
/// Implementación de calentador eléctrico
/// </summary>
public class CalentadorElectrico : ICalentador
{
    // Identificador único
    private readonly Guid _id = Guid.NewGuid();

    // true si está calentando, false si está apagado
    private bool _calentando = false;

    // Enciende el calentador
    public void Encender()
    {
        _calentando = true;
        Console.WriteLine("~ ~ calentando ~ ~ ~");
    }

    // Apaga el calentador
    public void Apagar()
    {
        _calentando = false;
    }

    // Devuelve true si está caliente
    public bool EstaCaliente()
    {
        return _calentando;
    }

    public override string ToString()
    {
        return $"CalentadorElectrico(id={_id})";
    }
}
