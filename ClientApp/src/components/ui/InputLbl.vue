<template>
  <div :class="['input-lbl', className]">
    <div class="text-wrapper">{{ text }}</div>
  </div>
</template>

<script setup lang="ts">
  import { defineProps, withDefaults } from 'vue';

  interface Props {
    className?: string;
    text?: string;
  }

  const props = withDefaults(defineProps<Props>(), {
    className: '',
    text: 'Label:',
  });
</script>

<style scoped>
  .input-lbl {
    background-color: var(--color-background-accent);
    border: var(--border-interactive) solid var(--color-border-1);
    border-radius: var(--radius-main);
    height: 100%;
    left: 0;
    position: absolute;
    top: 0;
    width: 30%; /* Default */
    display: flex;
    align-items: center;
    justify-content: center;
    visibility: visible;
  }

  .text-wrapper {
    color: var(--color-text-primary);
    font-family: var(--font-additional);
    font-size: var(--font-size-main);
    text-align: center;
  }
</style>
