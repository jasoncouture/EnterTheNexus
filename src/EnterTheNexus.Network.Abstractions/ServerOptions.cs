using System.ComponentModel.DataAnnotations;

namespace EnterTheNexus.Network.Abstractions;

public class ServerOptions
{
    [Required]
    public required Uri ListenUri { get; set; }
}