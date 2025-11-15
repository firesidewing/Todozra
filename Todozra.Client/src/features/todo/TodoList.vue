<script setup lang="ts">
import { useQuery, useMutation, useQueryClient } from "@tanstack/vue-query";
import TodoCard from "./TodoCard.vue";
import { ref, computed } from "vue";
import { todosApi, isTodoValidationError } from "./todos";
import type { CreateTodoRequest } from "./types";
import {
    validateTitle,
    validateDescription,
    validate,
    todoValidation,
} from "./validation";

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

const editMutation = useMutation({
    mutationFn: ({
        id,
        title,
        description,
    }: {
        id: string;
        title: string;
        description: string | undefined;
    }) => {
        const todo = todos.value?.find((t) => t.id === id);
        return todosApi.update(id, {
            title,
            description,
            completedAt: todo?.completedAt || null,
        });
    },
    onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: ["todos"] });
    },
});

const deleteMutation = useMutation({
    mutationFn: todosApi.delete,
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
        const errors = validateTitle(newTodoTitle.value);
        if (errors.length > 0) {
            clientFieldErrors.value.Title = errors.map((e) => e.message);
        } else {
            delete clientFieldErrors.value.Title;
        }
    } else if (field === "description") {
        const errors = validateDescription(newTodoDescription.value);
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

    const result = validate({
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

const handleEdit = (
    id: string,
    title: string,
    description: string | undefined,
) => {
    editMutation.mutate({ id, title, description });
};

const handleDelete = (id: string) => {
    deleteMutation.mutate(id);
};

// Filter/Search/Sort state
const searchQuery = ref("");
const filterStatus = ref<"all" | "active" | "completed">("all");
const sortBy = ref<"created" | "title">("created");

const filteredAndSortedTodos = computed(() => {
    if (!todos.value) return [];

    let result = [...todos.value];

    // Filter by search query
    if (searchQuery.value.trim()) {
        const query = searchQuery.value.toLowerCase();
        result = result.filter(
            (todo) =>
                todo.title.toLowerCase().includes(query) ||
                todo.description?.toLowerCase().includes(query),
        );
    }

    // Filter by status
    if (filterStatus.value === "active") {
        result = result.filter((todo) => !todo.completedAt);
    } else if (filterStatus.value === "completed") {
        result = result.filter((todo) => !!todo.completedAt);
    }

    // Sort
    if (sortBy.value === "title") {
        result.sort((a, b) => a.title.localeCompare(b.title));
    } else {
        result.sort(
            (a, b) =>
                new Date(b.createdAt).getTime() -
                new Date(a.createdAt).getTime(),
        );
    }

    return result;
});

const todoCounts = computed(() => {
    if (!todos.value) return { all: 0, active: 0, completed: 0 };
    return {
        all: todos.value.length,
        active: todos.value.filter((t) => !t.completedAt).length,
        completed: todos.value.filter((t) => !!t.completedAt).length,
    };
});
</script>

<template>
    <div class="min-h-screen bg-[#0F0E0D] py-8 px-4 sm:px-6 lg:px-8">
        <div class="max-w-3xl mx-auto">
            <h1 class="text-3xl font-bold text-[#E7E5E4] mb-8">My Todos</h1>

            <div
                class="bg-[#1C1917] rounded-lg shadow-md p-6 mb-8 border border-[#292524]"
            >
                <h2 class="text-xl font-semibold text-[#E7E5E4] mb-4">
                    Add New Todo
                </h2>
                <form @submit.prevent="handleCreateTodo" class="space-y-4">
                    <div>
                        <label
                            for="title"
                            class="block text-sm font-medium text-[#E7E5E4] mb-1"
                        >
                            Title <span class="text-[#EF4444]">*</span>
                        </label>
                        <input
                            id="title"
                            v-model="newTodoTitle"
                            @input="handleTitleInput"
                            @blur="handleTitleBlur"
                            type="text"
                            :maxlength="todoValidation.TITLE_MAX_LENGTH"
                            class="w-full px-3 py-2 bg-[#292524] border rounded-md text-[#E7E5E4] placeholder-[#78716C] focus:outline-none focus:ring-2 transition-colors"
                            :class="
                                titleErrors.length > 0
                                    ? 'border-[#EF4444] focus:ring-[#EF4444]'
                                    : 'border-[#57534E] focus:ring-[#87C7A1]'
                            "
                            placeholder="What needs to be done?"
                            :disabled="createMutation.isPending.value"
                        />
                        <div class="flex justify-between items-start mt-1">
                            <div class="flex-1">
                                <p
                                    v-for="(msg, idx) in titleErrors"
                                    :key="idx"
                                    class="text-sm text-[#EF4444]"
                                >
                                    {{ msg }}
                                </p>
                            </div>
                            <span
                                class="text-xs text-[#78716C] ml-2"
                                :class="
                                    newTodoTitle.length >
                                    todoValidation.TITLE_MAX_LENGTH * 0.9
                                        ? 'text-[#F97316] font-medium'
                                        : ''
                                "
                            >
                                {{ newTodoTitle.length }} /
                                {{ todoValidation.TITLE_MAX_LENGTH }}
                            </span>
                        </div>
                    </div>

                    <div>
                        <label
                            for="description"
                            class="block text-sm font-medium text-[#E7E5E4] mb-1"
                        >
                            Description
                        </label>
                        <textarea
                            id="description"
                            v-model="newTodoDescription"
                            @input="handleDescriptionInput"
                            @blur="handleDescriptionBlur"
                            rows="3"
                            :maxlength="todoValidation.DESCRIPTION_MAX_LENGTH"
                            class="w-full px-3 py-2 bg-[#292524] border rounded-md text-[#E7E5E4] placeholder-[#78716C] focus:outline-none focus:ring-2 transition-colors"
                            :class="
                                descriptionErrors.length > 0
                                    ? 'border-[#EF4444] focus:ring-[#EF4444]'
                                    : 'border-[#57534E] focus:ring-[#87C7A1]'
                            "
                            placeholder="Additional details..."
                            :disabled="createMutation.isPending.value"
                        />
                        <div class="flex justify-between items-start mt-1">
                            <div class="flex-1">
                                <p
                                    v-for="(msg, idx) in descriptionErrors"
                                    :key="idx"
                                    class="text-sm text-[#EF4444]"
                                >
                                    {{ msg }}
                                </p>
                            </div>
                            <span
                                class="text-xs text-[#78716C] ml-2"
                                :class="
                                    newTodoDescription.length >
                                    todoValidation.DESCRIPTION_MAX_LENGTH * 0.9
                                        ? 'text-[#F97316] font-medium'
                                        : ''
                                "
                            >
                                {{ newTodoDescription.length }} /
                                {{ todoValidation.DESCRIPTION_MAX_LENGTH }}
                            </span>
                        </div>
                    </div>

                    <button
                        type="submit"
                        :disabled="createMutation.isPending.value"
                        class="w-full sm:w-auto px-6 py-2 bg-[#5C7F67] text-white font-medium rounded-md hover:bg-[#87C7A1] focus:outline-none focus:ring-2 focus:ring-[#87C7A1] focus:ring-offset-2 focus:ring-offset-[#1C1917] disabled:opacity-50 disabled:cursor-not-allowed transition-colors duration-150"
                    >
                        {{
                            createMutation.isPending.value
                                ? "Adding..."
                                : "Add Todo"
                        }}
                    </button>
                </form>
            </div>

            <!-- Filter/Search/Sort Controls -->
            <div
                v-if="!isLoading && todos && todos.length > 0"
                class="bg-[#1C1917] rounded-lg shadow-md p-4 mb-4 border border-[#292524]"
            >
                <div class="flex flex-col sm:flex-row gap-4">
                    <!-- Search -->
                    <div class="flex-1">
                        <input
                            v-model="searchQuery"
                            type="text"
                            placeholder="Search todos..."
                            class="w-full px-3 py-2 bg-[#292524] border border-[#57534E] rounded-md text-[#E7E5E4] placeholder-[#78716C] focus:outline-none focus:ring-2 focus:ring-[#87C7A1]"
                        />
                    </div>

                    <!-- Filter -->
                    <div class="flex gap-2">
                        <button
                            @click="filterStatus = 'all'"
                            class="px-4 py-2 rounded-md font-medium transition-colors duration-150"
                            :class="
                                filterStatus === 'all'
                                    ? 'bg-[#5C7F67] text-white'
                                    : 'bg-[#292524] text-[#A8A29E] hover:bg-[#3F3C38]'
                            "
                        >
                            All ({{ todoCounts.all }})
                        </button>
                        <button
                            @click="filterStatus = 'active'"
                            class="px-4 py-2 rounded-md font-medium transition-colors duration-150"
                            :class="
                                filterStatus === 'active'
                                    ? 'bg-[#5C7F67] text-white'
                                    : 'bg-[#292524] text-[#A8A29E] hover:bg-[#3F3C38]'
                            "
                        >
                            Active ({{ todoCounts.active }})
                        </button>
                        <button
                            @click="filterStatus = 'completed'"
                            class="px-4 py-2 rounded-md font-medium transition-colors duration-150"
                            :class="
                                filterStatus === 'completed'
                                    ? 'bg-[#5C7F67] text-white'
                                    : 'bg-[#292524] text-[#A8A29E] hover:bg-[#3F3C38]'
                            "
                        >
                            Completed ({{ todoCounts.completed }})
                        </button>
                    </div>

                    <!-- Sort -->
                    <div>
                        <select
                            v-model="sortBy"
                            class="px-3 py-2 bg-[#292524] border border-[#57534E] rounded-md text-[#E7E5E4] focus:outline-none focus:ring-2 focus:ring-[#87C7A1]"
                        >
                            <option value="created">Sort by Date</option>
                            <option value="title">Sort by Title</option>
                        </select>
                    </div>
                </div>
            </div>

            <div v-if="isLoading" class="text-center py-12">
                <div
                    class="inline-block animate-spin rounded-full h-12 w-12 border-4 border-[#5C7F67] border-t-transparent"
                ></div>
                <p class="mt-4 text-[#A8A29E]">Loading todos...</p>
            </div>

            <div
                v-else-if="isError"
                class="bg-[#1C1917] border border-[#EF4444] rounded-lg p-6 text-center"
            >
                <svg
                    class="mx-auto h-12 w-12 text-[#EF4444] mb-4"
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
                <h3 class="text-lg font-semibold text-[#EF4444] mb-2">
                    Failed to load todos
                </h3>
                <p class="text-[#A8A29E]">{{ error?.message }}</p>
            </div>

            <div
                v-else-if="filteredAndSortedTodos.length > 0"
                class="space-y-4"
            >
                <TodoCard
                    v-for="todo in filteredAndSortedTodos"
                    :key="todo.id"
                    :todo="todo"
                    :on-toggle-complete="handleToggleComplete"
                    :on-edit="handleEdit"
                    :on-delete="handleDelete"
                    :loading="
                        updateMutation.isPending.value ||
                        editMutation.isPending.value ||
                        deleteMutation.isPending.value
                    "
                />
            </div>

            <div
                v-else-if="todos && todos.length > 0"
                class="text-center py-12 bg-[#1C1917] rounded-lg shadow-md border border-[#292524]"
            >
                <svg
                    class="mx-auto h-16 w-16 text-[#78716C] mb-4"
                    fill="none"
                    stroke="currentColor"
                    viewBox="0 0 24 24"
                >
                    <path
                        stroke-linecap="round"
                        stroke-linejoin="round"
                        stroke-width="2"
                        d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"
                    />
                </svg>
                <h3 class="text-lg font-semibold text-[#E7E5E4] mb-2">
                    No todos found
                </h3>
                <p class="text-[#A8A29E]">
                    Try adjusting your search or filters.
                </p>
            </div>

            <div
                v-else
                class="text-center py-12 bg-[#1C1917] rounded-lg shadow-md border border-[#292524]"
            >
                <svg
                    class="mx-auto h-16 w-16 text-[#78716C] mb-4"
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
                <h3 class="text-lg font-semibold text-[#E7E5E4] mb-2">
                    No todos yet
                </h3>
                <p class="text-[#A8A29E]">
                    Create your first todo to get started!
                </p>
            </div>
        </div>
    </div>
</template>
