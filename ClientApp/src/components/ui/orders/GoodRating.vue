<template>
  <div :class="['good-rating', className]">
    <div class="text-wrapper">{{ rating }}</div>
  </div>
</template>
<script lang="ts">
  import { defineComponent } from 'vue';
  export default defineComponent({
    name: 'GoodRating',
    props: {
      className: { type: String, default: '' },
      rating: { type: String, default: '4.72' },
    },
  });
</script>
<style scoped>
  .good-rating {
    background-color: var(--color-success);
    border: var(--border-interactive) solid var(--color-border-2);
    border-radius: var(--radius-main);
    display: flex;
    align-items: center;
    justify-content: center;
    height: 90px;
    width: 113px;
    overflow: hidden;
  }

  .text-wrapper {
    color: var(--color-text-primary);
    font-family: var(--font-additional);
    font-size: var(--font-size-main);
    font-weight: 400;
    text-align: center;
  }
</style>
