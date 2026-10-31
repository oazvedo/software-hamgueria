namespace SoftwareHamgueria.Application.Exceptions;

/// <summary>A restaurant rule refused the request (e.g. inactive dish). The message is shown to the customer (ADR 0022).</summary>
public sealed class BusinessRuleException(string message) : Exception(message);
