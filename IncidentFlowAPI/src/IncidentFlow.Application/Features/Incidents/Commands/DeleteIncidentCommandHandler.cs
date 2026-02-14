using IncidentFlow.Application.Interfaces;
using MediatR;

namespace IncidentFlow.Application.Features.Incidents.Commands;

public class DeleteIncidentCommandHandler : IRequestHandler<DeleteIncidentCommand>
{
    private readonly IIncidentRepository _repository;

    public DeleteIncidentCommandHandler(IIncidentRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(DeleteIncidentCommand request, CancellationToken cancellationToken)
    {
        await _repository.DeleteAsync(request.Id, cancellationToken);
    }
}