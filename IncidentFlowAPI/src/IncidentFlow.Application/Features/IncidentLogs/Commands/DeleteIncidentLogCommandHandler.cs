using IncidentFlow.Application.Interfaces;
using MediatR;

namespace IncidentFlow.Application.Features.IncidentLogs.Commands;

public class DeleteIncidentLogCommandHandler : IRequestHandler<DeleteIncidentLogCommand>
{
    private readonly IIncidentLogRepository _repository;

    public DeleteIncidentLogCommandHandler(IIncidentLogRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(DeleteIncidentLogCommand request, CancellationToken cancellationToken)
    {
        await _repository.DeleteAsync(request.Id, cancellationToken);
    }
}