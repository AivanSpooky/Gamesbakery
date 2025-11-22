<template>
  <div :class="['reviews-lbl', className]" @click="isButton ? $emit('click') : null" :style="{ cursor: isButton ? 'pointer' : 'default' }">
    <div class="text-wrapper">{{ text }}</div>
  </div>
</template>
<script lang="ts">
  import { defineComponent } from 'vue';
  export default defineComponent({
    name: 'ReviewsLbl',
    props: {
      className: { type: String, default: '' },
      text: { type: String, default: 'Отзывы' },
      isButton: { type: Boolean, default: false },
    },
    emits: ['click'],
  });
</script>
<style scoped>
  .reviews-lbl {
    background-color: var(--color-background-accent);
    border: var(--border-interactive) solid var(--color-border-1);
    border-radius: var(--radius-main);
    display: flex;
    align-items: center;
    justify-content: center;
    height: 120px;
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
