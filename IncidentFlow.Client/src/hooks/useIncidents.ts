import { useCallback, useEffect, useState } from "react";
import type { Incident } from "../features/incidents/types";
import { getIncidents } from "../features/incidents/incidentApi";

export function useIncidents() {
    const [incidents, setIncidents] = useState<Incident[]>([]);
    const [loading, setLoading] = useState<boolean>(true);
    const [error, setError] = useState<string | null>(null);

    const fetchIncidents = useCallback(async () => {
        setLoading(true);
        setError(null);

        try {
            const response = await getIncidents();
            setIncidents(response);
        } catch (error) {
            setError(error instanceof Error ? error.message : "Unknown error");
        } finally {
            setLoading(false);
        }
    }, []);

    useEffect(() => {
        fetchIncidents();
    }, [fetchIncidents]);

    return {
        incidents,
        loading,
        error,
        refetch: fetchIncidents
    };
    
}