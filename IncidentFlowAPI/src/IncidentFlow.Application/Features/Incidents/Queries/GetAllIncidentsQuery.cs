using IncidentFlow.Domain.Entities;
using MediatR;

namespace IncidentFlow.Application.Features.Incidents.Queries;

public enum IncidentListFilterType
{
	Total,
	Open,
	Critical,
	ResolvedThisWeek,
	Unassigned,
	AssignedToMe
}

public record GetAllIncidentsQuery(
	int Page = 1,
	int PageSize = 10,
	IncidentListFilterType Filter = IncidentListFilterType.Total,
	Guid? CurrentUserId = null) : IRequest<PaginatedIncidentsResult>;

public sealed record PaginatedIncidentsResult(
	IReadOnlyList<Incident> Items,
	int TotalCount,
	int Page,
	int PageSize);