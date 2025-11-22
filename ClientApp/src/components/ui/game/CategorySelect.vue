<template>
  <div :class="['category-select', className]">
    <select class="text-wrapper neon-input" v-model="selectedGenre">
      <option v-for="option in genres" :key="option.value" :value="option.value">{{ option.label }}</option>
    </select>
  </div>
</template>
<script setup lang="ts">
  import { ref, watch, onMounted } from 'vue';
  import { CategoryService } from '@/services/CategoryService';

  // Define props to fix TS2339
  defineProps<{
    className?: string;
  }>();

  const emit = defineEmits(['update:modelValue']);
  const selectedGenre = ref<string>('All'); // Default to 'All'
  const genres = ref<{ value: string; label: string }[]>([{ value: 'All', label: 'All' }]);

  onMounted(async () => {
    try {
      const categories: { genreName: string }[] = await CategoryService.getAll(); // Add type for categories
      genres.value = [...genres.value, ...categories.map((c: { genreName: string }) => ({ value: c.genreName, label: c.genreName }))];
    } catch (err) {
      console.error('Error loading categories:', err);
    }
  });

  watch(selectedGenre, (newVal) => emit('update:modelValue', newVal));
</script>
<style scoped>
  .category-select {
    background-color: var(--color-teal-accent);
    border: var(--border-notification) solid var(--color-teal-border);
    border-radius: var(--radius-main);
    display: flex;
    align-items: center;
    justify-content: center;
    height: 90px;
    width: 305px;
    overflow: hidden;
  }

  .text-wrapper {
    color: var(--color-text-secondary);
    font-family: var(--font-main);
    font-size: var(--font-size-main);
    font-weight: 400;
    text-align: center;
    background: transparent;
    border: none;
    width: 328px;
  }
</style>
