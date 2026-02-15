export interface Incident {
  id: string;
  title: string;
  description: string;
  status: string;
  severity: string;
  createdBy: string;
  assignedTo?: string | null;
  createdAt: string;
  resolvedAt?: string | null;
  updatedAt: string;
}

export interface IncidentCreateInput {
  title: string;
  description: string;
  severity: number;
  createdBy: string;
  assignedTo?: string | null;
}

export interface IncidentUpdateInput {
  title?: string;
  description?: string;
  severity?: number;
  assignedTo?: string | null;
}

export enum IncidentStatus
{
    Open,
    InProgress,
    Resolved,
    Closed
}

export enum IncidentSeverity
{
    Low,
    Medium,
    High,
    Critical
}