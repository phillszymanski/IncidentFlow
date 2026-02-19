import { useCallback, useEffect, useState } from "react";
import type { Incident } from "../features/incidents/types";
import { getIncidents, type IncidentListFilter } from "../features/incidents/incidentApi";

export function useIncidents(
    page = 1,
    pageSize = 10,
    filter: IncidentListFilter = "total"
) {
    const [incidents, setIncidents] = useState<Incident[]>([]);
    const [totalCount, setTotalCount] = useState(0);
    const [loading, setLoading] = useState<boolean>(true);
    const [error, setError] = useState<string | null>(null);

    const fetchIncidents = useCallback(async () => {
        setLoading(true);
        setError(null);

        try {
            const response = await getIncidents(page, pageSize, filter);
            setIncidents(response.items);
            setTotalCount(response.totalCount);
        } catch (error) {
            setError(error instanceof Error ? error.message : "Unknown error");
        } finally {
            setLoading(false);
        }
    }, [filter, page, pageSize]);

    useEffect(() => {
        fetchIncidents();
    }, [fetchIncidents]);

    return {
        incidents,
        totalCount,
        totalPages: Math.max(1, Math.ceil(totalCount / pageSize)),
        loading,
        error,
        refetch: fetchIncidents
    };
    
}