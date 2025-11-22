<template>
  <div :class="['review-user', className]">
    <div class="text-wrapper">{{ user }}</div>
  </div>
</template>
<script lang="ts">
  import { defineComponent } from 'vue';
  export default defineComponent({
    name: 'ReviewUser',
    props: {
      className: { type: String, default: '' },
      user: { type: String, default: 'User-21' },
    },
  });
</script>

<style scoped>
  .review-user {
    background-color: var(--color-background-accent);
    border: var(--border-interactive) solid var(--color-border-1);
    border-radius: var(--radius-main);
    display: flex;
    align-items: center;
    height: 90px;
    width: 340px;
    overflow: hidden;
  }

  .text-wrapper {
    color: var(--color-text-primary);
    font-family: var(--font-additional);
    font-size: var(--font-size-main);
    padding-left: 1rem;
    text-align: center;
  }
</style>
