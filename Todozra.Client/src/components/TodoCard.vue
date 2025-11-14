<script setup lang="ts">
import type { Todo } from "../api/todos";
import { computed } from "vue";

interface Props {
    todo: Todo;
    onToggleComplete: (id: string, completed: boolean) => void;
    loading?: boolean;
}

const props = defineProps<Props>();

const isCompleted = computed(() => !!props.todo.completedAt);

const formattedDate = computed(() => {
    return new Date(props.todo.createdAt).toLocaleDateString();
});

const handleToggle = () => {
    props.onToggleComplete(props.todo.id, !isCompleted.value);
};
</script>

<template>
    <div
        class="bg-white rounded-lg shadow-md p-4 hover:shadow-lg transition-shadow duration-200"
        :class="{ 'opacity-60': loading }"
    >
        <div class="flex items-start gap-3">
            <button
                @click="handleToggle"
                :disabled="loading"
                class="mt-1 flex-shrink-0 w-5 h-5 rounded border-2 transition-colors duration-150"
                :class="
                    isCompleted
                        ? 'bg-green-500 border-green-500'
                        : 'border-gray-300 hover:border-green-400'
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
                    class="text-lg font-semibold text-gray-900 mb-1"
                    :class="{ 'line-through text-gray-500': isCompleted }"
                >
                    {{ todo.title }}
                </h3>
                <p
                    v-if="todo.description"
                    class="text-sm text-gray-600 mb-2"
                    :class="{ 'line-through': isCompleted }"
                >
                    {{ todo.description }}
                </p>
                <p class="text-xs text-gray-400">
                    Created: {{ formattedDate }}
                </p>
            </div>
        </div>
    </div>
</template>
