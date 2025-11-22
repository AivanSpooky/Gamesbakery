<template>
  <div :class="['date-value', className]">
    <div class="text-wrapper">{{ text }}</div>
  </div>
</template>
<script lang="ts">
  import { defineComponent } from 'vue';
  export default defineComponent({
    name: 'DateValue',
    props: {
      className: { type: String, default: '' },
      text: { type: String, default: '11/12/2024' },
    },
  });
</script>
<style scoped>
  .date-value {
    background-color: var(--color-teal-accent); /* Teal */
    border: 6px solid var(--color-teal-border);
    border-radius: var(--radius-main);
    display: flex;
    align-items: center;
    justify-content: center;
    height: 85px;
    width: 184px;
    flex: 1;
  }

  .text-wrapper {
    color: var(--color-text-primary);
    font-family: var(--font-additional);
    font-size: 32px;
    text-align: center;
  }
</style>
