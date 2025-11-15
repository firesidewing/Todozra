<script setup lang="ts">
import { useQuery, useMutation, useQueryClient } from "@tanstack/vue-query";
import {
    todosApi,
    type CreateTodoRequest,
    isTodoValidationError,
} from "../api/todos";
import TodoCard from "./TodoCard.vue";
import { ref, computed } from "vue";
import { TodoValidator, TODO_VALIDATION } from "../validation/todoValidation";

const queryClient = useQueryClient();

const {
    data: todos,
    isLoading,
    isError,
    error,
} = useQuery({
    queryKey: ["todos"],
    queryFn: todosApi.getAll,
});

const updateMutation = useMutation({
    mutationFn: ({ id, completed }: { id: string; completed: boolean }) =>
        todosApi.update(id, {
            title: todos.value?.find((t) => t.id === id)?.title || "",
            description:
                todos.value?.find((t) => t.id === id)?.description || undefined,
            completedAt: completed ? new Date().toISOString() : null,
        }),
    onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: ["todos"] });
    },
});

const createMutation = useMutation({
    mutationFn: todosApi.create,
    onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: ["todos"] });
        newTodoTitle.value = "";
        newTodoDescription.value = "";
        clientFieldErrors.value = {};
        serverFieldErrors.value = {};
        touched.value = { title: false, description: false };
    },
    onError: (err: unknown) => {
        if (isTodoValidationError(err)) {
            serverFieldErrors.value = err.fieldErrors;
        }
    },
});

const newTodoTitle = ref("");
const newTodoDescription = ref("");
const touched = ref({ title: false, description: false });

const clientFieldErrors = ref<Record<string, string[]>>({});
const serverFieldErrors = ref<Record<string, string[]>>({});

const validateField = (field: "title" | "description") => {
    if (field === "title") {
        const errors = TodoValidator.validateTitle(newTodoTitle.value);
        if (errors.length > 0) {
            clientFieldErrors.value.Title = errors.map((e) => e.message);
        } else {
            delete clientFieldErrors.value.Title;
        }
    } else if (field === "description") {
        const errors = TodoValidator.validateDescription(
            newTodoDescription.value,
        );
        if (errors.length > 0) {
            clientFieldErrors.value.Description = errors.map((e) => e.message);
        } else {
            delete clientFieldErrors.value.Description;
        }
    }
};

const handleTitleInput = () => {
    if (touched.value.title) {
        validateField("title");
        serverFieldErrors.value = {};
    }
};

const handleTitleBlur = () => {
    touched.value.title = true;
    validateField("title");
};

const handleDescriptionInput = () => {
    if (touched.value.description) {
        validateField("description");
        serverFieldErrors.value = {};
    }
};

const handleDescriptionBlur = () => {
    touched.value.description = true;
    validateField("description");
};

const titleErrors = computed(() => {
    return serverFieldErrors.value.Title || clientFieldErrors.value.Title || [];
});

const descriptionErrors = computed(() => {
    return (
        serverFieldErrors.value.Description ||
        clientFieldErrors.value.Description ||
        []
    );
});

const handleCreateTodo = () => {
    touched.value = { title: true, description: true };

    const result = TodoValidator.validate({
        title: newTodoTitle.value,
        description: newTodoDescription.value,
    });

    if (!result.isValid) {
        clientFieldErrors.value = result.fieldErrors;
        return;
    }

    const data: CreateTodoRequest = {
        title: newTodoTitle.value.trim(),
        description: newTodoDescription.value.trim() || undefined,
    };

    createMutation.mutate(data);
};

const handleToggleComplete = (id: string, completed: boolean) => {
    updateMutation.mutate({ id, completed });
};
</script>

