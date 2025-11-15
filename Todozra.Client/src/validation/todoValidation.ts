export const TODO_VALIDATION = {
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

export class TodoValidator {
  static validateTitle(title: string): ValidationError[] {
    const errors: ValidationError[] = [];

    if (!title.trim()) {
      errors.push({
        field: "Title",
        message: "Title is required.",
      });
    } else if (title.length > TODO_VALIDATION.TITLE_MAX_LENGTH) {
      errors.push({
        field: "Title",
        message: `Title must be at most ${TODO_VALIDATION.TITLE_MAX_LENGTH} characters.`,
      });
    }

    return errors;
  }

  static validateDescription(description: string): ValidationError[] {
    const errors: ValidationError[] = [];

    if (
      description &&
      description.length > TODO_VALIDATION.DESCRIPTION_MAX_LENGTH
    ) {
      errors.push({
        field: "Description",
        message: `Description must be at most ${TODO_VALIDATION.DESCRIPTION_MAX_LENGTH} characters.`,
      });
    }

    return errors;
  }

  static validate(data: TodoFormData): TodoValidationResult {
    const allErrors: ValidationError[] = [
      ...this.validateTitle(data.title),
      ...this.validateDescription(data.description),
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
}
