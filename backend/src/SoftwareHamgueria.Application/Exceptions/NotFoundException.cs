namespace SoftwareHamgueria.Application.Exceptions;

/// <summary>A record the request points to does not exist. The message is shown to the customer (ADR 0022).</summary>
public sealed class NotFoundException(string message) : Exception(message);
