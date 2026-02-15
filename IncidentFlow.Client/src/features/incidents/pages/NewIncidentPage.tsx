import { useEffect, useMemo, useState } from "react";
import type { FormEvent } from "react";
import { createIncident, updateIncident } from "../incidentApi";
import { IncidentSeverity, type Incident } from "../types";

type NewIncidentPageProps = {
	onCancel: () => void;
	onSaved: () => void;
	mode?: "create" | "edit";
	incident?: Incident | null;
};

const severityOptions = ["Low", "Medium", "High", "Critical"] as const;

const normalizeSeverityOption = (rawSeverity: string) => {
	if (rawSeverity in IncidentSeverity) {
		const severity = IncidentSeverity[rawSeverity as keyof typeof IncidentSeverity];
		if (typeof severity === "string" && severityOptions.includes(severity as (typeof severityOptions)[number])) {
			return severity as (typeof severityOptions)[number];
		}
	}

	return "Medium" as (typeof severityOptions)[number];
};

export const NewIncidentPage = ({
	onCancel,
	onSaved,
	mode = "create",
	incident = null
}: NewIncidentPageProps) => {
	const isEditMode = mode === "edit";

	const [title, setTitle] = useState("");
	const [description, setDescription] = useState("");
	const [severity, setSeverity] = useState<(typeof severityOptions)[number]>("Medium");
	const [createdBy, setCreatedBy] = useState("11111111-1111-1111-1111-111111111111");
	const [assignedTo, setAssignedTo] = useState("");
	const [submitting, setSubmitting] = useState(false);
	const [error, setError] = useState<string | null>(null);

	const pageTitle = useMemo(() => (isEditMode ? "Edit Incident" : "Report Incident"), [isEditMode]);
	const pageSubtitle = useMemo(
		() => (isEditMode
			? "Update incident details and keep the record current."
			: "Capture a new operational issue and add it to the incident queue."),
		[isEditMode]
	);

	useEffect(() => {
		if (isEditMode && incident) {
			setTitle(incident.title);
			setDescription(incident.description);
			setSeverity(normalizeSeverityOption(incident.severity));
			setCreatedBy(incident.createdBy);
			setAssignedTo(incident.assignedTo ?? "");
			setError(null);
			return;
		}

		setTitle("");
		setDescription("");
		setSeverity("Medium");
		setCreatedBy("11111111-1111-1111-1111-111111111111");
		setAssignedTo("");
		setError(null);
	}, [incident, isEditMode]);

	const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
		event.preventDefault();

		if (!title.trim() || !description.trim() || !createdBy.trim()) {
			setError("Title, description, and created by are required.");
			return;
		}

		setSubmitting(true);
		setError(null);

		try {
			if (isEditMode && incident) {
				await updateIncident(incident.id, {
					title: title.trim(),
					description: description.trim(),
					severity: IncidentSeverity[severity],
					assignedTo: assignedTo.trim() || null
				});
			} else {
				await createIncident({
					title: title.trim(),
					description: description.trim(),
					severity: IncidentSeverity[severity],
					createdBy: createdBy.trim(),
					assignedTo: assignedTo.trim() || null
				});
			}

			onSaved();
		} catch (submissionError) {
			setError(submissionError instanceof Error ? submissionError.message : "Failed to save incident.");
		} finally {
			setSubmitting(false);
		}
	};

	return (
		<section className="mx-auto w-full max-w-5xl px-6 py-12">
			<div className="relative overflow-hidden rounded-3xl border border-slate-800/70 bg-slate-900/70 p-8 shadow-2xl shadow-slate-950/60 backdrop-blur">
				<div className="pointer-events-none absolute -right-20 -top-16 h-56 w-56 rounded-full bg-sky-500/20 blur-3xl" />
				<div className="pointer-events-none absolute -bottom-20 -left-16 h-56 w-56 rounded-full bg-indigo-500/15 blur-3xl" />

				<div className="relative flex flex-wrap items-center justify-between gap-4">
					<div>
						<p className="text-xs font-semibold uppercase tracking-[0.18em] text-sky-300/90">IncidentFlow</p>
						<h1 className="mt-1 text-3xl font-semibold tracking-tight text-white">{pageTitle}</h1>
						<p className="mt-2 text-sm text-slate-300">{pageSubtitle}</p>
					</div>

					<button
						type="button"
						onClick={onCancel}
						className="inline-flex items-center rounded-xl bg-slate-700 px-4 py-2.5 text-sm font-semibold text-slate-100 transition hover:bg-slate-600"
					>
						Back to Incidents
					</button>
				</div>

				<form onSubmit={handleSubmit} className="relative mt-8 grid gap-5">
					{error ? (
						<div className="rounded-xl border border-rose-400/25 bg-rose-500/10 px-4 py-3 text-sm text-rose-100">{error}</div>
					) : null}

					<div>
						<label htmlFor="title" className="mb-2 block text-xs font-semibold uppercase tracking-wide text-slate-300">
							Title
						</label>
						<input
							id="title"
							value={title}
							onChange={(event) => setTitle(event.target.value)}
							className="w-full rounded-xl border border-slate-700 bg-slate-900 px-4 py-3 text-sm text-slate-100 outline-none transition focus:border-sky-400/70 focus:ring-2 focus:ring-sky-400/30"
							placeholder="Brief title of the incident"
							required
						/>
					</div>

					<div>
						<label htmlFor="description" className="mb-2 block text-xs font-semibold uppercase tracking-wide text-slate-300">
							Description
						</label>
						<textarea
							id="description"
							value={description}
							onChange={(event) => setDescription(event.target.value)}
							className="min-h-36 w-full rounded-xl border border-slate-700 bg-slate-900 px-4 py-3 text-sm text-slate-100 outline-none transition focus:border-sky-400/70 focus:ring-2 focus:ring-sky-400/30"
							placeholder="What happened, impact, and current status"
							required
						/>
					</div>

					<div className="grid gap-5 md:grid-cols-2">
						<div>
							<label htmlFor="severity" className="mb-2 block text-xs font-semibold uppercase tracking-wide text-slate-300">
								Severity
							</label>
							<select
								id="severity"
								value={severity}
								onChange={(event) => setSeverity(event.target.value as (typeof severityOptions)[number])}
								className="w-full rounded-xl border border-slate-700 bg-slate-900 px-4 py-3 text-sm text-slate-100 outline-none transition focus:border-sky-400/70 focus:ring-2 focus:ring-sky-400/30"
							>
								{severityOptions.map((option) => (
									<option key={option} value={option}>
										{option}
									</option>
								))}
							</select>
						</div>

						<div>
							<label htmlFor="assignedTo" className="mb-2 block text-xs font-semibold uppercase tracking-wide text-slate-300">
								Assigned User ID (optional)
							</label>
							<input
								id="assignedTo"
								value={assignedTo}
								onChange={(event) => setAssignedTo(event.target.value)}
								className="w-full rounded-xl border border-slate-700 bg-slate-900 px-4 py-3 text-sm text-slate-100 outline-none transition focus:border-sky-400/70 focus:ring-2 focus:ring-sky-400/30"
								placeholder="GUID"
							/>
						</div>
					</div>

					<div>
						<label htmlFor="createdBy" className="mb-2 block text-xs font-semibold uppercase tracking-wide text-slate-300">
							Created By User ID
						</label>
						<input
							id="createdBy"
							value={createdBy}
							onChange={(event) => setCreatedBy(event.target.value)}
							disabled={isEditMode}
							className="w-full rounded-xl border border-slate-700 bg-slate-900 px-4 py-3 text-sm text-slate-100 outline-none transition focus:border-sky-400/70 focus:ring-2 focus:ring-sky-400/30"
							placeholder="GUID"
							required
						/>
					</div>

					<div className="mt-2 flex flex-wrap items-center gap-3">
						<button
							type="submit"
							disabled={submitting}
							className="inline-flex items-center rounded-xl bg-sky-400 px-5 py-2.5 text-sm font-semibold text-slate-950 transition hover:bg-sky-300 disabled:cursor-not-allowed disabled:opacity-70"
						>
							{submitting ? "Submitting..." : isEditMode ? "Save Changes" : "Create Incident"}
						</button>

						<button
							type="button"
							onClick={onCancel}
							className="inline-flex items-center rounded-xl border border-slate-700 bg-slate-900 px-5 py-2.5 text-sm font-semibold text-slate-200 transition hover:border-slate-500"
						>
							Cancel
						</button>
					</div>
				</form>
			</div>
		</section>
	);
};
