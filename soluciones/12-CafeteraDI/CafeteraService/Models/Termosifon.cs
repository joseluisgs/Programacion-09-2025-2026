namespace CafeteraService.Models;

/// <summary>
/// Termosifón que implementa Bomba
/// </summary>
public class Termosifon : IBomba
{
    // Identificador único
    private readonly Guid _id = Guid.NewGuid();
    
    // Referencia al calentador (inyectado en el constructor)
    private readonly ICalentador _calentador;

    // Constructor con DI - recibe el calentador
    public Termosifon(ICalentador calentador)
    {
        _calentador = calentador;
    }

    // Bombean agua si el calentador está caliente
    public void Bombear()
    {
        if (_calentador.EstaCaliente())
        {
            Console.WriteLine("=> => bombeando => =>");
        }
    }

    public override string ToString()
    {
        return $"Termosifon(id={_id}, calentador={_calentador})";
    }
}
