<script setup lang="ts">
import { computed, ref } from "vue";
import type { Todo } from "./types";
import TodoForm from "./TodoForm.vue";

interface Props {
    todo: Todo;
    onToggleComplete: (id: string, completed: boolean) => void;
    onTogglePriority: (id: string) => void;
    onEdit: (
        id: string,
        title: string,
        description: string | undefined,
    ) => void;
    onDelete: (id: string) => void;
    loading?: boolean;
    editServerFieldErrors?: Record<string, string[]>;
}

const props = defineProps<Props>();

const isCompleted = computed(() => !!props.todo.completedAt);
const isEditing = ref(false);

const formattedDate = computed(() => {
    return new Date(props.todo.createdAt).toLocaleDateString();
});

const handleToggle = () => {
    props.onToggleComplete(props.todo.id, !isCompleted.value);
};

const startEdit = () => {
    console.log(isEditing);
    isEditing.value = true;
};

const cancelEdit = () => {
    isEditing.value = false;
};

const handleEditSubmit = (title: string, description: string | undefined) => {
    props.onEdit(props.todo.id, title, description);
    isEditing.value = false;
};

const handleDelete = () => {
    if (confirm("Are you sure you want to delete this todo?")) {
        props.onDelete(props.todo.id);
    }
};

const handleTogglePriority = () => {
    props.onTogglePriority(props.todo.id);
};
</script>

<template>
    <div
        class="bg-[#1C1917] rounded-lg shadow-md p-4 hover:shadow-lg transition-shadow duration-200 border border-[#292524]"
        :class="{ 'opacity-60': loading }"
    >
        <div v-if="!isEditing" class="flex items-start gap-3">
            <button
                @click="handleToggle"
                :disabled="loading"
                class="mt-1 shrink-0 w-5 h-5 rounded border-2 transition-colors duration-150"
                :class="
                    isCompleted
                        ? 'bg-[#5C7F67] border-[#5C7F67]'
                        : 'border-[#57534E] hover:border-[#5C7F67]'
                "
            >
                <svg
                    v-if="isCompleted"
                    class="w-full h-full text-white"
                    fill="none"
                    stroke="currentColor"
                    viewBox="0 0 24 24"
                >
                    <path
                        stroke-linecap="round"
                        stroke-linejoin="round"
                        stroke-width="3"
                        d="M5 13l4 4L19 7"
                    />
                </svg>
            </button>

            <div class="flex-1 min-w-0">
                <h3
                    class="text-lg font-semibold text-[#E7E5E4] mb-1 truncate"
                    :class="{ 'line-through text-[#78716C]': isCompleted }"
                    :title="todo.title"
                >
                    {{ todo.title }}
                </h3>
                <p
                    v-if="todo.description"
                    class="text-sm text-[#A8A29E] mb-2 wrap-break-word"
                    :class="{ 'line-through': isCompleted }"
                >
                    {{ todo.description }}
                </p>
                <p class="text-xs text-[#78716C]">
                    Created: {{ formattedDate }}
                </p>
            </div>

            <div class="flex gap-2 shrink-0">
                <button
                    @click="handleTogglePriority"
                    :disabled="loading"
                    class="p-1.5 rounded transition-colors duration-150"
                    :class="
                        todo.isPriority
                            ? 'text-[#F59E0B] hover:bg-[#292524]'
                            : 'text-[#78716C] hover:text-[#F59E0B] hover:bg-[#292524]'
                    "
                    :title="
                        todo.isPriority
                            ? 'Remove from priority'
                            : 'Mark as priority'
                    "
                >
                    <svg
                        class="w-4 h-4"
                        :fill="todo.isPriority ? 'currentColor' : 'none'"
                        stroke="currentColor"
                        viewBox="0 0 24 24"
                    >
                        <path
                            stroke-linecap="round"
                            stroke-linejoin="round"
                            stroke-width="2"
                            d="M11.049 2.927c.3-.921 1.603-.921 1.902 0l1.519 4.674a1 1 0 00.95.69h4.915c.969 0 1.371 1.24.588 1.81l-3.976 2.888a1 1 0 00-.363 1.118l1.518 4.674c.3.922-.755 1.688-1.538 1.118l-3.976-2.888a1 1 0 00-1.176 0l-3.976 2.888c-.783.57-1.838-.197-1.538-1.118l1.518-4.674a1 1 0 00-.363-1.118l-3.976-2.888c-.784-.57-.38-1.81.588-1.81h4.914a1 1 0 00.951-.69l1.519-4.674z"
                        />
                    </svg>
                </button>
                <button
                    @click="startEdit"
                    :disabled="loading"
                    class="p-1.5 text-[#87C7A1] hover:bg-[#292524] rounded transition-colors duration-150"
                    title="Edit"
                >
                    <svg
                        class="w-4 h-4"
                        fill="none"
                        stroke="currentColor"
                        viewBox="0 0 24 24"
                    >
                        <path
                            stroke-linecap="round"
                            stroke-linejoin="round"
                            stroke-width="2"
                            d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z"
                        />
                    </svg>
                </button>
                <button
                    @click="handleDelete"
                    :disabled="loading"
                    class="p-1.5 text-[#EF4444] hover:bg-[#292524] rounded transition-colors duration-150"
                    title="Delete"
                >
                    <svg
                        class="w-4 h-4"
                        fill="none"
                        stroke="currentColor"
                        viewBox="0 0 24 24"
                    >
                        <path
                            stroke-linecap="round"
                            stroke-linejoin="round"
                            stroke-width="2"
                            d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"
                        />
                    </svg>
                </button>
            </div>
        </div>

        <div v-else>
            <TodoForm
                mode="edit"
                :initial-title="todo.title"
                :initial-description="todo.description"
                :on-submit="handleEditSubmit"
                :on-cancel="cancelEdit"
                :is-pending="loading"
                :server-field-errors="editServerFieldErrors"
            />
        </div>
    </div>
</template>
