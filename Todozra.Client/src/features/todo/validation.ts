export const todoValidation = {
  TITLE_MAX_LENGTH: 200,
  DESCRIPTION_MAX_LENGTH: 2000,
} as const;

export interface ValidationError {
  field: string;
  message: string;
}

export interface TodoValidationResult {
  isValid: boolean;
  errors: ValidationError[];
  fieldErrors: Record<string, string[]>;
}

export interface TodoFormData {
  title: string;
  description: string;
}

export function validateTitle(title: string): ValidationError[] {
  const errors: ValidationError[] = [];

  if (!title.trim()) {
    errors.push({
      field: "Title",
      message: "Title is required.",
    });
  } else if (title.length > todoValidation.TITLE_MAX_LENGTH) {
    errors.push({
      field: "Title",
      message: `Title must be at most ${todoValidation.TITLE_MAX_LENGTH} characters.`,
    });
  }

  return errors;
}

export function validateDescription(description: string): ValidationError[] {
  const errors: ValidationError[] = [];

  if (
    description &&
    description.length > todoValidation.DESCRIPTION_MAX_LENGTH
  ) {
    errors.push({
      field: "Description",
      message: `Description must be at most ${todoValidation.DESCRIPTION_MAX_LENGTH} characters.`,
    });
  }

  return errors;
}

export function validate(data: TodoFormData): TodoValidationResult {
  const allErrors: ValidationError[] = [
    ...validateTitle(data.title),
    ...validateDescription(data.description),
  ];

  const fieldErrors: Record<string, string[]> = {};
  allErrors.forEach((error) => {
    if (!fieldErrors[error.field]) {
      fieldErrors[error.field] = [];
    }
    fieldErrors[error.field].push(error.message);
  });

  return {
    isValid: allErrors.length === 0,
    errors: allErrors,
    fieldErrors,
  };
}
