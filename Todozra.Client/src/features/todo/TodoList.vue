<script setup lang="ts">
import { useQuery, useMutation, useQueryClient } from "@tanstack/vue-query";
import TodoCard from "./TodoCard.vue";
import TodoForm from "./TodoForm.vue";
import { ref, computed } from "vue";
import { todosApi, isTodoValidationError } from "./todos";
import type { CreateTodoRequest } from "./types";

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

const isDrawerOpen = ref(false);
const todoFormRefDesktop = ref<InstanceType<typeof TodoForm>>();
const todoFormRefDrawer = ref<InstanceType<typeof TodoForm>>();
const serverFieldErrors = ref<Record<string, string[]>>({});

const createMutation = useMutation({
    mutationFn: todosApi.create,
    onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: ["todos"] });
        todoFormRefDesktop.value?.reset();
        todoFormRefDrawer.value?.reset();
        serverFieldErrors.value = {};
        isDrawerOpen.value = false;
    },
    onError: (err: unknown) => {
        if (isTodoValidationError(err)) {
            serverFieldErrors.value = err.fieldErrors;
        }
    },
});

const handleCreateTodo = (title: string, description: string | undefined) => {
    const data: CreateTodoRequest = {
        title,
        description,
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

const priorityMutation = useMutation({
    mutationFn: todosApi.togglePriority,
    onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: ["todos"] });
    },
});

const handleTogglePriority = (id: string) => {
    priorityMutation.mutate(id);
};

const searchQuery = ref("");
const filterStatus = ref<"all" | "active" | "completed">("active");
const sortBy = ref<"created" | "title">("created");

const filteredAndSortedTodos = computed(() => {
    if (!todos.value) return [];

    let result = [...todos.value];

    if (searchQuery.value.trim()) {
        const query = searchQuery.value.toLowerCase();
        result = result.filter(
            (todo) =>
                todo.title.toLowerCase().includes(query) ||
                todo.description?.toLowerCase().includes(query),
        );
    }

    if (filterStatus.value === "active") {
        result = result.filter((todo) => !todo.completedAt);
    } else if (filterStatus.value === "completed") {
        result = result.filter((todo) => !!todo.completedAt);
    }

    result.sort((a, b) => {
        if (a.isPriority && !b.isPriority) return -1;
        if (!a.isPriority && b.isPriority) return 1;

        if (sortBy.value === "title") {
            return a.title.localeCompare(b.title);
        } else {
            return (
                new Date(b.createdAt).getTime() -
                new Date(a.createdAt).getTime()
            );
        }
    });

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

const openDrawer = () => {
    isDrawerOpen.value = true;
};

const closeDrawer = () => {
    isDrawerOpen.value = false;
};
</script>

<template>
    <div class="min-h-screen bg-[#0F0E0D] py-8 pb-24 px-4 sm:px-6 lg:px-8">
        <div class="max-w-3xl mx-auto">
            <h1 class="text-3xl font-bold text-[#E7E5E4] mb-8">My Todos</h1>

            <div
                class="bg-[#1C1917] rounded-lg shadow-md p-6 mb-8 border border-[#292524] hidden md:block"
            >
                <h2 class="text-xl font-semibold text-[#E7E5E4] mb-4">
                    Add New Todo
                </h2>
                <TodoForm
                    ref="todoFormRefDesktop"
                    :on-submit="handleCreateTodo"
                    :is-pending="createMutation.isPending.value"
                    :server-field-errors="serverFieldErrors"
                />
            </div>

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
                    :on-toggle-priority="handleTogglePriority"
                    :on-edit="handleEdit"
                    :on-delete="handleDelete"
                    :loading="
                        updateMutation.isPending.value ||
                        priorityMutation.isPending.value ||
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

        <button
            @click="openDrawer"
            class="md:hidden fixed bottom-6 right-6 w-14 h-14 bg-[#5C7F67] text-white rounded-full shadow-lg hover:bg-[#87C7A1] focus:outline-none focus:ring-4 focus:ring-[#5C7F67] focus:ring-opacity-50 transition-all duration-200 flex items-center justify-center z-40"
        >
            <svg
                class="w-6 h-6"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
            >
                <path
                    stroke-linecap="round"
                    stroke-linejoin="round"
                    stroke-width="2"
                    d="M12 4v16m8-8H4"
                />
            </svg>
        </button>

        <Transition name="drawer">
            <div
                v-if="isDrawerOpen"
                class="md:hidden fixed inset-0 z-50"
                @click="closeDrawer"
            >
                <div class="absolute inset-0 bg-black bg-opacity-50"></div>

                <div
                    @click.stop
                    class="absolute bottom-0 left-0 right-0 bg-[#1C1917] rounded-t-2xl shadow-2xl border-t border-[#292524] max-h-[85vh] overflow-y-auto"
                >
                    <div
                        class="sticky top-0 bg-[#1C1917] border-b border-[#292524] px-6 py-4 flex items-center justify-between"
                    >
                        <h2 class="text-xl font-semibold text-[#E7E5E4]">
                            Add New Todo
                        </h2>
                        <button
                            @click="closeDrawer"
                            class="p-2 text-[#A8A29E] hover:text-[#E7E5E4] hover:bg-[#292524] rounded-lg transition-colors"
                        >
                            <svg
                                class="w-6 h-6"
                                fill="none"
                                stroke="currentColor"
                                viewBox="0 0 24 24"
                            >
                                <path
                                    stroke-linecap="round"
                                    stroke-linejoin="round"
                                    stroke-width="2"
                                    d="M6 18L18 6M6 6l12 12"
                                />
                            </svg>
                        </button>
                    </div>

                    <div class="p-6">
                        <TodoForm
                            ref="todoFormRefDrawer"
                            :on-submit="handleCreateTodo"
                            :is-pending="createMutation.isPending.value"
                            :server-field-errors="serverFieldErrors"
                        />
                    </div>
                </div>
            </div>
        </Transition>
    </div>
</template>

<style scoped>
.drawer-enter-active,
.drawer-leave-active {
    transition: opacity 0.3s ease;
}

.drawer-enter-active > div:last-child,
.drawer-leave-active > div:last-child {
    transition: transform 0.3s ease;
}

.drawer-enter-from,
.drawer-leave-to {
    opacity: 0;
}

.drawer-enter-from > div:last-child,
.drawer-leave-to > div:last-child {
    transform: translateY(100%);
}
</style>
