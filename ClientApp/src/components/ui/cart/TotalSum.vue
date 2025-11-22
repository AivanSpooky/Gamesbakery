<template>
  <div :class="['total-sum', className]">
    <div class="text-wrapper">Общая сумма: {{ total }}</div>
  </div>
</template>
<script lang="ts">
  import { defineComponent } from 'vue';
  export default defineComponent({
    name: 'TotalSum',
    props: {
      className: { type: String, default: '' },
      total: { type: Number, default: 0 },
    },
  });
</script>
<style scoped>
  .total-sum {
    background-color: var(--color-background-accent);
    border: var(--border-interactive) solid var(--color-border-1);
    border-radius: var(--radius-main);
    display: flex;
    align-items: center;
    justify-content: center;
    height: 90px;
    width: 100%;
    max-width: 507px;
    overflow: hidden;
  }

  .text-wrapper {
    color: var(--color-text-primary);
    font-family: var(--font-additional);
    font-size: var(--font-size-main);
    text-align: center;
  }
</style>
