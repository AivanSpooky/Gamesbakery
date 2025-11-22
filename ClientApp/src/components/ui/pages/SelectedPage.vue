<template>
  <div :class="['selected-page', className, selected ? 'selected' : '']" @click="$emit('click')">
    <div class="text-wrapper">{{ text }}</div>
  </div>
</template>

<script lang="ts">
  import { defineComponent } from 'vue';

  export default defineComponent({
    name: 'SelectedPage',
    props: {
      className: { type: String, default: '' },
      text: { type: String, default: '1' },
      selected: { type: Boolean, default: false },
    },
    emits: ['click'],
  });
</script>

<style scoped>
  .selected-page {
    background-color: var(--color-background-accent);
    border: var(--border-interactive) solid var(--color-border-1);
    border-radius: var(--radius-main);
    display: flex;
    align-items: center;
    justify-content: center;
    height: 90px;
    width: 113px;
    overflow: hidden;
    cursor: pointer;
  }

  .selected {
    background-color: var(--color-background-accent);
  }

  .text-wrapper {
    color: var(--color-text-primary);
    font-family: var(--font-additional);
    font-size: var(--font-size-main);
    text-align: center;
  }
</style>
