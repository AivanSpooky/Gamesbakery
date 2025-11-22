<template>
  <div :class="['input', className]">
    <input v-model="model" :type="type" :placeholder="placeholder" :readonly="readonly" class="text-wrapper input-field" />
  </div>
</template>

<script setup lang="ts">
  import { defineProps, defineModel, withDefaults } from 'vue';

  interface Props {
    className?: string;
    type?: string;
    placeholder?: string;
    readonly?: boolean;
  }

  const props = withDefaults(defineProps<Props>(), {
    className: '',
    type: 'text',
    placeholder: 'Введите значение',
    readonly: false,
  });

  const model = defineModel<string>();
</script>

<style scoped>
  .input {
    background-color: var(--color-background);
    border: var(--border-interactive) solid var(--color-border-1);
    border-radius: var(--radius-main);
    height: 100%;
    left: 30%; /* Default */
    position: absolute;
    top: 0;
    width: 70%;
    display: flex;
    align-items: center;
    justify-content: center;
    visibility: visible; /* На всякий случай */
  }

  .input-field {
    background: transparent;
    border: none;
    color: var(--color-text-primary);
    font-family: var(--font-additional);
    font-size: var(--font-size-main);
    padding: 0 1rem;
    text-align: center;
    width: 100%;
  }

  .input-field[readonly] {
    cursor: default;
    background-color: transparent; /* Optional: Make it look less editable */
  }

  .input-field::placeholder {
    color: rgba(var(--color-text-primary-rgb), 0.5);
  }
</style>
