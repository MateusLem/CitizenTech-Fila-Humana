namespace FilaHumana.Domain.Entities;

/// <summary>
/// Cidadão que usa o app Fila Humana.
/// O projeto não guarda dados pessoais reais: os registros de demonstração são fictícios.
/// </summary>
public class Usuario
{
    public int Id { get; private set; }

    public string Nome { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    /// <summary>Se o cidadão quer receber aviso quando a fila esvaziar.</summary>
    public bool ReceberAlerta { get; private set; } = true;

    public DateTime CadastradoEm { get; private set; } = DateTime.UtcNow;

    /// <summary>Uso do Entity Framework Core.</summary>
    private Usuario()
    {
    }

    public Usuario(string nome, string email, bool receberAlerta = true)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new ArgumentException("O nome é obrigatório.", nameof(nome));
        }

        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
        {
            throw new ArgumentException("Informe um e-mail válido.", nameof(email));
        }

        Nome = nome.Trim();
        Email = email.Trim();
        ReceberAlerta = receberAlerta;
    }
}
