<template>
  <div :class="['category-lbl', className]">
    <div class="text-wrapper">Жанр:</div>
  </div>
</template>
<script lang="ts">
  import { defineComponent } from 'vue';
  export default defineComponent({
    name: 'CategoryLbl',
    props: {
      className: { type: String, default: '' },
    },
  });
</script>
<style scoped>
  .category-lbl {
    background-color: var(--color-gift-accent);
    border: 6px solid var(--color-pink-border);
    border-radius: var(--radius-main);
    display: flex;
    align-items: center;
    justify-content: center;
    height: 90px;
    width: 150px;
    overflow: hidden;
  }

  .text-wrapper {
    color: var(--color-text-primary);
    font-family: var(--font-main);
    font-size: var(--font-size-main);
    font-weight: 400;
    text-align: center;
    width: 205px;
  }
</style>
