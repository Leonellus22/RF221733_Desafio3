namespace BolsaTrabajo.Common;

// Tipos de resultado que puede devolver una operación de la capa de negocio
public enum TipoResultado
{
    Exito,
    Validacion,
    NoEncontrado,
    NoAutorizado,
    Prohibido,
    Conflicto
}

// Envoltorio que utiliza la capa de negocio para indicar si una operación fue exitosa o no
public class ResultadoOperacion<T>
{
    public bool Exito => Tipo == TipoResultado.Exito;
    public TipoResultado Tipo { get; private set; }
    public string Mensaje { get; private set; } = string.Empty;
    public T? Datos { get; private set; }

    public static ResultadoOperacion<T> Correcto(T datos, string mensaje = "Operación realizada correctamente.")
    {
        return new ResultadoOperacion<T> { Tipo = TipoResultado.Exito, Datos = datos, Mensaje = mensaje };
    }

    public static ResultadoOperacion<T> Fallo(TipoResultado tipo, string mensaje)
    {
        return new ResultadoOperacion<T> { Tipo = tipo, Mensaje = mensaje };
    }
}
