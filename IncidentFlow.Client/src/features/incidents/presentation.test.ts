import {
  incidentSeverityStyles,
  incidentStatusOptions,
  incidentStatusStyles,
  normalizeIncidentSeverity,
  normalizeIncidentStatus
} from "./presentation";

describe("incident presentation helpers", () => {
  it("normalizes known enum status values", () => {
    expect(normalizeIncidentStatus("Open")).toBe("Open");
    expect(normalizeIncidentStatus("InProgress")).toBe("InProgress");
  });

  it("returns unknown status unchanged", () => {
    expect(normalizeIncidentStatus("Escalated")).toBe("Escalated");
  });

  it("normalizes known enum severity values", () => {
    expect(normalizeIncidentSeverity("Low")).toBe("Low");
    expect(normalizeIncidentSeverity("Critical")).toBe("Critical");
  });

  it("returns unknown severity unchanged", () => {
    expect(normalizeIncidentSeverity("Urgent")).toBe("Urgent");
  });

  it("exposes base status options and style mappings", () => {
    expect(incidentStatusOptions).toEqual(["Open", "InProgress", "Resolved", "Closed"]);
    expect(incidentStatusStyles.Open).toContain("bg-sky");
    expect(incidentSeverityStyles.Critical).toContain("bg-rose");
  });
});
