<template>
  <div class="sign-in-square">
    <svg width="72" height="72" viewBox="0 0 72 72" fill="none" xmlns="http://www.w3.org/2000/svg">
      <rect width="72" height="72" rx="10" fill="var(--color-background-accent)" />
      <path d="M36 18L48 36L36 54L24 36L36 18Z" fill="var(--color-text-primary)" />
    </svg>
  </div>
</template>

<script lang="ts">
  import { defineComponent } from 'vue';

  export default defineComponent({
    name: 'SignInSquare',
  });
</script>

<style scoped>
  .sign-in-square {
    height: 72px;
    width: 72px;
  }
</style>
