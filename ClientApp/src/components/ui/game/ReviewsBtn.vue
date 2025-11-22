<template>
  <div class="reviews-btn neon-button" @click="$emit('click')">
    <div class="text-wrapper">Отзывы</div>
  </div>
</template>
<script lang="ts">
  import { defineComponent } from 'vue';
  export default defineComponent({
    name: 'ReviewsBtn',
    emits: ['click'],
  });
</script>
<style scoped>
  .reviews-btn {
    background-color: var(--color-background-accent);
    border: var(--border-interactive) solid var(--color-border-1);
    border-radius: var(--radius-main);
    display: flex;
    align-items: center;
    justify-content: center;
    height: 90px;
    overflow: hidden;
    position: relative;
    width: 144px;
    cursor: pointer;
  }

  .text-wrapper {
    color: var(--color-text-primary);
    font-family: var(--font-additional);
    font-size: var(--font-size-medium);
    font-weight: 400;
    text-align: center;
  }
</style>
