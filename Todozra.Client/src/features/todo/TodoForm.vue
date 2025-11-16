<script setup lang="ts">
import { ref, computed, watch } from "vue";
import {
    validateTitle,
    validateDescription,
    validate,
    todoValidation,
} from "./validation";

interface Props {
    onSubmit: (title: string, description: string | undefined) => void;
    isPending?: boolean;
    serverFieldErrors?: Record<string, string[]>;
    mode?: "create" | "edit";
    initialTitle?: string;
    initialDescription?: string | null;
    submitButtonText?: string;
    onCancel?: () => void;
}

const props = withDefaults(defineProps<Props>(), {
    mode: "create",
    initialTitle: "",
    initialDescription: "",
    submitButtonText: undefined,
});

const newTodoTitle = ref(props.initialTitle);
const newTodoDescription = ref(props.initialDescription || "");
const touched = ref({ title: false, description: false });
const clientFieldErrors = ref<Record<string, string[]>>({});

watch(
    () => [props.initialTitle, props.initialDescription],
    ([title, description]) => {
        if (props.mode === "edit") {
            newTodoTitle.value = title || "";
            newTodoDescription.value = description || "";
        }
    },
);

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
    }
};

const handleTitleBlur = () => {
    touched.value.title = true;
    validateField("title");
};

const handleDescriptionInput = () => {
    if (touched.value.description) {
        validateField("description");
    }
};

const handleDescriptionBlur = () => {
    touched.value.description = true;
    validateField("description");
};

const titleErrors = computed(() => {
    return (
        props.serverFieldErrors?.Title || clientFieldErrors.value.Title || []
    );
});

const descriptionErrors = computed(() => {
    return (
        props.serverFieldErrors?.Description ||
        clientFieldErrors.value.Description ||
        []
    );
});

const handleSubmit = () => {
    touched.value = { title: true, description: true };

    const result = validate({
        title: newTodoTitle.value,
        description: newTodoDescription.value,
    });

    if (!result.isValid) {
        clientFieldErrors.value = result.fieldErrors;
        return;
    }

    props.onSubmit(
        newTodoTitle.value.trim(),
        newTodoDescription.value.trim() || undefined,
    );
};

const reset = () => {
    newTodoTitle.value = "";
    newTodoDescription.value = "";
    clientFieldErrors.value = {};
    touched.value = { title: false, description: false };
};

defineExpose({ reset });
</script>

<template>
    <form @submit.prevent="handleSubmit" class="space-y-4">
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
                :disabled="isPending"
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
                :disabled="isPending"
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

        <div :class="mode === 'edit' ? 'flex gap-2 justify-end' : ''">
            <button
                v-if="mode === 'edit' && onCancel"
                type="button"
                @click="onCancel"
                class="px-4 py-2 bg-[#292524] text-[#E7E5E4] rounded-md hover:bg-[#3F3C38] transition-colors duration-150"
            >
                Cancel
            </button>
            <button
                type="submit"
                :disabled="isPending"
                :class="mode === 'edit' ? 'px-4 py-2' : 'w-full px-6 py-2'"
                class="bg-[#5C7F67] text-white font-medium rounded-md hover:bg-[#87C7A1] focus:outline-none focus:ring-2 focus:ring-[#87C7A1] focus:ring-offset-2 focus:ring-offset-[#1C1917] disabled:opacity-50 disabled:cursor-not-allowed transition-colors duration-150"
            >
                {{
                    isPending
                        ? mode === "edit"
                            ? "Saving..."
                            : "Adding..."
                        : submitButtonText ||
                          (mode === "edit" ? "Save" : "Add Todo")
                }}
            </button>
        </div>
    </form>
</template>
