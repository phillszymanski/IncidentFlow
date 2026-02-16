import { useEffect, useMemo, useState } from "react";
import type { FormEvent } from "react";
import type { AuthUser } from "../../auth/authApi";
import { createIncident, getAssignableUsers, updateIncident, type AssignableUser } from "../incidentApi";
import { IncidentSeverity, type Incident } from "../types";

type NewIncidentPageProps = {
	currentUser: AuthUser;
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
	currentUser,
	onCancel,
	onSaved,
	mode = "create",
	incident = null
}: NewIncidentPageProps) => {
	const isEditMode = mode === "edit";
	const hasPermission = (permission: string) => currentUser.permissions.includes(permission);
	const canAssign = hasPermission("incidents:assign");
	const canChangeAnySeverity = hasPermission("incidents:severity:any");

	const [title, setTitle] = useState("");
	const [description, setDescription] = useState("");
	const [severity, setSeverity] = useState<(typeof severityOptions)[number]>("Medium");
	const [assignedTo, setAssignedTo] = useState("");
	const [assignableUsers, setAssignableUsers] = useState<AssignableUser[]>([]);
	const [assignableUsersLoading, setAssignableUsersLoading] = useState(false);
	const [assignableUsersError, setAssignableUsersError] = useState<string | null>(null);
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
		const loadAssignableUsers = async () => {
			if (!canAssign) {
				setAssignableUsers([]);
				setAssignableUsersError(null);
				setAssignableUsersLoading(false);
				return;
			}

			setAssignableUsersLoading(true);
			setAssignableUsersError(null);

			try {
				const users = await getAssignableUsers();
				setAssignableUsers(users);
			} catch (loadError) {
				setAssignableUsersError(loadError instanceof Error ? loadError.message : "Failed to load users.");
			} finally {
				setAssignableUsersLoading(false);
			}
		};

		void loadAssignableUsers();
	}, [canAssign]);

	useEffect(() => {
		if (isEditMode && incident) {
			setTitle(incident.title);
			setDescription(incident.description);
			setSeverity(normalizeSeverityOption(incident.severity));
			setAssignedTo(incident.assignedTo ?? "");
			setError(null);
			return;
		}

		setTitle("");
		setDescription("");
		setSeverity("Medium");
		setAssignedTo("");
		setError(null);
	}, [incident, isEditMode]);

	const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
		event.preventDefault();

		if (!title.trim() || !description.trim()) {
			setError("Title and description are required.");
			return;
		}

		setSubmitting(true);
		setError(null);

		try {
			if (isEditMode && incident) {
				const updatePayload: Parameters<typeof updateIncident>[1] = {
					title: title.trim(),
					description: description.trim()
				};

				if (canChangeAnySeverity) {
					updatePayload.severity = IncidentSeverity[severity];
				}

				if (canAssign) {
					updatePayload.assignedTo = assignedTo.trim() || null;
				}

				await updateIncident(incident.id, {
					...updatePayload
				});
			} else {
				await createIncident({
					title: title.trim(),
					description: description.trim(),
					severity: IncidentSeverity[severity],
					createdBy: currentUser.id,
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
								disabled={isEditMode && !canChangeAnySeverity}
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
								Assigned User (optional)
							</label>
							<select
								id="assignedTo"
								value={assignedTo}
								onChange={(event) => setAssignedTo(event.target.value)}
								disabled={!canAssign || assignableUsersLoading}
								className="w-full rounded-xl border border-slate-700 bg-slate-900 px-4 py-3 text-sm text-slate-100 outline-none transition focus:border-sky-400/70 focus:ring-2 focus:ring-sky-400/30"
							>
								<option value="">Unassigned</option>
								{assignableUsers.map((user) => (
									<option key={user.id} value={user.id}>
										{user.fullName} ({user.username}) - {user.role}
									</option>
								))}
							</select>
							{assignableUsersError ? (
								<p className="mt-2 text-xs text-rose-300">{assignableUsersError}</p>
							) : null}
						</div>
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
