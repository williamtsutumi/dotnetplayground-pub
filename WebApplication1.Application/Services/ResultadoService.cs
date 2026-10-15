namespace WebApplication1.Application.Services;


public sealed class ResultadoService<T>
{
    private readonly T? _valor;

    public bool Sucesso => CodigoErro is null;
    public string? CodigoErro { get; }

    public T Valor => Sucesso ? _valor! : throw new InvalidOperationException("Um resultado com falha não possui valor.");

    private ResultadoService(T valor)
    {
        _valor = valor;
    }

    private ResultadoService(string erro)
    {
        CodigoErro = erro;
    }

    public static ResultadoService<T> Ok(T valor) => new(valor);

    public static ResultadoService<T> Falha(string erro) => new(erro);
}


public sealed class ResultadoService
{
    public bool Sucesso => CodigoErro is null;
    public string? CodigoErro { get; }

    private ResultadoService() { }

    private ResultadoService(string erro)
    {
        CodigoErro = erro;
    }

    public static ResultadoService Ok() => new();

    public static ResultadoService Falha(string erro) => new(erro);
}