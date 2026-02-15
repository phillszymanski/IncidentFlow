import { apiClient } from "../../api/axios";
import { type Incident, type IncidentCreateInput, type IncidentUpdateInput } from "./types";

export const getIncidents = async (): Promise<Incident[]> => {
  const response = await apiClient.get<Incident[]>("/Incident");
  return response.data;
};

export const createIncident = async (
  incident: IncidentCreateInput
): Promise<Incident> => {
  const response = await apiClient.post<Incident>("/Incident", incident);
  return response.data;
};

export const updateIncident = async (
  id: string,
  incident: IncidentUpdateInput
): Promise<void> => {
  await apiClient.put(`/Incident/${id}`, incident);
};
