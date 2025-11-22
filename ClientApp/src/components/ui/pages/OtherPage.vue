<template>
  <div :class="['other-page', className, selected ? 'selected' : '', disabled ? 'disabled' : '']" @click="!disabled && $emit('click')">
    <div class="text-wrapper">{{ text }}</div>
  </div>
</template>
<script lang="ts">
  import { defineComponent } from 'vue';
  export default defineComponent({
    name: 'OtherPage',
    props: {
      className: { type: String, default: '' },
      text: { type: String, default: '2' },
      selected: { type: Boolean, default: false },
      disabled: { type: Boolean, default: false },
    },
    emits: ['click'],
  });
</script>
<style scoped>
  .other-page {
    background-color: var(--color-background);
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

  .disabled {
    cursor: default;
    opacity: 0.5;
  }

  .text-wrapper {
    color: var(--color-text-primary);
    font-family: var(--font-additional);
    font-size: var(--font-size-main);
    text-align: center;
  }
</style>
