<template>
  <div :class="['bad-rating', className]">
    <div class="text-wrapper">{{ rating }}</div>
  </div>
</template>
<script lang="ts">
  import { defineComponent } from 'vue';
  export default defineComponent({
    name: 'BadRating',
    props: {
      className: { type: String, default: '' },
      rating: { type: String, default: '2.40' },
    },
  });
</script>
<style scoped>
  .bad-rating {
    background-color: var(--color-btn-accent);
    border: var(--border-interactive) solid var(--color-border-3);
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
