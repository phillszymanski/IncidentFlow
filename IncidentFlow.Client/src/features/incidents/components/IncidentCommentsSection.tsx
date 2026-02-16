import type { IncidentComment } from "../types";

type IncidentCommentsSectionProps = {
  newComment: string;
  onNewCommentChange: (value: string) => void;
  onPostComment: () => void;
  commentSubmitting: boolean;
  commentError: string | null;
  commentsLoading: boolean;
  comments: IncidentComment[];
};

export const IncidentCommentsSection = ({
  newComment,
  onNewCommentChange,
  onPostComment,
  commentSubmitting,
  commentError,
  commentsLoading,
  comments
}: IncidentCommentsSectionProps) => {
  return (
    <section className="rounded-2xl border border-slate-800/80 bg-slate-900/70 p-5 shadow-xl shadow-slate-950/30">
      <h3 className="text-base font-semibold text-white">Comments</h3>

      <textarea
        value={newComment}
        onChange={(event) => onNewCommentChange(event.target.value)}
        className="mt-4 min-h-28 w-full rounded-xl border border-blue-900/70 bg-blue-950/60 px-4 py-3 text-sm text-slate-100 outline-none transition focus:border-sky-400/70 focus:ring-2 focus:ring-sky-400/30"
        placeholder="Add a new comment..."
      />

      <button
        type="button"
        onClick={onPostComment}
        disabled={commentSubmitting || !newComment.trim()}
        className="mt-3 inline-flex items-center justify-center rounded-xl bg-sky-400 px-4 py-2.5 text-sm font-semibold text-slate-950 transition hover:bg-sky-300 disabled:cursor-not-allowed disabled:opacity-70"
      >
        {commentSubmitting ? "Posting..." : "Post Comment"}
      </button>

      {commentError ? (
        <p className="mt-3 text-sm text-rose-300">{commentError}</p>
      ) : null}

      <div className="mt-5 space-y-3">
        {commentsLoading ? (
          <p className="text-sm text-slate-400">Loading comments...</p>
        ) : comments.length === 0 ? (
          <p className="text-sm text-slate-400">No comments yet.</p>
        ) : (
          comments.map((comment) => (
            <article
              key={comment.id}
              className="rounded-xl border border-slate-800 bg-slate-900/90 px-4 py-3"
            >
              <p className="whitespace-pre-wrap text-sm text-slate-200">{comment.content}</p>
              <p className="mt-2 text-xs text-slate-400">
                {new Date(comment.createdAt).toLocaleString()} · {comment.createdByUserId}
              </p>
            </article>
          ))
        )}
      </div>
    </section>
  );
};
