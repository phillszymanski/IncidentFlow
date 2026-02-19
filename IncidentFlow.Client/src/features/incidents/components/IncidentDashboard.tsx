import type { IncidentDashboardSummary, IncidentListFilter } from "../incidentApi";

type IncidentDashboardProps = {
  summary: IncidentDashboardSummary | null;
  loading?: boolean;
  mode?: "basic" | "full";
  activeFilter?: IncidentListFilter;
  onMetricClick?: (metric: IncidentListFilter) => void;
};

type ChartDatum = {
  label: string;
  count: number;
};
const TrendChart = ({ trendData }: { trendData: ChartDatum[] }) => {
  const max = Math.max(1, ...trendData.map((item) => item.count));

  return (
    <div className="rounded-2xl border border-slate-800 bg-slate-900/80 p-4">
      <h3 className="text-sm font-semibold text-slate-100">Trend over time</h3>
      <div className="mt-4 grid grid-cols-7 items-end gap-2">
        {trendData.map((item) => (
          <div key={item.label} className="flex flex-col items-center gap-2">
            <div className="text-[11px] text-slate-400">{item.count}</div>
            <div className="flex h-24 w-full items-end rounded-md bg-slate-800/80 px-1 py-1">
              <div
                className="w-full rounded bg-sky-400"
                style={{ height: `${(item.count / max) * 100}%` }}
                title={`${item.label}: ${item.count}`}
              />
            </div>
            <div className="text-[11px] text-slate-400">{item.label}</div>
          </div>
        ))}
      </div>
    </div>
  );
};

const HorizontalChart = ({
  title,
  data,
  barClassName
}: {
  title: string;
  data: ChartDatum[];
  barClassName: string;
}) => {
  const max = Math.max(1, ...data.map((item) => item.count));

  return (
    <div className="rounded-2xl border border-slate-800 bg-slate-900/80 p-4">
      <h3 className="text-sm font-semibold text-slate-100">{title}</h3>
      <div className="mt-4 space-y-3">
        {data.map((item) => (
          <div key={item.label} className="space-y-1">
            <div className="flex items-center justify-between text-xs text-slate-300">
              <span>{item.label}</span>
              <span>{item.count}</span>
            </div>
            <div className="h-2 overflow-hidden rounded-full bg-slate-800/80">
              <div
                className={`h-full rounded-full ${barClassName}`}
                style={{ width: `${(item.count / max) * 100}%` }}
              />
            </div>
          </div>
        ))}
      </div>
    </div>
  );
};

export const IncidentDashboard = ({
  summary,
  loading = false,
  mode = "full",
  activeFilter = "total",
  onMetricClick
}: IncidentDashboardProps) => {
  const severityData = summary?.severity ?? [];
  const statusData = summary?.status ?? [];
  const trendData = summary?.trend ?? [];

  const metricCards = [
    {
      key: "total" as const,
      label: "Total incidents",
      value: summary?.totalIncidents ?? 0,
      isActive: activeFilter === "total"
    },
    {
      key: "open" as const,
      label: "Open incidents",
      value: summary?.openIncidents ?? 0,
      isActive: activeFilter === "open"
    },
    {
      key: "critical" as const,
      label: "Critical incidents",
      value: summary?.criticalIncidents ?? 0,
      isActive: activeFilter === "critical"
    },
    {
      key: "resolvedthisweek" as const,
      label: "Resolved this week",
      value: summary?.resolvedThisWeek ?? 0,
      isActive: activeFilter === "resolvedthisweek"
    },
    {
      key: "unassigned" as const,
      label: "Unassigned incidents",
      value: summary?.unassignedIncidents ?? 0,
      isActive: activeFilter === "unassigned"
    },
    {
      key: "assignedtome" as const,
      label: "Incidents assigned to me",
      value: summary?.assignedToMeIncidents ?? 0,
      isActive: activeFilter === "assignedtome"
    }
  ];

  return (
    <section className="relative mt-8 space-y-4 rounded-2xl border border-slate-800/80 bg-slate-900/60 p-5">
      <h2 className="text-lg font-semibold text-white">Dashboard</h2>

      {loading ? <p className="text-xs text-slate-400">Loading dashboard metrics...</p> : null}

      <div className="grid gap-3 sm:grid-cols-2 xl:grid-cols-6">
        {metricCards.map((metric) => (
          <button
            key={metric.label}
            type="button"
            onClick={() => onMetricClick?.(metric.key)}
            className={`rounded-xl border p-4 text-left transition ${
              metric.isActive
                ? "border-sky-400/70 bg-sky-500/15"
                : "border-slate-800 bg-slate-900/80 hover:border-slate-700"
            }`}
          >
            <p className="text-xs uppercase tracking-wide text-slate-400">{metric.label}</p>
            <p className="mt-2 text-2xl font-semibold text-slate-100">{metric.value}</p>
          </button>
        ))}
      </div>

      {mode === "full" ? (
        <div className="grid gap-4 xl:grid-cols-3">
          <HorizontalChart title="Incidents by severity" data={severityData} barClassName="bg-amber-400" />
          <HorizontalChart title="Incidents by status" data={statusData} barClassName="bg-emerald-400" />
          <TrendChart trendData={trendData} />
        </div>
      ) : null}
    </section>
  );
};
