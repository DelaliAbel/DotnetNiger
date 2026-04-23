using MediatR;

namespace DotnetNiger.Community.Application.CQRS;

/// <summary>Marqueur pour les commandes CQRS.</summary>
public interface ICommand : IRequest
{
}

/// <summary>Marqueur pour les commandes CQRS retournant une réponse.</summary>
public interface ICommand<out TResponse> : IRequest<TResponse>
{
}

/// <summary>Marqueur pour les queries CQRS.</summary>
public interface IQuery<out TResponse> : IRequest<TResponse>
{
}

/// <summary>Handler pour commandes sans réponse.</summary>
public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand> where TCommand : ICommand
{
}

/// <summary>Handler pour commandes avec réponse.</summary>
public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, TResponse> where TCommand : ICommand<TResponse>
{
}

/// <summary>Handler pour queries.</summary>
public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, TResponse> where TQuery : IQuery<TResponse>
{
}
