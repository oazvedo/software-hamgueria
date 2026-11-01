namespace SoftwareHamgueria.Application.Exceptions;

/// <summary>The caller is identified but may not do this (403). Message in Portuguese, safe to show.</summary>
public class ForbiddenException(string message) : Exception(message);
