import { IncidentSeverity, IncidentStatus, type Incident } from "../types";

type IncidentDashboardProps = {
  incidents: Incident[];
  mode?: "basic" | "full";
};

type ChartDatum = {
  label: string;
  count: number;
};

const normalizeStatus = (rawStatus: string): string => {
  if (rawStatus in IncidentStatus) {
    return String(IncidentStatus[rawStatus as keyof typeof IncidentStatus]);
  }

  return String(rawStatus);
};

const normalizeSeverity = (rawSeverity: string): string => {
  if (rawSeverity in IncidentSeverity) {
    return String(IncidentSeverity[rawSeverity as keyof typeof IncidentSeverity]);
  }

  return String(rawSeverity);
};

const getStartOfWeek = (date: Date) => {
  const copy = new Date(date);
  const day = copy.getDay();
  const diff = day === 0 ? -6 : 1 - day;
  copy.setDate(copy.getDate() + diff);
  copy.setHours(0, 0, 0, 0);
  return copy;
};

const buildCounts = (values: string[], labels: string[]): ChartDatum[] => {
  return labels.map((label) => ({
    label,
    count: values.filter((value) => value === label).length
  }));
};

const TrendChart = ({ incidents }: { incidents: Incident[] }) => {
  const days = [...Array(7)].map((_, index) => {
    const date = new Date();
    date.setDate(date.getDate() - (6 - index));
    date.setHours(0, 0, 0, 0);
    return date;
  });

  const trendData = days.map((day) => {
    const nextDay = new Date(day);
    nextDay.setDate(day.getDate() + 1);

    const count = incidents.filter((incident) => {
      const created = new Date(incident.createdAt);
      return created >= day && created < nextDay;
    }).length;

    return {
      label: day.toLocaleDateString(undefined, { weekday: "short" }),
      count
    };
  });

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

export const IncidentDashboard = ({ incidents, mode = "full" }: IncidentDashboardProps) => {
  const totalIncidents = incidents.length;

  const openIncidents = incidents.filter((incident) => normalizeStatus(incident.status) === "Open").length;

  const criticalIncidents = incidents.filter((incident) => normalizeSeverity(incident.severity) === "Critical").length;

  const startOfWeek = getStartOfWeek(new Date());
  const resolvedThisWeek = incidents.filter((incident) => {
    if (!incident.resolvedAt) {
      return false;
    }

    const resolvedAt = new Date(incident.resolvedAt);
    return resolvedAt >= startOfWeek;
  }).length;

  const severityData = buildCounts(
    incidents.map((incident) => normalizeSeverity(incident.severity)),
    ["Low", "Medium", "High", "Critical"]
  );

  const statusData = buildCounts(
    incidents.map((incident) => normalizeStatus(incident.status)),
    ["Open", "InProgress", "Resolved", "Closed"]
  );

  const metricCards = [
    { label: "Total incidents", value: totalIncidents },
    { label: "Open incidents", value: openIncidents },
    { label: "Critical incidents", value: criticalIncidents },
    { label: "Resolved this week", value: resolvedThisWeek }
  ];

  return (
    <section className="relative mt-8 space-y-4 rounded-2xl border border-slate-800/80 bg-slate-900/60 p-5">
      <h2 className="text-lg font-semibold text-white">Dashboard</h2>

      <div className="grid gap-3 sm:grid-cols-2 xl:grid-cols-4">
        {metricCards.map((metric) => (
          <div key={metric.label} className="rounded-xl border border-slate-800 bg-slate-900/80 p-4">
            <p className="text-xs uppercase tracking-wide text-slate-400">{metric.label}</p>
            <p className="mt-2 text-2xl font-semibold text-slate-100">{metric.value}</p>
          </div>
        ))}
      </div>

      {mode === "full" ? (
        <div className="grid gap-4 xl:grid-cols-3">
          <HorizontalChart title="Incidents by severity" data={severityData} barClassName="bg-amber-400" />
          <HorizontalChart title="Incidents by status" data={statusData} barClassName="bg-emerald-400" />
          <TrendChart incidents={incidents} />
        </div>
      ) : null}
    </section>
  );
};
