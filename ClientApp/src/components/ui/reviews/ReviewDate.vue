<template>
  <div :class="['review-date', className]">
    <div class="text-wrapper">{{ date }}</div>
  </div>
</template>
<script lang="ts">
  import { defineComponent } from 'vue';
  export default defineComponent({
    name: 'ReviewDate',
    props: {
      className: { type: String, default: '' },
      date: { type: String, default: '01/02/2025' },
    },
  });
</script>
<style scoped>
  .review-date {
    background-color: var(--color-gift-accent);
    border: var(--border-notification) solid var(--color-border-5);
    border-radius: var(--radius-main);
    display: flex;
    align-items: center;
    justify-content: center;
    height: 71px;
    width: 124px;
    overflow: hidden;
  }

  .text-wrapper {
    color: var(--color-text-primary);
    font-family: var(--font-additional);
    font-size: var(--font-size-small);
    text-align: center;
  }
</style>
