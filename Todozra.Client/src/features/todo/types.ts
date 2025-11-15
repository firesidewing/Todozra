export interface Todo {
  id: string;
  title: string;
  description: string | null;
  completedAt: string | null;
  createdAt: string;
}

export interface CreateTodoRequest {
  title: string;
  description?: string;
}

export interface UpdateTodoRequest {
  title: string;
  description?: string;
  completedAt?: string | null;
}

export interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  instance?: string;
  errors?: Record<string, string[]>;
}

export type TodoValidationError = {
  kind: "validation";
  message: string;
  fieldErrors: Record<string, string[]>;
};

export type ApiError = {
  kind: "api";
  message: string;
  status?: number;
};

export type TodoApiError = TodoValidationError | ApiError;
