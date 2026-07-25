namespace SharedKernel.Utilities.Exceptions;

public class BadRequestException(string message) : Exception(message);
public class NotFoundException(string message) : Exception(message);
public class UnauthorizedException(string message, Exception? inner = null) : Exception(message, inner);
public class ForbiddenException(string message) : Exception(message);
public class AlreadyExistsException(string message) : Exception(message);