<template>
    <div class="min-h-screen bg-gray-50 py-8 px-4 sm:px-6 lg:px-8">
        <div class="max-w-3xl mx-auto">
            <h1 class="text-3xl font-bold text-gray-900 mb-8">My Todos</h1>

            <div class="bg-white rounded-lg shadow-md p-6 mb-8">
                <h2 class="text-xl font-semibold text-gray-900 mb-4">
                    Add New Todo
                </h2>
                <form @submit.prevent="handleCreateTodo" class="space-y-4">
                    <div>
                        <label
                            for="title"
                            class="block text-sm font-medium text-gray-700 mb-1"
                        >
                            Title <span class="text-red-500">*</span>
                        </label>
                        <input
                            id="title"
                            v-model="newTodoTitle"
                            @input="handleTitleInput"
                            @blur="handleTitleBlur"
                            type="text"
                            :maxlength="TODO_VALIDATION.TITLE_MAX_LENGTH"
                            class="w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 transition-colors"
                            :class="
                                titleErrors.length > 0
                                    ? 'border-red-500 focus:ring-red-500'
                                    : 'border-gray-300 focus:ring-blue-500'
                            "
                            placeholder="What needs to be done?"
                            :disabled="createMutation.isPending.value"
                        />
                        <div class="flex justify-between items-start mt-1">
                            <div class="flex-1">
                                <p
                                    v-for="(msg, idx) in titleErrors"
                                    :key="idx"
                                    class="text-sm text-red-600"
                                >
                                    {{ msg }}
                                </p>
                            </div>
                            <span
                                class="text-xs text-gray-500 ml-2"
                                :class="
                                    newTodoTitle.length >
                                    TODO_VALIDATION.TITLE_MAX_LENGTH * 0.9
                                        ? 'text-orange-500 font-medium'
                                        : ''
                                "
                            >
                                {{ newTodoTitle.length }} /
                                {{ TODO_VALIDATION.TITLE_MAX_LENGTH }}
                            </span>
                        </div>
                    </div>

                    <div>
                        <label
                            for="description"
                            class="block text-sm font-medium text-gray-700 mb-1"
                        >
                            Description
                        </label>
                        <textarea
                            id="description"
                            v-model="newTodoDescription"
                            @input="handleDescriptionInput"
                            @blur="handleDescriptionBlur"
                            rows="3"
                            :maxlength="TODO_VALIDATION.DESCRIPTION_MAX_LENGTH"
                            class="w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 transition-colors"
                            :class="
                                descriptionErrors.length > 0
                                    ? 'border-red-500 focus:ring-red-500'
                                    : 'border-gray-300 focus:ring-blue-500'
                            "
                            placeholder="Additional details..."
                            :disabled="createMutation.isPending.value"
                        />
                        <div class="flex justify-between items-start mt-1">
                            <div class="flex-1">
                                <p
                                    v-for="(msg, idx) in descriptionErrors"
                                    :key="idx"
                                    class="text-sm text-red-600"
                                >
                                    {{ msg }}
                                </p>
                            </div>
                            <span
                                class="text-xs text-gray-500 ml-2"
                                :class="
                                    newTodoDescription.length >
                                    TODO_VALIDATION.DESCRIPTION_MAX_LENGTH * 0.9
                                        ? 'text-orange-500 font-medium'
                                        : ''
                                "
                            >
                                {{ newTodoDescription.length }} /
                                {{ TODO_VALIDATION.DESCRIPTION_MAX_LENGTH }}
                            </span>
                        </div>
                    </div>

                    <button
                        type="submit"
                        :disabled="createMutation.isPending.value"
                        class="w-full sm:w-auto px-6 py-2 bg-blue-600 text-white font-medium rounded-md hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 disabled:opacity-50 disabled:cursor-not-allowed transition-colors duration-150"
                    >
                        {{
                            createMutation.isPending.value
                                ? "Adding..."
                                : "Add Todo"
                        }}
                    </button>
                </form>
            </div>

            <div v-if="isLoading" class="text-center py-12">
                <div
                    class="inline-block animate-spin rounded-full h-12 w-12 border-4 border-blue-500 border-t-transparent"
                ></div>
                <p class="mt-4 text-gray-600">Loading todos...</p>
            </div>

            <div
                v-else-if="isError"
                class="bg-red-50 border border-red-200 rounded-lg p-6 text-center"
            >
                <svg
                    class="mx-auto h-12 w-12 text-red-500 mb-4"
                    fill="none"
                    stroke="currentColor"
                    viewBox="0 0 24 24"
                >
                    <path
                        stroke-linecap="round"
                        stroke-linejoin="round"
                        stroke-width="2"
                        d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
                    />
                </svg>
                <h3 class="text-lg font-semibold text-red-900 mb-2">
                    Failed to load todos
                </h3>
                <p class="text-red-700">{{ error?.message }}</p>
            </div>

            <div v-else-if="todos && todos.length > 0" class="space-y-4">
                <TodoCard
                    v-for="todo in todos"
                    :key="todo.id"
                    :todo="todo"
                    :on-toggle-complete="handleToggleComplete"
                    :loading="updateMutation.isPending.value"
                />
            </div>

            <div v-else class="text-center py-12 bg-white rounded-lg shadow-md">
                <svg
                    class="mx-auto h-16 w-16 text-gray-400 mb-4"
                    fill="none"
                    stroke="currentColor"
                    viewBox="0 0 24 24"
                >
                    <path
                        stroke-linecap="round"
                        stroke-linejoin="round"
                        stroke-width="2"
                        d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2"
                    />
                </svg>
                <h3 class="text-lg font-semibold text-gray-900 mb-2">
                    No todos yet
                </h3>
                <p class="text-gray-600">
                    Create your first todo to get started!
                </p>
            </div>
        </div>
    </div>
</template>
