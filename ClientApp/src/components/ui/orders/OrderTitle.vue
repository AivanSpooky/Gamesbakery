<template>
  <div :class="['order-title', className]">
    <div class="text-wrapper">{{ text }}</div>
  </div>
</template>
<script lang="ts">
  import { defineComponent } from 'vue';
  export default defineComponent({
    name: 'OrderTitle',
    props: {
      className: { type: String, default: '' },
      text: { type: String, default: 'Детали заказа' },
    },
  });
</script>
<style scoped>
  .order-title {
    background-color: var(--color-background-accent); /* Blue */
    border-bottom: 6px solid var(--color-background-border); /* Border only bottom if needed */
    border-radius: var(--radius-main) var(--radius-main) 0 0;
    display: flex;
    align-items: center;
    justify-content: center;
    height: 85px;
  }

  .text-wrapper {
    color: var(--color-text-primary);
    font-family: var(--font-additional);
    font-size: var(--font-size-main);
    text-align: center;
  }
</style>
