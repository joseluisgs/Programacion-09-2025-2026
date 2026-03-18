namespace CafeteraService.Models;

/// <summary>
/// Cafetera que usa calentador y bomba
/// </summary>
public class Cafetera : ICafetera
{
    // Identificador único
    private readonly Guid _id = Guid.NewGuid();
    
    // Referencias inyectadas
    private readonly ICalentador _calentador;
    private readonly IBomba _bomba;

    // Constructor con DI
    public Cafetera(ICalentador calentador, IBomba bomba)
    {
        _calentador = calentador;
        _bomba = bomba;
    }

    // Sirve un café
    public void Servir()
    {
        Console.WriteLine("Encendiendo...");
        _calentador.Encender();
        
        Console.WriteLine("Bombando...");
        _bomba.Bombear();
        
        Console.WriteLine("[_]P !Taza de Café! [_]P ");
        
        Console.WriteLine("Apagando...");
        _calentador.Apagar();
    }

    public override string ToString()
    {
        return $"Cafetera(id={_id}, calentador={_calentador}, bomba={_bomba})";
    }
}
