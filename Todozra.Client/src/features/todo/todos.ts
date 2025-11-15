import type {
  ApiError,
  CreateTodoRequest,
  ProblemDetails,
  Todo,
  TodoValidationError,
  UpdateTodoRequest,
} from "./types";

export function isTodoValidationError(
  error: unknown,
): error is TodoValidationError {
  return (
    typeof error === "object" &&
    error !== null &&
    (error as TodoValidationError).kind === "validation"
  );
}

export function isApiError(error: unknown): error is ApiError {
  return (
    typeof error === "object" &&
    error !== null &&
    (error as ApiError).kind === "api"
  );
}

async function handleResponse<T>(response: Response): Promise<T> {
  if (response.ok) {
    return (await response.json()) as T;
  }

  let problem: ProblemDetails | null = null;

  try {
    problem = (await response.json()) as ProblemDetails;
  } catch {}

  const status = response.status;

  if (status === 400 && problem?.errors) {
    const fieldErrors = problem.errors;
    const message =
      problem.detail || problem.title || "Validation failed for todo.";

    const error: TodoValidationError = {
      kind: "validation",
      message,
      fieldErrors,
    };

    throw error;
  }

  const message =
    problem?.detail || problem?.title || `Request failed with status ${status}`;

  const error: ApiError = {
    kind: "api",
    message,
    status,
  };

  throw error;
}

const API_BASE = "/api/todos";

export const todosApi = {
  async getAll(): Promise<Todo[]> {
    const response = await fetch(API_BASE);
    return handleResponse<Todo[]>(response);
  },

  async getById(id: string): Promise<Todo> {
    const response = await fetch(`${API_BASE}/${id}`);
    return handleResponse<Todo>(response);
  },

  async create(data: CreateTodoRequest): Promise<Todo> {
    const response = await fetch(API_BASE, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(data),
    });
    return handleResponse<Todo>(response);
  },

  async update(id: string, data: UpdateTodoRequest): Promise<Todo> {
    const response = await fetch(`${API_BASE}/${id}`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(data),
    });
    return handleResponse<Todo>(response);
  },
};
