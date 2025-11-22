<template>
  <select class="status" v-model="selectedType">
    <option v-for="option in types" :key="option" :value="option">{{ option }}</option>
  </select>
</template>
<script setup lang="ts">
  import { ref, watch } from 'vue';
  const props = defineProps<{ modelValue: string }>();
  const emit = defineEmits(['update:modelValue']);
  const selectedType = ref(props.modelValue);
  const types = ['All', 'Sent', 'Received'];
  watch(selectedType, (newVal) => emit('update:modelValue', newVal));
</script>
<style scoped>
  .status {
    background-color: var(--color-teal-accent);
    border: 6px solid var(--color-teal-border);
    border-radius: var(--radius-main);
    color: var(--color-text-primary);
    display: flex;
    height: 81px;
    align-items: center;
    justify-content: center;
    padding: 10px;
    font-family: var(--font-additional);
  }
</style>
